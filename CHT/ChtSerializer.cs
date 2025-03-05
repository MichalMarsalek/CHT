using System.Collections;
using System.Text;

namespace CHT
{
    public class ChtSerializer
    {
        /// <summary>
        /// This value is appended to the existing line indentation upon each indentation increase.
        /// </summary>
        public string Indentation { get; set; } = "  ";

        /// <summary>
        /// 0 = no function nodes, 1 = no nested function nodes, null = unlimited nesting of function nodes
        /// </summary>
        public int? MaximumFunctionDepth = 1;

        /// <summary>
        /// 0 = no rest of line nodes, 1 = no nested rest of line nodes, null = unlimited nesting of rest of line nodes
        /// </summary>
        public int? MaximumRestOfLineDepth = 1;

        public IList<IChtMapper> Mappers { get; set; } = [];

        public ChtSerializer AddNullMapper()
        {
            Mappers.Add(new NullMapper());
            return this;
        }

        public ChtSerializer AddStringMapper()
        {
            Mappers.Add(new StringMapper());
            return this;
        }

        public ChtSerializer AddBoolMapper()
        {
            Mappers.Add(new BoolMapper());
            return this;
        }

        public ChtSerializer AddIntMapper()
        {
            Mappers.Add(new IntMapper());
            return this;
        }

        public ChtSerializer AddDateOnlyMapper()
        {
            Mappers.Add(new DateOnlyMapper());
            return this;
        }

        public ChtSerializer AddTimeOnlyMapper()
        {
            Mappers.Add(new TimeOnlyMapper());
            return this;
        }

        public ChtSerializer AddObjectMapper(IEnumerable<Type> types)
        {
            Mappers.Add(new ObjectMapper(types));
            return this;
        }

        public ChtSerializer AddCommonMappers(IEnumerable<Type> types)
            => AddBoolMapper().AddIntMapper().AddStringMapper().AddDateOnlyMapper().AddTimeOnlyMapper().AddNullMapper().AddObjectMapper(types);

        public ChtNode ToNode<T>(T value)
        {
            foreach (var mapper in Mappers.Reverse())
            {
                if (mapper.ToNode(value, this, out var output))
                {
                    return output;
                }
            }
            throw new Exception($"No mapper able to handle value of type {value.GetType()}.");
        }

        private string Emit(ChtNode node)
            => Emit(node, new StringBuilder()).ToString();

        private StringBuilder Emit(ChtNode node, StringBuilder stringBuilder)
        {
            throw new NotImplementedException();
        }
    }

    public interface IChtMapper
    {
        bool FromNode(ChtNode node, ChtSerializer serializer, out object? output);
        bool ToNode(object? value, ChtSerializer serializer, out ChtNode output);
    }

    public abstract class ChtMapper<T> : IChtMapper
    {
        public abstract bool FromNode(ChtNode node, ChtSerializer serializer, out T output);
        public abstract bool ToNode(T value, ChtSerializer serializer, out ChtNode output);

        public bool FromNode(ChtNode node, ChtSerializer serializer, out object output)
        {
            if (FromNode(node, serializer, out T typedOutput))
            {
                output = typedOutput;
                return true;
            }
            output = default;
            return false;
        }

        public bool ToNode(object? value, ChtSerializer serializer, out ChtNode output)
        {
            if (value is T typedValue)
            {
                return ToNode(typedValue, serializer, out output);
            }
            output = default;
            return false;
        }
    }

    public class ObjectMapper : IChtMapper
    {
        // TODO: generic types
        public Dictionary<string, Type> TypeMap { get; set; }

        public ObjectMapper(IEnumerable<Type> types)
        {
            TypeMap = types.ToDictionary(x => x.Name, x => x);
        }

        public bool FromNode(ChtNode node, ChtSerializer serializer, out object? output)
        {
            output = default;
            if (node is ChtNonterminal nonterminal && TypeMap.TryGetValue(nonterminal.Type, out Type type))
            {
                output = FromNode(nonterminal, serializer, type);
                return true;
            }
            return false;
        }

        public bool ToNode(object? value, ChtSerializer serializer, out ChtNode output)
        {
            if (value is null)
            {
                output = default;
                return false;
            }
            output = new ChtNonterminal
            {
                Type = value.GetType().Name,
                Children = value.GetType().GetProperties().Where(x => x.CanRead)
                    .SelectMany(prop => prop.PropertyType.IsAssignableTo(typeof(IEnumerable)) ? (prop.GetValue(value) as IEnumerable).Cast<object>() : Enumerable.Repeat(prop.GetValue(value), 1))
                    .Select(serializer.ToNode).ToList()
            };
            return false;
        }

