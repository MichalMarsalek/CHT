﻿using Cht.Mappers;
using System.Text;

namespace Cht;

public class ChtSerializer
{
    private Dictionary<int, object> currentlyMappingObjects = [];

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

    /// <summary>
    /// Mappers used to map between CHT nodes and .NET objects.
    /// Later ones take precedence. That is, mappers are tried in reverse order of addition.
    /// </summary>
    public IList<IChtMapper> Mappers { get; set; } = [];

    /// <summary>
    /// Converts a .NET object to a CHT node.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="value">Value to map.</param>
    /// <returns>The mapped node.</returns>
    public ChtNode ToNode<T>(T value)
    {
        int hashCode = 0;
        if (value is not null) {
            hashCode = value.GetHashCode();
            if (currentlyMappingObjects.TryGetValue(hashCode, out var obj) && Object.ReferenceEquals(obj, value))
            {
                throw new ChtMappingException(null, "Circular reference detected.");
            }
            currentlyMappingObjects[hashCode] = value;
        }
        try
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
        finally
        {
            currentlyMappingObjects.Remove(hashCode);
        }
    }

    /// <summary>
    /// Converts a CHT node to a .NET object.
    /// </summary>
    /// <typeparam name="T">The required type of the output. May be object when there is no constraint.</typeparam>
    /// <param name="node">Node to map.</param>
    /// <returns>The resulting mapped value.</returns>
    public T FromNode<T>(ChtNode node)
        => (T)FromNode(node, typeof(T));

    /// <summary>
    /// Converts a CHT node to a .NET object.
    /// </summary>
    /// <param name="node">Node to map.</param>
    /// <param name="type">The required type of the output. May be object when there is no constraint.</param>
    /// <returns>The resulting mapped value.</returns>
    public object? FromNode(ChtNode node, Type type)
    {
        foreach (var mapper in Mappers.Reverse())
        {
            try
            {
                if (mapper.FromNode(node, type, this, out var output) && (output is null || output.GetType().IsAssignableTo(type)))
                {
                    return output;
                }
            }
            catch (Exception ex)
            {
                throw new ChtMappingException(mapper, ex);
            }
        }
        throw new ChtMappingException(null, $"No mapper able to handle node {(node is ChtTerminal terminal ? terminal.ToString() : node is ChtNonterminal nonterminal ? nonterminal.Type + "(...)" : '?')}");
    }

    /// <summary>
    /// Serializes a .NET object to a CHT document.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="value">Value to serialize.</param>
    /// <returns>The serialized value as a string.</returns>
    public string Serialize<T>(T value)
        => Emit(ToNode(value));

    /// <summary>
    /// Deserializes a CHT document to a .NET object.
    /// </summary>
    /// <typeparam name="T">The required type of the value.</typeparam>
    /// <param name="source">Content of the CHT document to deserialize.</param>
    /// <returns>Tje deserialized value.</returns>
    public T Deserialize<T>(string source)
        => FromNode<T>(Parse(source));

    /// <summary>
    /// Parse a CHT document.
    /// </summary>
    /// <param name="source">Source string representing the tree.</param>
    /// <returns>A parsed CHT as the root node.</returns>
    public ChtNode Parse(string source)
        => ChtParser.Parse(source);

    /// <summary>
    /// Emits a CHT node as a string.
    /// </summary>
    /// <param name="node">Node to emit.</param>
    /// <returns>A CHT document corresponding to the input CHT node.</returns>
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