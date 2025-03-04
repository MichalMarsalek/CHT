namespace Cht.Mappers;

public class DateOnlyMapper : ChtMapper<DateOnly>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out DateOnly output)
    {
        output = default;
        if (node is ChtTerminal terminal && terminal.IsJustRaw)
        {
            return DateOnly.TryParse(terminal.Raw, out output);
        }
        return false;
    }

    public override bool ToNode(DateOnly value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtTerminal { Raw = value.ToString("yyyy-MM-dd") };
        return true;
    }
}