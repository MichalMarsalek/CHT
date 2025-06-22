namespace Cht.Mappers;

public class GuidMapper : ChtMapper<Guid>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out Guid output)
    {
        output = default;
        return node.IsJustRaw && Guid.TryParse(node.Raw, out output);
    }

    public override bool ToNode(Guid value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtNode(value.ToString(), null);
        return true;
    }
}
