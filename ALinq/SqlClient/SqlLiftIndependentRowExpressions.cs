using System.Collections.Generic;

namespace ALinq.SqlClient
{
    internal class SqlLiftIndependentRowExpressions
    {
        // Methods
        internal static SqlNode Lift(SqlNode node)
        {
            node = new ColumnLifter().Visit(node);
            return node;
        }

        // Nested Types
        private class ColumnLifter : SqlVisitor
        {
            // Fields
            private SqlAggregateChecker aggregateChecker = new SqlAggregateChecker();
            private SelectScope expressionSink;

            // Methods
            internal ColumnLifter()
            {
            }

            private SqlSource PushSourceDown(SqlSource sqlSource, List<SqlColumn> cols)
            {
                SqlSelect node = new SqlSelect(
                    new SqlNop(cols[0].ClrType, cols[0].SqlType, sqlSource.SourceExpression), sqlSource,
                    sqlSource.SourceExpression);
                //node.Row.Columns.AddRange(cols);
                foreach (var col in cols)
                {
                    node.Row.Columns.Add(col);
                }
                return new SqlAlias(node);
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                if (this.expressionSink != null)
                {
                    this.expressionSink.ReferencedExpressions[cref.Column] = true;
                }
                return cref;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                if (join.JoinType != SqlJoinType.CrossApply)
                {
                    return base.VisitJoin(join);
                }
                join.Left = this.VisitSource(join.Left);
                join.Condition = this.VisitExpression(join.Condition);
                SelectScope expressionSink = this.expressionSink;
                this.expressionSink = new SelectScope();
                this.expressionSink.LeftProduction = SqlGatherProducedAliases.Gather(join.Left).Keys;
                join.Right = this.VisitSource(join.Right);
                SqlSource sqlSource = join;
                foreach (List<SqlColumn> list in this.expressionSink.Lifted)
                {
                    sqlSource = this.PushSourceDown(sqlSource, list);
                }
                this.expressionSink = expressionSink;
                return sqlSource;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                SelectScope expressionSink = this.expressionSink;
                if (select.Top != null)
                {
                    this.expressionSink = null;
                }
                if ((select.GroupBy.Count > 0) || this.aggregateChecker.HasAggregates(select))
                {
                    this.expressionSink = null;
                }
                if (select.IsDistinct)
                {
                    this.expressionSink = null;
                }
                if (this.expressionSink != null)
                {
                    List<SqlColumn> collection = new List<SqlColumn>();
                    List<SqlColumn> item = new List<SqlColumn>();
                    foreach (SqlColumn column in select.Row.Columns)
                    {
                        bool flag = SqlAliasesReferenced.ReferencesAny(column.Expression,
                                                                       this.expressionSink.LeftProduction);
                        bool flag2 = this.expressionSink.ReferencedExpressions.ContainsKey(column);
                        if (flag && !flag2)
                        {
                            item.Add(column);
                        }
                        else
                        {
                            collection.Add(column);
                        }
                    }
                    select.Row.Columns.Clear();
                    //select.Row.Columns.AddRange(collection);
                    foreach (var column in collection)
                    {
                        select.Row.Columns.Add(column);
                    }
                    if (item.Count > 0)
                    {
                        this.expressionSink.Lifted.Push(item);
                    }
                }
                SqlSelect select2 = base.VisitSelect(select);
                this.expressionSink = expressionSink;
                return select2;
            }

            // Nested Types
            private class SelectScope
            {
                // Fields
                internal IEnumerable<SqlAlias> LeftProduction;
                internal Stack<List<SqlColumn>> Lifted = new Stack<List<SqlColumn>>();
                internal Dictionary<SqlColumn, bool> ReferencedExpressions = new Dictionary<SqlColumn, bool>();
            }
        }
    }
}