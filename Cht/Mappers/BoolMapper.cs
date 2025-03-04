namespace Cht.Mappers;

public class BoolMapper : ChtMapper<bool>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out bool output)
    {
        output = default;
        if (node is ChtTerminal terminal && terminal.Quoted is null && (terminal.Raw == "true" || terminal.Raw == "false"))
        {
            output = terminal.Raw == "true";
            return true;
        }
        return false;
    }

    public override bool ToNode(bool value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtTerminal { Raw = value.ToString().ToLower() };
        return true;
    }
}