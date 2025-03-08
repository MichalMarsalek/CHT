namespace Cht;

/// <summary>
/// Causes the property to be ignored by <see cref="Mappers.ObjectMapper"/>,
/// when mapping in either direction.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ChtIgnoreAttribute : Attribute
{
    
}
