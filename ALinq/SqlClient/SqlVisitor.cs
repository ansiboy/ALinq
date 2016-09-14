using System;
using System.Diagnostics;

namespace ALinq.SqlClient
{
    internal abstract class SqlVisitor
    {
        // Fields
        private int nDepth;

        // Methods
        protected SqlVisitor()
        {
        }

        public PostBindDotNetConverter PostBindDotNetConverter { get; set; }

        [Conditional("DEBUG")]
        internal static void CheckRecursionDepth(int maxLevel, int level)
        {
            if (level > maxLevel)
            {
                throw new Exception("Infinite Descent?");
            }
        }

        internal static object Eval(SqlExpression expr)
        {
            if (expr.NodeType != SqlNodeType.Value)
            {
                throw Error.UnexpectedNode(expr.NodeType);
            }
            return ((SqlValue)expr).Value;
        }

        internal bool RefersToColumn(SqlExpression exp, SqlColumn col)
        {
            if (exp != null)
            {
                switch (exp.NodeType)
                {
                    case SqlNodeType.Column:
                        return ((exp == col) || this.RefersToColumn(((SqlColumn)exp).Expression, col));

                    case SqlNodeType.ColumnRef:
                        {
                            var ref2 = (SqlColumnRef)exp;
                            return ((ref2.Column == col) || this.RefersToColumn(ref2.Column.Expression, col));
                        }
                    case SqlNodeType.ExprSet:
                        {
                            var set = (SqlExprSet)exp;
                            int num = 0;
                            int count = set.Expressions.Count;
                            while (num < count)
                            {
                                if (this.RefersToColumn(set.Expressions[num], col))
                                {
                                    return true;
                                }
                                num++;
                            }
                            break;
                        }
                    case SqlNodeType.OuterJoinedValue:
                        return this.RefersToColumn(((SqlUnary)exp).Operand, col);
                }
            }
            return false;
        }

