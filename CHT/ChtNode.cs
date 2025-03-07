using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Cht;

public abstract class ChtNode
{

}

public class ChtNonterminal : ChtNode
{
    private string _type = "";
    public string Type {
        get => _type;
        set => _type = Regex.IsMatch(value, @"^[A-Z][^\s"":\(\)]*$") ? value : throw new ChtException($"Invalid nonterminal type: {value}");
    }
    public List<ChtNode> Children { get; set; } = [];

    public ChtNonterminal(string type, params ChtNode[] children)
    {
        Type = type;
        Children = children.ToList();
    }

    public ChtNonterminal(string type,IEnumerable<ChtNode> children)
    {
        Type = type;
        Children = children.ToList();
    }
}

public class ChtTerminal : ChtNode
{
    private string? _raw;
    public virtual string? Raw
    {
        get => _raw;
        set => _raw = value is null || Regex.IsMatch(value, @"^[^A-Z]((\d:\d)|[^\s"":\(\)])*$") ? value
            : throw new ChtException($"Invalid raw terminal value: {value}");
    }

    public virtual string? Quoted { get; set; }

    [MemberNotNullWhen(true, nameof(Raw))]
    public bool IsJustRaw => Raw is not null && Quoted is null;

    [MemberNotNullWhen(true, nameof(Quoted))]
    public bool IsJustQuoted => Raw is null && Quoted is not null;

    public static ChtTerminal JustQuoted(string value) => new ChtTerminal { Quoted = value };
    public static ChtTerminal JustRaw(string value) => new ChtTerminal { Raw = value };

    public override string ToString()
    {
        var result = Raw ?? "";
        if (Quoted is not null)
        {
            result += "\"";
            result += Quoted.Replace(@"\", @"\\").Replace("\"", "\\\"");
            result += "\"";
        }
        return result;
    }
}
