using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Cht;

/// <summary>
/// Base CHT node.
/// </summary>
public abstract class ChtNode
{

}

/// <summary>
/// A nonterminal node in a CHT tree.
/// </summary>
public class ChtNonterminal : ChtNode
{
    private string _type = "";

    /// <summary>
    /// Type of the nonterminal.
    /// </summary>
    public string Type {
        get => _type;
        set => _type = Regex.IsMatch(value, @"^[A-Z][^\s"":\(\)]*$") ? value : throw new ChtException($"Invalid nonterminal type: {value}");
    }

    /// <summary>
    /// Children nodes of the nonterminal.
    /// </summary>
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

/// <summary>
/// A terminal node in a CHT tree.
/// </summary>
public class ChtTerminal : ChtNode
{
    private string? _raw;

    /// <summary>
    /// A raw part of the value of this terminal node.
    /// </summary>
    public string? Raw
    {
        get => _raw;
        set => _raw = value is null || Regex.IsMatch(value, @"^[^A-Z]((\d:\d)|[^\s"":\(\)])*$") ? value
            : throw new ChtException($"Invalid raw terminal value: {value}");
    }

    /// <summary>
    /// A quoted part of the value of this terminal node.
    /// </summary>
    public string? Quoted { get; set; }

    /// <summary>
    /// Whether this terminal node is just raw. That is the raw part is not null and the quoted part is null.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Raw))]
    public bool IsJustRaw => Raw is not null && Quoted is null;

    /// <summary>
    /// Whether this terminal node is just quoted. That is the quoted part is not null and the raw part is null.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Quoted))]
    public bool IsJustQuoted => Raw is null && Quoted is not null;

    /// <summary>
    /// A convenience method to create a terminal node with just a quoted part.
    /// </summary>
    public static ChtTerminal JustQuoted(string value) => new ChtTerminal { Quoted = value };

    /// <summary>
    /// A convenience method to create a terminal node with just a raw part.
    /// </summary>
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
