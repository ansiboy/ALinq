using System.Collections.Generic;

namespace ALinq.SqlClient
{
    internal class SqlGatherProducedAliases
    {
        // Methods
        internal static Dictionary<SqlAlias, bool> Gather(SqlNode node)
        {
            Gatherer gatherer = new Gatherer();
            gatherer.Visit(node);
            return gatherer.Produced;
        }

        // Nested Types
        private class Gatherer : SqlVisitor
        {
            // Fields
            internal Dictionary<SqlAlias, bool> Produced = new Dictionary<SqlAlias, bool>();

            // Methods
            internal override SqlAlias VisitAlias(SqlAlias a)
            {
                this.Produced[a] = true;
                return base.VisitAlias(a);
            }
        }
    }
}