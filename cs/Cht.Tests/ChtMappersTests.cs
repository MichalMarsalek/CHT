using Cht;
using Cht.Mappers;
using System.Drawing;
using System.Numerics;

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

        serializer = new ChtSerializer().AddObjectMapper([typeof(TestObjectWithList)], autoFlatten: true).AddIEnumerableMapper().AddStringMapper();
        var data2 = new TestObjectWithList { Name = "Test", Values = new List<string> { "A", "B", "C" } };
        var nodes2 = new ChtNonterminal("TestObjectWithList", ChtTerminal.JustQuoted("Test"), ChtTerminal.JustQuoted("A"), ChtTerminal.JustQuoted("B"), ChtTerminal.JustQuoted("C"));

        await Assert.That(serializer.ToNode(data2)).IsEquivalentTo(nodes2);
        await Assert.That(serializer.FromNode<object>(nodes2)).IsEquivalentTo(data2);
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

        await Mapper_DefinesInverseFunctions(serializer, data, nodes);
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
        await Mapper_DefinesInverseFunctions(mapper, new DateOnly(2000, 1, 1), ChtTerminal.JustRaw("2000-01-01"), typeof(DateOnly));
        await Mapper_DefinesInverseFunctions(mapper, new DateOnly(2000, 1, 1), ChtTerminal.JustRaw("2000-01-01"), typeof(DateOnly?));
        await Mapper_DefinesInverseFunctions(mapper, new DateOnly(2025, 11, 15), ChtTerminal.JustRaw("2025-11-15"));
    }

    [Test]
    public async Task IDictionaryMapper_DefinesInverseFunctions()
    {
        var serializer = new ChtSerializer().AddIntMapper().AddIDictionaryMapper();
        await Mapper_DefinesInverseFunctions(serializer, new Dictionary<int, int> { [5] = 2, [3] = 4 }, new ChtNonterminal(
            "Dictionary",
            new ChtNonterminal("KeyValue", ChtTerminal.JustRaw("5"), ChtTerminal.JustRaw("2")),
            new ChtNonterminal("KeyValue", ChtTerminal.JustRaw("3"), ChtTerminal.JustRaw("4"))
        ));
    }

    [Test]
    public async Task IEnumerableMapper_DefinesInverseFunctions()
    {
        var serializer = new ChtSerializer().AddIntMapper().AddIEnumerableMapper();
        await Mapper_DefinesInverseFunctions(serializer, new List<int> { 2, 4 }, new ChtNonterminal(
            "List",
            new ChtTerminal { Raw = "2" }, ChtTerminal.JustRaw("4")
        ));
    }

    [Test]
    public async Task IntMapper_DefinesInverseFunctions()
    {
        var mapper = new IntMapper();
        await Mapper_DefinesInverseFunctions(mapper, 4, ChtTerminal.JustRaw("4"));
        await Mapper_DefinesInverseFunctions(mapper, -58, ChtTerminal.JustRaw("-58"));

        await Assert.That(new ChtSerializer().AddMapper(mapper).Deserialize<object>("0xff")).IsEqualTo(0xff);
    }

    [Test]
    public async Task NumberMappers_WithSuffixes_DefineInverseFunctions()
    {
        var serializer = new ChtSerializer().AddCommonMappers([], useNumberSuffixes: true);
        await Mapper_DefinesInverseFunctions(serializer, 4f, ChtTerminal.JustRaw("4.0f"));
        await Mapper_DefinesInverseFunctions(serializer, 4d, ChtTerminal.JustRaw("4.0d"));
        await Mapper_DefinesInverseFunctions(serializer, 4m, ChtTerminal.JustRaw("4.0m"));
        await Mapper_DefinesInverseFunctions(serializer, 4, ChtTerminal.JustRaw("4"));
        await Mapper_DefinesInverseFunctions(serializer, 4L, ChtTerminal.JustRaw("4L"));
        await Mapper_DefinesInverseFunctions(serializer, new BigInteger(4), ChtTerminal.JustRaw("4n"));
    }

    [Test]
    public async Task NumberMappers_WithTargets_DefineInverseFunctions()
    {
        var serializer = new ChtSerializer().AddCommonMappers([]);
        await Mapper_DefinesInverseFunctions(serializer, 4f, ChtTerminal.JustRaw("4.0"), typeof(float));
        await Mapper_DefinesInverseFunctions(serializer, 4d, ChtTerminal.JustRaw("4.0"), typeof(double));
        await Mapper_DefinesInverseFunctions(serializer, 4m, ChtTerminal.JustRaw("4.0"), typeof(decimal));
        await Mapper_DefinesInverseFunctions(serializer, 4, ChtTerminal.JustRaw("4"), typeof(int));
        await Mapper_DefinesInverseFunctions(serializer, 4L, ChtTerminal.JustRaw("4"), typeof(long));
        await Mapper_DefinesInverseFunctions(serializer, new BigInteger(4), ChtTerminal.JustRaw("4"), typeof(BigInteger));
    }

    [Test]
    public async Task NullMapper_DefinesInverseFunctions()
    {
        var mapper = new NullMapper();
        await Mapper_DefinesInverseFunctions(mapper, null, ChtTerminal.JustRaw("null"));
        await Mapper_DefinesInverseFunctions(mapper, null, ChtTerminal.JustRaw("null"), typeof(DateOnly));
        await Mapper_DefinesInverseFunctions(mapper, null, ChtTerminal.JustRaw("null"), typeof(DateOnly?));
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
    public async Task DateTimeOffsetMapper_DefinesInverseFunctions()
    {
        var mapper = new DateTimeOffsetMapper();
        await Mapper_DefinesInverseFunctions(mapper, new DateTimeOffset(new DateOnly(2025, 3, 14), new TimeOnly(9, 30, 0, 0, 1), TimeSpan.Zero), ChtTerminal.JustRaw("2025-03-14T09:30:00.0000010+00:00"));
        await Mapper_DefinesInverseFunctions(mapper, new DateTimeOffset(new DateOnly(2000, 1, 1), new TimeOnly(0, 0, 0), TimeSpan.FromMinutes(-210)), ChtTerminal.JustRaw("2000-01-01T00:00:00-03:30"));
    }

    [Test]
    public async Task GuidMapper_DefinesInverseFunctions()
    {
        var mapper = new GuidMapper();
        var guid = Guid.NewGuid();
        await Mapper_DefinesInverseFunctions(mapper, guid, ChtTerminal.JustRaw(guid.ToString()));
    }

    [Test]
    public async Task ColorMapper_DefinesInverseFunctions()
    {
        var mapper = new ColorMapper();
        await Mapper_DefinesInverseFunctions(mapper, ColorTranslator.FromHtml("#000000"), ChtTerminal.JustRaw("#000000"));
        await Mapper_DefinesInverseFunctions(mapper, ColorTranslator.FromHtml("#123456"), ChtTerminal.JustRaw("#123456"));
    }

    [Test]
    public async Task EnumMapper_DefinesInverseFunctions()
    {
        var type = typeof(TestEnum);
        await Mapper_DefinesInverseFunctions(new EnumMapper([], EnumMappingStyle.UntypedRawName), TestEnum.Variant1, ChtTerminal.JustRaw("variant1"), type);
        await Mapper_DefinesInverseFunctions(new EnumMapper([], EnumMappingStyle.UntypedQuotedName), TestEnum.Variant1, ChtTerminal.JustQuoted("Variant1"), type);
        await Mapper_DefinesInverseFunctions(new EnumMapper([], EnumMappingStyle.UntypedOrdinal), TestEnum.Variant1, ChtTerminal.JustRaw("1"), type);
        await Mapper_DefinesInverseFunctions(new EnumMapper([type], EnumMappingStyle.TypedRawName), TestEnum.Variant1, new ChtNonterminal("Enum", ChtTerminal.JustRaw("variant1")));
        await Mapper_DefinesInverseFunctions(new EnumMapper([], EnumMappingStyle.TypedRawName), TestEnum.Variant1, new ChtNonterminal("Enum", ChtTerminal.JustRaw("variant1")), type);
        await Mapper_DefinesInverseFunctions(new EnumMapper([type], EnumMappingStyle.TypedQuotedName), TestEnum.Variant1, new ChtNonterminal("Enum", ChtTerminal.JustQuoted("Variant1")));
        await Mapper_DefinesInverseFunctions(new EnumMapper([], EnumMappingStyle.TypedQuotedName), TestEnum.Variant1, new ChtNonterminal("Enum", ChtTerminal.JustQuoted("Variant1")), type);
        await Mapper_DefinesInverseFunctions(new EnumMapper([type], EnumMappingStyle.TypedOrdinal), TestEnum.Variant1, new ChtNonterminal("Enum", ChtTerminal.JustRaw("1")));
        await Mapper_DefinesInverseFunctions(new EnumMapper([], EnumMappingStyle.TypedOrdinal), TestEnum.Variant1, new ChtNonterminal("Enum", ChtTerminal.JustRaw("1")), type);
    }

    [Test]
    public async Task UriMapper_DefinesInverseFunctions()
    {
        var type = typeof(Uri);
        var uri = "https://example.com/";
        var value = new Uri(uri);
        await Mapper_DefinesInverseFunctions(new UriMapper(null), value, ChtTerminal.JustQuoted(uri), type);
        await Mapper_DefinesInverseFunctions(new UriMapper(), value, new ChtNonterminal("Uri", ChtTerminal.JustQuoted(uri)));
        await Mapper_DefinesInverseFunctions(new UriMapper("Url"), value, new ChtNonterminal("Url", ChtTerminal.JustQuoted(uri)));
        await Mapper_DefinesInverseFunctions(new UriMapper("Url"), value, new ChtNonterminal("Url", ChtTerminal.JustQuoted(uri)), type);
    }

    private async Task Mapper_DefinesInverseFunctions(IChtMapper mapper, object? value, ChtNode node, Type? targetType = null)
    {
        var serializer = new ChtSerializer().AddMapper(mapper);
        await Mapper_DefinesInverseFunctions(serializer, value, node, targetType);
    }
    private async Task Mapper_DefinesInverseFunctions(ChtSerializer serializer, object? value, ChtNode node, Type? targetType = null)
    {
        await Assert.That(serializer.ToNode(value)).IsEquivalentTo(node);
        await Assert.That(serializer.FromNode(node, targetType ?? typeof(object))).IsEquivalentTo(value);
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
        Variant1 = 1,
        Variant2 = 2
    }

    public class TestObjectWithDictionary
    {
        public string Name { get; set; } = "";

        [ChtFlatten]
        public Dictionary<string, string> Values { get; set; } = [];
    }

    public class TestObjectWithList
    {
        public string Name { get; set; } = "";

        public List<string> Values { get; set; } = [];
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