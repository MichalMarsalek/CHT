using Cht;
using Cht.Mappers;
using System.Xml.Linq;
using TUnit.Assertions.AssertConditions.Operators;

public class ChtMappersTests
{
    [Test]
    public async Task ObjectMapper_DefinesInverseFunctions()
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

        var nodes = new ChtNonterminal(
            "Binary",
            new ChtNonterminal("Nullary", ChtTerminal.JustQuoted("Left")),
            new ChtNonterminal(
                "Binary",
                new ChtNonterminal("Nullary", ChtTerminal.JustQuoted("RightLeft")),
                new ChtNonterminal("Nullary", ChtTerminal.JustQuoted("RightRight"), ChtTerminal.JustQuoted("A"), ChtTerminal.JustQuoted("C"), ChtTerminal.JustQuoted("B"))
            )
        );

        var serializer = new ChtSerializer().AddObjectMapper([typeof(TestBinaryNode), typeof(TestNullaryNode)]).AddIEnumerableMapper().AddStringMapper();

        await Assert.That(serializer.ToNode(data)).IsEquivalentTo(nodes);

        (data as dynamic).Left.Meta = (data as dynamic).Right.Left.Meta = (data as dynamic).Right.Right.Meta = (data as dynamic).Right.Meta = data.Meta = null;
        await Assert.That(serializer.FromNode<object>(nodes)).IsEquivalentTo(data);
    }

    [Test]
    public async Task ObjectMapper_WithGenericsAndInheritance_DefinesInverseFunctions()
    {
        var data1 = new TestGenericObject<string> { Value = "Test" };
        var data2 = new TestInheritedObject { Value = "Test" };
        var node1 = new ChtNonterminal("TestGenericObject", ChtTerminal.JustQuoted("Test"));
        var node2 = new ChtNonterminal("TestInheritedObject", ChtTerminal.JustQuoted("Test"));

        var serializer = new ChtSerializer().AddObjectMapper([typeof(TestBaseObject), typeof(TestGenericObject<>), typeof(TestInheritedObject)]).AddStringMapper();

        await Assert.That(serializer.ToNode(data1)).IsEquivalentTo(node1);
        await Assert.That(serializer.FromNode<object>(node1)).IsEquivalentTo(data1);
        await Assert.That(serializer.FromNode<TestBaseObject>(node1)).IsEquivalentTo(data1);
        await Assert.That(serializer.FromNode<TestGenericObject<string>>(node1)).IsEquivalentTo(data1);


        await Assert.That(serializer.ToNode(data2)).IsEquivalentTo(node2);
        await Assert.That(serializer.FromNode<object>(node2)).IsEquivalentTo(data2);
        await Assert.That(serializer.FromNode<TestBaseObject>(node2)).IsEquivalentTo(data2);
        await Assert.That(serializer.FromNode<TestGenericObject<string>>(node2)).IsEquivalentTo(data2);
        await Assert.That(serializer.FromNode<TestInheritedObject>(node2)).IsEquivalentTo(data2);
    }

    [Test]
    public async Task IDictionaryMapper_WhenInsideObject_DefinesInverseFunctions()
    {
        var serializer = new ChtSerializer().AddObjectMapper([typeof(TestObjectWithDictionary)]).AddIDictionaryMapper().AddStringMapper();
        
        var data = new TestObjectWithDictionary
        {
            Name = "Test",
            Values = new Dictionary<string, string> { ["A"] = "1", ["B"] = "2" }
        };
        var nodes = new ChtNonterminal(
            "TestObjectWithDictionary",
            ChtTerminal.JustQuoted("Test"),
            new ChtNonterminal("KeyValue", ChtTerminal.JustQuoted("A"), ChtTerminal.JustQuoted("1")),
            new ChtNonterminal("KeyValue", ChtTerminal.JustQuoted("B"), ChtTerminal.JustQuoted("2"))
        );

        await Assert.That(serializer.ToNode(data)).IsEquivalentTo(nodes);
        await Assert.That(serializer.FromNode<object>(nodes)).IsEquivalentTo(data);
    }

    [Test]
    public async Task BoolMapper_DefinesInverseFunctions()
    {
        var mapper = new BoolMapper();
        await Mapper_DefinesInverseFunctions(mapper, true, ChtTerminal.JustRaw("true"));
        await Mapper_DefinesInverseFunctions(mapper, false, ChtTerminal.JustRaw("false"));
    }

    [Test]
    public async Task DateOnlyMapper_DefinesInverseFunctions()
    {
        var mapper = new DateOnlyMapper();
        await Mapper_DefinesInverseFunctions(mapper, new DateOnly(2000, 1, 1), ChtTerminal.JustRaw("2000-01-01"));
        await Mapper_DefinesInverseFunctions(mapper, new DateOnly(2025, 11, 15), ChtTerminal.JustRaw("2025-11-15"));
    }

    [Test]
    public async Task IDictionaryMapper_DefinesInverseFunctions()
    {
        var mapper = new IDictionaryMapper();
        await Mapper_DefinesInverseFunctions(mapper, new Dictionary<int, int> { [5] = 2, [3] = 4 }, new ChtNonterminal(
            "Dictionary",
            new ChtNonterminal("KeyValue", ChtTerminal.JustRaw("5"), ChtTerminal.JustRaw("2")),
            new ChtNonterminal("KeyValue", ChtTerminal.JustRaw("3"), ChtTerminal.JustRaw("4"))
        ),
        new ChtSerializer().AddIntMapper());
    }

    [Test]
    public async Task IEnumerableMapper_DefinesInverseFunctions()
    {
        var mapper = new IEnumerableMapper();
        await Mapper_DefinesInverseFunctions(mapper, new List<int> { 2, 4 }, new ChtNonterminal(
            "List",
            new ChtTerminal { Raw = "2"}, ChtTerminal.JustRaw("4")
        ),
        new ChtSerializer().AddIntMapper());
    }

    [Test]
    public async Task IntMapper_DefinesInverseFunctions()
    {
        var mapper = new IntMapper();
        await Mapper_DefinesInverseFunctions(mapper, 4, ChtTerminal.JustRaw("4"));
        await Mapper_DefinesInverseFunctions(mapper, -58, ChtTerminal.JustRaw("-58"));
    }

    [Test]
    public async Task NullMapper_DefinesInverseFunctions()
    {
        var mapper = new NullMapper();
        await Mapper_DefinesInverseFunctions(mapper, null, ChtTerminal.JustRaw("null"));
    }

    [Test]
    public async Task StringMapper_DefinesInverseFunctions()
    {
        var mapper = new StringMapper();
        await Mapper_DefinesInverseFunctions(mapper, "abc", ChtTerminal.JustQuoted("abc"));
        await Mapper_DefinesInverseFunctions(mapper, "X Y Z", ChtTerminal.JustQuoted("X Y Z"));
    }

    [Test]
    public async Task TimeOnlyMapper_DefinesInverseFunctions()
    {
        var mapper = new TimeOnlyMapper();
        await Mapper_DefinesInverseFunctions(mapper, new TimeOnly(9, 30, 0), ChtTerminal.JustRaw("09:30:00"));
        await Mapper_DefinesInverseFunctions(mapper, new TimeOnly(18, 11, 15), ChtTerminal.JustRaw("18:11:15"));
    }

    [Test]
    public async Task GuidMapper_DefinesInverseFunctions()
    {
        var mapper = new GuidMapper();
        var guid = Guid.NewGuid();
        await Mapper_DefinesInverseFunctions(mapper, guid, ChtTerminal.JustRaw(guid.ToString()));
    }

    [Test]
    public async Task EnumMapper_DefinesInverseFunctions()
    {
        var mapper = new EnumMapper([typeof(TestEnum)]);
        await Mapper_DefinesInverseFunctions(mapper, TestEnum.Variant1, new ChtNonterminal("Enum", ChtTerminal.JustRaw("variant1")));
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

    public class TestObjectWithDictionary
    {
        public string Name { get; set; } = "";

        [ChtFlatten]
        public Dictionary<string, string> Values { get; set; } = new();
    }

    public class TestBaseObject
    {
    }

    public class TestGenericObject<T> : TestBaseObject
    {
        public T Value { get; set; }
    }

    public class TestInheritedObject : TestGenericObject<string>
    {
    }
}