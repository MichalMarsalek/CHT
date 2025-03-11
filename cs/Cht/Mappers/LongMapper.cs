namespace Cht.Mappers;

public class LongMapper(string suffix) : ChtMapper<long>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out long output)
    {
        output = default;
        return node is ChtTerminal terminal && terminal.IsJustRaw && long.TryParse(RemoveSuffix(terminal.Raw, suffix), out output);
    }

    public override bool ToNode(long value, ChtSerializer serializer, out ChtNode output)
    {
        output = ChtTerminal.JustRaw(value.ToString() + suffix);
        return true;
    }
}