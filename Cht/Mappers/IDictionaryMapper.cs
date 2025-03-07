using System.Collections;

namespace Cht.Mappers;
public class IDictionaryMapper : ChtMapper<IDictionary>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out IDictionary output)
    {
        output = default;
        if (node is ChtNonterminal nonterminal && nonterminal.Type == "Dictionary")
        {
            if (nonterminal.Children.All(x => x is ChtNonterminal pair && pair.Children.Count == 2))
            {
                output = nonterminal.Children.Cast<ChtNonterminal>().ToDictionary(
                    pair => serializer.FromNode<object>(pair.Children[0]),
                    pair => serializer.FromNode<object>(pair.Children[1])
                );
                return true;
            }
            throw new ArgumentException("Dictionary node must contain only pairs.");
        }
        return false;
    }

    public override bool ToNode(IDictionary value, ChtSerializer serializer, out ChtNode output)
    {
        if (value is null)
        {
            output = default;
            return false;
        }
        output = new ChtNonterminal(
            "Dictionary",
            value.Keys.Cast<object>().Select(key => new ChtNonterminal(
                "KeyValue",
                serializer.ToNode(key), serializer.ToNode(value[key])
            ))
        );
        return true;
    }
}