        internal virtual SqlNode Visit(SqlNode node)
        {
            if (node == null)
            {
                return null;
            }
            try
            {
                this.nDepth++;
                switch (node.NodeType)
                {
                    case SqlNodeType.Add:
                    case SqlNodeType.And:
                    case SqlNodeType.BitAnd:
                    case SqlNodeType.BitOr:
                    case SqlNodeType.BitXor:
                    case SqlNodeType.Coalesce:
                    case SqlNodeType.Concat:
                    case SqlNodeType.Div:
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                    case SqlNodeType.LE:
                    case SqlNodeType.LT:
                    case SqlNodeType.GE:
                    case SqlNodeType.GT:
                    case SqlNodeType.Mod:
                    case SqlNodeType.Mul:
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                    case SqlNodeType.Or:
                    case SqlNodeType.Sub:
                        return this.VisitBinaryOperator((SqlBinary)node);

                    case SqlNodeType.Alias:
                        return this.VisitAlias((SqlAlias)node);

                    case SqlNodeType.AliasRef:
                        return this.VisitAliasRef((SqlAliasRef)node);

                    case SqlNodeType.Assign:
                        return this.VisitAssign((SqlAssign)node);

                    case SqlNodeType.Avg:
                    case SqlNodeType.BitNot:
                    case SqlNodeType.ClrLength:
                    case SqlNodeType.Convert:
                    case SqlNodeType.Count:
                    case SqlNodeType.Covar:
                    case SqlNodeType.IsNotNull:
                    case SqlNodeType.IsNull:
                    case SqlNodeType.LongCount:
                    case SqlNodeType.Max:
                    case SqlNodeType.Min:
                    case SqlNodeType.Negate:
                    case SqlNodeType.Not:
                    case SqlNodeType.Not2V:
                    case SqlNodeType.OuterJoinedValue:
                    case SqlNodeType.Stddev:
                    case SqlNodeType.Sum:
                    case SqlNodeType.ValueOf:
                        return this.VisitUnaryOperator((SqlUnary)node);

                    case SqlNodeType.Between:
                        return this.VisitBetween((SqlBetween)node);

                    case SqlNodeType.Block:
                        return this.VisitBlock((SqlBlock)node);

                    case SqlNodeType.Cast:
                        return this.VisitCast((SqlUnary)node);

                    case SqlNodeType.ClientArray:
                        return this.VisitClientArray((SqlClientArray)node);

                    case SqlNodeType.ClientCase:
                        return this.VisitClientCase((SqlClientCase)node);

                    case SqlNodeType.ClientParameter:
                        return this.VisitClientParameter((SqlClientParameter)node);

                    case SqlNodeType.ClientQuery:
                        return this.VisitClientQuery((SqlClientQuery)node);

                    case SqlNodeType.Column:
                        return this.VisitColumn((SqlColumn)node);

                    case SqlNodeType.ColumnRef:
                        var result = this.VisitColumnRef((SqlColumnRef)node);
                        return result;
                    case SqlNodeType.Delete:
                        return this.VisitDelete((SqlDelete)node);

                    case SqlNodeType.DiscriminatedType:
                        return this.VisitDiscriminatedType((SqlDiscriminatedType)node);

                    case SqlNodeType.DiscriminatorOf:
                        return this.VisitDiscriminatorOf((SqlDiscriminatorOf)node);

                    case SqlNodeType.DoNotVisit:
                        return this.VisitDoNotVisit((SqlDoNotVisitExpression)node);

                    case SqlNodeType.Element:
                    case SqlNodeType.Exists:
                    case SqlNodeType.Multiset:
                    case SqlNodeType.ScalarSubSelect:
                        return this.VisitSubSelect((SqlSubSelect)node);

                    case SqlNodeType.ExprSet:
                        return this.VisitExprSet((SqlExprSet)node);

                    case SqlNodeType.FunctionCall:
                        return this.VisitFunctionCall((SqlFunctionCall)node);

                    case SqlNodeType.In:
                        return this.VisitIn((SqlIn)node);

                    case SqlNodeType.IncludeScope:
                        return this.VisitIncludeScope((SqlIncludeScope)node);

                    case SqlNodeType.Lift:
                        return this.VisitLift((SqlLift)node);

                    case SqlNodeType.Link:
                        return this.VisitLink((SqlLink)node);

                    case SqlNodeType.Like:
                        return this.VisitLike((SqlLike)node);

                    case SqlNodeType.Grouping:
                        return this.VisitGrouping((SqlGrouping)node);

                    case SqlNodeType.Insert:
                        return this.VisitInsert((SqlInsert)node);

                    case SqlNodeType.Join:
                        return this.VisitJoin((SqlJoin)node);

                    case SqlNodeType.JoinedCollection:
                        return this.VisitJoinedCollection((SqlJoinedCollection)node);

                    case SqlNodeType.MethodCall:
                        return this.VisitMethodCall((SqlMethodCall)node);

                    case SqlNodeType.Member:
                        return this.VisitMember((SqlMember)node);

                    case SqlNodeType.MemberAssign:
                        return this.VisitMemberAssign((SqlMemberAssign)node);

                    case SqlNodeType.New:
                        return this.VisitNew((SqlNew)node);

                    case SqlNodeType.Nop:
                        return this.VisitNop((SqlNop)node);

                    case SqlNodeType.ObjectType:
                        return this.VisitObjectType((SqlObjectType)node);

                    case SqlNodeType.OptionalValue:
                        return this.VisitOptionalValue((SqlOptionalValue)node);

                    case SqlNodeType.Parameter:
                        return this.VisitParameter((SqlParameter)node);

                    case SqlNodeType.Row:
                        return this.VisitRow((SqlRow)node);

                    case SqlNodeType.RowNumber:
                        return this.VisitRowNumber((SqlRowNumber)node);

                    case SqlNodeType.SearchedCase:
                        return this.VisitSearchedCase((SqlSearchedCase)node);

                    case SqlNodeType.Select:
                        return this.VisitSelect((SqlSelect)node);

                    case SqlNodeType.SharedExpression:
                        return this.VisitSharedExpression((SqlSharedExpression)node);

                    case SqlNodeType.SharedExpressionRef:
                        return this.VisitSharedExpressionRef((SqlSharedExpressionRef)node);

                    case SqlNodeType.SimpleCase:
                        return this.VisitSimpleCase((SqlSimpleCase)node);

                    case SqlNodeType.SimpleExpression:
                        return this.VisitSimpleExpression((SqlSimpleExpression)node);

                    case SqlNodeType.StoredProcedureCall:
                        return this.VisitStoredProcedureCall((SqlStoredProcedureCall)node);

                    case SqlNodeType.Table:
                        return this.VisitTable((SqlTable)node);

                    case SqlNodeType.TableValuedFunctionCall:
                        return this.VisitTableValuedFunctionCall((SqlTableValuedFunctionCall)node);

                    case SqlNodeType.Treat:
                        return this.VisitTreat((SqlUnary)node);

                    case SqlNodeType.TypeCase:
                        return this.VisitTypeCase((SqlTypeCase)node);

                    case SqlNodeType.Union:
                        return this.VisitUnion((SqlUnion)node);

                    case SqlNodeType.Update:
                        return this.VisitUpdate((SqlUpdate)node);

                    case SqlNodeType.UserColumn:
                        return this.VisitUserColumn((SqlUserColumn)node);

                    case SqlNodeType.UserQuery:
                        return this.VisitUserQuery((SqlUserQuery)node);

                    case SqlNodeType.UserRow:
                        return this.VisitUserRow((SqlUserRow)node);

                    case SqlNodeType.Variable:
                        return this.VisitVariable((SqlVariable)node);

                    case SqlNodeType.Value:
                        return this.VisitValue((SqlValue)node);
                }
                throw Error.UnexpectedNode(node);
            }
            finally
            {
                this.nDepth--;
            }
        }

