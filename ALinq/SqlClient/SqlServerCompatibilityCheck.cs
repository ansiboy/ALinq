using System.Collections.ObjectModel;

namespace ALinq.SqlClient
{
    internal static class SqlServerCompatibilityCheck
    {
        // Methods
        internal static void ThrowIfUnsupported(SqlNode node, SqlNodeAnnotations annotations,
                                                SqlProvider.ProviderMode provider)
        {
            if (annotations.HasAnnotationType(typeof (SqlServerCompatibilityAnnotation)))
            {
                Visitor visitor = new Visitor(provider);
                visitor.annotations = annotations;
                visitor.Visit(node);
                if (visitor.reasons.Count > 0)
                {
                    throw Error.ExpressionNotSupportedForSqlServerVersion(visitor.reasons);
                }
            }
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Fields
            internal SqlNodeAnnotations annotations;
            private readonly SqlProvider.ProviderMode provider;
            internal readonly Collection<string> reasons = new Collection<string>();

            // Methods
            internal Visitor(SqlProvider.ProviderMode provider)
            {
                this.provider = provider;
            }

            internal override SqlNode Visit(SqlNode node)
            {
                if (annotations.NodeIsAnnotated(node))
                {
                    foreach (SqlNodeAnnotation annotation in annotations.Get(node))
                    {
                        SqlServerCompatibilityAnnotation annotation2 = annotation as SqlServerCompatibilityAnnotation;
                        if ((annotation2 != null) && annotation2.AppliesTo(provider))
                        {
                            reasons.Add(annotation.Message);
                        }
                    }
                }
                return base.Visit(node);
            }
        }
    }
}