namespace Cht.Mappers;

public class DateTimeOffsetMapper : ChtMapper<DateTimeOffset>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out DateTimeOffset output)
    {
        output = default;
        return node.IsJustRaw && DateTimeOffset.TryParse(node.Raw, out output);
    }

    public override bool ToNode(DateTimeOffset value, ChtSerializer serializer, out ChtNode output)
    {
        string format = value.Millisecond > 0 || value.Microsecond > 0 || value.Nanosecond > 0  ? "o" : "yyyy-MM-ddTHH:mm:sszzz";
        output = new ChtNode(value.ToString(format), null);
        return true;
    }
}