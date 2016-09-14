namespace ALinq.SqlClient
{
    internal class SqlTopReducer
    {
        // Methods
        internal static SqlNode Reduce(SqlNode node, SqlNodeAnnotations annotations, SqlFactory sql)
        {
            return new Visitor(annotations, sql).Visit(node);
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Fields
            private readonly SqlNodeAnnotations annotations;
            private readonly SqlFactory sql;

            // Methods
            internal Visitor(SqlNodeAnnotations annotations, SqlFactory sql)
            {
                this.annotations = annotations;
                this.sql = sql;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                base.VisitSelect(select);
                if (select.Top != null)
                {
                    if (select.Top.NodeType == SqlNodeType.Value)
                    {
                        SqlValue top = (SqlValue) select.Top;
                        if (top.IsClientSpecified)
                        {
                            select.Top = sql.Value(top.ClrType, top.SqlType, top.Value, false, top.SourceExpression);
                        }
                        return select;
                    }
                    annotations.Add(select.Top,
                                         new SqlServerCompatibilityAnnotation(
                                             Strings.SourceExpressionAnnotation(select.Top.SourceExpression),
                                             new[] {SqlProvider.ProviderMode.Sql2000}));
                }
                return select;
            }
        }
    }
}