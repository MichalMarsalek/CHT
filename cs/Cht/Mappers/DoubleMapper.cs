namespace Cht.Mappers;

public class DoubleMapper(string suffix = "") : ChtMapper<double>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out double output)
    {
        output = default;
        return node.IsJustRaw && double.TryParse(RemoveSuffix(node.Raw, suffix), System.Globalization.CultureInfo.InvariantCulture, out output);
    }

    public override bool ToNode(double value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtNode(value.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + suffix, null);
        return true;
    }
}   