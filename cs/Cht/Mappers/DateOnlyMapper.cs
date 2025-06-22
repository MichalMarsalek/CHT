namespace Cht.Mappers;

public class DateOnlyMapper : ChtMapper<DateOnly>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out DateOnly output)
    {
        output = default;
        return node.IsJustRaw && DateOnly.TryParse(node.Raw, out output);
    }

    public override bool ToNode(DateOnly value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtNode(value.ToString("yyyy-MM-dd"), null);
        return true;
    }
}