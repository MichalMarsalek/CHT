using Cht;
using Cht.Mappers;
using TUnit.Assertions.AssertConditions.Throws;

public class ChtSerializerTests
{
    [Test]
    [MethodDataSource(nameof(Emit_Emits_Data))]
    public async Task Emit_Emits(ChtSerializer serializer, string expectedOutput)
    {
        var node = new ChtNonterminal(
            "Block",
            new ChtNonterminal(
                "Assignment",
                ChtTerminal.JustRaw("$x"),
                new ChtNonterminal(
                    "List",
                    new ChtNonterminal(
                        "List",
                        ChtTerminal.JustRaw("0"),
                        ChtTerminal.JustRaw("1")
                    ),
                    ChtTerminal.JustRaw("58"),
                    ChtTerminal.JustRaw("15")
                )
            ),
            new ChtNonterminal(
                "Assignment",
                ChtTerminal.JustRaw("$x"),
                new ChtNonterminal(
                    "List",
                    ChtTerminal.JustRaw("0"),
                    ChtTerminal.JustRaw("58"),
                    ChtTerminal.JustRaw("15")
                )
            ),
            new ChtNonterminal(
                "MethodCall",
                ChtTerminal.JustRaw("$x"),
                ChtTerminal.JustRaw("$append"),
                ChtTerminal.JustRaw("7")
            ),
            new ChtNonterminal(
                "Assignment",
                ChtTerminal.JustRaw("$y"),
                new ChtNonterminal(
                    "Indexing",
                    ChtTerminal.JustRaw("$x"),
                    ChtTerminal.JustRaw("3")
                )
            ),
            new ChtNonterminal(
                "FunctionCall",
                ChtTerminal.JustRaw("$print"),
                ChtTerminal.JustRaw("$y")
            ),
            new ChtNonterminal(
                "Assignment",
                ChtTerminal.JustRaw("$text"),
                ChtTerminal.JustQuoted("Some text with spaces")
            )
        );

        var result = serializer.Emit(node);
        await Assert.That(result).IsEqualTo(expectedOutput);
    }

    public static IEnumerable<Func<(ChtSerializer, string)>> Emit_Emits_Data() => [
            () => (new ChtSerializer(),"""
                Block:
                  Assignment($x):
                    List: List(0 1) 58 15
                  Assignment: $x List(0 58 15)
                  MethodCall: $x $append 7
                  Assignment: $y Indexing($x 3)
                  FunctionCall: $print $y
                  Assignment: $text "Some text with spaces"
                """),
            () => (new ChtSerializer { UseCombinedNodes = false },"""
                Block:
                  Assignment:
                    $x
                    List: List(0 1) 58 15
                  Assignment: $x List(0 58 15)
                  MethodCall: $x $append 7
                  Assignment: $y Indexing($x 3)
                  FunctionCall: $print $y
                  Assignment: $text "Some text with spaces"
                """),
            () => (new ChtSerializer { Indentation = "    ", UseRestOfLineNodes = false, MaximumParenthesesDepth = 2 }, """
                Block:
                    Assignment($x):
                        List(List(0 1) 58 15)
                    Assignment($x List(0 58 15))
                    MethodCall($x $append 7)
                    Assignment($y Indexing($x 3))
                    FunctionCall($print $y)
                    Assignment($text "Some text with spaces")
                """),
            () => (new ChtSerializer { Indentation = "    ", UseRestOfLineNodes = false, MaximumParenthesesDepth = 2, UseCombinedNodes = false }, """
                Block:
                    Assignment:
                        $x
                        List(List(0 1) 58 15)
                    Assignment($x List(0 58 15))
                    MethodCall($x $append 7)
                    Assignment($y Indexing($x 3))
                    FunctionCall($print $y)
                    Assignment($text "Some text with spaces")
                """),
            () => (new ChtSerializer { Indentation = "    ", UseRestOfLineNodes = false, MaximumParenthesesDepth = null }, """Block(Assignment($x List(List(0 1) 58 15)) Assignment($x List(0 58 15)) MethodCall($x $append 7) Assignment($y Indexing($x 3)) FunctionCall($print $y) Assignment($text "Some text with spaces"))""")
        ];

