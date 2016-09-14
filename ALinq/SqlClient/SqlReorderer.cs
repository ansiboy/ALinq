using System;
using System.Collections.Generic;
using ALinq.Mapping;

namespace ALinq.SqlClient
{
    internal class SqlReorderer
    {
        // Fields
        private SqlFactory sql;
        private ITypeSystemProvider typeProvider;

        // Methods
        internal SqlReorderer(ITypeSystemProvider typeProvider, SqlFactory sqlFactory)
        {
            this.typeProvider = typeProvider;
            this.sql = sqlFactory;
        }

        internal SqlNode Reorder(SqlNode node)
        {
            return new Visitor(this.typeProvider, this.sql).Visit(node);
        }

        // Nested Types
        internal class SqlGatherColumnsProduced
        {
            // Methods
            internal static List<SqlColumn> GatherColumns(SqlSource source)
            {
                List<SqlColumn> columns = new List<SqlColumn>();
                new Visitor(columns).Visit(source);
                return columns;
            }

            // Nested Types
            private class Visitor : SqlVisitor
            {
                // Fields
                private List<SqlColumn> columns;

                // Methods
                internal Visitor(List<SqlColumn> columns)
                {
                    this.columns = columns;
                }

                internal override SqlSelect VisitSelect(SqlSelect select)
                {
                    foreach (SqlColumn column in select.Row.Columns)
                    {
                        this.columns.Add(column);
                    }
                    return select;
                }

                internal override SqlNode VisitUnion(SqlUnion su)
                {
                    return su;
                }
            }
        }

        private class Visitor : SqlVisitor
        {
            // Fields
            private bool addPrimaryKeys;
            private SqlAggregateChecker aggregateChecker;
            private SqlSelect currentSelect;
            private List<SqlOrderExpression> orders = new List<SqlOrderExpression>();
            private List<SqlOrderExpression> rowNumberOrders;
            private SqlFactory sql;
            private bool topSelect = true;
            private ITypeSystemProvider typeProvider;

            // Methods
            internal Visitor(ITypeSystemProvider typeProvider, SqlFactory sqlFactory)
            {
                this.typeProvider = typeProvider;
                this.sql = sqlFactory;
                this.aggregateChecker = new SqlAggregateChecker();
            }

            private static bool IsTableAlias(SqlSource src)
            {
                SqlAlias alias = src as SqlAlias;
                return ((alias != null) && (alias.Node is SqlTable));
            }

            private void PrependOrderExpressions(IEnumerable<SqlOrderExpression> exprs)
            {
                if (exprs != null)
                {
                    this.Orders.InsertRange(0, exprs);
                }
            }

            private void PushDown(SqlColumn column)
            {
                SqlSelect node = new SqlSelect(new SqlNop(column.ClrType, column.SqlType, column.SourceExpression),
                                               this.currentSelect.From, this.currentSelect.SourceExpression);
                this.currentSelect.From = new SqlAlias(node);
                node.Row.Columns.Add(column);
            }

            internal override SqlAlias VisitAlias(SqlAlias a)
            {
                if (!IsTableAlias(a) || !this.addPrimaryKeys)
                {
                    return base.VisitAlias(a);
                }
                SqlTable node = (SqlTable) a.Node;
                List<SqlOrderExpression> exprs = new List<SqlOrderExpression>();
                foreach (MetaDataMember member in node.RowType.IdentityMembers)
                {
                    string mappedName = member.MappedName;
                    SqlColumn item = node.Find(mappedName);
                    if (item == null)
                    {
                        item = new SqlColumn(member.MemberAccessor.Type,
                                             this.typeProvider.From(member.MemberAccessor.Type), mappedName, member,
                                             null, node.SourceExpression);
                        item.Alias = a;
                        node.Columns.Add(item);
                    }
                    exprs.Add(new SqlOrderExpression(SqlOrderType.Ascending, new SqlColumnRef(item)));
                }
                this.PrependOrderExpressions(exprs);
                return a;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                this.Visit(join.Left);
                List<SqlOrderExpression> orders = this.orders;
                this.orders = null;
                this.Visit(join.Right);
                this.PrependOrderExpressions(orders);
                return join;
            }

