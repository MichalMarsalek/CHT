namespace Cht.Mappers;

public class IntMapper : ChtMapper<int>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out int output)
    {
        output = default;
        if (node is ChtTerminal terminal && terminal.IsJustRaw)
        {
            return int.TryParse(terminal.Raw, out output);
        }
        return false;
    }

    public override bool ToNode(int value, ChtSerializer serializer, out ChtNode output)
    {
        output = ChtTerminal.JustRaw(value.ToString());
        return true;
    }
}