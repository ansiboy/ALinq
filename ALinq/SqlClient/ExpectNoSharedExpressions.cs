namespace ALinq.SqlClient
{
    internal class ExpectNoSharedExpressions : SqlVisitor
    {
        // Methods
        internal override SqlExpression VisitSharedExpression(SqlSharedExpression shared)
        {
            throw Error.UnexpectedSharedExpression();
        }

        internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref)
        {
            throw Error.UnexpectedSharedExpressionReference();
        }
    }
}