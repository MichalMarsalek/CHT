using System.Numerics;

namespace Cht.Mappers;

public class BigIntegerMapper(string suffix = "") : ChtMapper<BigInteger>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out BigInteger output)
    {
        output = default;
        return node is ChtTerminal terminal && terminal.IsJustRaw && BigInteger.TryParse(RemoveSuffix(terminal.Raw, suffix), out output);
    }

    public override bool ToNode(BigInteger value, ChtSerializer serializer, out ChtNode output)
    {
        output = ChtTerminal.JustRaw(value.ToString() + suffix);
        return true;
    }
}