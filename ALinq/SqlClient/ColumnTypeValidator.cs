namespace ALinq.SqlClient
{
    internal class ColumnTypeValidator : SqlVisitor
    {
        // Methods
        internal override SqlRow VisitRow(SqlRow row)
        {
            int num = 0;
            int count = row.Columns.Count;
            while (num < count)
            {
                SqlColumn column = row.Columns[num];
                SqlExpression expression = this.VisitExpression(column.Expression);
                if ((expression != null) &&
                    (TypeSystem.GetNonNullableType(column.ClrType) != TypeSystem.GetNonNullableType(expression.ClrType)))
                {
                    throw Error.ColumnClrTypeDoesNotAgreeWithExpressionsClrType();
                }
                num++;
            }
            return row;
        }
    }
}