namespace Cht.Mappers;

public class StringMapper : ChtMapper<string>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out string output)
    {
        output = default;
        if (node is ChtTerminal terminal && terminal.IsJustQuoted)
        {
            output = terminal.Quoted;
            return true;
        }
        return false;
    }

    public override bool ToNode(string value, ChtSerializer serializer, out ChtNode output)
    {
        output = ChtTerminal.JustQuoted(value);
        return true;
    }
}