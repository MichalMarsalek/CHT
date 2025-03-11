namespace Cht.Mappers;

public class DecimalMapper(string suffix = "") : ChtMapper<decimal>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out decimal output)
    {
        output = default;
        return node is ChtTerminal terminal && terminal.IsJustRaw && decimal.TryParse(RemoveSuffix(terminal.Raw, suffix), out output);
    }

    public override bool ToNode(decimal value, ChtSerializer serializer, out ChtNode output)
    {
        output = ChtTerminal.JustRaw(value.ToString("0.0") + suffix);
        return true;
    }
}