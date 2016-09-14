using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ALinq.SqlClient
{
    public partial class SqlProvider
    {
        private class SubQueryCompiler : SqlVisitor
        {
            // Fields
            private readonly SqlProvider provider;
            private List<ICompiledSubQuery> subQueries;

            // Methods
            internal SubQueryCompiler(SqlProvider provider)
            {
                this.provider = provider;
            }

            internal ICompiledSubQuery[] Compile(SqlNode node)
            {
                subQueries = new List<ICompiledSubQuery>();
                Visit(node);
                return subQueries.ToArray();
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                Type elementType = (cq.Query.NodeType == SqlNodeType.Multiset)
                                       ? TypeSystem.GetElementType(cq.ClrType)
                                       : cq.ClrType;
                ICompiledSubQuery item = provider.CompileSubQuery(cq.Query.Select, 
                                                                  elementType, new ReadOnlyCollection<SqlParameter>(cq.Parameters));
                cq.Ordinal = subQueries.Count;
                subQueries.Add(item);
                return cq;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                Visit(select.Selection);
                return select;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                return ss;
            }
        }
    }
}
