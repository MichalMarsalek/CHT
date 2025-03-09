namespace Cht.Mappers;

/// <param name="type"></param>
public class UriMapper(string? type = "Uri") : IChtMapper
{
    private readonly string? _type = type;

    public bool FromNode(ChtNode node, Type targetType, ChtSerializer serializer, out object? output)
    {
        output = default;
        var isUri = targetType == typeof(Uri) || (_type is not null && (node as ChtNonterminal)?.Type == _type) && typeof(Uri).IsAssignableTo(targetType);
        if (!isUri) return false;

        if (node is ChtTerminal terminal && terminal.IsJustQuoted)
        {
            output = new Uri(terminal.Quoted);
            return true;
        }
        if (node is ChtNonterminal nonterminal && nonterminal.Children.Count == 1 && nonterminal.Children[0] is ChtTerminal valueNode && valueNode.IsJustQuoted)
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
        output = _type is null ? ChtTerminal.JustQuoted(value.ToString()) : new ChtNonterminal(_type, ChtTerminal.JustQuoted(value.ToString()));
        return true;
    }
}