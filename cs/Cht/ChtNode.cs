using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Cht;

/// <summary>
/// CHT node.
/// </summary>
public class ChtNode
{
    private string? _raw;

    /// <summary>
    /// A raw part of the value of this node.
    /// </summary>
    public string? Raw
    {
        get => _raw;
        set => _raw = value is null || Regex.IsMatch(value, @"^((\d:\d)|[^\s"":\(\)])+$") ? value
            : throw new ChtException($"Invalid raw value: {value}");
    }

    /// <summary>
    /// A quoted part of the value of this node.
    /// </summary>
    public string? Quoted { get; set; }

    /// <summary>
    /// Children nodes of the node.
    /// </summary>
    public List<ChtNode>? Children { get; set; }

    /// <summary>
    /// Whether this node is just raw. That is the raw part is not null and the quoted part is null and children are null.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Raw))]
    public bool IsJustRaw => Raw is not null && Quoted is null && Children is null;

    /// <summary>
    /// Whether this node is just quoted. That is the quoted part is not null and the raw part is null and children are null.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Quoted))]
    public bool IsJustQuoted => Raw is null && Quoted is not null && Children is null;

    /// <summary>
    /// Whether this node is just raw with children. That is the raw part and children are not null and the quoted part is null.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Raw))]
    [MemberNotNullWhen(true, nameof(Children))]
    public bool IsRawWithChildren => Raw is not null && Quoted is null && Children is not null;

    public ChtNode(string? raw, string? quoted, IEnumerable<ChtNode>? children = null)
    {
        Raw = raw;
        Quoted = quoted;
        Children = children?.ToList();
    }

    internal string ValueToString()
    {
        var result = "";
        if (Raw is not null)
        {
            result += Raw;
        }
        if (Quoted is not null)
        {
            result += "\"";
            result += Quoted.Replace(@"\", @"\\").Replace("\"", "\\\"");
            result += "\"";
        }
        return result;
    }

    public override string ToString()
        => ValueToString() + (Children is null ? "" : Children.Any() ? "(...)" : "()");
}

