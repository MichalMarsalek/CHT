using System.Reflection;

namespace Cht.Mappers;
public class ObjectMapper : IChtMapper
{
    // TODO: generic types
    private readonly Dictionary<string, Type> _typeMap;

    public ObjectMapper(IEnumerable<Type> types)
    {
        _typeMap = types
            .Where(x => !x.IsEnum)
            .ToDictionary(x => x.GetCustomAttribute<ChtTypeAttribute>()?.TypeName ?? x.Name, x => x);
    }

    public bool FromNode(ChtNode node, ChtSerializer serializer, out object? output)
    {
        if (node is ChtNonterminal nonterminal && _typeMap.TryGetValue(nonterminal.Type, out Type type))
        {
            output = FromNode(nonterminal, serializer, type);
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

    private static object FromNode(ChtNonterminal node, ChtSerializer serializer, Type type)
    {
        throw new NotImplementedException();
    }
}