        internal virtual SqlAlias VisitAlias(SqlAlias a)
        {
            a.Node = this.Visit(a.Node);
            return a;
        }

        internal virtual SqlExpression VisitAliasRef(SqlAliasRef aref)
        {
            return aref;
        }

        internal virtual SqlStatement VisitAssign(SqlAssign sa)
        {
            sa.LValue = this.VisitExpression(sa.LValue);
            sa.RValue = this.VisitExpression(sa.RValue);
            return sa;
        }

        internal virtual SqlExpression VisitBetween(SqlBetween between)
        {
            between.Expression = this.VisitExpression(between.Expression);
            between.Start = this.VisitExpression(between.Start);
            between.End = this.VisitExpression(between.End);
            return between;
        }

        internal virtual SqlExpression VisitBinaryOperator(SqlBinary bo)
        {
            bo.Left = this.VisitExpression(bo.Left);
            bo.Right = this.VisitExpression(bo.Right);
            return bo;
        }

        internal virtual SqlBlock VisitBlock(SqlBlock b)
        {
            int num = 0;
            int count = b.Statements.Count;
            while (num < count)
            {
                b.Statements[num] = (SqlStatement)this.Visit(b.Statements[num]);
                num++;
            }
            return b;
        }

        internal virtual SqlExpression VisitCast(SqlUnary c)
        {
            c.Operand = this.VisitExpression(c.Operand);
            return c;
        }

        internal virtual SqlExpression VisitClientArray(SqlClientArray scar)
        {
            int num = 0;
            int count = scar.Expressions.Count;
            while (num < count)
            {
                scar.Expressions[num] = this.VisitExpression(scar.Expressions[num]);
                num++;
            }
            return scar;
        }

        internal virtual SqlExpression VisitClientCase(SqlClientCase c)
        {
            c.Expression = this.VisitExpression(c.Expression);
            int num = 0;
            int count = c.Whens.Count;
            while (num < count)
            {
                SqlClientWhen when = c.Whens[num];
                when.Match = this.VisitExpression(when.Match);
                when.Value = this.VisitExpression(when.Value);
                num++;
            }
            return c;
        }

        internal virtual SqlExpression VisitClientParameter(SqlClientParameter cp)
        {
            return cp;
        }

        internal virtual SqlExpression VisitClientQuery(SqlClientQuery cq)
        {
            int num = 0;
            int count = cq.Arguments.Count;
            while (num < count)
            {
                cq.Arguments[num] = this.VisitExpression(cq.Arguments[num]);
                num++;
            }
            return cq;
        }

        internal virtual SqlExpression VisitColumn(SqlColumn col)
        {
            col.Expression = this.VisitExpression(col.Expression);
            return col;
        }

