using Cht;
using Cht.Mappers;
using TUnit.Assertions.AssertConditions.Throws;

public class ChtSerializerTests
{
    [Test]
    [MethodDataSource(nameof(Emit_Emits_Data))]
    public async Task Emit_Emits(ChtSerializer serializer, string expectedOutput)
    {
        var node = Raw(
            "Block",
            Raw(
                "Assignment",
                new ChtNode("$x", null),
                Raw(
                    "List",
                    Raw(
                        "List",
                        Raw(0),
                        Raw(1)
                    ),
                    Raw(58),
                    Raw(15)
                )
            ),
            Raw(
                "Assignment",
                Raw("$x"),
                Raw(
                    "List",
                    Raw(0),
                    Raw(58),
                    Raw(15)
                )
            ),
            Raw(
                "MethodCall",
                Raw("$x"),
                Raw("$append"),
                Raw(7)
            ),
            Raw(
                "Assignment",
                Raw("$y"),
                Raw(
                    "Indexing",
                    Raw("$x"),
                    Raw(3)
                )
            ),
            Raw(
                "FunctionCall",
                Raw("$print"),
                Raw("$y")
            ),
            Raw(
                "Assignment",
                Raw("$text"),
                Quoted("Some text with spaces")
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
        () => ("58", Raw(58)),
        () => ("$x", Raw("$x")),
        () => ("12:34:56", Raw("12:34:56")),
        () => ("\"Some\\n\\\"text\"", Quoted("Some\n\"text")),
        () => ("A: #0 1 \"2 # \"# 3 #4", Raw("A", Raw("#0"), Raw(1), Quoted("2 # "))),
        () => ("re\"quoted\"", new ChtNode("re", "quoted")),
        () => ("A(B(C()))", Raw("A", Raw("B", Raw("C", [])))),
        () => ("A:B:C()", Raw("A", Raw("B", Raw("C", [])))),
        () => ("A:\n  B:\n    C()", Raw("A", Raw("B", Raw("C", [])))),
        () => ("A(0):\n  B:\n    C()", Raw("A", Raw(0), Raw("B", Raw("C", [])))),
        () => ("A(B:C())", Raw("A", Raw("B", Raw("C", [])))),
        () => ("A(B(0):C())", Raw("A", Raw("B", Raw(0), Raw("C", [])))),
        () => ("A:  59  \t$x    []    ", Raw("A", Raw(59), Raw("$x"), Raw("[]"))),
        () => ("a()", Raw("a", [])),
        () => ("a\"quoted\"()", new ChtNode("a", "quoted", [])),
        () => ("A(B:C)", Raw("A", Raw("B", Raw("C")))),
        () => ("A(0)(1):2 3", Raw("A", Raw(0), Raw(1), Raw(2), Raw(3))),
        () => ("A(0)(1):\n 2\n 3", Raw("A", Raw(0), Raw(1), Raw(2), Raw(3))),
    ];

    public static IEnumerable<Func<(string, ChtParsingException)>> Parse_WhenInvalidSource_Throws_Data() => [
        () => (" 58", new ChtParsingException(1, 1, "")),
        () => ("A:", new ChtParsingException(1, 3, "")),
        () => ("A()\n  B()", new ChtParsingException(1, 4, "")),
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

    private static ChtNode Raw(object? raw) => new ChtNode(raw?.ToString(), null, null);
    private static ChtNode Raw(object? raw, params ChtNode[] children) => new ChtNode(raw?.ToString(), null, children);
    private static ChtNode Quoted(string? quoted) => new ChtNode(null, quoted, null);

    private class TestNode
    {
        public TestNode? Parent { get; set; }
    }

    private class TestNodeMapper : ChtMapper<TestNode>
    {
        public override bool ToNode(TestNode value, ChtSerializer serializer, out ChtNode output)
        {
            output = Raw("TestNode", serializer.ToNode(value.Parent));
            return true;
        }

        public override bool FromNode(ChtNode node, ChtSerializer serializer, out TestNode output)
            => throw new NotImplementedException();
    }
}