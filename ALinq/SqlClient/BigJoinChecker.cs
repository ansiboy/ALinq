namespace ALinq.SqlClient
{
    internal class BigJoinChecker
    {
        // Methods
        internal static bool CanBigJoin(SqlSelect select)
        {
            Visitor visitor = new Visitor();
            visitor.Visit(select);
            return visitor.canBigJoin;
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Fields
            internal bool canBigJoin = true;

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
                return sms;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                return ss;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                this.canBigJoin &= ((select.GroupBy.Count == 0) && (select.Top == null)) && !select.IsDistinct;
                if (!this.canBigJoin)
                {
                    return select;
                }
                return base.VisitSelect(select);
            }
        }
    }
}