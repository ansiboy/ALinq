using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.SqlClient
{
    internal interface ISqlParameterizer
    {
        ReadOnlyCollection<SqlParameterInfo> Parameterize(SqlNode node);
        ReadOnlyCollection<ReadOnlyCollection<SqlParameterInfo>> ParameterizeBlock(SqlBlock sqlBlock);
        ITypeSystemProvider TypeProvider { get; }
        SqlNodeAnnotations Annotations { get; }
    }

    internal class SqlParameterizer : ISqlParameterizer
    {
        // Fields
        private readonly SqlNodeAnnotations annotations;

        internal readonly ITypeSystemProvider typeProvider;

        // Methods
        internal SqlParameterizer(ITypeSystemProvider typeProvider, SqlNodeAnnotations annotations)
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
                                            "@ROWCOUNT", block.SourceExpression));
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

        public ITypeSystemProvider TypeProvider
        {
            get { return this.typeProvider; }
        }

        public SqlNodeAnnotations Annotations
        {
            get { return this.annotations; }
        }

        private List<SqlParameterInfo> ParameterizeInternal(SqlNode node)
        {
            var visitor = new Visitor(this);
            visitor.Visit(node);
            return new List<SqlParameterInfo>(visitor.currentParams);
        }

        // Nested Types
        internal class Visitor : SqlVisitor
        {
            // Fields
            internal readonly List<SqlParameterInfo> currentParams;
            internal readonly Dictionary<object, SqlParameterInfo> map;
            internal readonly ISqlParameterizer parameterizer;
            internal bool topLevel;
            private int index;

            // Methods
            internal Visitor(ISqlParameterizer parameterizer)
            {
                this.parameterizer = parameterizer;
                this.topLevel = true;
                this.map = new Dictionary<object, SqlParameterInfo>();
                this.currentParams = new List<SqlParameterInfo>();
            }

            internal virtual string CreateParameterName()
            {
                return ("@p" + index++);
            }

            internal static ParameterDirection GetParameterDirection(MetaParameter p)
            {
                if (p.Parameter.IsRetval)
                {
                    return ParameterDirection.ReturnValue;
                }
                if (p.Parameter.IsOut)
                {
                    return ParameterDirection.Output;
                }
                if (p.Parameter.ParameterType.IsByRef)
                {
                    return ParameterDirection.InputOutput;
                }
                return ParameterDirection.Input;
            }

            internal virtual SqlParameter InsertLookup(SqlValue cp)
            {
                SqlParameterInfo info;
                if (!map.TryGetValue(cp, out info))
                {
                    var parameter = new SqlParameter(cp.ClrType, cp.SqlType, CreateParameterName(), cp.SourceExpression);
                    info = new SqlParameterInfo(parameter, cp.Value);
                    map.Add(cp, info);
                    currentParams.Add(info);
                }
                return info.Parameter;
            }

            internal bool RetypeOutParameter(SqlParameter node)
            {
                if (node.SqlType.IsLargeType)
                {
                    IProviderType bestLargeType = this.parameterizer.TypeProvider.GetBestLargeType(node.SqlType);
                    if (node.SqlType != bestLargeType)
                    {
                        node.SetSqlType(bestLargeType);
                        return true;
                    }
                    this.parameterizer.Annotations.Add(node, new SqlServerCompatibilityAnnotation(Strings.MaxSizeNotSupported(node.SourceExpression),
                                                                                                  new[] { SqlProvider.ProviderMode.Sql2000 }));
                }
                return false;
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                base.VisitBinaryOperator(bo);
                return bo;
            }

            internal override SqlExpression VisitClientParameter(SqlClientParameter cp)
            {
                if (cp.SqlType.CanBeParameter)
                {
                    var parameter = new SqlParameter(cp.ClrType, cp.SqlType,
                                                     CreateParameterName(),
                                                     cp.SourceExpression);
                    currentParams.Add(new SqlParameterInfo(parameter, cp.Accessor.Compile()));
                    return parameter;
                }
                return cp;
            }

            internal override SqlStatement VisitDelete(SqlDelete sd)
            {
                bool topLevel = this.topLevel;
                this.topLevel = false;
                base.VisitDelete(sd);
                this.topLevel = topLevel;
                return sd;
            }

            internal override SqlStatement VisitInsert(SqlInsert sin)
            {
                bool topLevel = this.topLevel;
                this.topLevel = false;
                base.VisitInsert(sin);
                this.topLevel = topLevel;
                return sin;
            }

            internal SqlExpression VisitParameter(SqlExpression expr)
            {
                SqlExpression expression = VisitExpression(expr);
                SqlNodeType nodeType = expression.NodeType;
                if (nodeType != SqlNodeType.Parameter)
                {
                    return nodeType == SqlNodeType.Value ? InsertLookup((SqlValue)expression) : expression;
                }
                return expression;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                bool topLevel = this.topLevel;
                this.topLevel = false;
                select.From = (SqlSource)Visit(select.From);
                select.Where = VisitExpression(select.Where);
                int num = 0;
                int count = select.GroupBy.Count;
                while (num < count)
                {
                    select.GroupBy[num] = VisitExpression(select.GroupBy[num]);
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
                select.Top = this.VisitExpression(select.Top);
                select.Row = (SqlRow)this.Visit(select.Row);
                this.topLevel = topLevel;
                select.Selection = this.VisitExpression(select.Selection);
                return select;
            }

            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc)
            {
                var returnType = spc.Function.Method.ReturnType;
                if (returnType != typeof(void))
                {
                    var parameter3 = new SqlParameter(returnType,
                                                  parameterizer.TypeProvider.From(returnType),
                                                  "@RETURN_VALUE", spc.SourceExpression) { Direction = ParameterDirection.Output };
                    this.currentParams.Add(new SqlParameterInfo(parameter3));
                }

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
                        node.Name = p.MappedName.StartsWith("@") ? p.MappedName : "@" + p.MappedName;
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
                //                                  "@RETURN_VALUE", spc.SourceExpression) { Direction = ParameterDirection.Output };
                //currentParams.Add(new SqlParameterInfo(parameter3));

                return spc;
            }

            internal override SqlStatement VisitUpdate(SqlUpdate sup)
            {
                bool topLevel = this.topLevel;
                this.topLevel = false;
                base.VisitUpdate(sup);
                this.topLevel = topLevel;
                return sup;
            }

            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq)
            {
                bool topLevel = this.topLevel;
                this.topLevel = false;
                int num = 0;
                int count = suq.Arguments.Count;
                while (num < count)
                {
                    suq.Arguments[num] = this.VisitParameter(suq.Arguments[num]);
                    num++;
                }
                this.topLevel = topLevel;
                suq.Projection = this.VisitExpression(suq.Projection);
                return suq;
            }

            internal override SqlExpression VisitValue(SqlValue value)
            {
                if ((!this.topLevel && value.IsClientSpecified) && value.SqlType.CanBeParameter)
                {
                    return InsertLookup(value);
                }
                return value;
            }
        }
    }
}