        private static object FromNode(ChtNonterminal node, ChtSerializer serializer, Type type)
        {
            throw new NotImplementedException();
        }
    }

    public class NullMapper : IChtMapper
    {
        public bool FromNode(ChtNode node, ChtSerializer serializer, out object? output)
        {
            output = default;
            if (node is ChtRawTerminal rawTerminal && rawTerminal.Value == "null")
            {
                output = null;
                return true;
            }
            return false;
        }

        public bool ToNode(object? value, ChtSerializer serializer, out ChtNode output)
        {
            output = default;
            if (value is null)
            {
                output = new ChtRawTerminal { Value = "null" };
                return true;
            }
            return false;
        }
    }

    public class StringMapper : ChtMapper<string>
    {
        public override bool FromNode(ChtNode node, ChtSerializer serializer, out string output)
        {
            output = default;
            if (node is ChtQuotedTerminal quotedTerminal)
            {
                output = quotedTerminal.Value;
                return true;
            }
            return false;
        }

        public override bool ToNode(string value, ChtSerializer serializer, out ChtNode output)
        {
            output = new ChtQuotedTerminal { Value = value };
            return true;
        }
    }

    public class BoolMapper : ChtMapper<bool>
    {
        public override bool FromNode(ChtNode node, ChtSerializer serializer, out bool output)
        {
            output = default;
            if (node is ChtRawTerminal rawTerminal && (rawTerminal.Value == "true" || rawTerminal.Value == "false"))
            {
                output = rawTerminal.Value == "true";
                return true;
            }
            return false;
        }

        public override bool ToNode(bool value, ChtSerializer serializer, out ChtNode output)
        {
            output = new ChtRawTerminal { Value = value.ToString() };
            return true;
        }
    }

    public class IntMapper : ChtMapper<int>
    {
        public override bool FromNode(ChtNode node, ChtSerializer serializer, out int output)
        {
            output = default;
            if (node is ChtRawTerminal rawTerminal)
            {
                return int.TryParse(rawTerminal.Value, out output);
            }
            return false;
        }

        public override bool ToNode(int value, ChtSerializer serializer, out ChtNode output)
        {
            output = new ChtRawTerminal { Value = value.ToString() };
            return true;
        }
    }

    public class DateOnlyMapper : ChtMapper<DateOnly>
    {
        public override bool FromNode(ChtNode node, ChtSerializer serializer, out DateOnly output)
        {
            output = default;
            if (node is ChtRawTerminal rawTerminal)
            {
                return DateOnly.TryParse(rawTerminal.Value, out output);
            }
            return false;
        }

        public override bool ToNode(DateOnly value, ChtSerializer serializer, out ChtNode output)
        {
            output = new ChtRawTerminal { Value = value.ToString() };
            return true;
        }
    }

    public class TimeOnlyMapper : ChtMapper<TimeOnly>
    {
        public override bool FromNode(ChtNode node, ChtSerializer serializer, out TimeOnly output)
        {
            output = default;
            if (node is ChtRawTerminal rawTerminal)
            {
                return TimeOnly.TryParse(rawTerminal.Value, out output);
            }
            return false;
        }

        public override bool ToNode(TimeOnly value, ChtSerializer serializer, out ChtNode output)
        {
            output = new ChtRawTerminal { Value = value.ToString() };
            return true;
        }
    }
}

/*
Block:
    Assignment: $x List(0 58 15)
    MethodCall: $x $append 7
    Assignment: $y Indexing($x 3)
    FunctionCall: $print $y
    Assignment: $text "Some text with spaces"

Block:
    "Block"
    RestOfLine: "Assignment" Raw("$x") Function("List" Raw("0") Raw("58") Raw("15"))
    RestOfLine: "MethodCall" Raw("$x") Raw("$append") Raw("7")
    RestOfLine "Assignment" Raw("$y") Function("Indexing" Raw("$x") Raw("3"))
    RestOfLine: "FunctionCall" Raw("$print") Raw("$y")
    RestOfLine: "Assignment" Raw("$text") Quoted("Some text with spaces")
*/