namespace Cht;

[AttributeUsage(AttributeTargets.Class)]
class ChtTypeAttribute : Attribute
{
    public string TypeName { get; set; }

    public ChtTypeAttribute(string typeName)
    {
        TypeName = typeName;
    }
}
