using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using ALinq;
using ALinq.SqlClient;

namespace ALinq.SqlClient
{
    internal class SqlDuplicator
    {
        // Fields
        private DuplicatingVisitor superDuper;

        // Methods
        internal SqlDuplicator()
            : this(true)
        {
        }

        internal SqlDuplicator(bool ignoreExternalRefs)
        {
            this.superDuper = new DuplicatingVisitor(ignoreExternalRefs);
        }

        internal static SqlNode Copy(SqlNode node)
        {
            if (node == null)
            {
                return null;
            }
            SqlNodeType nodeType = node.NodeType;
            if (nodeType != SqlNodeType.ColumnRef)
            {
                switch (nodeType)
                {
                    case SqlNodeType.Variable:
                    case SqlNodeType.Value:
                        return node;

                    case SqlNodeType.Parameter:
                        return node;
                }
                return new SqlDuplicator().Duplicate(node);
            }
            return node;
        }

        internal SqlNode Duplicate(SqlNode node)
        {
            return this.superDuper.Visit(node);
        }

        // Nested Types
        internal class DuplicatingVisitor : SqlVisitor
        {
            // Fields
            private bool ingoreExternalRefs;
            private Dictionary<SqlNode, SqlNode> nodeMap;

            // Methods
            internal DuplicatingVisitor(bool ignoreExternalRefs)
            {
                this.ingoreExternalRefs = ignoreExternalRefs;
                this.nodeMap = new Dictionary<SqlNode, SqlNode>();
            }

            internal override SqlNode Visit(SqlNode node)
            {
                if (node == null)
                {
                    return null;
                }
                SqlNode node2 = null;
                if (!this.nodeMap.TryGetValue(node, out node2))
                {
                    node2 = base.Visit(node);
                    this.nodeMap[node] = node2;
                }
                return node2;
            }