        internal virtual SqlExpression VisitColumnRef(SqlColumnRef cref)
        {
            return cref;
        }

        internal virtual SqlStatement VisitDelete(SqlDelete delete)
        {
            delete.Select = this.VisitSequence(delete.Select);
            return delete;
        }

        internal virtual SqlExpression VisitDiscriminatedType(SqlDiscriminatedType dt)
        {
            dt.Discriminator = this.VisitExpression(dt.Discriminator);
            return dt;
        }

        internal virtual SqlExpression VisitDiscriminatorOf(SqlDiscriminatorOf dof)
        {
            dof.Object = this.VisitExpression(dof.Object);
            return dof;
        }

        internal virtual SqlExpression VisitDoNotVisit(SqlDoNotVisitExpression expr)
        {
            return expr.Expression;
        }

        internal virtual SqlExpression VisitElement(SqlSubSelect elem)
        {
            elem.Select = this.VisitSequence(elem.Select);
            return elem;
        }

        internal virtual SqlExpression VisitExists(SqlSubSelect sqlExpr)
        {
            sqlExpr.Select = this.VisitSequence(sqlExpr.Select);
            return sqlExpr;
        }

        internal virtual SqlExpression VisitExpression(SqlExpression exp)
        {
            return (SqlExpression)this.Visit(exp);
        }

        internal virtual SqlExpression VisitExprSet(SqlExprSet xs)
        {
            int num = 0;
            int count = xs.Expressions.Count;
            while (num < count)
            {
                xs.Expressions[num] = this.VisitExpression(xs.Expressions[num]);
                num++;
            }
            return xs;
        }

        internal virtual SqlExpression VisitFunctionCall(SqlFunctionCall fc)
        {
            int num = 0;
            int count = fc.Arguments.Count;
            while (num < count)
            {
                fc.Arguments[num] = this.VisitExpression(fc.Arguments[num]);
                num++;
            }
            return fc;
        }

        internal virtual SqlExpression VisitGrouping(SqlGrouping g)
        {
            g.Key = this.VisitExpression(g.Key);
            g.Group = this.VisitExpression(g.Group);
            return g;
        }

        internal virtual SqlExpression VisitIn(SqlIn sin)
        {
            sin.Expression = this.VisitExpression(sin.Expression);
            int num = 0;
            int count = sin.Values.Count;
            while (num < count)
            {
                sin.Values[num] = this.VisitExpression(sin.Values[num]);
                num++;
            }
            return sin;
        }

        internal virtual SqlNode VisitIncludeScope(SqlIncludeScope node)
        {
            node.Child = this.Visit(node.Child);
            return node;
        }

        internal virtual SqlStatement VisitInsert(SqlInsert insert)
        {
            insert.Table = (SqlTable)this.Visit(insert.Table);
            insert.Expression = this.VisitExpression(insert.Expression);
            insert.Row = (SqlRow)this.Visit(insert.Row);
            return insert;
        }

        internal virtual SqlSource VisitJoin(SqlJoin join)
        {
            join.Left = this.VisitSource(join.Left);
            join.Right = this.VisitSource(join.Right);
            join.Condition = this.VisitExpression(join.Condition);
            return join;
        }

        internal virtual SqlExpression VisitJoinedCollection(SqlJoinedCollection jc)
        {
            jc.Expression = this.VisitExpression(jc.Expression);
            jc.Count = this.VisitExpression(jc.Count);
            return jc;
        }

        internal virtual SqlExpression VisitLift(SqlLift lift)
        {
            lift.Expression = this.VisitExpression(lift.Expression);
            return lift;
        }

        internal virtual SqlExpression VisitLike(SqlLike like)
        {
            like.Expression = this.VisitExpression(like.Expression);
            like.Pattern = this.VisitExpression(like.Pattern);
            like.Escape = this.VisitExpression(like.Escape);
            return like;
        }

        internal virtual SqlNode VisitLink(SqlLink link)
        {
            int num = 0;
            int count = link.KeyExpressions.Count;
            while (num < count)
            {
                link.KeyExpressions[num] = this.VisitExpression(link.KeyExpressions[num]);
                num++;
            }
            return link;
        }

