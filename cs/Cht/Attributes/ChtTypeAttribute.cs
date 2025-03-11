namespace Cht;

/// <summary>
/// Overrides the default name of the type when mapping using a <see cref="Mappers.EnumMapper"/> or <see cref="Mappers.ObjectMapper"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
public class ChtTypeAttribute : Attribute
{
    public string TypeName { get; set; }

    public ChtTypeAttribute(string typeName)
    {
        TypeName = typeName;
    }
}
