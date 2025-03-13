# The C# library

The C# library is distributed on [nuget.org](https://www.nuget.org/packages/CompactHeterogeneousTree).

The `ChtSerializer` class is used to `Serialize`/`Deserialize` values to/from CHT. CHT does not inherently define any types and their serializations. To convert between CHT nodes and CLR types the `ChtSerializer` needs a collection of mappers - implementations of `IChtMapper`. These mappers can be defined by the user, but some common mappers are predefined and can be registered using extension methods: `AddStringMapper`, `AddObjectMapper`, `AddEnumMapper`, `AddIntMapper`, etc., or all at once using `AddCommonMappers`. The predefined object mapper and enum mapper read attributes `ChtFlattenAttribute`, `ChtIgnoreAttribute`, `ChtTypeAttribute`. If the predefined enum mapper is to be used for deserialization, it either always needs a precise target or they need to be provided with a collection of supported types. If the predefined object mapper is to be used for deserialization, it needs to be provided with a collection of supported types.

Types and serializer for the AST example can be defined as:

```cs
var serializer = new ChtSerializer()
    .AddObjectMapper<Node>()
    .AddIEnumerableMapper()
    .AddMapper(new SymbolMapper())
    .AddMapper(new IntLiteralMapper())
    .AddMapper(new StringLiteralMapper())

public class SymbolMapper : ChtMapper<Symbol>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out Symbol output)
    {
        if (node is ChtTerminal terminal && terminal.IsJustRaw && terminal.Raw.StartsWith("$"))
        {
            output = new Symbol { Name = terminal.Raw[1..] };
            return true;
        }
        output = default;
        return false;
    }

    public override bool ToNode(Symbol value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtTerminal { Raw = "$" + value.Name };
        return true;
    }
}

public class IntLiteralMapper : ChtMapper<IntLiteral>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out IntLiteral output)
    {
        if (node is ChtTerminal terminal && terminal.IsJustRaw)
        {
            if (int.TryParse(terminal.Raw, out var value))
            {
                output = new IntLiteral { Value = value };
                return true;
            }
        }
        output = default;
        return false;
    }

    public override bool ToNode(IntLiteral value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtTerminal { Raw = value.ToString() };
        return true;
    }
}

public class StringLiteralMapper : ChtMapper<StringLiteral>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out StringLiteral output)
    {
        if (node is ChtTerminal terminal && terminal.IsJustQuoted)
        {
            output = new StringLiteral { Value = terminal.Quoted };
            return true;
        }
        output = default;
        return false;
    }

    public override bool ToNode(StringLiteral value, ChtSerializer serializer, out ChtNode output)
    {
        output = new ChtTerminal { Quoted = value.ToString() };
        return true;
    }
}

public abstract class Node
{
}

public class Block : Node
{
    [ChtFlatten]
    public List<Node> Children { get; set; } = [];
}

[ChtType("List")]
public class ListLiteral : Node
{
    [ChtFlatten]
    public List<Node> Children { get; set; } = [];
}

public class Symbol : Node
{
    public required string Name { get; set; }
}

public class Assignment : Node
{
    public required Symbol Variable { get; set; }
    public required Node Value { get; set; }
}

public class MethodCall : Node
{
    public required Node Object { get; set; }
    public required Symbol Method { get; set; }
    [ChtFlatten]
    public List<Node> Arguments { get; set; } = [];
}

public class FunctionCall : Node
{
    public required Node Function { get; set; }
    [ChtFlatten]
    public List<Node> Arguments { get; set; } = [];
}

public class Indexing : Node
{
    public required Node Data { get; set; }
    public required Node Index { get; set; }
}

public class StringLiteral : Node
{
    public required string Value { get; set; }
}

public class IntLiteral : Node
{
    public required int Value { get; set; }
}
```
