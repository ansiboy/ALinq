namespace ALinq.SqlClient
{
    internal class HierarchyChecker
    {
        // Methods
        internal static bool HasHierarchy(SqlExpression expr)
        {
            Visitor visitor = new Visitor();
            visitor.Visit(expr);
            return visitor.foundHierarchy;
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Fields
            internal bool foundHierarchy;

            // Methods
            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                this.foundHierarchy = true;
                return cq;
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem)
            {
                this.foundHierarchy = true;
                return elem;
            }

            internal override SqlExpression VisitExists(SqlSubSelect ss)
            {
                return ss;
            }

            internal override SqlExpression VisitMultiset(SqlSubSelect sms)
            {
                this.foundHierarchy = true;
                return sms;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                return ss;
            }
        }
    }
}