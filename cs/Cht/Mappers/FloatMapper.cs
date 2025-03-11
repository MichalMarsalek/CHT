namespace Cht.Mappers;

public class FloatMapper(string suffix = "") : ChtMapper<float>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out float output)
    {
        output = default;
        return node is ChtTerminal terminal && terminal.IsJustRaw && float.TryParse(RemoveSuffix(terminal.Raw, suffix), out output);
    }

    public override bool ToNode(float value, ChtSerializer serializer, out ChtNode output)
    {
        output = ChtTerminal.JustRaw(value.ToString("0.0") + suffix);
        return true;
    }
}