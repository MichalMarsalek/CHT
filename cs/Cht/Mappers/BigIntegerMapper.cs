using System.Numerics;

namespace Cht.Mappers;

public class BigIntegerMapper(string suffix = "") : ChtMapper<BigInteger>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out BigInteger output)
    {
        output = default;
        return node.IsJustRaw && BigInteger.TryParse(RemoveSuffix(node.Raw, suffix), out output);
    }

    public override bool ToNode(BigInteger value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtNode(value.ToString() + suffix, null);
        return true;
    }
}