namespace CHT
{
    public abstract class ChtNode
    {

    }

    public class ChtNonterminal : ChtNode
    {
        public required string Type { get; set; }
        public List<ChtNode> Children { get; set; } = [];
    }

    public abstract class ChtTerminal : ChtNode
    {
        public required string Value { get; set; }
    }

    public class ChtRawTerminal : ChtTerminal
    {

    }
    public class ChtQuotedTerminal : ChtTerminal
    {

    }
}
