using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using ALinq.SqlClient;

namespace ALinq.SqlClient
{
    internal class SqlColumnizer
    {
        // Fields
        private ColumnDeclarer declarer;
        private ColumnNominator nominator;

        // Methods
        internal SqlColumnizer(Func<SqlExpression, bool> fnCanBeColumn)
        {
            this.nominator = new ColumnNominator(fnCanBeColumn);
            this.declarer = new ColumnDeclarer();
        }

        internal SqlExpression ColumnizeSelection(SqlExpression selection)
        {
            return this.declarer.Declare(selection, this.nominator.Nominate(selection));
        }

        // Nested Types
        private class ColumnDeclarer : SqlVisitor
        {
            // Fields
            private HashSet<SqlExpression> candidates;

            // Methods
            internal ColumnDeclarer()
            {
            }

            internal SqlExpression Declare(SqlExpression expression, HashSet<SqlExpression> candidates)
            {
                this.candidates = candidates;
                return (SqlExpression) this.Visit(expression);
            }

            internal override SqlNode Visit(SqlNode node)
            {
                SqlExpression item = node as SqlExpression;
                if ((item == null) || !this.candidates.Contains(item))
                {
                    return base.Visit(node);
                }
                if ((item.NodeType != SqlNodeType.Column) && (item.NodeType != SqlNodeType.ColumnRef))
                {
                    return new SqlColumn(item.ClrType, item.SqlType, null, null, item, item.SourceExpression);
                }
                return item;
            }
        }

        private class ColumnNominator : SqlVisitor
        {
            // Fields
            private HashSet<SqlExpression> candidates;
            private Func<SqlExpression, bool> fnCanBeColumn;
            private bool isBlocked;

            // Methods
            internal ColumnNominator(Func<SqlExpression, bool> fnCanBeColumn)
            {
                this.fnCanBeColumn = fnCanBeColumn;
            }

            private static bool CanRecurseColumnize(SqlExpression expr)
            {
                switch (expr.NodeType)
                {
                    case SqlNodeType.Element:
                    case SqlNodeType.Exists:
                    case SqlNodeType.ClientQuery:
                    case SqlNodeType.Column:
                    case SqlNodeType.ColumnRef:
                    case SqlNodeType.AliasRef:
                    case SqlNodeType.Link:
                    case SqlNodeType.Multiset:
                    case SqlNodeType.ScalarSubSelect:
                    case SqlNodeType.Select:
                    case SqlNodeType.SharedExpressionRef:
                    case SqlNodeType.Value:
                    case SqlNodeType.Nop:
                        return false;
                }
                return true;
            }

            private static bool IsClientOnly(SqlExpression expr)
            {
                switch (expr.NodeType)
                {
                    case SqlNodeType.DiscriminatedType:
                    case SqlNodeType.Element:
                    case SqlNodeType.Link:
                    case SqlNodeType.ClientArray:
                    case SqlNodeType.ClientCase:
                    case SqlNodeType.ClientQuery:
                    case SqlNodeType.AliasRef:
                    case SqlNodeType.Grouping:
                    case SqlNodeType.Multiset:
                    case SqlNodeType.Nop:
                    case SqlNodeType.SharedExpression:
                    case SqlNodeType.SharedExpressionRef:
                    case SqlNodeType.SimpleExpression:
                    case SqlNodeType.TypeCase:
                        return true;

                    case SqlNodeType.OuterJoinedValue:
                        return IsClientOnly(((SqlUnary) expr).Operand);
                }
                return false;
            }

            internal HashSet<SqlExpression> Nominate(SqlExpression expression)
            {
                this.candidates = new HashSet<SqlExpression>();
                this.isBlocked = false;
                this.Visit(expression);
                return this.candidates;
            }

            internal override SqlNode Visit(SqlNode node)
            {
                SqlExpression expr = node as SqlExpression;
                if (expr != null)
                {
                    bool isBlocked = this.isBlocked;
                    this.isBlocked = false;
                    if (CanRecurseColumnize(expr))
                    {
                        base.Visit(expr);
                    }
                    if (!this.isBlocked)
                    {
                        if (!IsClientOnly(expr) && (expr.NodeType != SqlNodeType.Column) && expr.SqlType.CanBeColumn &&
                            (this.fnCanBeColumn == null || this.fnCanBeColumn(expr)))
                        {
                            this.candidates.Add(expr);
                        }
                        else
                        {
                            this.isBlocked = true;
                        }
                    }
                    this.isBlocked |= isBlocked;
                }
                return node;
            }

            internal override SqlExpression VisitClientCase(SqlClientCase c)
            {
                c.Expression = this.VisitExpression(c.Expression);
                int num = 0;
                int count = c.Whens.Count;
                while (num < count)
                {
                    c.Whens[num].Value = this.VisitExpression(c.Whens[num].Value);
                    num++;
                }
                return c;
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c)
            {
                c.Expression = this.VisitExpression(c.Expression);
                int num = 0;
                int count = c.Whens.Count;
                while (num < count)
                {
                    c.Whens[num].Value = this.VisitExpression(c.Whens[num].Value);
                    num++;
                }
                return c;
            }

            internal override SqlExpression VisitTypeCase(SqlTypeCase tc)
            {
                tc.Discriminator = this.VisitExpression(tc.Discriminator);
                int num = 0;
                int count = tc.Whens.Count;
                while (num < count)
                {
                    tc.Whens[num].TypeBinding = this.VisitExpression(tc.Whens[num].TypeBinding);
                    num++;
                }
                return tc;
            }
        }

        private class ColumnAppendToTable : SqlVisitor
        {
            internal override SqlTable VisitTable(SqlTable tab)
            {
                return base.VisitTable(tab);
            }
        }
    }
}