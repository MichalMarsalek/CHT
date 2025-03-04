using Cht;
using Cht.Mappers;

public class ChtMappersTests
{
    [Test]
    public async Task ObjectMapperToNode_RespectsAttributes()
    {
        var data = new TestBinaryNode
        {
            Left = new TestNullaryNode { Value = "Left", Meta = "meta 1" },
            Right = new TestBinaryNode
            {
                Left = new TestNullaryNode { Value = "RightLeft", Meta = "meta 2" },
                Right = new TestNullaryNode { Value = "RightRight", Meta = "meta 3", Tags = ["A", "C", "B"] },
                Meta = "meta 4"
            },
            Meta = "meta 5"
        };

        var serializer = new ChtSerializer().AddObjectMapper([]).AddIEnumerableMapper().AddStringMapper();
        var output = serializer.ToNode(data);
        var expectedOutput = """
        Binary:
          Nullary: "Left"
          Binary: Nullary("RightLeft") Nullary("RightRight" "A" "C" "B")
        """;

        await Assert.That(serializer.Emit(output)).IsEqualTo(expectedOutput);
    }

    [Test]
    public async Task BoolMapper_DefinesInverseFunctions()
    {
        var mapper = new BoolMapper();
        await Mapper_DefinesInverseFunctions(mapper, true, new ChtTerminal { Raw = "true" });
        await Mapper_DefinesInverseFunctions(mapper, false, new ChtTerminal { Raw = "false" });
    }

    [Test]
    public async Task DateOnlyMapper_DefinesInverseFunctions()
    {
        var mapper = new DateOnlyMapper();
        await Mapper_DefinesInverseFunctions(mapper, new DateOnly(2000, 1, 1), new ChtTerminal { Raw = "2000-01-01" });
        await Mapper_DefinesInverseFunctions(mapper, new DateOnly(2025, 11, 15), new ChtTerminal { Raw = "2025-11-15" });
    }

    [Test]
    public async Task IDictionaryMapper_DefinesInverseFunctions()
    {
        var mapper = new IDictionaryMapper();
        await Mapper_DefinesInverseFunctions(mapper, new Dictionary<int, int> { [5] = 2, [3] = 4 }, new ChtNonterminal
        {
            Type = "Dictionary",
            Children = [
                new ChtNonterminal { Type = "KeyValue", Children = [new ChtTerminal { Raw = "5" }, new ChtTerminal { Raw = "2" }] },
                new ChtNonterminal { Type = "KeyValue", Children = [new ChtTerminal { Raw = "3" }, new ChtTerminal { Raw = "4" }] }
            ]
        },
        new ChtSerializer().AddIntMapper());
    }

    [Test]
    public async Task IEnumerableMapper_DefinesInverseFunctions()
    {
        var mapper = new IEnumerableMapper();
        await Mapper_DefinesInverseFunctions(mapper, new List<int> { 2, 4 }, new ChtNonterminal
        {
            Type = "List",
            Children = [ new ChtTerminal { Raw = "2"}, new ChtTerminal { Raw = "4" }]
        },
        new ChtSerializer().AddIntMapper());
    }

    [Test]
    public async Task IntMapper_DefinesInverseFunctions()
    {
        var mapper = new IntMapper();
        await Mapper_DefinesInverseFunctions(mapper, 4, new ChtTerminal { Raw = "4" });
        await Mapper_DefinesInverseFunctions(mapper, -58, new ChtTerminal { Raw = "-58" });
    }

    [Test]
    public async Task NullMapper_DefinesInverseFunctions()
    {
        var mapper = new NullMapper();
        await Mapper_DefinesInverseFunctions(mapper, null, new ChtTerminal { Raw = "null" });
    }

    [Test]
    public async Task StringMapper_DefinesInverseFunctions()
    {
        var mapper = new StringMapper();
        await Mapper_DefinesInverseFunctions(mapper, "abc", new ChtTerminal { Quoted = "abc" });
        await Mapper_DefinesInverseFunctions(mapper, "X Y Z", new ChtTerminal { Quoted = "X Y Z" });
    }

    [Test]
    public async Task TimeOnlyMapper_DefinesInverseFunctions()
    {
        var mapper = new TimeOnlyMapper();
        await Mapper_DefinesInverseFunctions(mapper, new TimeOnly(9, 30, 0), new ChtTerminal { Raw = "09:30:00" });
        await Mapper_DefinesInverseFunctions(mapper, new TimeOnly(18, 11, 15), new ChtTerminal { Raw = "18:11:15" });
    }

    [Test]
    public async Task GuidMapper_DefinesInverseFunctions()
    {
        var mapper = new GuidMapper();
        var guid = Guid.NewGuid();
        await Mapper_DefinesInverseFunctions(mapper, guid, new ChtTerminal { Raw = guid.ToString() });
    }

    [Test]
    public async Task EnumMapper_DefinesInverseFunctions()
    {
        var mapper = new EnumMapper([typeof(TestEnum)]);
        await Mapper_DefinesInverseFunctions(mapper, TestEnum.Variant1, new ChtNonterminal
        {
            Type = "Enum",
            Children = [
                new ChtTerminal { Raw = "variant1" }
            ]
        });
    }

    private async Task Mapper_DefinesInverseFunctions(IChtMapper mapper, object? value, ChtNode node, ChtSerializer? serializer = null)
    {
        serializer ??= new ChtSerializer();
        serializer.AddMapper(mapper);
        await Assert.That(serializer.ToNode(value)).IsEquivalentTo(node);
        await Assert.That(serializer.FromNode<object>(node)).IsEquivalentTo(value);
    }

    public abstract class TestNode { }

    [ChtType("Binary")]
    public class TestBinaryNode : TestNode
    {
        public TestNode Left { get; set; }
        public TestNode Right { get; set; }

        [ChtIgnore]
        public string Meta { get; set; }
    }

    [ChtType("Nullary")]
    public class TestNullaryNode : TestNode
    {
        public string Value { get; set; } = "";

        [ChtFlatten]
        public IEnumerable<string> Tags { get; set; } = [];

        [ChtIgnore]
        public string Meta { get; set; }
    }

    [ChtType("Enum")]
    public enum TestEnum
    {
        Variant1,
        Variant2
    }

}