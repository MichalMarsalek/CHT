using System.Drawing;

namespace Cht.Mappers;

public class ColorMapper : ChtMapper<Color>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out Color output)
    {
        output = default;
        if (node is ChtTerminal terminal && terminal.IsJustRaw && terminal.Raw.StartsWith("#"))
        {
            try
            {
                output = ColorTranslator.FromHtml(terminal.Raw);
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
        output = ChtTerminal.JustRaw($"#{value.R:X2}{value.G:X2}{value.B:X2}");
        return true;
    }
}
