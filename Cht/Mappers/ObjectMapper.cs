using System.Collections;
using System.Reflection;

namespace Cht.Mappers;
public class ObjectMapper(IEnumerable<Type> types, bool autoFlatten = false) : IChtMapper
{
    private readonly Dictionary<string, Type> _typeMap = types
            .Where(x => !x.IsEnum)
            .ToDictionary(GetTypeName, x => x);

    public bool FromNode(ChtNode node, Type targetType, ChtSerializer serializer, out object? output)
    {
        if (node is ChtNonterminal nonterminal)
        {
            var type = Unify(targetType, nonterminal.Type);
            if (type is not null)
            {
                output = FromNode(nonterminal, type, serializer);
                return true;
            }
        }
        output = default;
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
        output = new ChtNonterminal(
            GetTypeName(type),
            type.GetProperties().Where(x => x.CanRead)
                .Where(prop => prop.GetCustomAttribute<ChtIgnoreAttribute>() is null)
                .SelectMany(prop =>
                {
                    var propValue = prop.GetValue(value);
                    if (autoFlatten && propValue is IEnumerable enumerable && propValue is not string)
                    {
                        return enumerable.Cast<object>().Select(serializer.ToNode);
                    }
                    var child = serializer.ToNode(propValue);
                    if (child is ChtNonterminal nonterminal && prop.GetCustomAttribute<ChtFlattenAttribute>() is not null)
                    {
                        return nonterminal.Children;
                    }
                    return Enumerable.Repeat(child, 1);
                })
        );
        return true;
    }

    private Type? Unify(Type targetType, string nodeType)
    {
        if (!_typeMap.TryGetValue(nodeType, out Type result))
        {
            return null;
        }

        if (result.IsGenericTypeDefinition)
        {
            int arity = result.GetGenericArguments().Length;
            if (targetType.IsGenericType && targetType.GenericTypeArguments.Length == arity)
            {
                result = result.MakeGenericType(targetType.GenericTypeArguments);
            }
            else
            {
                result = result.MakeGenericType(Enumerable.Repeat(typeof(object), arity).ToArray());
            }
        }

        return result.IsAssignableTo(targetType) ? result : null;
    }
        
    private static string GetTypeName(Type type)
        => type.GetCustomAttribute<ChtTypeAttribute>()?.TypeName ?? type.Name.Split("`")[0];

    private object? FromNode(ChtNonterminal node, Type type, ChtSerializer serializer)
    {
        var props = type.GetProperties().Where(x => x.CanWrite && x.GetCustomAttribute<ChtIgnoreAttribute>() is null)
            .Select(x => new {
                Property = x,
                Type = x.PropertyType,
                IsFlattened = x.GetCustomAttribute<ChtFlattenAttribute>() is not null
                    || (autoFlatten && x.PropertyType.IsAssignableTo(typeof(IEnumerable)) && x.PropertyType != typeof(string))
            }).ToList();
        if (props[..^1].Any(x => x.IsFlattened))
        {
            throw new ArgumentException("Only the last property can be flattened.");
        }
        var result = Activator.CreateInstance(type);
        var i = 0;
        var isLastFlattened = props.Last().IsFlattened;
        if (isLastFlattened ? node.Children.Count < props.Count - 1 : node.Children.Count != props.Count)
        {
            throw new ChtMappingException(this, $"Invalid number of children to deserialize type {node.Type}. Got {node.Children.Count} , expected {(isLastFlattened ? $"{props.Count() - 1}+" : $"{props.Count()}")}.");
        }
        foreach (var prop in props)
        {
            object? value;
            if (prop.IsFlattened)
            {
                ChtNonterminal? template = null;
                Exception? ex = null;
                try
                {   
                    var templateType = prop.Type.IsAssignableTo(typeof(IDictionary)) ? typeof(Dictionary<object, object>) : prop.Type.IsAssignableTo(typeof(IEnumerable)) ? typeof(List<object>) : prop.Type;
                    template = serializer.ToNode(Activator.CreateInstance(templateType)) as ChtNonterminal;
                }
                catch (Exception e)
                {
                    ex = e;
                }
                if (template is null)
                {
                    throw new ChtMappingException(this, "Can only deserialize flattened values serializable to Nonterminal nodes.", ex);
                }

                template.Children = node.Children[i..];
                value = serializer.FromNode(template, prop.Type);
            } else
            {                
                value = serializer.FromNode(node.Children[i], prop.Type);
            }
            prop.Property.SetValue(result, value);
            i++;
        }
        return result;
    }
}