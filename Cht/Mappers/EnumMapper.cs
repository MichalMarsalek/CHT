using System.Reflection;

namespace Cht.Mappers;

public class EnumMapper : IChtMapper
{
    private readonly Dictionary<string, Type> _typeMap;
    private readonly EnumMappingStyle _style;

    public EnumMapper(IEnumerable<Type> enumTypes, EnumMappingStyle style)
    {
        _typeMap = enumTypes
            .Where(x => x.IsEnum)
            .ToDictionary(x => x.GetCustomAttribute<ChtTypeAttribute>()?.TypeName ?? x.Name, x => x);
        _style = style;
    }

    public bool FromNode(ChtNode node, Type targetType, ChtSerializer serializer, out object? output)
    {
        output = default;
        var type = Unify(targetType, (node as ChtNonterminal)?.Type);
        if (type is null) return false;
        if (node is ChtNonterminal nonterminal)
        {
            if (nonterminal.Children.Count == 1 && nonterminal.Children[0] is ChtTerminal valueNode)
            {
                if (valueNode.IsJustQuoted && Enum.TryParse(type, valueNode.Quoted, out output))
                {
                    return true;
                }
                if (valueNode.IsJustRaw && Enum.TryParse(type, char.ToUpper(valueNode.Raw[0]) + valueNode.Raw[1..], out output))
                {
                    return true;
                }
            }
            throw new ArgumentException("Invalid enum node.");
        }
        if (node is ChtTerminal terminal)
        {
            if (terminal.IsJustQuoted && Enum.TryParse(type, terminal.Quoted, out output))
            {
                return true;
            }
            if (terminal.IsJustRaw && Enum.TryParse(type, char.ToUpper(terminal.Raw[0]) + terminal.Raw[1..], out output))
            {
                return true;
            }
        }
        return false;
    }

    private Type? Unify(Type targetType, string? nodeType)
        => nodeType is not null && _typeMap.TryGetValue(nodeType, out Type type) && type.IsAssignableTo(targetType) ? type
        : targetType.IsEnum ? targetType
        : null;

    public bool ToNode(object? value, ChtSerializer serializer, out ChtNode output)
    {
        if (value is Enum enumValue)
        {
            var type = value.GetType();
            var valueName = Enum.GetName(type, enumValue);
            var innerNode = _style switch
            {
                EnumMappingStyle.UntypedRawName or EnumMappingStyle.TypedRawName => ChtTerminal.JustRaw(char.ToLower(valueName[0]) + valueName[1..]),
                EnumMappingStyle.UntypedQuotedName or EnumMappingStyle.TypedQuotedName => ChtTerminal.JustQuoted(valueName),
                EnumMappingStyle.UntypedOrdinal or EnumMappingStyle.TypedOrdinal => ChtTerminal.JustRaw(((int)(object)enumValue).ToString()),
                _ => throw new ArgumentException("Invalid enum mapping style.")
            };
            output = _style is EnumMappingStyle.UntypedRawName or EnumMappingStyle.UntypedQuotedName or EnumMappingStyle.UntypedOrdinal ? innerNode
                : new ChtNonterminal(type.GetCustomAttribute<ChtTypeAttribute>()?.TypeName ?? type.Name, innerNode);
            return true;
        }
        output = default;
        return false;
    }
}

public enum EnumMappingStyle
{
    /// <summary>
    /// `Enum.Variant = 1` is serialized as `variant`.
    /// </summary>
    UntypedRawName,

    /// <summary>
    /// `Enum.Variant = 1` is serialized as `"Variant"`.
    /// </summary>
    UntypedQuotedName,

    /// <summary>
    /// `Enum.Variant = 1` is serialized as `1`.
    /// </summary>
    UntypedOrdinal,

    /// <summary>
    /// `Enum.Variant = 1` is serialized as `Enum(variant)`.
    /// </summary>
    TypedRawName,

    /// <summary>
    /// `Enum.Variant = 1` is serialized as `Enum("Variant")`.
    /// </summary>
    TypedQuotedName,

    /// <summary>
    /// `Enum.Variant = 1` is serialized as `Enum(1)`.
    /// </summary>
    TypedOrdinal,
}