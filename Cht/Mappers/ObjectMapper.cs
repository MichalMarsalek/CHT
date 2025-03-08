using System.Collections;
using System.Reflection;

namespace Cht.Mappers;
public class ObjectMapper : IChtMapper
{
    private readonly Dictionary<string, Type> _typeMap;

    public ObjectMapper(IEnumerable<Type> types)
    {
        _typeMap = types
            .Where(x => !x.IsEnum)
            .ToDictionary(x => x.GetCustomAttribute<ChtTypeAttribute>()?.TypeName ?? x.Name, x => x);
    }

    public bool FromNode(ChtNode node, Type targetType, ChtSerializer serializer, out object? output)
    {
        if (node is ChtNonterminal nonterminal && _typeMap.TryGetValue(nonterminal.Type, out Type type) && type.IsAssignableTo(targetType))
        {
            output = FromNode(nonterminal, type, serializer);
            return true;
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
            type.GetCustomAttribute<ChtTypeAttribute>()?.TypeName ?? type.Name,
            value.GetType().GetProperties().Where(x => x.CanRead)
                .Where(prop => prop.GetCustomAttribute<ChtIgnoreAttribute>() is null)
                .SelectMany(prop =>
                {
                    var child = serializer.ToNode(prop.GetValue(value));
                    if (child is ChtNonterminal nonterminal && prop.GetCustomAttribute<ChtFlattenAttribute>() is not null)
                    {
                        return nonterminal.Children;
                    }
                    return Enumerable.Repeat(child, 1);
                })
        );
        return true;
    }

    private object? FromNode(ChtNonterminal node, Type type, ChtSerializer serializer)
    {
        var props = type.GetProperties().Where(x => x.CanWrite && x.GetCustomAttribute<ChtIgnoreAttribute>() is null).ToList();
        if (props[..^1].Any(x => x.GetCustomAttribute<ChtFlattenAttribute>() is not null))
        {
            throw new ArgumentException("Only the last property can be flattened.");
        }
        var result = Activator.CreateInstance(type);
        var i = 0;
        var isLastFlattened = props.Last().GetCustomAttribute<ChtFlattenAttribute>() is not null;
        if (isLastFlattened ? node.Children.Count < props.Count - 1 : node.Children.Count != props.Count)
        {
            throw new ChtMappingException(this, $"Invalid number of children to deserialize type {node.Type}. Got {node.Children.Count} , expected {(isLastFlattened ? $"{props.Count() - 1}+" : $"{props.Count()}")}.");
        }
        foreach (var prop in props)
        {
            object? value;
            if (prop.GetCustomAttribute<ChtFlattenAttribute>() is not null)
            {
                ChtNonterminal? template = null;
                Exception? ex = null;
                try
                {   
                    var templateType = prop.PropertyType.IsAssignableTo(typeof(IDictionary)) ? typeof(Dictionary<object, object>) : prop.PropertyType.IsAssignableTo(typeof(IEnumerable)) ? typeof(List<object>) : prop.PropertyType;
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
                value = serializer.FromNode(template, prop.PropertyType);
            } else
            {                
                value = serializer.FromNode(node.Children[i], prop.PropertyType);
            }
            prop.SetValue(result, value);
            i++;
        }
        return result;
    }
}