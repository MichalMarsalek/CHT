using System.Collections;
using System.Reflection;

namespace Cht.Mappers;
public class ObjectMapper(IEnumerable<Type> types, bool skipTrailingNulls = true) : IChtMapper
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
        var props = type.GetProperties().Where(x => x.CanRead)
                .Where(prop => prop.GetCustomAttribute<ChtIgnoreAttribute>() is null)
                .Select(prop => new { Property = prop, Value = prop.GetValue(value) });
        if (skipTrailingNulls)
        {
            props = props.Reverse().SkipWhile(x => x.Value is null).Reverse();
        }
        output = new ChtNonterminal(
            GetTypeName(type),
            props.SelectMany(prop =>
            {
                var propType = prop.Property.PropertyType;
                if (prop.Value is IList list && propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    return list.Cast<object>().Select(serializer.ToNode);
                }
                var child = serializer.ToNode(prop.Value);
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
            .Select(x => {
                var type = x.PropertyType;
                var acceptsSubvalue = false;
                Type? listType = null;
                Type? listItemType = null;
                if (type.IsGenericType)
                {
                    listItemType = type.GenericTypeArguments[0];
                    listType = typeof(List<>).MakeGenericType(listItemType);
                    if (listType.IsAssignableTo(type))
                    {
                        acceptsSubvalue = true;
                    }
                    else
                    {
                        listItemType = listType = null;
                    }
                }
                return new WritableProperty
                {
                    Property = x,
                    Type = type,
                    ListType = listType,
                    ListItemType = listItemType,
                    AcceptsValue = true,
                    AcceptsSubvalue = acceptsSubvalue,
                    List = listType != null ? (IList)Activator.CreateInstance(listType) : null,
                };
            }).ToList();
        var result = Activator.CreateInstance(type);
        foreach (var child in node.Children)
        {
            var childPlaced = false;
            foreach (var prop in props)
            {
                if (prop.AcceptsValue)
                {
                    try
                    {
                        var childValue = serializer.FromNode(child, prop.Type);
                        prop.Property.SetValue(result, childValue);
                        prop.AcceptsValue = false;
                        prop.AcceptsSubvalue = false;
                        childPlaced = true;
                        break;
                    }
                    catch (ChtMappingException) { }
                }

                if (prop.AcceptsSubvalue)
                {
                    try
                    {
                        var childValue = serializer.FromNode(child, prop.ListItemType!);
                        prop.List!.Add(childValue);
                        if (prop.AcceptsValue)
                        {
                            prop.Property.SetValue(result, prop.List);
                            prop.AcceptsValue = false;
                        }
                        childPlaced = true;
                        break;
                    }
                    catch (ChtMappingException) { }
                }
            }
            if (childPlaced) continue;
            throw new ChtMappingException(this, $"Node {child.ToString()} could not be mapped to any unused property of {GetTypeName(type)}.");
        }
        return result;
    }

    private class WritableProperty
    {
        public required PropertyInfo Property { get; set; }
        public required Type Type { get; set; }
        public Type? ListType { get; set; }
        public Type? ListItemType { get; set; }
        public IList? List{ get; set; }
        public bool AcceptsValue { get; set; }
        public bool AcceptsSubvalue { get; set; }
    }
}