using System.Linq;

namespace ALinq.SqlClient
{
    internal class SqlAliaser
    {
        // Fields
        private Visitor visitor = new Visitor();

        // Methods
        internal SqlAliaser()
        {
        }

        internal SqlNode AssociateColumnsWithAliases(SqlNode node)
        {
            return this.visitor.Visit(node);
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Fields
            private SqlAlias alias;

            // Methods
            internal Visitor()
            {
            }

            internal override SqlAlias VisitAlias(SqlAlias sqlAlias)
            {
                SqlAlias alias = this.alias;
                this.alias = sqlAlias;
                sqlAlias.Node = this.Visit(sqlAlias.Node);
                this.alias = alias;
                return sqlAlias;
            }

            internal override SqlRow VisitRow(SqlRow row)
            {
                foreach (SqlColumn column in row.Columns)
                {
                    column.Alias = this.alias;
                }

                return base.VisitRow(row);
            }

            internal override SqlTable VisitTable(SqlTable tab)
            {
                foreach (SqlColumn column in tab.Columns)
                {
                    column.Alias = this.alias;
                }
                return base.VisitTable(tab);
            }

            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc)
            {
                foreach (SqlColumn column in fc.Columns)
                {
                    column.Alias = this.alias;
                }
                return base.VisitTableValuedFunctionCall(fc);
            }
        }
    }
}