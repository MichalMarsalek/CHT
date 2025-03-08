namespace Cht.Mappers;

public class GuidMapper : ChtMapper<Guid>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out Guid output)
    {
        output = default;
        return node is ChtTerminal terminal && terminal.IsJustRaw && Guid.TryParse(terminal.Raw, out output);
    }

    public override bool ToNode(Guid value, ChtSerializer serializer, out ChtNode output)
    {
        output = ChtTerminal.JustRaw(value.ToString());
        return true;
    }
}
