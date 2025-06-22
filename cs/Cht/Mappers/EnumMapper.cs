using System.Reflection;

namespace Cht.Mappers;

public class EnumMapper(IEnumerable<Type> enumTypes, EnumMappingStyle style) : IChtMapper
{
    private readonly Dictionary<string, Type> _typeMap = enumTypes
            .Where(x => x.IsEnum)
            .ToDictionary(x => x.GetCustomAttribute<ChtTypeAttribute>()?.TypeName ?? x.Name, x => x);
    private readonly EnumMappingStyle _style = style;

    public bool FromNode(ChtNode node, Type targetType, ChtSerializer serializer, out object? output)
    {
        output = default;
        var type = Unify(targetType, node.IsRawWithChildren ? node.Raw : null);
        if (type is null) return false;
        if (node.IsRawWithChildren)
        {
            if (node.Children.Count == 1 && node.Children[0] is ChtNode valueNode)
            {
                if (valueNode.IsJustQuoted && Enum.TryParse(type, valueNode.Quoted.Replace("|", ", "), out output))
                {
                    return true;
                }
                if (valueNode.IsJustRaw && Enum.TryParse(type, valueNode.Raw.Replace("|", ", "), out output))
                {
                    return true;
                }
            }
            throw new ArgumentException("Invalid enum node.");
        }
        else
        {
            if (node.IsJustQuoted && Enum.TryParse(type, node.Quoted.Replace("|", ", "), out output))
            {
                return true;
            }
            if (node.IsJustRaw && Enum.TryParse(type, node.Raw.Replace("|", ", "), out output))
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
            var valueName = enumValue.ToString();
            if (_style is not EnumMappingStyle.UntypedOrdinal or EnumMappingStyle.TypedOrdinal && int.TryParse(valueName, out var _)) {
                throw new ChtMappingException(this, $"Enum value could not be serialized to {_style}. For flag enums, make sure the enum has the [Flags] attribute. For serializing unnamed enum values, use EnumMappingStyle.TypedOrdinal or EnumMappingStyle.UntypedOrdinal.");
            }
            var innerNode = _style switch
            {
                EnumMappingStyle.UntypedRawName or EnumMappingStyle.TypedRawName => new ChtNode(valueName.Replace(", ", "|"), null),
                EnumMappingStyle.UntypedQuotedName or EnumMappingStyle.TypedQuotedName => new ChtNode(null, valueName.Replace(", ", "|")),
                EnumMappingStyle.UntypedOrdinal or EnumMappingStyle.TypedOrdinal => new ChtNode(((int)(object)enumValue).ToString(), null),
                _ => throw new ArgumentException("Invalid enum mapping style.")
            };
            output = _style is EnumMappingStyle.UntypedRawName or EnumMappingStyle.UntypedQuotedName or EnumMappingStyle.UntypedOrdinal ? innerNode
                : new ChtNode(type.GetCustomAttribute<ChtTypeAttribute>()?.TypeName ?? type.Name, null, [innerNode]);
            return true;
        }
        output = default;
        return false;
    }
}

public enum EnumMappingStyle
{
    /// <summary>
    /// `Enum.Variant = 1` is serialized as `Variant`.
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
    /// `Enum.Variant = 1` is serialized as `Enum(Variant)`.
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