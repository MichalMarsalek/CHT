using Cht;

public class ChtSerializerTests
{
    [Test]
    [MethodDataSource(nameof(Emit_Emits_Data))]
    public async Task Emit_Emits(ChtSerializer serializer, string expectedOutput)
    {
        var node = new ChtNonterminal
        {
            Type = "Block",
            Children = [
                new ChtNonterminal {
                    Type = "Assignment",
                    Children = [
                        new ChtTerminal { Raw = "$x" },
                        new ChtNonterminal {
                            Type = "List",
                            Children = [
                                new ChtTerminal { Raw = "0" },
                                new ChtTerminal { Raw = "58" },
                                new ChtTerminal { Raw = "15" },
                            ]
                        }
                    ]
                },
                new ChtNonterminal {
                    Type = "MethodCall",
                    Children = [
                        new ChtTerminal { Raw = "$x" },
                        new ChtTerminal { Raw = "$append" },
                        new ChtTerminal { Raw = "7" },
                    ]
                },
                new ChtNonterminal {
                    Type = "Assignment",
                    Children = [
                        new ChtTerminal { Raw = "$y" },
                        new ChtNonterminal {
                            Type = "Indexing",
                            Children = [
                                new ChtTerminal { Raw = "$x" },
                                new ChtTerminal { Raw = "3" },
                            ]
                        }
                    ]
                },
                new ChtNonterminal {
                    Type = "FunctionCall",
                    Children = [
                        new ChtTerminal { Raw = "$print" },
                        new ChtTerminal { Raw = "$y" },
                    ]
                },
                new ChtNonterminal {
                    Type = "Assignment",
                    Children = [
                        new ChtTerminal { Raw = "$text" },
                        new ChtTerminal { Quoted = "Some text with spaces" },
                    ]
                }
            ]
        };

        var result = serializer.Emit(node);
        await Assert.That(result).IsEqualTo(expectedOutput);
    }

    public static IEnumerable<(ChtSerializer, string)> Emit_Emits_Data() => [
            (new ChtSerializer(),"""
        Block:
          Assignment: $x List(0 58 15)
          MethodCall: $x $append 7
          Assignment: $y Indexing($x 3)
          FunctionCall: $print $y
          Assignment: $text "Some text with spaces"
        """),
            (new ChtSerializer { Indentation = "    ", UseRestOfLineNodes = false }, """
        Block:
            Assignment($x List(0 58 15))
            MethodCall($x $append 7)
            Assignment($y Indexing($x 3))
            FunctionCall($print $y)
            Assignment($text "Some text with spaces")
        """),
            (new ChtSerializer { Indentation = "    ", UseRestOfLineNodes = false, MaximumDepth = null }, """Block(Assignment($x List(0 58 15)) MethodCall($x $append 7) Assignment($y Indexing($x 3)) FunctionCall($print $y) Assignment($text "Some text with spaces"))""")
        ];    
}