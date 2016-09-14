using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.MySQL
{
    class MySqlParameterizer //: SqlParameterizerBase
        : ISqlParameterizer
    {
        // Fields
        private readonly SqlNodeAnnotations annotations;
        private int index;
        internal readonly ITypeSystemProvider typeProvider;

        // Methods
        internal MySqlParameterizer(ITypeSystemProvider typeProvider, SqlNodeAnnotations annotations)
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
            var item = new SqlParameterInfo(new SqlParameter(typeof(int), typeProvider.From(typeof(int)),
                                            "ROWCOUNT", block.SourceExpression));
            var list = new List<ReadOnlyCollection<SqlParameterInfo>>();
            int num = 0;
            int count = block.Statements.Count;
            while (num < count)
            {
                SqlNode node = block.Statements[num];
                List<SqlParameterInfo> list2 = this.ParameterizeInternal(node);
                if (num > 0)
                {
                    list2.Add(item);
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
            internal Visitor(ISqlParameterizer parameterizer)
                : base(parameterizer)
            {
            }

            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc)
            {
                this.VisitUserQuery(spc);
                int num = 0;
                int count = spc.Function.Parameters.Count;
                while (num < count)
                {
                    MetaParameter p = spc.Function.Parameters[num];
                    var node = spc.Arguments[num] as SqlParameter;
                    if (node != null)
                    {
                        node.Direction = GetParameterDirection(p);
                        Debug.Assert(!string.IsNullOrEmpty(p.MappedName));
                        node.Name = p.MappedName;
                        if ((node.Direction == ParameterDirection.InputOutput) ||
                            (node.Direction == ParameterDirection.Output))
                        {
                            RetypeOutParameter(node);
                        }
                    }
                    num++;
                }
                //var parameter3 = new SqlParameter(typeof(int?),
                //                                  parameterizer.TypeProvider.From(typeof(int)),
                //                                  "RETURN_VALUE", spc.SourceExpression) { Direction = ParameterDirection.Output };
                //this.currentParams.Add(new SqlParameterInfo(parameter3));
                return spc;
            }
        }
    }
}