        protected virtual SqlNode VisitMember(SqlMember m)
        {
            m.Expression = this.VisitExpression(m.Expression);
            return m;
        }

        internal virtual SqlMemberAssign VisitMemberAssign(SqlMemberAssign ma)
        {
            ma.Expression = this.VisitExpression(ma.Expression);
            return ma;
        }

        internal virtual SqlExpression VisitMethodCall(SqlMethodCall mc)
        {
            mc.Object = this.VisitExpression(mc.Object);
            int num = 0;
            int count = mc.Arguments.Count;
            while (num < count)
            {
                mc.Arguments[num] = this.VisitExpression(mc.Arguments[num]);
                num++;
            }
            return mc;
        }

        internal virtual SqlExpression VisitMultiset(SqlSubSelect sms)
        {
            sms.Select = this.VisitSequence(sms.Select);
            return sms;
        }

        internal virtual SqlExpression VisitNew(SqlNew sox)
        {
            int num = 0;
            int count = sox.Args.Count;
            while (num < count)
            {
                sox.Args[num] = this.VisitExpression(sox.Args[num]);
                num++;
            }
            int num3 = 0;
            int num4 = sox.Members.Count;
            while (num3 < num4)
            {
                sox.Members[num3].Expression = this.VisitExpression(sox.Members[num3].Expression);
                num3++;
            }
            return sox;
        }

        internal virtual SqlExpression VisitNop(SqlNop nop)
        {
            return nop;
        }

        internal virtual SqlExpression VisitObjectType(SqlObjectType ot)
        {
            ot.Object = this.VisitExpression(ot.Object);
            return ot;
        }

        internal virtual SqlExpression VisitOptionalValue(SqlOptionalValue sov)
        {
            sov.HasValue = this.VisitExpression(sov.HasValue);
            sov.Value = this.VisitExpression(sov.Value);
            return sov;
        }

        internal virtual SqlExpression VisitParameter(SqlParameter p)
        {
            return p;
        }

        internal virtual SqlRow VisitRow(SqlRow row)
        {
            int num = 0;
            int count = row.Columns.Count;
            while (num < count)
            {
                row.Columns[num].Expression = this.VisitExpression(row.Columns[num].Expression);
                num++;
            }
            return row;
        }

        internal virtual SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber)
        {
            int num = 0;
            int count = rowNumber.OrderBy.Count;
            while (num < count)
            {
                rowNumber.OrderBy[num].Expression = this.VisitExpression(rowNumber.OrderBy[num].Expression);
                num++;
            }
            return rowNumber;
        }

        internal virtual SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
        {
            ss.Select = this.VisitSequence(ss.Select);
            return ss;
        }

        internal virtual SqlExpression VisitSearchedCase(SqlSearchedCase c)
        {
            int num = 0;
            int count = c.Whens.Count;
            while (num < count)
            {
                SqlWhen when = c.Whens[num];
                when.Match = this.VisitExpression(when.Match);
                when.Value = this.VisitExpression(when.Value);
                num++;
            }
            c.Else = this.VisitExpression(c.Else);
            return c;
        }

        internal virtual SqlSelect VisitSelect(SqlSelect select)
        {
            select = this.VisitSelectCore(select);
            select.Selection = this.VisitExpression(select.Selection);
            return select;
        }

        internal virtual SqlSelect VisitSelectCore(SqlSelect select)
        {
            select.From = this.VisitSource(select.From);
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
            select.Top = this.VisitExpression(select.Top);
            select.Row = (SqlRow)this.Visit(select.Row);
            return select;
        }

        internal virtual SqlSelect VisitSequence(SqlSelect sel)
        {
            return (SqlSelect)this.Visit(sel);
        }

        internal virtual SqlExpression VisitSharedExpression(SqlSharedExpression shared)
        {
            shared.Expression = this.VisitExpression(shared.Expression);
            return shared;
        }

