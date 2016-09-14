namespace ALinq.SqlClient
{
    internal class ExpectNoMethodCalls : SqlVisitor
    {
        // Methods
        internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
        {
            throw Error.MethodHasNoSupportConversionToSql(mc.Method.Name);
        }

        internal override SqlSelect VisitSelect(SqlSelect select)
        {
            select.From = this.VisitSource(select.From);
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
            select.Row = (SqlRow) this.Visit(select.Row);
            return select;
        }
    }
}