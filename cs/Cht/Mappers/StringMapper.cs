namespace Cht.Mappers;

public class StringMapper : ChtMapper<string>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out string output)
    {
        output = default;
        if (node.IsJustQuoted)
        {
            output = node.Quoted;
            return true;
        }
        return false;
    }

    public override bool ToNode(string value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtNode(null, value);
        return true;
    }
}