namespace Cht.Mappers;

public class LongMapper(string suffix) : ChtMapper<long>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out long output)
    {
        output = default;
        return node.IsJustRaw && long.TryParse(RemoveSuffix(node.Raw, suffix), out output);
    }

    public override bool ToNode(long value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtNode(value.ToString() + suffix, null);
        return true;
    }
}