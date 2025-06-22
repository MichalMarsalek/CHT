namespace Cht.Mappers;

public class TimeOnlyMapper : ChtMapper<TimeOnly>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out TimeOnly output)
    {
        output = default;
        return node.IsJustRaw && TimeOnly.TryParse(node.Raw, out output);
    }

    public override bool ToNode(TimeOnly value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtNode(value.ToString("HH:mm:ss"), null);
        return true;
    }
}