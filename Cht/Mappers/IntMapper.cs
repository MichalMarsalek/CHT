namespace Cht.Mappers;

public class IntMapper : ChtMapper<int>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out int output)
    {
        output = default;
        return node is ChtTerminal terminal && terminal.IsJustRaw && int.TryParse(terminal.Raw, out output);
    }

    public override bool ToNode(int value, ChtSerializer serializer, out ChtNode output)
    {
        output = ChtTerminal.JustRaw(value.ToString());
        return true;
    }
}