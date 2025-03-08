namespace Cht.Mappers;

public class DateOnlyMapper : ChtMapper<DateOnly>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out DateOnly output)
    {
        output = default;
        return node is ChtTerminal terminal && terminal.IsJustRaw && DateOnly.TryParse(terminal.Raw, out output);
    }

    public override bool ToNode(DateOnly value, ChtSerializer serializer, out ChtNode output)
    {
        output = ChtTerminal.JustRaw(value.ToString("yyyy-MM-dd"));
        return true;
    }
}