using System.Reflection;

namespace Cht.Mappers;

public class EnumMapper : IChtMapper
{
    private readonly Dictionary<string, Type> _typeMap;

    public EnumMapper(IEnumerable<Type> enumTypes)
    {
        _typeMap = enumTypes
            .Where(x => x.IsEnum)
            .ToDictionary(x => x.GetCustomAttribute<ChtTypeAttribute>()?.TypeName ?? x.Name, x => x);
    }

    public bool FromNode(ChtNode node, Type targetType, ChtSerializer serializer, out object? output)
    {
        if (node is ChtNonterminal nonterminal)
        {
            var type = Unify(targetType, nonterminal.Type);
            if (type is not null)
            {

                if (nonterminal.Children.Count == 1 && nonterminal.Children[0] is ChtTerminal valueNode && valueNode.IsJustRaw)
                {
                    if (Enum.TryParse(type, char.ToUpper(valueNode.Raw[0]) + valueNode.Raw[1..], out output))
                    {
                        return true;
                    }
                }
                throw new ArgumentException("Invalid enum node.");
            }
        }
        output = default;
        return false;
    }

    private Type? Unify(Type targetType, string nodeType)
        => _typeMap.TryGetValue(nodeType, out Type type) && type.IsAssignableTo(targetType) ? type : null;

    public bool ToNode(object? value, ChtSerializer serializer, out ChtNode output)
    {
        if (value is Enum enumValue)
        {
            var type = value.GetType();
            var valueName = Enum.GetName(type, enumValue);
            output = new ChtNonterminal(            
                type.GetCustomAttribute<ChtTypeAttribute>()?.TypeName ?? type.Name,
                ChtTerminal.JustRaw(char.ToLower(valueName[0]) + valueName[1..])
            );
            return true;
        }
        output = default;
        return false;
    }
}
