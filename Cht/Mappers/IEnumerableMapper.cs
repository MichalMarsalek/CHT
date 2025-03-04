using System.Collections;

namespace Cht.Mappers;
public class IEnumerableMapper : ChtMapper<IEnumerable>
{
    public override bool FromNode(ChtNode node, ChtSerializer serializer, out IEnumerable output)
    {
        output = default;
        if (node is ChtNonterminal nonterminal && nonterminal.Type == "List")
        {
            output = nonterminal.Children.Select(serializer.FromNode<object>);
            return true;
        }
        return false;
    }

    public override bool ToNode(IEnumerable value, ChtSerializer serializer, out ChtNode output)
    {
        if (value is null)
        {
            output = default;
            return false;
        }
        output = new ChtNonterminal
        {
            Type = "List",
            Children = value.Cast<object>().Select(serializer.ToNode).ToList()
        };
        return true;
    }
}