using Cht;
using TUnit.Assertions.AssertConditions.Throws;

public class ChtNodeTests
{
    [Test]
    [MethodDataSource(nameof(ChtRawTerminal_WithInvalidValue_Throws_Data))]
    public async Task ChtRawTerminal_WithInvalidValue_Throws(string value)
    {
        await Assert.That(() => new ChtNode(value, null)).Throws<Exception>();
    }

    public static IEnumerable<string> ChtRawTerminal_WithInvalidValue_Throws_Data() => [
        "invalid value",
        "invalid(value)",
        "invalid:value",
        "invalid\"value\"",
        "",
    ];

}