            internal override SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber)
            {
                if (rowNumber.OrderBy.Count <= 0)
                {
                    SqlDuplicator duplicator = new SqlDuplicator(true);
                    List<SqlOrderExpression> list = new List<SqlOrderExpression>();
                    List<SqlOrderExpression> list2 = new List<SqlOrderExpression>();
                    if ((this.rowNumberOrders != null) && (this.rowNumberOrders.Count != 0))
                    {
                        list2 = new List<SqlOrderExpression>(this.rowNumberOrders);
                    }
                    else if (this.orders != null)
                    {
                        list2 = new List<SqlOrderExpression>(this.orders);
                    }
                    foreach (SqlOrderExpression expression in list2)
                    {
                        if (!expression.Expression.IsConstantColumn)
                        {
                            list.Add(expression);
                            if (this.rowNumberOrders != null)
                            {
                                this.rowNumberOrders.Remove(expression);
                            }
                            if (this.orders != null)
                            {
                                this.orders.Remove(expression);
                            }
                        }
                    }
                    rowNumber.OrderBy.Clear();
                    if (list.Count == 0)
                    {
                        foreach (
                            SqlColumn column in
                                SqlReorderer.SqlGatherColumnsProduced.GatherColumns(this.currentSelect.From))
                        {
                            if (column.Expression.SqlType.IsOrderable)
                            {
                                list.Add(new SqlOrderExpression(SqlOrderType.Ascending, new SqlColumnRef(column)));
                            }
                        }
                        if (list.Count == 0)
                        {
                            var column2 = new SqlColumn("rowNumberOrder",
                                                         sql.Value(typeof (int),
                                                                             typeProvider.From(typeof (int)), 1,
                                                                             false, rowNumber.SourceExpression));
                            this.PushDown(column2);
                            list.Add(new SqlOrderExpression(SqlOrderType.Ascending, new SqlColumnRef(column2)));
                        }
                    }
                    foreach (SqlOrderExpression expression2 in list)
                    {
                        rowNumber.OrderBy.Add(new SqlOrderExpression(expression2.OrderType,
                                                                     (SqlExpression)
                                                                     duplicator.Duplicate(expression2.Expression)));
                    }
                }
                return rowNumber;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                bool topSelect = this.topSelect;
                bool addPrimaryKeys = this.addPrimaryKeys;
                SqlSelect currentSelect = this.currentSelect;
                this.currentSelect = select;
                if (select.OrderingType == SqlOrderingType.Always)
                {
                    this.addPrimaryKeys = true;
                }
                this.topSelect = false;
                if (select.GroupBy.Count > 0)
                {
                    this.Visit(select.From);
                    this.orders = null;
                }
                else
                {
                    this.Visit(select.From);
                }
                if (select.OrderBy.Count > 0)
                {
                    this.PrependOrderExpressions(select.OrderBy);
                }
                List<SqlOrderExpression> orders = this.orders;
                this.orders = null;
                this.rowNumberOrders = orders;
                select.Where = this.VisitExpression(select.Where);
                int num = 0;
                int count = select.GroupBy.Count;
                while (num < count)
                {
                    select.GroupBy[num] = this.VisitExpression(select.GroupBy[num]);
                    num++;
                }
                select.Having = this.VisitExpression(select.Having);
                int num3 = 0;
                int num4 = select.OrderBy.Count;
                while (num3 < num4)
                {
                    select.OrderBy[num3].Expression = this.VisitExpression(select.OrderBy[num3].Expression);
                    num3++;
                }
                select.Top = this.VisitExpression(select.Top);
                select.Selection = this.VisitExpression(select.Selection);
                select.Row = (SqlRow) this.Visit(select.Row);
                this.topSelect = topSelect;
                this.addPrimaryKeys = addPrimaryKeys;
                this.orders = orders;
                if (select.OrderingType == SqlOrderingType.Blocked)
                {
                    this.orders = null;
                }
                select.OrderBy.Clear();
                SqlRowNumberChecker checker = new SqlRowNumberChecker();
                if (checker.HasRowNumber(select) && (checker.RowNumberColumn != null))
                {
                    select.Row.Columns.Remove(checker.RowNumberColumn);
                    this.PushDown(checker.RowNumberColumn);
                    this.Orders.Add(new SqlOrderExpression(SqlOrderType.Ascending,
                                                           new SqlColumnRef(checker.RowNumberColumn)));
                }
                if ((this.topSelect || (select.Top != null)) &&
                    ((select.OrderingType != SqlOrderingType.Never) && (this.orders != null)))
                {
                    SqlDuplicator duplicator = new SqlDuplicator(true);
                    foreach (SqlOrderExpression expression in this.orders)
                    {
                        select.OrderBy.Add(new SqlOrderExpression(expression.OrderType,
                                                                  (SqlExpression)
                                                                  duplicator.Duplicate(expression.Expression)));
                    }
                }
                this.currentSelect = currentSelect;
                return select;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                List<SqlOrderExpression> orders = this.orders;
                this.orders = new List<SqlOrderExpression>();
                base.VisitSubSelect(ss);
                this.orders = orders;
                return ss;
            }

            internal override SqlNode VisitUnion(SqlUnion su)
            {
                this.orders = null;
                su.Left = this.Visit(su.Left);
                this.orders = null;
                su.Right = this.Visit(su.Right);
                this.orders = null;
                return su;
            }

            // Properties
            private List<SqlOrderExpression> Orders
            {
                get
                {
                    if (this.orders == null)
                    {
                        this.orders = new List<SqlOrderExpression>();
                    }
                    return this.orders;
                }
            }
        }
    }
}