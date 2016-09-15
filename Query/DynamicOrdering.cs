using System.Linq.Expressions;

namespace ALinq.Dynamic
{
    internal class DynamicOrdering
    {
        public Expression Selector;
        public bool Ascending;
    }
}