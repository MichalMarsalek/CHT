namespace Cht.Mappers;

public class DecimalMapper(string suffix = "") : ChtMapper<decimal>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out decimal output)
    {
        output = default;
        return node.IsJustRaw && decimal.TryParse(RemoveSuffix(node.Raw, suffix), System.Globalization.CultureInfo.InvariantCulture, out output);
    }

    public override bool ToNode(decimal value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtNode(value.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + suffix, null);
        return true;
    }
}