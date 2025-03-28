using System.Collections;

namespace Cht.Mappers;
public class GenericDictionaryMapper : IChtMapper
{
    private static Type dictionaryType = typeof(Dictionary<,>);
    public bool FromNode(ChtNode node, Type targetType, ChtSerializer serializer, out object? output)
    {
        output = default;
        if (node is ChtNonterminal nonterminal && nonterminal.Type == "Dictionary")
        {
            if (nonterminal.Children.All(x => x is ChtNonterminal pair && pair.Children.Count == 2))
            {
                var children = nonterminal.Children.Cast<ChtNonterminal>();
                if (targetType.IsGenericType && targetType.GenericTypeArguments.Length == 2)
                {
                    var keyType = targetType.GetGenericArguments()[0];
                    var valueType = targetType.GetGenericArguments()[1];
                    targetType = dictionaryType.MakeGenericType(keyType, valueType);
                    output = Activator.CreateInstance(targetType);
                    foreach (var pair in children)
                    {
                        var key = serializer.FromNode(pair.Children[0], keyType);
                        var value = serializer.FromNode(pair.Children[1], valueType);
                        ((IDictionary)output!).Add(key, value);
                    }
                }
                else
                {
                    output = children.ToDictionary(
                        pair => serializer.FromNode<object>(pair.Children[0]),
                        pair => serializer.FromNode<object>(pair.Children[1])
                    );
                }
                return output is not null && output.GetType().IsAssignableTo(targetType);
            }
            throw new ArgumentException("Dictionary node must contain only pairs.");
        }
        return false;
    }

    public bool ToNode(object? value, ChtSerializer serializer, out ChtNode output)
    {
        if (value is IDictionary dictionary)
        {
            var type = dictionary.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == dictionaryType)
            {
                output = new ChtNonterminal(
                    "Dictionary",
                    dictionary.Keys.Cast<object>().Select(key => new ChtNonterminal(
                        "KeyValue",
                        serializer.ToNode(key), serializer.ToNode(dictionary[key])
                    ))
                );
                return true;
            }
        }

        output = default;
        return false;
    }
}