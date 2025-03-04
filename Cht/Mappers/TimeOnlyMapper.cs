namespace Cht.Mappers;

public class TimeOnlyMapper : ChtMapper<TimeOnly>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out TimeOnly output)
    {
        output = default;
        if (node is ChtTerminal terminal && terminal.IsJustRaw)
        {
            return TimeOnly.TryParse(terminal.Raw, out output);
        }
        return false;
    }

    public override bool ToNode(TimeOnly value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtTerminal { Raw = value.ToString("HH:mm:ss") };
        return true;
    }
}