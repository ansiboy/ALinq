namespace ALinq.SqlClient
{
    internal class SqlAggregateChecker
    {
        // Fields
        private Visitor visitor = new Visitor();

        // Methods
        internal SqlAggregateChecker()
        {
        }

        internal bool HasAggregates(SqlNode node)
        {
            this.visitor.hasAggregates = false;
            this.visitor.Visit(node);
            return this.visitor.hasAggregates;
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Fields
            internal bool hasAggregates;

            // Methods
            internal Visitor()
            {
            }

            internal override SqlSource VisitSource(SqlSource source)
            {
                return source;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                return ss;
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                SqlNodeType nodeType = uo.NodeType;
                if (nodeType <= SqlNodeType.LongCount)
                {
                    switch (nodeType)
                    {
                        case SqlNodeType.Avg:
                        case SqlNodeType.Count:
                        case SqlNodeType.LongCount:
                            goto Label_002B;
                    }
                    goto Label_0034;
                }
                if (((nodeType != SqlNodeType.Max) && (nodeType != SqlNodeType.Min)) && (nodeType != SqlNodeType.Sum))
                {
                    goto Label_0034;
                }
                Label_002B:
                this.hasAggregates = true;
                return uo;
                Label_0034:
                return base.VisitUnaryOperator(uo);
            }
        }
    }
}