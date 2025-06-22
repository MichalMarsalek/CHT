using System.Drawing;

namespace Cht.Mappers;

public class ColorMapper : ChtMapper<Color>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out Color output)
    {
        output = default;
        if (node.IsJustRaw && node.Raw.StartsWith("#"))
        {
            try
            {
                output = ColorTranslator.FromHtml(node.Raw);
                return true;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    public override bool ToNode(Color value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtNode($"#{value.R:X2}{value.G:X2}{value.B:X2}", null);
        return true;
    }
}
