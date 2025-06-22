using System.Globalization;

namespace Cht.Mappers;

public class IntMapper(string suffix = "") : ChtMapper<int>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out int output)
    {
        output = default;
        if (node.IsJustRaw)
        {
            string rawValue = RemoveSuffix(node.Raw, suffix);
            if (rawValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(rawValue[2..], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out output);
            }
            else if (rawValue.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(rawValue[2..], NumberStyles.AllowBinarySpecifier, CultureInfo.InvariantCulture, out output);
            }
            else
            {
                return int.TryParse(rawValue, out output);
            }
        }
        return false;
    }

    public override bool ToNode(int value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtNode(value.ToString() + suffix, null);
        return true;
    }
}