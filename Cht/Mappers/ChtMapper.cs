namespace Cht.Mappers;

public interface IChtMapper
{
    bool FromNode(ChtNode node, ChtSerializer serializer, out object? output);
    bool ToNode(object? value, ChtSerializer serializer, out ChtNode output);
}

public abstract class ChtMapper<T> : IChtMapper
{
    public abstract bool FromNode(ChtNode node, ChtSerializer serializer, out T output);
    public abstract bool ToNode(T value, ChtSerializer serializer, out ChtNode output);

    public bool FromNode(ChtNode node, ChtSerializer serializer, out object output)
    {
        if (FromNode(node, serializer, out T typedOutput))
        {
            output = typedOutput;
            return true;
        }
        output = default;
        return false;
    }

    public bool ToNode(object? value, ChtSerializer serializer, out ChtNode output)
    {
        if (value is T typedValue)
        {
            return ToNode(typedValue, serializer, out output);
        }
        output = default;
        return false;
    }
}