            internal override SqlAlias VisitAlias(SqlAlias a)
            {
                SqlAlias alias = new SqlAlias(a.Node);
                this.nodeMap[a] = alias;
                alias.Node = this.Visit(a.Node);
                alias.Name = a.Name;
                return alias;
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
            {
                if (this.ingoreExternalRefs && !this.nodeMap.ContainsKey(aref.Alias))
                {
                    return aref;
                }
                return new SqlAliasRef((SqlAlias)this.Visit(aref.Alias));
            }

            internal override SqlStatement VisitAssign(SqlAssign sa)
            {
                return new SqlAssign(this.VisitExpression(sa.LValue), this.VisitExpression(sa.RValue), sa.SourceExpression);
            }

            internal override SqlExpression VisitBetween(SqlBetween between)
            {
                return new SqlBetween(between.ClrType, between.SqlType, this.VisitExpression(between.Expression), this.VisitExpression(between.Start), this.VisitExpression(between.End), between.SourceExpression);
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                SqlExpression left = (SqlExpression)this.Visit(bo.Left);
                return new SqlBinary(bo.NodeType, bo.ClrType, bo.SqlType, left, (SqlExpression)this.Visit(bo.Right), bo.Method);
            }

            internal override SqlBlock VisitBlock(SqlBlock block)
            {
                SqlBlock block2 = new SqlBlock(block.SourceExpression);
                foreach (SqlStatement statement in block.Statements)
                {
                    block2.Statements.Add((SqlStatement)this.Visit(statement));
                }
                return block2;
            }

            internal override SqlExpression VisitCast(SqlUnary c)
            {
                return new SqlUnary(SqlNodeType.Cast, c.ClrType, c.SqlType, (SqlExpression)this.Visit(c.Operand), c.SourceExpression);
            }

            internal override SqlExpression VisitClientArray(SqlClientArray scar)
            {
                var exprs = new SqlExpression[scar.Expressions.Count];
                int index = 0;
                int length = exprs.Length;
                while (index < length)
                {
                    exprs[index] = this.VisitExpression(scar.Expressions[index]);
                    index++;
                }
                return new SqlClientArray(scar.ClrType, scar.SqlType, exprs, scar.SourceExpression);
            }

            internal override SqlExpression VisitClientCase(SqlClientCase c)
            {
                SqlExpression expr = this.VisitExpression(c.Expression);
                var whens = new SqlClientWhen[c.Whens.Count];
                int index = 0;
                int length = whens.Length;
                while (index < length)
                {
                    SqlClientWhen when = c.Whens[index];
                    whens[index] = new SqlClientWhen(this.VisitExpression(when.Match), this.VisitExpression(when.Value));
                    index++;
                }
                return new SqlClientCase(c.ClrType, expr, whens, c.SourceExpression);
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                var subquery = (SqlSubSelect)this.VisitExpression(cq.Query);
                var query = new SqlClientQuery(subquery);
                int num = 0;
                int count = cq.Arguments.Count;
                while (num < count)
                {
                    query.Arguments.Add(this.VisitExpression(cq.Arguments[num]));
                    num++;
                }
                int num3 = 0;
                int num4 = cq.Parameters.Count;
                while (num3 < num4)
                {
                    query.Parameters.Add((SqlParameter)this.VisitExpression(cq.Parameters[num3]));
                    num3++;
                }
                return query;
            }

            internal override SqlExpression VisitColumn(SqlColumn col)
            {
                SqlColumn column;

                //if(col is SqlDynamicColumn)
                //{
                //    column = new SqlDynamicColumn(col.ClrType, col.SqlType, col.Name, col.MetaMember, null, col.SourceExpression);
                //}
                //else
                //{
                Debug.Assert(col.GetType() == typeof(SqlColumn));
                column = new SqlColumn(col.ClrType, col.SqlType, col.Name, col.MetaMember, null, col.SourceExpression);
                //}
                this.nodeMap[col] = column;
                column.Expression = this.VisitExpression(col.Expression);
                column.Alias = (SqlAlias)this.Visit(col.Alias);
                return column;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                if (this.ingoreExternalRefs && !this.nodeMap.ContainsKey(cref.Column))
                {
                    return cref;
                }
                return new SqlColumnRef((SqlColumn)this.Visit(cref.Column));
            }

            internal override SqlStatement VisitDelete(SqlDelete sd)
            {
                return new SqlDelete((SqlSelect)this.Visit(sd.Select), sd.SourceExpression);
            }

            internal override SqlExpression VisitDiscriminatedType(SqlDiscriminatedType dt)
            {
                return new SqlDiscriminatedType(dt.SqlType, this.VisitExpression(dt.Discriminator), dt.TargetType, dt.SourceExpression);
            }

            internal override SqlExpression VisitDiscriminatorOf(SqlDiscriminatorOf dof)
            {
                return new SqlDiscriminatorOf(this.VisitExpression(dof.Object), dof.ClrType, dof.SqlType, dof.SourceExpression);
            }

            internal override SqlExpression VisitDoNotVisit(SqlDoNotVisitExpression expr)
            {
                return new SqlDoNotVisitExpression(this.VisitExpression(expr.Expression));
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem)
            {
                return this.VisitMultiset(elem);
            }

            internal override SqlExpression VisitExists(SqlSubSelect sqlExpr)
            {
                return new SqlSubSelect(sqlExpr.NodeType, sqlExpr.ClrType, sqlExpr.SqlType, (SqlSelect)this.Visit(sqlExpr.Select));
            }

            internal override SqlExpression VisitExprSet(SqlExprSet xs)
            {
                SqlExpression[] exprs = new SqlExpression[xs.Expressions.Count];
                int index = 0;
                int length = exprs.Length;
                while (index < length)
                {
                    exprs[index] = this.VisitExpression(xs.Expressions[index]);
                    index++;
                }
                return new SqlExprSet(xs.ClrType, exprs, xs.SourceExpression);
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                var args = new SqlExpression[fc.Arguments.Count];
                int index = 0;
                int count = fc.Arguments.Count;
                while (index < count)
                {
                    args[index] = VisitExpression(fc.Arguments[index]);
                    index++;
                }
                return new SqlFunctionCall(fc.ClrType, fc.SqlType, fc.Name, args, fc.SourceExpression)
                           {
                               Comma = fc.Comma
                           };
            }

            internal override SqlExpression VisitGrouping(SqlGrouping g)
            {
                return new SqlGrouping(g.ClrType, g.SqlType, this.VisitExpression(g.Key), this.VisitExpression(g.Group), g.SourceExpression);
            }

            internal override SqlExpression VisitIn(SqlIn sin)
            {
                SqlIn @in = new SqlIn(sin.ClrType, sin.SqlType, this.VisitExpression(sin.Expression), sin.Values, sin.SourceExpression);
                int num = 0;
                int count = @in.Values.Count;
                while (num < count)
                {
                    @in.Values[num] = this.VisitExpression(@in.Values[num]);
                    num++;
                }
                return @in;
            }

            internal override SqlNode VisitIncludeScope(SqlIncludeScope scope)
            {
                return new SqlIncludeScope(this.Visit(scope.Child), scope.SourceExpression);
            }

            internal override SqlStatement VisitInsert(SqlInsert si)
            {
                SqlInsert insert = new SqlInsert(si.Table, this.VisitExpression(si.Expression), si.SourceExpression);
                insert.OutputKey = si.OutputKey;
                insert.OutputToLocal = si.OutputToLocal;
                insert.Row = this.VisitRow(si.Row);
                return insert;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                SqlSource left = this.VisitSource(join.Left);
                SqlSource right = this.VisitSource(join.Right);
                return new SqlJoin(join.JoinType, left, right, (SqlExpression)this.Visit(join.Condition), join.SourceExpression);
            }

            internal override SqlExpression VisitJoinedCollection(SqlJoinedCollection jc)
            {
                return new SqlJoinedCollection(jc.ClrType, jc.SqlType, this.VisitExpression(jc.Expression), this.VisitExpression(jc.Count), jc.SourceExpression);
            }

            internal override SqlExpression VisitLift(SqlLift lift)
            {
                return new SqlLift(lift.ClrType, this.VisitExpression(lift.Expression), lift.SourceExpression);
            }

            internal override SqlExpression VisitLike(SqlLike like)
            {
                return new SqlLike(like.ClrType, like.SqlType, this.VisitExpression(like.Expression), this.VisitExpression(like.Pattern), this.VisitExpression(like.Escape), like.SourceExpression);
            }

            internal override SqlNode VisitLink(SqlLink link)
            {
                SqlExpression[] keyExpressions = new SqlExpression[link.KeyExpressions.Count];
                int index = 0;
                int length = keyExpressions.Length;
                while (index < length)
                {
                    keyExpressions[index] = this.VisitExpression(link.KeyExpressions[index]);
                    index++;
                }
                SqlLink link2 = new SqlLink(new object(), link.RowType, link.ClrType, link.SqlType, null, link.Member, keyExpressions, null, link.SourceExpression);
                this.nodeMap[link] = link2;
                link2.Expression = this.VisitExpression(link.Expression);
                link2.Expansion = this.VisitExpression(link.Expansion);
                return link2;
            }

            protected override SqlNode VisitMember(SqlMember m)
            {
                return new SqlMember(m.ClrType, m.SqlType, (SqlExpression)this.Visit(m.Expression), m.Member);
            }

            internal override SqlMemberAssign VisitMemberAssign(SqlMemberAssign ma)
            {
                return new SqlMemberAssign(ma.Member, (SqlExpression)this.Visit(ma.Expression));
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
            {
                SqlExpression[] args = new SqlExpression[mc.Arguments.Count];
                int index = 0;
                int count = mc.Arguments.Count;
                while (index < count)
                {
                    args[index] = this.VisitExpression(mc.Arguments[index]);
                    index++;
                }
                return new SqlMethodCall(mc.ClrType, mc.SqlType, mc.Method, this.VisitExpression(mc.Object), args, mc.SourceExpression);
            }

            internal override SqlExpression VisitMultiset(SqlSubSelect sms)
            {
                return new SqlSubSelect(sms.NodeType, sms.ClrType, sms.SqlType, (SqlSelect)this.Visit(sms.Select));
            }

            internal override SqlExpression VisitNew(SqlNew sox)
            {
                SqlExpression[] args = new SqlExpression[sox.Args.Count];
                SqlMemberAssign[] members = new SqlMemberAssign[sox.Members.Count];
                int index = 0;
                int length = args.Length;
                while (index < length)
                {
                    args[index] = this.VisitExpression(sox.Args[index]);
                    index++;
                }
                int num3 = 0;
                int num4 = members.Length;
                while (num3 < num4)
                {
                    members[num3] = this.VisitMemberAssign(sox.Members[num3]);
                    num3++;
                }
                return new SqlNew(sox.MetaType, sox.SqlType, sox.Constructor, args, sox.ArgMembers, members, sox.SourceExpression);
            }

            internal override SqlExpression VisitObjectType(SqlObjectType ot)
            {
                return new SqlObjectType(this.VisitExpression(ot.Object), ot.SqlType, ot.SourceExpression);
            }

            internal override SqlExpression VisitOptionalValue(SqlOptionalValue sov)
            {
                SqlExpression hasValue = this.VisitExpression(sov.HasValue);
                return new SqlOptionalValue(hasValue, this.VisitExpression(sov.Value));
            }

            internal override SqlExpression VisitParameter(SqlParameter p)
            {
                SqlParameter parameter = new SqlParameter(p.ClrType, p.SqlType, p.Name, p.SourceExpression);
                parameter.Direction = p.Direction;
                return parameter;
            }

            internal override SqlRow VisitRow(SqlRow row)
            {
                SqlRow row2 = new SqlRow(row.SourceExpression);
                foreach (SqlColumn column in row.Columns)
                {
                    row2.Columns.Add((SqlColumn)this.Visit(column));
                }
                return row2;
            }

            internal override SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber)
            {
                List<SqlOrderExpression> orderByList = new List<SqlOrderExpression>();
                foreach (SqlOrderExpression expression in rowNumber.OrderBy)
                {
                    orderByList.Add(new SqlOrderExpression(expression.OrderType, (SqlExpression)this.Visit(expression.Expression)));
                }
                return new SqlRowNumber(rowNumber.ClrType, rowNumber.SqlType, orderByList, rowNumber.SourceExpression);
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                return new SqlSubSelect(SqlNodeType.ScalarSubSelect, ss.ClrType, ss.SqlType, this.VisitSequence(ss.Select));
            }

            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c)
            {
                SqlExpression @else = this.VisitExpression(c.Else);
                SqlWhen[] whens = new SqlWhen[c.Whens.Count];
                int index = 0;
                int length = whens.Length;
                while (index < length)
                {
                    SqlWhen when = c.Whens[index];
                    whens[index] = new SqlWhen(this.VisitExpression(when.Match), this.VisitExpression(when.Value));
                    index++;
                }
                return new SqlSearchedCase(c.ClrType, whens, @else, c.SourceExpression);
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                SqlSource from = this.VisitSource(select.From);
                List<SqlExpression> collection = null;
                if (select.GroupBy.Count > 0)
                {
                    collection = new List<SqlExpression>(select.GroupBy.Count);
                    foreach (SqlExpression expression in select.GroupBy)
                    {
                        collection.Add((SqlExpression)this.Visit(expression));
                    }
                }
                SqlExpression expression2 = (SqlExpression)this.Visit(select.Having);
                List<SqlOrderExpression> list2 = null;
                if (select.OrderBy.Count > 0)
                {
                    list2 = new List<SqlOrderExpression>(select.OrderBy.Count);
                    foreach (SqlOrderExpression expression3 in select.OrderBy)
                    {
                        SqlOrderExpression item = new SqlOrderExpression(expression3.OrderType, (SqlExpression)this.Visit(expression3.Expression));
                        list2.Add(item);
                    }
                }
                SqlExpression expression5 = (SqlExpression)this.Visit(select.Top);
                SqlExpression expression6 = (SqlExpression)this.Visit(select.Where);
                SqlRow row = (SqlRow)this.Visit(select.Row);
                SqlSelect select2 = new SqlSelect(this.VisitExpression(select.Selection), from, select.SourceExpression);
                if (collection != null)
                {
                    select2.GroupBy.AddRange(collection);
                }
                select2.Having = expression2;
                if (list2 != null)
                {
                    select2.OrderBy.AddRange(list2);
                }
                select2.OrderingType = select.OrderingType;
                select2.Row = row;
                select2.Top = expression5;
                select2.IsDistinct = select.IsDistinct;
                select2.IsPercent = select.IsPercent;
                select2.Where = expression6;
                select2.DoNotOutput = select.DoNotOutput;
                return select2;
            }

