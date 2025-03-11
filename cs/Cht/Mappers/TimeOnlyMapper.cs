namespace Cht.Mappers;

public class TimeOnlyMapper : ChtMapper<TimeOnly>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out TimeOnly output)
    {
        output = default;
        return node is ChtTerminal terminal && terminal.IsJustRaw && TimeOnly.TryParse(terminal.Raw, out output);
    }

    public override bool ToNode(TimeOnly value, ChtSerializer serializer, out ChtNode output)
    {
        output = ChtTerminal.JustRaw(value.ToString("HH:mm:ss"));
        return true;
    }
}