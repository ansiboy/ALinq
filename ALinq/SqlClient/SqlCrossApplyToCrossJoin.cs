using System.Collections.Generic;

namespace ALinq.SqlClient
{
    internal class SqlCrossApplyToCrossJoin
    {
        // Methods
        internal static SqlNode Reduce(SqlNode node, SqlNodeAnnotations annotations)
        {
            Reducer reducer = new Reducer();
            reducer.Annotations = annotations;
            return reducer.Visit(node);
        }

        // Nested Types
        private class Reducer : SqlVisitor
        {
            // Fields
            internal SqlNodeAnnotations Annotations;

            // Methods
            internal override SqlSource VisitJoin(SqlJoin join)
            {
                if (join.JoinType != SqlJoinType.CrossApply)
                {
                    return base.VisitJoin(join);
                }
                Dictionary<SqlAlias, bool> dictionary = SqlGatherProducedAliases.Gather(join.Left);
                foreach (SqlAlias alias in SqlGatherConsumedAliases.Gather(join.Right).Keys)
                {
                    if (dictionary.ContainsKey(alias))
                    {
                        this.Annotations.Add(join,
                                             new SqlServerCompatibilityAnnotation(
                                                 Strings.SourceExpressionAnnotation(join.SourceExpression),
                                                 new SqlProvider.ProviderMode[] {SqlProvider.ProviderMode.Sql2000}));
                        return base.VisitJoin(join);
                    }
                }
                join.JoinType = SqlJoinType.Cross;
                return this.VisitJoin(join);
            }
        }
    }
}