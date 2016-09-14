namespace ALinq.SqlClient
{
    internal class SqlRowNumberChecker
    {
        // Fields
        private readonly Visitor rowNumberVisitor = new Visitor();

        // Methods

        internal bool HasRowNumber(SqlNode node)
        {
            rowNumberVisitor.Visit(node);
            return rowNumberVisitor.HasRowNumber;
        }

        internal bool HasRowNumber(SqlRow row)
        {
            foreach (SqlColumn column in row.Columns)
            {
                if (HasRowNumber(column))
                {
                    return true;
                }
            }
            return false;
        }

        // Properties
        internal SqlColumn RowNumberColumn
        {
            get
            {
                if (!rowNumberVisitor.HasRowNumber)
                {
                    return null;
                }
                return rowNumberVisitor.CurrentColumn;
            }
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Fields
            private bool hasRowNumber;

            // Methods
            internal override SqlRow VisitRow(SqlRow row)
            {
                int num = 0;
                int count = row.Columns.Count;
                while (num < count)
                {
                    row.Columns[num].Expression = VisitExpression(row.Columns[num].Expression);
                    if (hasRowNumber)
                    {
                        CurrentColumn = row.Columns[num];
                        return row;
                    }
                    num++;
                }
                return row;
            }

            internal override SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber)
            {
                hasRowNumber = true;
                return rowNumber;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                return ss;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                Visit(select.Row);
                Visit(select.Where);
                return select;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                return ss;
            }

            // Properties
            public SqlColumn CurrentColumn { get; set; }

            public bool HasRowNumber
            {
                get { return hasRowNumber; }
            }
        }
    }
}