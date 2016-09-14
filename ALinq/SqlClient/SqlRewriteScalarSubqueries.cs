namespace ALinq.SqlClient
{
    internal class SqlRewriteScalarSubqueries
    {
        // Fields
        private Visitor visitor;

        // Methods
        internal SqlRewriteScalarSubqueries(SqlFactory sqlFactory)
        {
            this.visitor = new Visitor(sqlFactory);
        }

        internal SqlNode Rewrite(SqlNode node)
        {
            return this.visitor.Visit(node);
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Fields
            private SqlAggregateChecker aggregateChecker;
            private SqlSelect currentSelect;
            private SqlFactory sql;

            // Methods
            internal Visitor(SqlFactory sqlFactory)
            {
                this.sql = sqlFactory;
                this.aggregateChecker = new SqlAggregateChecker();
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                SqlSelect node = this.VisitSelect(ss.Select);
                if (!this.aggregateChecker.HasAggregates(node))
                {
                    node.Top = this.sql.ValueFromObject(1, ss.SourceExpression);
                }
                node.OrderingType = SqlOrderingType.Blocked;
                SqlAlias right = new SqlAlias(node);
                this.currentSelect.From = new SqlJoin(SqlJoinType.OuterApply, this.currentSelect.From, right, null,
                                                      ss.SourceExpression);
                return new SqlColumnRef(node.Row.Columns[0]);
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                SqlSelect select3;
                SqlSelect currentSelect = this.currentSelect;
                try
                {
                    this.currentSelect = select;
                    select3 = base.VisitSelect(select);
                }
                finally
                {
                    this.currentSelect = currentSelect;
                }
                return select3;
            }
        }
    }
}