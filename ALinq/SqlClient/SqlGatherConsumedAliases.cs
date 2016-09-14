using System.Collections.Generic;

namespace ALinq.SqlClient
{
    internal class SqlGatherConsumedAliases
    {
        // Methods
        internal static Dictionary<SqlAlias, bool> Gather(SqlNode node)
        {
            Gatherer gatherer = new Gatherer();
            gatherer.Visit(node);
            return gatherer.Consumed;
        }

        // Nested Types
        private class Gatherer : SqlVisitor
        {
            // Fields
            internal Dictionary<SqlAlias, bool> Consumed = new Dictionary<SqlAlias, bool>();

            // Methods
            internal void VisitAliasConsumed(SqlAlias a)
            {
                this.Consumed[a] = true;
            }

            internal override SqlExpression VisitColumn(SqlColumn col)
            {
                this.VisitAliasConsumed(col.Alias);
                this.VisitExpression(col.Expression);
                return col;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                this.VisitAliasConsumed(cref.Column.Alias);
                return cref;
            }
        }
    }
}