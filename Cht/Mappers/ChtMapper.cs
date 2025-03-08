namespace Cht.Mappers;

/// <summary>
/// A mapper for converting between CHT nodes and .NET objects.
/// </summary>
public interface IChtMapper
{
    bool FromNode(ChtNode node, Type targetType, ChtSerializer serializer, out object? output);
    bool ToNode(object? value, ChtSerializer serializer, out ChtNode output);
}

/// <summary>
/// A mapper for converting between CHT nodes and .NET objects of a specific type.
/// </summary>
/// <typeparam name="T">Type that this mapper supports.</typeparam>
public abstract class ChtMapper<T> : IChtMapper
{
    public abstract bool FromNode(ChtNode node, ChtSerializer serializer, out T output);
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
}