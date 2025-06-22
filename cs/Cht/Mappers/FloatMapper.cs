namespace Cht.Mappers;

public class FloatMapper(string suffix = "") : ChtMapper<float>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out float output)
    {
        output = default;
        return node.IsJustRaw && float.TryParse(RemoveSuffix(node.Raw, suffix), System.Globalization.CultureInfo.InvariantCulture, out output);
    }

    public override bool ToNode(float value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtNode(value.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + suffix, null);
        return true;
    }
}