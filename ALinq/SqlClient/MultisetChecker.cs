namespace ALinq.SqlClient
{
    internal class MultisetChecker
    {
        // Methods
        internal static bool HasMultiset(SqlExpression expr)
        {
            Visitor visitor = new Visitor();
            visitor.Visit(expr);
            return visitor.foundMultiset;
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Fields
            internal bool foundMultiset;

            // Methods
            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                return cq;
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem)
            {
                return elem;
            }

            internal override SqlExpression VisitExists(SqlSubSelect ss)
            {
                return ss;
            }

            internal override SqlExpression VisitMultiset(SqlSubSelect sms)
            {
                this.foundMultiset = true;
                return sms;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                return ss;
            }
        }
    }
}