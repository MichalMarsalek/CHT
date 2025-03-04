using Cht;
using TUnit.Assertions.AssertConditions.Throws;

public class ChtNodeTests
{
    [Test]
    [MethodDataSource(nameof(ChtRawTerminal_WithInvalidValue_Throws_Data))]
    public async Task ChtRawTerminal_WithInvalidValue_Throws(string value)
    {
        await Assert.That(() => new ChtTerminal { Raw = value }).Throws<Exception>();
    }

    public static IEnumerable<string> ChtRawTerminal_WithInvalidValue_Throws_Data() => [
        "invalid value",
        "invalid(value)",
        "invalid:value",
        "invalid\"value\"",
        "InvalidValue",
        "",
    ];

    [Test]
    [MethodDataSource(nameof(ChtNonterminal_WithInvalidType_Throws_Data))]
    public async Task ChtNonterminal_WithInvalidType_Throws(string value)
    {
        await Assert.That(() => new ChtNonterminal { Type = value }).Throws<Exception>();
    }

    public static IEnumerable<string> ChtNonterminal_WithInvalidType_Throws_Data() => [
        "Invalid type",
        "Invalid(type)",
        "Invalid:type",
        "Invalid\"type\"",
        "invalidType",
        "",
        "58",
    ];
}