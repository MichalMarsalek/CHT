using System.Collections;
using System.Reflection;
using System.Text;

namespace Cht;

public class ChtSerializer
{
    /// <summary>
    /// This value is appended to the existing line indentation upon each indentation increase.
    /// </summary>
    public string Indentation { get; set; } = "  ";

    /// <summary>
    /// 0 = no single line nesting, null = unlimited nesting of function nodes
    /// </summary>
    public int? MaximumDepth = 2;

    /// <summary>
    /// true = rest of line nodes are used as direct children on blocks, false = rest of line nodes are never used
    /// </summary>
    public bool UseRestOfLineNodes = true;

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
        throw new Exception($"No mapper able to handle value of type {typeof(T).Name}.");
    }
    public T FromNode<T>(ChtNode node)
    {
        foreach (var mapper in Mappers.Reverse())
        {
            if (mapper.FromNode(node, this, out var output) && output is T result)
            {
                return result;
            }
        }
        throw new Exception($"No mapper able to handle value of type {typeof(T).Name}.");
    }

    public string Serialize<T>(T value)
    {
        return Emit(ToNode(value));
    }

    public T Deserialize<T>(string value)
    {
        return FromNode<T>(Parse(value));
    }

    public ChtNode Parse(string value)
    {
        throw new NotImplementedException();
    }

    public string Emit(ChtNode node)
    {
        var depths = new Dictionary<ChtNode, int>();
        int calcDepth(ChtNode node)
        {
            var depth = 0;
            if (node is ChtNonterminal nonterminal)
            {
                foreach (var child in nonterminal.Children)
                {
                    depth = Math.Max(depth, 1+calcDepth(child));
                }
            }
            depths[node] = depth;
            return depth;
        }
        calcDepth(node);
        var builder = new StringBuilder();

        void Append(ChtNode node, int indentationLevel, int lineNestingLevel)
        {
            switch (node)
            {
                case ChtNonterminal nonterminal:
                    if (!nonterminal.Children.Any())
                    {
                        builder.Append(nonterminal.Type).Append("()");
                        break;
                    }
                    else
                    {
                        var depth = depths[node];
                        if (MaximumDepth < depth) {
                            builder.Append(nonterminal.Type).Append(":");
                            foreach (var child in nonterminal.Children)
                            {
                                builder.AppendLine();
                                for (int i = 0; i <= indentationLevel; i++)
                                {
                                    builder.Append(Indentation);
                                }
                                Append(child, indentationLevel + 1, 0);
                            }
                        }
                        else if (lineNestingLevel == 0 && UseRestOfLineNodes)
                        {
                            builder.Append(nonterminal.Type).Append(":");
                            foreach (var child in nonterminal.Children)
                            {
                                builder.Append(" ");
                                Append(child, indentationLevel, lineNestingLevel + 1);
                            }
                        }
                        else
                        {
                            builder.Append(nonterminal.Type).Append("(");
                            Append(nonterminal.Children[0], indentationLevel, lineNestingLevel+1);
                            foreach (var child in nonterminal.Children.Skip(1))
                            {
                                builder.Append(" ");
                                Append(child, indentationLevel, lineNestingLevel + 1);
                            }
                            builder.Append(")");
                        }
                        
                    }
                        break;
                case ChtRawTerminal rawTerminal:
                    builder.Append(rawTerminal.Value);
                    break;
                case ChtQuotedTerminal quotedTerminal:
                    builder.Append("\"").Append(quotedTerminal.Value.Replace(@"\", @"\\").Replace("\"", "\\\"")).Append("\"");
                    break;
                default:
                    throw new Exception($"Unknown node type {node.GetType().Name}.");
            }
        }
        Append(node, 0, 0);
        return builder.ToString();
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
        TypeMap = types.ToDictionary(x => x.GetCustomAttribute<ChtTypeAttribute>()?.TypeName ?? x.Name, x => x);
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
        var type = value.GetType();
        output = new ChtNonterminal
        {
            Type = type.GetCustomAttribute<ChtTypeAttribute>()?.TypeName ?? type.Name,
            Children = value.GetType().GetProperties().Where(x => x.CanRead)
                .Where(x => x.GetCustomAttribute<ChtIgnoreAttribute>() is null)
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