namespace Cht.Mappers;

public class BoolMapper : ChtMapper<bool>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out bool output)
    {
        output = default;
        if (node.Quoted is null && (node.Raw == "true" || node.Raw == "false"))
        {
            output = node.Raw == "true";
            return true;
        }
        return false;
    }

    public override bool ToNode(bool value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtNode(value.ToString().ToLower(), null);
        return true;
    }
}