    [Test]
    [MethodDataSource(nameof(Parse_WhenValidSource_Parses_Data))]
    public async Task Parse_WhenValidSource_Parses(string source, ChtNode expectedNode)
    {
        var serializer = new ChtSerializer();
        var actualNode = serializer.Parse(source);
        await Assert.That(actualNode).IsEquivalentTo(expectedNode);
    }

    [Test]
    [MethodDataSource(nameof(Parse_WhenInvalidSource_Throws_Data))]
    public async Task Parse_WhenInvalidSource_Throws(string source, ChtParsingException expectedException)
    {
        var serializer = new ChtSerializer();
        ChtParsingException? actualException = null;
        try
        {
            serializer.Parse(source);
        }
        catch (ChtParsingException ex)
        {
            actualException = ex;
        }
        await Assert.That(actualException?.LineNumber).IsEquivalentTo(expectedException.LineNumber);
        await Assert.That(actualException?.ColumnNumber).IsEquivalentTo(expectedException.ColumnNumber);
    }

    public static IEnumerable<Func<(string, ChtNode)>> Parse_WhenValidSource_Parses_Data() => [
        () => ("58", ChtTerminal.JustRaw("58")),
        () => ("$x", ChtTerminal.JustRaw("$x")),
        () => ("12:34:56", ChtTerminal.JustRaw("12:34:56")),
        () => ("\"Some\\n\\\"text\"", ChtTerminal.JustQuoted("Some\n\"text")),
        () => ("A: #0 1 \"2 # \"# 3 #4", new ChtNonterminal("A", ChtTerminal.JustRaw("#0"), ChtTerminal.JustRaw("1"), ChtTerminal.JustQuoted("2 # "))),
        () => ("re\"quoted\"", new ChtTerminal { Raw = "re", Quoted = "quoted"}),
        () => ("A(B(C()))", new ChtNonterminal("A", new ChtNonterminal("B", new ChtNonterminal("C")))),
        () => ("A:B:C()", new ChtNonterminal("A", new ChtNonterminal("B", new ChtNonterminal("C")))),
        () => ("A:\n  B:\n    C()", new ChtNonterminal("A", new ChtNonterminal("B", new ChtNonterminal("C")))),
        () => ("A(0):\n  B:\n    C()", new ChtNonterminal("A", ChtTerminal.JustRaw("0"), new ChtNonterminal("B", new ChtNonterminal("C")))),
        () => ("A(B:C())", new ChtNonterminal("A", new ChtNonterminal("B", new ChtNonterminal("C")))),
        () => ("A(B(0):C())", new ChtNonterminal("A", new ChtNonterminal("B", ChtTerminal.JustRaw("0"), new ChtNonterminal("C")))),
        () => ("A:  59  \t$x    []    ", new ChtNonterminal("A", ChtTerminal.JustRaw("59"), ChtTerminal.JustRaw("$x"), ChtTerminal.JustRaw("[]"))),
    ];

    public static IEnumerable<Func<(string, ChtParsingException)>> Parse_WhenInvalidSource_Throws_Data() => [
        () => (" 58", new ChtParsingException(1, 1, "")),
        () => ("A:", new ChtParsingException(1, 3, "")),
        () => ("A()\n  B()", new ChtParsingException(1, 4, "")),
        () => ("a()", new ChtParsingException(1, 2, "")),
        () => ("A(B:C)", new ChtParsingException(1, 6, "")),
        () => ("A:\n    B:\n  C()", new ChtParsingException(2, 7, "")),
    ];

    [Test]
    public async Task ToNode_OnCycle_Throws()
    {
        var serializer = new ChtSerializer().AddMapper(new TestNodeMapper());
        var node = new TestNode { Parent = new TestNode() };
        node.Parent!.Parent = node;
        await Assert.That(() => serializer.ToNode(node)).Throws<ChtMappingException>().WithMessage("Circular reference detected.");
    }

    private class TestNode
    {
        public TestNode? Parent { get; set; }
    }

    private class TestNodeMapper : ChtMapper<TestNode>
    {
        public override bool ToNode(TestNode value, ChtSerializer serializer, out ChtNode output)
        {
            output = new ChtNonterminal("TestNode", serializer.ToNode(value.Parent));
            return true;
        }

        public override bool FromNode(ChtNode node, ChtSerializer serializer, out TestNode output)
            => throw new NotImplementedException();
    }
}