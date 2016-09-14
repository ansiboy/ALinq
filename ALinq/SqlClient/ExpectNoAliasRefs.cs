namespace ALinq.SqlClient
{
    internal class ExpectNoAliasRefs : SqlVisitor
    {
        // Methods
        internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
        {
            throw Error.UnexpectedNode(aref.NodeType);
        }
    }
}