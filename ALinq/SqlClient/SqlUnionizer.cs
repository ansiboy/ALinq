using System;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlUnionizer
    {
        // Methods
        internal static SqlNode Unionize(SqlNode node)
        {
            return new Visitor().Visit(node);
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Methods
            private SqlUnion GetUnion(SqlSource source)
            {
                SqlAlias alias = source as SqlAlias;
                if (alias != null)
                {
                    SqlUnion node = alias.Node as SqlUnion;
                    if (node != null)
                    {
                        return node;
                    }
                }
                return null;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                base.VisitSelect(select);
                SqlUnion union = this.GetUnion(select.From);
                if (union != null)
                {
                    SqlSelect left = union.Left as SqlSelect;
                    SqlSelect right = union.Right as SqlSelect;
                    if (!((left != null) & (right != null)))
                    {
                        return select;
                    }
                    int num = 0;
                    int count = left.Row.Columns.Count;
                    while (num < count)
                    {
                        left.Row.Columns[num].Ordinal = select.Row.Columns.Count + num;
                        num++;
                    }
                    int num3 = 0;
                    int num4 = right.Row.Columns.Count;
                    while (num3 < num4)
                    {
                        right.Row.Columns[num3].Ordinal = select.Row.Columns.Count + num3;
                        num3++;
                    }
                    int num5 = 0;
                    int num6 = select.Row.Columns.Count;
                    while (num5 < num6)
                    {
                        SqlExprSet expression = select.Row.Columns[num5].Expression as SqlExprSet;
                        if (expression != null)
                        {
                            int num7 = 0;
                            int num8 = expression.Expressions.Count;
                            while (num7 < num8)
                            {
                                SqlColumnRef ref2 = expression.Expressions[num7] as SqlColumnRef;
                                if ((ref2 != null) && (num7 >= select.Row.Columns.Count))
                                {
                                    ref2.Column.Ordinal = num5;
                                }
                                num7++;
                            }
                        }
                        num5++;
                    }
                    Comparison<SqlColumn> comparison = delegate(SqlColumn x, SqlColumn y)
                                                           {
                                                               return x.Ordinal - y.Ordinal;
                                                           };
                    left.Row.Columns.Sort(comparison);
                    right.Row.Columns.Sort(comparison);
                }
                return select;
            }
        }
    }


}