namespace Cht.Mappers;

public class GuidMapper : ChtMapper<Guid>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out Guid output)
    {
        if (node is ChtTerminal terminal && terminal.IsJustRaw && Guid.TryParse(terminal.Raw, out output))
        {
            return true;
        }
        output = default;
        return false;
    }

    public override bool ToNode(Guid value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtTerminal { Raw = value.ToString() };
        return true;
    }
}
