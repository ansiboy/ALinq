namespace ALinq.SqlClient
{
    internal class ExpectNoFloatingColumns : SqlVisitor
    {
        // Methods
        internal override SqlExpression VisitColumn(SqlColumn col)
        {
            //if (col is SqlDynamicColumn)
            //    return col;
            
            throw Error.UnexpectedFloatingColumn();
        }

        internal override SqlRow VisitRow(SqlRow row)
        {
            foreach (SqlColumn column in row.Columns)
            {
                this.Visit(column.Expression);
            }
            return row;
        }

        internal override SqlTable VisitTable(SqlTable tab)
        {
            foreach (SqlColumn column in tab.Columns)
            {
                this.Visit(column.Expression);
            }
            return tab;
        }
    }
}