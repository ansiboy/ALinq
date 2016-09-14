namespace ALinq.SqlClient
{
    internal class ValidateNoInvalidComparison : SqlVisitor
    {
        // Methods
        internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
        {
            if (((((bo.NodeType == SqlNodeType.EQ) || (bo.NodeType == SqlNodeType.NE)) ||
                  ((bo.NodeType == SqlNodeType.EQ2V) || (bo.NodeType == SqlNodeType.NE2V))) ||
                 (((bo.NodeType == SqlNodeType.GT) || (bo.NodeType == SqlNodeType.GE)) ||
                  ((bo.NodeType == SqlNodeType.LT) || (bo.NodeType == SqlNodeType.LE)))) &&
                (!bo.Left.SqlType.SupportsComparison || !bo.Right.SqlType.SupportsComparison))
            {
                throw Error.UnhandledStringTypeComparison();
            }
            bo.Left = VisitExpression(bo.Left);
            bo.Right = VisitExpression(bo.Right);
            return bo;
        }
    }
}