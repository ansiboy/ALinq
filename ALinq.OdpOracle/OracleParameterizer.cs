using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using ALinq.Mapping;
using ALinq.SqlClient;
using Oracle.DataAccess.Types;

namespace ALinq.Oracle.Odp
{
    class OracleParameterizer //: SqlParameterizerBase
        : ISqlParameterizer
    {
        // Fields
        private readonly SqlNodeAnnotations annotations;
        private int index;
        internal readonly ITypeSystemProvider typeProvider;

        // Methods
        internal OracleParameterizer(ITypeSystemProvider typeProvider, SqlNodeAnnotations annotations)
        {
            this.typeProvider = typeProvider;
            this.annotations = annotations;
        }

        ReadOnlyCollection<SqlParameterInfo> ISqlParameterizer.Parameterize(SqlNode node)
        {
            return ParameterizeInternal(node).AsReadOnly();
        }

        ReadOnlyCollection<ReadOnlyCollection<SqlParameterInfo>> ISqlParameterizer.ParameterizeBlock(SqlBlock block)
        {
            //var item = new SqlParameterInfo(new SqlParameter(typeof(int), typeProvider.From(typeof(int)),
            //                                                 "@ROWCOUNT", block.SourceExpression));
            var list = new List<ReadOnlyCollection<SqlParameterInfo>>();
            int num = 0;
            int count = block.Statements.Count;
            while (num < count)
            {
                SqlNode node = block.Statements[num];
                List<SqlParameterInfo> list2 = this.ParameterizeInternal(node);
                if (num > 0)
                {
                    //list2.Add(item);
                }
                list.Add(list2.AsReadOnly());
                num++;
            }

            return list.AsReadOnly();
        }

        ITypeSystemProvider ISqlParameterizer.TypeProvider
        {
            get { return typeProvider; }
        }

        SqlNodeAnnotations ISqlParameterizer.Annotations
        {
            get { return annotations; }
        }

        private List<SqlParameterInfo> ParameterizeInternal(SqlNode node)
        {
            var visitor = new Visitor(this);
            visitor.Visit(node);
            return new List<SqlParameterInfo>(visitor.currentParams);
        }

        class Visitor : SqlParameterizer.Visitor
        {
            private int index;

            internal Visitor(ISqlParameterizer parameterizer)
                : base(parameterizer)
            {
            }

            internal override string CreateParameterName()
            {
                return (":p" + index++);
            }

            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc)
            {
                var returnType = spc.Function.Method.ReturnType;
                if (returnType != typeof(void))//(QueryConverter.IsValidateReturnType(returnType))
                {
                    IProviderType paramType;
                    if (returnType.IsValueType || returnType == typeof(string))
                        paramType = parameterizer.TypeProvider.From(returnType);
                    else if (returnType.IsGenericType &&
                            ((returnType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                             (returnType.GetGenericTypeDefinition() == typeof(ISingleResult<>))))
                    {
                        paramType = parameterizer.TypeProvider.From(typeof(OracleRefCursor));
                    }
                    else
                    {
                        try
                        {
                            paramType = parameterizer.TypeProvider.From(returnType);
                        }
                        catch(Exception)
                        {
                            throw SqlClient.Error.InvalidReturnFromSproc(returnType);
                        }
                    }
                    var parameter3 = new SqlParameter(returnType, paramType,
                                                     ":RETURN_VALUE", spc.SourceExpression)
                                         {
                                             Direction = ParameterDirection.Output
                                         };
                    currentParams.Add(new SqlParameterInfo(parameter3));
                }

                this.VisitUserQuery(spc);

                //if (returnType != typeof(void))
                int num = 0;
                int count = spc.Function.Parameters.Count;
                while (num < count)
                {
                    MetaParameter p = spc.Function.Parameters[num];
                    var node = spc.Arguments[num] as SqlParameter;
                    if (node != null)
                    {
                        if (string.Equals(p.DbType, "Cursor", System.StringComparison.CurrentCultureIgnoreCase))
                            node.Direction = ParameterDirection.Output;
                        else
                            node.Direction = GetParameterDirection(p);
                        Debug.Assert(!string.IsNullOrEmpty(p.MappedName));
                        node.Name = p.MappedName.StartsWith(":") ? p.MappedName : ":" + p.MappedName;//p.MappedName;
                        if ((node.Direction == ParameterDirection.InputOutput) ||
                            (node.Direction == ParameterDirection.Output))
                        {
                            RetypeOutParameter(node);
                        }
                    }
                    num++;
                }


                return spc;
            }

            internal override SqlStatement VisitUpdate(SqlUpdate sup)
            {
                bool topLevel = this.topLevel;
                this.topLevel = false;

                int num = 0;
                int count = sup.Assignments.Count;
                while (num < count)
                {
                    sup.Assignments[num] = (SqlAssign)Visit(sup.Assignments[num]);
                    num++;
                }
                sup.Select = VisitSequence(sup.Select);

                this.topLevel = topLevel;
                return sup;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                bool topLevel = this.topLevel;
                this.topLevel = false;

                select.Top = this.VisitExpression(select.Top);
                select.Row = (SqlRow)this.Visit(select.Row);

                select.From = (SqlSource)this.Visit(select.From);
                select.Where = this.VisitExpression(select.Where);
                int num = 0;
                int count = select.GroupBy.Count;
                while (num < count)
                {
                    select.GroupBy[num] = this.VisitExpression(select.GroupBy[num]);
                    num++;
                }
                select.Having = this.VisitExpression(select.Having);
                int num3 = 0;
                int num4 = select.OrderBy.Count;
                while (num3 < num4)
                {
                    select.OrderBy[num3].Expression = this.VisitExpression(select.OrderBy[num3].Expression);
                    num3++;
                }

                this.topLevel = topLevel;
                select.Selection = this.VisitExpression(select.Selection);
                return select;
            }
        }
    }
}