namespace Cht.Mappers;

/// <summary>
/// A mapper for converting between CHT nodes and .NET objects.
/// </summary>
public interface IChtMapper
{
    /// <summary>
    /// Defines a mapping from a CHT node to a .NET object.
    /// </summary>
    /// <param name="node">Node to map.</param>
    /// <param name="targetType">A type of the target (may be `typeof(object)`). To succeed, the output must be assignable to this type.</param>
    /// <param name="serializer">A serializer that can be used for recursive mapping.</param>
    /// <param name="output">The mapped value.</param>
    /// <returns>Whether the mapping succeeded.</returns>
    bool FromNode(ChtNode node, Type targetType, ChtSerializer serializer, out object? output);

    /// <summary>
    /// Defines a mapping from a .NET object to a CHT node.
    /// </summary>
    /// <param name="value">Value to map.</param>
    /// <param name="serializer">A serializer that can be used for recursive mapping.</param>
    /// <param name="output">The mapped node.</param>
    /// <returns>Whether the mapping succeeded.</returns>
    bool ToNode(object? value, ChtSerializer serializer, out ChtNode output);
}

/// <summary>
/// A mapper for converting between CHT nodes and .NET objects of a specific type.
/// </summary>
/// <typeparam name="T">Type that this mapper supports.</typeparam>
public abstract class ChtMapper<T> : IChtMapper
{

    /// <summary>
    /// Defines a mapping from a CHT node to a .NET object.
    /// </summary>
    /// <param name="node">Node to map.</param>
    /// <param name="serializer">A serializer that can be used for recursive mapping.</param>
    /// <param name="output">The mapped value.</param>
    /// <returns>Whether the mapping succeeded.</returns>
    public abstract bool FromNode(ChtNode node, ChtSerializer serializer, out T output);

    /// <summary>
    /// Defines a mapping from a .NET object to a CHT node.
    /// </summary>
    /// <param name="value">Value to map.</param>
    /// <param name="serializer">A serializer that can be used for recursive mapping.</param>
    /// <param name="output">The mapped node.</param>
    /// <returns>Whether the mapping succeeded.</returns>
    public abstract bool ToNode(T value, ChtSerializer serializer, out ChtNode output);

    public bool FromNode(ChtNode node, Type targetType, ChtSerializer serializer, out object output)
    {
        if (FromNode(node, serializer, out T typedOutput) && typedOutput?.GetType().IsAssignableTo(targetType) == true)
        {
            output = typedOutput;
            return true;
        }
        output = default;
        return false;
    }

    public bool ToNode(object? value, ChtSerializer serializer, out ChtNode output)
    {
        output = default;
        return value is T typedValue && ToNode(typedValue, serializer, out output);
    }

    protected internal static string RemoveSuffix(string value, string suffix)
        => value.EndsWith(suffix) ? value.Substring(0, value.Length - suffix.Length) : value;
}