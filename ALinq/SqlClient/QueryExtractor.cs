using System;
using System.Collections.Generic;

namespace ALinq.SqlClient
{
    internal class QueryExtractor
    {
        // Methods
        internal static SqlClientQuery Extract(SqlSubSelect subquery, IEnumerable<SqlParameter> parentParameters, SqlIdentifier sqlIdentity)
        {
            var query = new SqlClientQuery(subquery);
            if (parentParameters != null)
            {
                //query.Parameters.AddRange(parentParameters);
                foreach (var parameter in parentParameters)
                {
                    query.Parameters.Add(parameter);
                }
            }
            var visitor = new Visitor(query.Arguments, query.Parameters, sqlIdentity);
            query.Query = (SqlSubSelect)visitor.Visit(subquery);
            return query;
        }

        // Nested Types
        private class Visitor : SqlDuplicator.DuplicatingVisitor
        {
            // Fields
            private readonly IList<SqlExpression> externals;
            private readonly IList<SqlParameter> parameters;
            private readonly SqlIdentifier sqlIdentity;

            // Methods
            internal Visitor(IList<SqlExpression> externals, IList<SqlParameter> parameters, SqlIdentifier sqlIdentity)
                : base(true)
            {
                this.externals = externals;
                this.parameters = parameters;
                this.sqlIdentity = sqlIdentity;
            }

            private SqlExpression ExtractParameter(SqlExpression expr)
            {
                var clrType = expr.ClrType;
                if (expr.ClrType.IsValueType && !TypeSystem.IsNullableType(expr.ClrType))
                {
                    clrType = typeof(Nullable<>).MakeGenericType(new[] { expr.ClrType });
                }
                externals.Add(expr);
                var pName = sqlIdentity.ParameterPrefix + "x" + (parameters.Count + 1);
                var item = new SqlParameter(clrType, expr.SqlType, pName, expr.SourceExpression);
                parameters.Add(item);
                return item;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                SqlExpression expr = base.VisitColumnRef(cref);
                if (expr == cref)
                {
                    return this.ExtractParameter(expr);
                }
                return expr;
            }

            internal override SqlNode VisitLink(SqlLink link)
            {
                var keyExpressions = new SqlExpression[link.KeyExpressions.Count];
                int index = 0;
                int length = keyExpressions.Length;
                while (index < length)
                {
                    keyExpressions[index] = this.VisitExpression(link.KeyExpressions[index]);
                    index++;
                }
                return new SqlLink(new object(), link.RowType, link.ClrType, link.SqlType, null, link.Member, keyExpressions, null, link.SourceExpression);
            }

            internal override SqlExpression VisitUserColumn(SqlUserColumn suc)
            {
                SqlExpression expr = base.VisitUserColumn(suc);
                if (expr == suc)
                {
                    return ExtractParameter(expr);
                }
                return expr;
            }
        }
    }

}