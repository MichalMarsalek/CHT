namespace Cht.Mappers;

/// <param name="type"></param>
public class UriMapper(string? type = "Uri") : IChtMapper
{
    private readonly string? _type = type;

    public bool FromNode(ChtNode node, Type targetType, ChtSerializer serializer, out object? output)
    {
        output = default;
        var isUri = targetType == typeof(Uri) || (_type is not null && node.IsRawWithChildren && node.Raw == _type) && typeof(Uri).IsAssignableTo(targetType);
        if (!isUri) return false;

        if (node.IsJustQuoted)
        {
            output = new Uri(node.Quoted);
            return true;
        }
        if (node.IsRawWithChildren && node.Children.Count == 1 && node.Children[0] is ChtNode valueNode && valueNode.IsJustQuoted)
        {
            output = new Uri(valueNode.Quoted);
            return true;
        }

        return false;
    }

    public bool ToNode(object value, ChtSerializer serializer, out ChtNode output)
    {
        output = default;
        if (value is not Uri uri) return false;
        output = _type is null ? new ChtNode(null, uri.ToString()) : new ChtNode(_type, null, [new ChtNode(null, uri.ToString())]);
        return true;
    }
}