            internal override SqlExpression VisitSharedExpression(SqlSharedExpression sub)
            {
                SqlSharedExpression expression = new SqlSharedExpression(sub.Expression);
                this.nodeMap[sub] = expression;
                expression.Expression = this.VisitExpression(sub.Expression);
                return expression;
            }

            internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref)
            {
                if (this.ingoreExternalRefs && !this.nodeMap.ContainsKey(sref.SharedExpression))
                {
                    return sref;
                }
                return new SqlSharedExpressionRef((SqlSharedExpression)this.Visit(sref.SharedExpression));
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c)
            {
                SqlExpression expr = this.VisitExpression(c.Expression);
                SqlWhen[] whens = new SqlWhen[c.Whens.Count];
                int index = 0;
                int length = whens.Length;
                while (index < length)
                {
                    SqlWhen when = c.Whens[index];
                    whens[index] = new SqlWhen(this.VisitExpression(when.Match), this.VisitExpression(when.Value));
                    index++;
                }
                return new SqlSimpleCase(c.ClrType, expr, whens, c.SourceExpression);
            }

            internal override SqlExpression VisitSimpleExpression(SqlSimpleExpression simple)
            {
                return new SqlSimpleExpression(this.VisitExpression(simple.Expression));
            }

            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc)
            {
                List<SqlExpression> args = new List<SqlExpression>(spc.Arguments.Count);
                foreach (SqlExpression expression in spc.Arguments)
                {
                    args.Add(this.VisitExpression(expression));
                }
                SqlExpression projection = this.VisitExpression(spc.Projection);
                SqlStoredProcedureCall call = new SqlStoredProcedureCall(spc.Function, projection, args, spc.SourceExpression);
                this.nodeMap[spc] = call;
                foreach (SqlUserColumn column in spc.Columns)
                {
                    call.Columns.Add((SqlUserColumn)this.Visit(column));
                }
                return call;
            }

