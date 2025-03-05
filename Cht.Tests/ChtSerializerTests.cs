using Cht;

public class ChtSerializerTests
{
    [Test]
    public async Task Emit_Emits()
    {
        var node = new ChtNonterminal
        {
            Type = "Block",
            Children = [
                new ChtNonterminal {
                    Type = "Assignment",
                    Children = [
                        new ChtRawTerminal { Value = "$x" },
                        new ChtNonterminal {
                            Type = "List",
                            Children = [
                                new ChtRawTerminal { Value = "0" },
                                new ChtRawTerminal { Value = "58" },
                                new ChtRawTerminal { Value = "15" },
                            ]
                        }
                    ]
                },
                new ChtNonterminal {
                    Type = "MethodCall",
                    Children = [
                        new ChtRawTerminal { Value = "$x" },
                        new ChtRawTerminal { Value = "$append" },
                        new ChtRawTerminal { Value = "7" },
                    ]
                },
                new ChtNonterminal {
                    Type = "Assignment",
                    Children = [
                        new ChtRawTerminal { Value = "$y" },
                        new ChtNonterminal {
                            Type = "Indexing",
                            Children = [
                                new ChtRawTerminal { Value = "$x" },
                                new ChtRawTerminal { Value = "3" },
                            ]
                        }
                    ]
                },
                new ChtNonterminal {
                    Type = "FunctionCall",
                    Children = [
                        new ChtRawTerminal { Value = "$print" },
                        new ChtRawTerminal { Value = "$y" },
                    ]
                },
                new ChtNonterminal {
                    Type = "Assignment",
                    Children = [
                        new ChtRawTerminal { Value = "$text" },
                        new ChtQuotedTerminal { Value = "Some text with spaces" },
                    ]
                }
            ]
        };

        var serializer = new ChtSerializer();
        var expectedOutput = """
        Block:
          Assignment: $x List(0 58 15)
          MethodCall: $x $append 7
          Assignment: $y Indexing($x 3)
          FunctionCall: $print $y
          Assignment: $text "Some text with spaces"
        """;


        var result = serializer.Emit(node);

        await Assert.That(result).IsEqualTo(expectedOutput);
    } 
}