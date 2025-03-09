﻿namespace Cht.Mappers;

public class DoubleMapper(string suffix = "") : ChtMapper<double>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out double output)
    {
        output = default;
        return node is ChtTerminal terminal && terminal.IsJustRaw && double.TryParse(RemoveSuffix(terminal.Raw, suffix), out output);
    }

    public override bool ToNode(double value, ChtSerializer serializer, out ChtNode output)
    {
        output = ChtTerminal.JustRaw(value.ToString("0.0") + suffix);
        return true;
    }
}