using Cht.Mappers;
using System.Text;

namespace Cht;

public class ChtSerializer
{
    /// <summary>
    /// This value is appended to the existing line indentation upon each indentation increase.
    /// </summary>
    public string Indentation { get; set; } = "  ";

    /// <summary>
    /// 0 = no single line nesting, null = unlimited nesting of function nodes
    /// </summary>
    public int? MaximumDepth = 2;

    /// <summary>
    /// true = rest of line nodes are used as direct children on blocks, false = rest of line nodes are never used
    /// </summary>
    public bool UseRestOfLineNodes = true;

    public IList<IChtMapper> Mappers { get; set; } = [];

    public ChtNode ToNode<T>(T value)
    {
        foreach (var mapper in Mappers.Reverse())
        {
            try
            {
                if (mapper.ToNode(value, this, out var output))
                {
                    return output;
                }
            }
            catch (Exception ex)
            {
                throw new ChtMappingException(mapper, ex);
            }
        }
        throw new ChtMappingException(null, $"No mapper able to handle value of type {typeof(T).Name}.");
    }
    public T FromNode<T>(ChtNode node)
    {
        foreach (var mapper in Mappers.Reverse())
        {
            try
            {
                if (mapper.FromNode(node, this, out var output) && (output is T || output is null))
                {
                    return (T)output!;
                }
            }
            catch (Exception ex)
            {
                throw new ChtMappingException(mapper, ex);
            }
        }
        throw new ChtMappingException(null, $"No mapper able to handle node {(node is ChtTerminal terminal ? terminal.ToString() : node is ChtNonterminal nonterminal ? nonterminal.Type + "(...)" : '?')}");
    }

    public string Serialize<T>(T value)
    {
        return Emit(ToNode(value));
    }

    public T Deserialize<T>(string value)
    {
        return FromNode<T>(Parse(value));
    }

    public ChtNode Parse(string source)
        => ChtParser.Parse(source);

    public string Emit(ChtNode node)
    {
        var depths = new Dictionary<ChtNode, int>();
        int calcDepth(ChtNode node)
        {
            var depth = 0;
            if (node is ChtNonterminal nonterminal)
            {
                foreach (var child in nonterminal.Children)
                {
                    depth = Math.Max(depth, 1 + calcDepth(child));
                }
            }
            depths[node] = depth;
            return depth;
        }
        calcDepth(node);
        var builder = new StringBuilder();

        void Append(ChtNode node, int indentationLevel, int lineNestingLevel)
        {
            switch (node)
            {
                case ChtNonterminal nonterminal:
                    if (!nonterminal.Children.Any())
                    {
                        builder.Append(nonterminal.Type).Append("()");
                        break;
                    }
                    else
                    {
                        var depth = depths[node];
                        if (MaximumDepth < depth) {
                            builder.Append(nonterminal.Type).Append(":");
                            foreach (var child in nonterminal.Children)
                            {
                                builder.AppendLine();
                                for (int i = 0; i <= indentationLevel; i++)
                                {
                                    builder.Append(Indentation);
                                }
                                Append(child, indentationLevel + 1, 0);
                            }
                        }
                        else if (lineNestingLevel == 0 && UseRestOfLineNodes)
                        {
                            builder.Append(nonterminal.Type).Append(":");
                            foreach (var child in nonterminal.Children)
                            {
                                builder.Append(" ");
                                Append(child, indentationLevel, lineNestingLevel + 1);
                            }
                        }
                        else
                        {
                            builder.Append(nonterminal.Type).Append("(");
                            Append(nonterminal.Children[0], indentationLevel, lineNestingLevel+1);
                            foreach (var child in nonterminal.Children.Skip(1))
                            {
                                builder.Append(" ");
                                Append(child, indentationLevel, lineNestingLevel + 1);
                            }
                            builder.Append(")");
                        }
                        
                    }
                        break;
                case ChtTerminal terminal:
                    builder.Append(terminal.ToString());
                    break;
                default:
                    throw new ArgumentException($"Unknown node type {node.GetType().Name}.");
            }
        }
        Append(node, 0, 0);
        return builder.ToString();
    }
}