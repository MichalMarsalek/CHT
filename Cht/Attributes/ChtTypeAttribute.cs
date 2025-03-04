namespace Cht;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
public class ChtTypeAttribute : Attribute
{
    public string TypeName { get; set; }

    public ChtTypeAttribute(string typeName)
    {
        TypeName = typeName;
    }
}
