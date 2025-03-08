using System.Collections;
using System.Reflection;

namespace Cht.Mappers;
public class IEnumerableMapper : IChtMapper
{
    public bool FromNode(ChtNode node, Type targetType, ChtSerializer serializer, out object? output)
    {
        output = default;

        if (node is ChtNonterminal nonterminal && nonterminal.Type == "List")
        {
            if (targetType.IsGenericType && targetType.GenericTypeArguments.Length == 1)
            {
                var targetItemType = targetType.GenericTypeArguments[0];
                targetType = typeof(List<>).MakeGenericType(targetItemType);
                output = Activator.CreateInstance(targetType);
                foreach (var item in nonterminal.Children)
                {
                    ((IList)output!).Add(serializer.FromNode(item, targetItemType));
                }
            }
            else
            {
                output = nonterminal.Children.Select(serializer.FromNode<object>).ToList();
            }

            return output is not null && output.GetType().IsAssignableTo(targetType);
        }
        return false;
    }

    public bool ToNode(object? value, ChtSerializer serializer, out ChtNode output)
    {
        if (value is IEnumerable enumerable)
        {
            output = new ChtNonterminal("List", enumerable.Cast<object>().Select(serializer.ToNode));
            return true;
        }

        output = default;
        return false;
    }
}