            internal override SqlTable VisitTable(SqlTable tab)
            {
                SqlTable table = new SqlTable(tab.MetaTable, tab.RowType, tab.SqlRowType, tab.SourceExpression);
                this.nodeMap[tab] = table;
                foreach (SqlColumn column in tab.Columns)
                {
                    table.Columns.Add((SqlColumn)this.Visit(column));
                }
                return table;
            }

            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc)
            {
                SqlExpression[] args = new SqlExpression[fc.Arguments.Count];
                int index = 0;
                int count = fc.Arguments.Count;
                while (index < count)
                {
                    args[index] = this.VisitExpression(fc.Arguments[index]);
                    index++;
                }
                SqlTableValuedFunctionCall call = new SqlTableValuedFunctionCall(fc.RowType, fc.ClrType, fc.SqlType, fc.Name, args, fc.SourceExpression);
                this.nodeMap[fc] = call;
                foreach (SqlColumn column in fc.Columns)
                {
                    call.Columns.Add((SqlColumn)this.Visit(column));
                }
                return call;
            }

            internal override SqlExpression VisitTreat(SqlUnary t)
            {
                return new SqlUnary(SqlNodeType.Treat, t.ClrType, t.SqlType, (SqlExpression)this.Visit(t.Operand), t.SourceExpression);
            }

            internal override SqlExpression VisitTypeCase(SqlTypeCase tc)
            {
                SqlExpression discriminator = this.VisitExpression(tc.Discriminator);
                var whens = new List<SqlTypeCaseWhen>();
                foreach (SqlTypeCaseWhen when in tc.Whens)
                {
                    whens.Add(new SqlTypeCaseWhen(this.VisitExpression(when.Match), VisitExpression(when.TypeBinding)));
                }
                return new SqlTypeCase(tc.ClrType, tc.SqlType, tc.RowType, discriminator, whens, tc.SourceExpression);
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                return new SqlUnary(uo.NodeType, uo.ClrType, uo.SqlType, (SqlExpression)this.Visit(uo.Operand), uo.Method, uo.SourceExpression);
            }

            internal override SqlNode VisitUnion(SqlUnion su)
            {
                return new SqlUnion(this.Visit(su.Left), this.Visit(su.Right), su.All);
            }

            internal override SqlStatement VisitUpdate(SqlUpdate su)
            {
                SqlSelect select = (SqlSelect)this.Visit(su.Select);
                List<SqlAssign> assignments = new List<SqlAssign>(su.Assignments.Count);
                foreach (SqlAssign assign in su.Assignments)
                {
                    assignments.Add((SqlAssign)this.Visit(assign));
                }
                return new SqlUpdate(select, assignments, su.SourceExpression);
            }

            internal override SqlExpression VisitUserColumn(SqlUserColumn suc)
            {
                if (this.ingoreExternalRefs && !this.nodeMap.ContainsKey(suc))
                {
                    return suc;
                }
                return new SqlUserColumn(suc.ClrType, suc.SqlType, suc.Query, suc.Name, suc.IsRequired, suc.SourceExpression);
            }

            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq)
            {
                List<SqlExpression> args = new List<SqlExpression>(suq.Arguments.Count);
                foreach (SqlExpression expression in suq.Arguments)
                {
                    args.Add(this.VisitExpression(expression));
                }
                SqlExpression projection = this.VisitExpression(suq.Projection);
                SqlUserQuery query = new SqlUserQuery(suq.QueryText, projection, args, suq.SourceExpression);
                this.nodeMap[suq] = query;
                foreach (SqlUserColumn column in suq.Columns)
                {
                    SqlUserColumn item = new SqlUserColumn(column.ClrType, column.SqlType, column.Query, column.Name, column.IsRequired, column.SourceExpression);
                    this.nodeMap[column] = item;
                    query.Columns.Add(item);
                }
                return query;
            }

            internal override SqlExpression VisitUserRow(SqlUserRow row)
            {
                return new SqlUserRow(row.RowType, row.SqlType, (SqlUserQuery)this.Visit(row.Query), row.SourceExpression);
            }

            internal override SqlExpression VisitValue(SqlValue value)
            {
                return value;
            }

            internal override SqlExpression VisitVariable(SqlVariable v)
            {
                return v;
            }
        }
    }



}