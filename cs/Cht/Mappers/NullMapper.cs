namespace Cht.Mappers;

public class NullMapper : IChtMapper
{
    public bool FromNode(ChtNode node, Type targetType, ChtSerializer serializer, out object? output)
    {
        output = null;
        return node.IsJustRaw && node.Raw == "null";
    }

    public bool ToNode(object? value, ChtSerializer serializer, out ChtNode output)
    {
        output = default;
        if (value is null)
        {
            output = new ChtNode("null", null);
            return true;
        }
        return false;
    }
}