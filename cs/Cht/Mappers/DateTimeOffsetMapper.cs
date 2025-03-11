namespace Cht.Mappers;

public class DateTimeOffsetMapper : ChtMapper<DateTimeOffset>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out DateTimeOffset output)
    {
        output = default;
        return node is ChtTerminal terminal && terminal.IsJustRaw && DateTimeOffset.TryParse(terminal.Raw, out output);
    }

    public override bool ToNode(DateTimeOffset value, ChtSerializer serializer, out ChtNode output)
    {
        string format = value.Millisecond > 0 || value.Microsecond > 0 || value.Nanosecond > 0  ? "o" : "yyyy-MM-ddTHH:mm:sszzz";
        output = ChtTerminal.JustRaw(value.ToString(format));
        return true;
    }
}