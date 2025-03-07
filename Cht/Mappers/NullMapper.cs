namespace Cht.Mappers;

public class NullMapper : IChtMapper
{
    public bool FromNode(ChtNode node, ChtSerializer serializer, out object? output)
    {
        output = default;
        if (node is ChtTerminal terminal && terminal.IsJustRaw && terminal.Raw == "null")
        {
            output = null;
            return true;
        }
        return false;
    }

    public bool ToNode(object? value, ChtSerializer serializer, out ChtNode output)
    {
        output = default;
        if (value is null)
        {
            output = ChtTerminal.JustRaw("null");
            return true;
        }
        return false;
    }
}