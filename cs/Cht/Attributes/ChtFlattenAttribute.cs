namespace Cht;

/// <summary>
/// When the property value is mapped to <see cref="ChtNonterminal"/> using the <see cref="Mappers.ObjectMapper"/>,
/// causes the children nodes to be stored directly on the parent.
/// Only the last property may be annotated with this attribute if <see cref="Mappers.ObjectMapper"/> is used for mapping from CHT nodes
/// and the property must be serializable.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ChtFlattenAttribute : Attribute
{
}