        internal virtual SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref)
        {
            return sref;
        }

        internal virtual SqlExpression VisitSimpleCase(SqlSimpleCase c)
        {
            c.Expression = this.VisitExpression(c.Expression);
            int num = 0;
            int count = c.Whens.Count;
            while (num < count)
            {
                SqlWhen when = c.Whens[num];
                when.Match = this.VisitExpression(when.Match);
                when.Value = this.VisitExpression(when.Value);
                num++;
            }
            return c;
        }

        internal virtual SqlExpression VisitSimpleExpression(SqlSimpleExpression simple)
        {
            simple.Expression = this.VisitExpression(simple.Expression);
            return simple;
        }

        internal virtual SqlSource VisitSource(SqlSource source)
        {
            return (SqlSource)this.Visit(source);
        }

        internal virtual SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc)
        {
            int num = 0;
            int count = spc.Arguments.Count;
            while (num < count)
            {
                spc.Arguments[num] = this.VisitExpression(spc.Arguments[num]);
                num++;
            }
            spc.Projection = this.VisitExpression(spc.Projection);
            int num3 = 0;
            int num4 = spc.Columns.Count;
            while (num3 < num4)
            {
                spc.Columns[num3] = (SqlUserColumn)this.Visit(spc.Columns[num3]);
                num3++;
            }
            return spc;
        }

        internal virtual SqlExpression VisitSubSelect(SqlSubSelect ss)
        {
            switch (ss.NodeType)
            {
                case SqlNodeType.Element:
                    return this.VisitElement(ss);

                case SqlNodeType.Exists:
                    return this.VisitExists(ss);

                case SqlNodeType.Multiset:
                    return this.VisitMultiset(ss);

                case SqlNodeType.ScalarSubSelect:
                    return this.VisitScalarSubSelect(ss);
            }
            throw Error.UnexpectedNode(ss.NodeType);
        }

        internal virtual SqlTable VisitTable(SqlTable tab)
        {
            return tab;
        }

        internal virtual SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc)
        {
            int num = 0;
            int count = fc.Arguments.Count;
            while (num < count)
            {
                fc.Arguments[num] = this.VisitExpression(fc.Arguments[num]);
                num++;
            }
            return fc;
        }

        internal virtual SqlExpression VisitTreat(SqlUnary t)
        {
            t.Operand = this.VisitExpression(t.Operand);
            return t;
        }

        internal virtual SqlExpression VisitTypeCase(SqlTypeCase tc)
        {
            tc.Discriminator = this.VisitExpression(tc.Discriminator);
            int num = 0;
            int count = tc.Whens.Count;
            while (num < count)
            {
                SqlTypeCaseWhen when = tc.Whens[num];
                when.Match = this.VisitExpression(when.Match);
                when.TypeBinding = this.VisitExpression(when.TypeBinding);
                num++;
            }
            return tc;
        }

        internal virtual SqlExpression VisitUnaryOperator(SqlUnary uo)
        {
            uo.Operand = this.VisitExpression(uo.Operand);
            return uo;
        }

        internal virtual SqlNode VisitUnion(SqlUnion su)
        {
            su.Left = this.Visit(su.Left);
            su.Right = this.Visit(su.Right);
            return su;
        }

        internal virtual SqlStatement VisitUpdate(SqlUpdate update)
        {
            update.Select = this.VisitSequence(update.Select);
            int num = 0;
            int count = update.Assignments.Count;
            while (num < count)
            {
                update.Assignments[num] = (SqlAssign)this.Visit(update.Assignments[num]);
                num++;
            }
            return update;
        }

        internal virtual SqlExpression VisitUserColumn(SqlUserColumn suc)
        {
            return suc;
        }

        internal virtual SqlUserQuery VisitUserQuery(SqlUserQuery suq)
        {
            int num = 0;
            int count = suq.Arguments.Count;
            while (num < count)
            {
                suq.Arguments[num] = this.VisitExpression(suq.Arguments[num]);
                num++;
            }
            suq.Projection = this.VisitExpression(suq.Projection);
            int num3 = 0;
            int num4 = suq.Columns.Count;
            while (num3 < num4)
            {
                suq.Columns[num3] = (SqlUserColumn)this.Visit(suq.Columns[num3]);
                num3++;
            }
            return suq;
        }

        internal virtual SqlExpression VisitUserRow(SqlUserRow row)
        {
            return row;
        }

        internal virtual SqlExpression VisitValue(SqlValue value)
        {
            return value;
        }

        internal virtual SqlExpression VisitVariable(SqlVariable v)
        {
            return v;
        }
    }



}