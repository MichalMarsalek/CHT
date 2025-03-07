﻿using System.Text.Json;

namespace Cht;

internal static class ChtParser
{
    private const string _specialChars = "():\"";

    public static ChtNode Parse(string source)
        => Parse(NestLines(GetLines(source)));

    private static IEnumerable<Line> GetLines(string source)
    {
        var lines = source.Split(["\n", "\r\n", "\r"], StringSplitOptions.None)
            .Select((x, i) =>
            {
                var indentationEnd = 0;
                while (indentationEnd < x.Length && char.IsWhiteSpace(x[indentationEnd]))
                {
                    indentationEnd++;
                }
                return new Line { Indentation = x[..indentationEnd], Content = x[indentationEnd..] + '\n', LineIndex = i };
            })
            .Where(x => x.Content != "").ToList();

        if (!lines.Any())
        {
            throw new ChtParsingException(1, 1, "Empty source.");
        }

        if (lines.First().Indentation != "")
        {
            throw new ChtParsingException(1, 1, "First line must not be indented.");
        }

        return lines;
    }

    private static ChtNode Parse(Line line)
    {
        var pointer = 0;
        var (result, isRestOfLineNode) = ReadInlineNode();
        if (result is null) Throw("Expected node");

        if (result is ChtNonterminal nonterminal && isRestOfLineNode && !nonterminal.Children.Any())
        {
            if (!line.Children.Any())
                Throw("Unexpected empty rest of line nonterminal. Empty nonterminals must use the `Type()` syntax.");
            nonterminal.Children = line.Children.Select(Parse).ToList();
        }
        else if (line.Children.Any()) Throw($"Unexpected indentation on the following line.");

        if (PeekChar() != '\n') Throw("Expected end of line.");
        return result!;

        char PeekChar() => line.Content[pointer];

        void Throw(string message)
            => throw new ChtParsingException(line.LineIndex + 1, line.Indentation.Length + pointer + 1, message);

        (ChtNode?, bool) ReadInlineNode()
        {
            while (PeekChar() != '\n' && char.IsWhiteSpace(PeekChar())) pointer++;
            if (PeekChar() == '\n') return (null, false);

            var nodeStart = pointer;
            if (char.IsUpper(line.Content[pointer]))
            {
                pointer++;
                while (!char.IsWhiteSpace(PeekChar()) && !_specialChars.Contains(PeekChar())) pointer++;
                var type = line.Content[nodeStart..pointer];

                if (PeekChar() == ':')
                {
                    pointer++;
                    return (new ChtNonterminal(type, ReadInlineNodes()), true);
                }

                if (PeekChar() == '(')
                {
                    pointer++;
                    var resultNonterminal = new ChtNonterminal(type, ReadInlineNodes());
                    if (PeekChar() == ')')
                    {
                        pointer++;
                        return (resultNonterminal, false);
                    }
                    Throw("Expected node or ')'");
                }

                Throw("Expected ':' or '('.");
            }

            while (
              !char.IsWhiteSpace(PeekChar()) &&
              (!_specialChars.Contains(PeekChar()) ||
                (PeekChar() == ':' &&
                  pointer > 0 &&
                  char.IsDigit(line.Content[pointer - 1]) &&
                  char.IsDigit(line.Content[pointer + 1])))
            ) pointer++;
            var rawEnd = pointer;

            if (PeekChar() == '"')
            {
                pointer++;
                while (true)
                {
                    if (PeekChar() == '"')
                    {
                        pointer++;
                        break;
                    }
                    if (PeekChar() == '\\') pointer++;
                    if (PeekChar() == '\n') Throw("Unexpected end of line. Quoted terminal node is not terminated.");
                    pointer++;
                }
            }

            var resultTerminal = new ChtTerminal
            {
                Raw = rawEnd > nodeStart ? line.Content[nodeStart..rawEnd] : null,
                Quoted = pointer > rawEnd ? JsonSerializer.Deserialize<string>(line.Content[rawEnd..pointer]) : null
            };
            return (resultTerminal.Raw is null && resultTerminal.Quoted is null ? null : resultTerminal, false);
        }

        IEnumerable<ChtNode> ReadInlineNodes()
        {
            var (node, _) = ReadInlineNode();
            while (node is not null)
            {
                yield return node;
                (node, _) = ReadInlineNode();
            }
        }
    }

    private static Line NestLines(IEnumerable<Line> lines)
    {
        // Last possible sibling or parent
        Line last = lines.First();

        foreach (var line in lines.Skip(1))
        {
            while (line.Indentation.Length < last.Indentation.Length)
            {
                last = last.Parent!;
            }

            // Sibling
            if (line.Indentation == last.Indentation)
            {
                if (last.Parent is null)
                {
                    throw new ChtParsingException(line.LineIndex + 1, 0, $"There may only be a single root node.");
                }
                line.Parent = last.Parent;
                last.Parent.Children.Add(line);
                last = line;
            }
            // Child
            else if (line.Indentation.StartsWith(last.Indentation))
            {
                line.Parent = last;
                last.Children.Add(line);
                last = line;
            }
            else
            {
                throw new ChtParsingException(line.LineIndex + 1, 0, $"Invalid indentation.");
            }
        }

        return lines.First();
    }

    private class Line
    {
        public required string Indentation { get; set; }
        public required string Content { get; set; }
        public required int LineIndex { get; set; }

        public Line? Parent { get; set; }
        public List<Line> Children { get; set; } = [];
    }
}