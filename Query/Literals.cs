using System.Linq.Expressions;

namespace ALinq.Dynamic
{
    static class Literals
    {
        public static Expression Null
        {
            get { return Expression.Constant(null); }
        }

        public static Expression True
        {
            get { return Expression.Constant(true); }
        }

        public static Expression False
        {
            get { return Expression.Constant(false); }
        }
    }
}