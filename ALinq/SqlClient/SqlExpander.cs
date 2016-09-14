using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ALinq.SqlClient
{
    internal class SqlExpander
    {
        // Fields
        private SqlFactory factory;

        // Methods
        internal SqlExpander(SqlFactory factory)
        {
            this.factory = factory;
        }

        internal SqlExpression Expand(SqlExpression exp)
        {
            return new Visitor(this.factory).VisitExpression(exp);
        }

        // Nested Types
        private class Visitor : SqlDuplicator.DuplicatingVisitor
        {
            // Fields
            private SqlFactory factory;
            private Expression sourceExpression;

            // Methods
            internal Visitor(SqlFactory factory)
                : base(true)
            {
                this.factory = factory;
            }

            private SqlExpression ExpandIntoExprSet(IList<SqlExpression> exprs)
            {
                SqlExpression[] expressionArray = new SqlExpression[exprs.Count];
                int index = 0;
                int count = exprs.Count;
                while (index < count)
                {
                    expressionArray[index] = this.VisitExpression(exprs[index]);
                    index++;
                }
                return this.factory.ExprSet(expressionArray, this.sourceExpression);
            }

            private SqlExpression ExpandTogether(IList<SqlExpression> exprs)
            {
                switch (exprs[0].NodeType)
                {
                    case SqlNodeType.Element:
                    case SqlNodeType.Grouping:
                    case SqlNodeType.ClientQuery:
                    case SqlNodeType.Multiset:
                        throw Error.UnionWithHierarchy();

                    case SqlNodeType.Link:
                        {
                            SqlLink[] linkArray = new SqlLink[exprs.Count];
                            linkArray[0] = (SqlLink) exprs[0];
                            int index = 1;
                            int count = exprs.Count;
                            while (index < count)
                            {
                                if ((exprs[index] == null) || (exprs[index].NodeType != SqlNodeType.Link))
                                {
                                    throw Error.UnionIncompatibleConstruction();
                                }
                                linkArray[index] = (SqlLink) exprs[index];
                                if (((linkArray[index].KeyExpressions.Count != linkArray[0].KeyExpressions.Count) ||
                                     (linkArray[index].Member != linkArray[0].Member)) ||
                                    ((linkArray[index].Expansion != null) != (linkArray[0].Expansion != null)))
                                {
                                    throw Error.UnionIncompatibleConstruction();
                                }
                                index++;
                            }
                            SqlExpression[] keyExpressions = new SqlExpression[linkArray[0].KeyExpressions.Count];
                            List<SqlExpression> list11 = new List<SqlExpression>();
                            int num29 = 0;
                            int num30 = linkArray[0].KeyExpressions.Count;
                            while (num29 < num30)
                            {
                                list11.Clear();
                                int num31 = 0;
                                int num32 = exprs.Count;
                                while (num31 < num32)
                                {
                                    list11.Add(linkArray[num31].KeyExpressions[num29]);
                                    num31++;
                                }
                                keyExpressions[num29] = this.ExpandTogether(list11);
                                int num33 = 0;
                                int num34 = exprs.Count;
                                while (num33 < num34)
                                {
                                    linkArray[num33].KeyExpressions[num29] = list11[num33];
                                    num33++;
                                }
                                num29++;
                            }
                            SqlExpression expansion = null;
                            if (linkArray[0].Expansion != null)
                            {
                                list11.Clear();
                                int num35 = 0;
                                int num36 = exprs.Count;
                                while (num35 < num36)
                                {
                                    list11.Add(linkArray[num35].Expansion);
                                    num35++;
                                }
                                expansion = this.ExpandTogether(list11);
                                int num37 = 0;
                                int num38 = exprs.Count;
                                while (num37 < num38)
                                {
                                    linkArray[num37].Expansion = list11[num37];
                                    num37++;
                                }
                            }
                            return new SqlLink(linkArray[0].Id, linkArray[0].RowType, linkArray[0].ClrType,
                                               linkArray[0].SqlType, linkArray[0].Expression, linkArray[0].Member,
                                               keyExpressions, expansion, linkArray[0].SourceExpression);
                        }
                    case SqlNodeType.ClientCase:
                        {
                            SqlClientCase[] caseArray = new SqlClientCase[exprs.Count];
                            caseArray[0] = (SqlClientCase) exprs[0];
                            for (int i = 1; i < caseArray.Length; i++)
                            {
                                caseArray[i] = (SqlClientCase) exprs[i];
                            }
                            List<SqlExpression> list3 = new List<SqlExpression>();
                            for (int j = 0; j < caseArray.Length; j++)
                            {
                                list3.Add(caseArray[j].Expression);
                            }
                            SqlExpression expr = this.ExpandTogether(list3);
                            List<SqlClientWhen> whens = new List<SqlClientWhen>();
                            for (int k = 0; k < caseArray[0].Whens.Count; k++)
                            {
                                List<SqlExpression> list5 = new List<SqlExpression>();
                                for (int m = 0; m < caseArray.Length; m++)
                                {
                                    SqlClientWhen when = caseArray[m].Whens[k];
                                    list5.Add(when.Value);
                                }
                                whens.Add(new SqlClientWhen(caseArray[0].Whens[k].Match, this.ExpandTogether(list5)));
                            }
                            return new SqlClientCase(caseArray[0].ClrType, expr, whens, caseArray[0].SourceExpression);
                        }
                    case SqlNodeType.DiscriminatedType:
                        {
                            SqlDiscriminatedType type = (SqlDiscriminatedType) exprs[0];
                            List<SqlExpression> list15 = new List<SqlExpression>(exprs.Count);
                            list15.Add(type.Discriminator);
                            int num44 = 1;
                            int num45 = exprs.Count;
                            while (num44 < num45)
                            {
                                SqlDiscriminatedType type2 = (SqlDiscriminatedType) exprs[num44];
                                if (type2.TargetType != type.TargetType)
                                {
                                    throw Error.UnionIncompatibleConstruction();
                                }
                                list15.Add(type2.Discriminator);
                                num44++;
                            }
                            return this.factory.DiscriminatedType(this.ExpandTogether(list15),
                                                                  ((SqlDiscriminatedType) exprs[0]).TargetType);
                        }
                    case SqlNodeType.MethodCall:
                        {
                            SqlMethodCall[] callArray = new SqlMethodCall[exprs.Count];
                            for (int n = 0; n < callArray.Length; n++)
                            {
                                callArray[n] = (SqlMethodCall) exprs[n];
                            }
                            List<SqlExpression> list = new List<SqlExpression>();
                            for (int num2 = 0; num2 < callArray[0].Arguments.Count; num2++)
                            {
                                List<SqlExpression> list2 = new List<SqlExpression>();
                                for (int num3 = 0; num3 < callArray.Length; num3++)
                                {
                                    list2.Add(callArray[num3].Arguments[num2]);
                                }
                                SqlExpression item = this.ExpandTogether(list2);
                                list.Add(item);
                            }
                            return this.factory.MethodCall(callArray[0].Method, callArray[0].Object, list.ToArray(),
                                                           callArray[0].SourceExpression);
                        }
                    case SqlNodeType.New:
                        {
                            SqlNew[] newArray = new SqlNew[exprs.Count];
                            newArray[0] = (SqlNew) exprs[0];
                            int num13 = 1;
                            int num14 = exprs.Count;
                            while (num13 < num14)
                            {
                                if ((exprs[num13] == null) || (exprs[num13].NodeType != SqlNodeType.New))
                                {
                                    throw Error.UnionIncompatibleConstruction();
                                }
                                newArray[num13] = (SqlNew) exprs[1];
                                if (newArray[num13].Members.Count != newArray[0].Members.Count)
                                {
                                    throw Error.UnionDifferentMembers();
                                }
                                int num15 = 0;
                                int num16 = newArray[0].Members.Count;
                                while (num15 < num16)
                                {
                                    if (newArray[num13].Members[num15].Member != newArray[0].Members[num15].Member)
                                    {
                                        throw Error.UnionDifferentMemberOrder();
                                    }
                                    num15++;
                                }
                                num13++;
                            }
                            SqlMemberAssign[] bindings = new SqlMemberAssign[newArray[0].Members.Count];
                            int num17 = 0;
                            int length = bindings.Length;
                            while (num17 < length)
                            {
                                List<SqlExpression> list9 = new List<SqlExpression>();
                                int num19 = 0;
                                int num20 = exprs.Count;
                                while (num19 < num20)
                                {
                                    list9.Add(newArray[num19].Members[num17].Expression);
                                    num19++;
                                }
                                bindings[num17] = new SqlMemberAssign(newArray[0].Members[num17].Member,
                                                                      this.ExpandTogether(list9));
                                int num21 = 0;
                                int num22 = exprs.Count;
                                while (num21 < num22)
                                {
                                    newArray[num21].Members[num17].Expression = list9[num21];
                                    num21++;
                                }
                                num17++;
                            }
                            SqlExpression[] args = new SqlExpression[newArray[0].Args.Count];
                            int num23 = 0;
                            int num24 = args.Length;
                            while (num23 < num24)
                            {
                                List<SqlExpression> list10 = new List<SqlExpression>();
                                int num25 = 0;
                                int num26 = exprs.Count;
                                while (num25 < num26)
                                {
                                    list10.Add(newArray[num25].Args[num23]);
                                    num25++;
                                }
                                args[num23] = this.ExpandTogether(list10);
                                num23++;
                            }
                            return this.factory.New(newArray[0].MetaType, newArray[0].Constructor, args,
                                                    newArray[0].ArgMembers, bindings, exprs[0].SourceExpression);
                        }
                    case SqlNodeType.OptionalValue:
                        {
                            if (exprs[0].SqlType.CanBeColumn)
                            {
                                break;
                            }
                            List<SqlExpression> list12 = new List<SqlExpression>(exprs.Count);
                            List<SqlExpression> list13 = new List<SqlExpression>(exprs.Count);
                            int num40 = 0;
                            int num41 = exprs.Count;
                            while (num40 < num41)
                            {
                                if ((exprs[num40] == null) || (exprs[num40].NodeType != SqlNodeType.OptionalValue))
                                {
                                    throw Error.UnionIncompatibleConstruction();
                                }
                                SqlOptionalValue value4 = (SqlOptionalValue) exprs[num40];
                                list12.Add(value4.HasValue);
                                list13.Add(value4.Value);
                                num40++;
                            }
                            return new SqlOptionalValue(this.ExpandTogether(list12), this.ExpandTogether(list13));
                        }
                    case SqlNodeType.OuterJoinedValue:
                        {
                            if (exprs[0].SqlType.CanBeColumn)
                            {
                                break;
                            }
                            List<SqlExpression> list14 = new List<SqlExpression>(exprs.Count);
                            int num42 = 0;
                            int num43 = exprs.Count;
                            while (num42 < num43)
                            {
                                if ((exprs[num42] == null) || (exprs[num42].NodeType != SqlNodeType.OuterJoinedValue))
                                {
                                    throw Error.UnionIncompatibleConstruction();
                                }
                                SqlUnary unary = (SqlUnary) exprs[num42];
                                list14.Add(unary.Operand);
                                num42++;
                            }
                            return this.factory.Unary(SqlNodeType.OuterJoinedValue, this.ExpandTogether(list14));
                        }
                    case SqlNodeType.TypeCase:
                        {
                            SqlTypeCase[] caseArray2 = new SqlTypeCase[exprs.Count];
                            caseArray2[0] = (SqlTypeCase) exprs[0];
                            for (int num8 = 1; num8 < caseArray2.Length; num8++)
                            {
                                caseArray2[num8] = (SqlTypeCase) exprs[num8];
                            }
                            List<SqlExpression> list6 = new List<SqlExpression>();
                            for (int num9 = 0; num9 < caseArray2.Length; num9++)
                            {
                                list6.Add(caseArray2[num9].Discriminator);
                            }
                            SqlExpression discriminator = this.ExpandTogether(list6);
                            for (int num10 = 0; num10 < caseArray2.Length; num10++)
                            {
                                caseArray2[num10].Discriminator = list6[num10];
                            }
                            List<SqlTypeCaseWhen> list7 = new List<SqlTypeCaseWhen>();
                            for (int num11 = 0; num11 < caseArray2[0].Whens.Count; num11++)
                            {
                                List<SqlExpression> list8 = new List<SqlExpression>();
                                for (int num12 = 0; num12 < caseArray2.Length; num12++)
                                {
                                    SqlTypeCaseWhen when2 = caseArray2[num12].Whens[num11];
                                    list8.Add(when2.TypeBinding);
                                }
                                SqlExpression typeBinding = this.ExpandTogether(list8);
                                list7.Add(new SqlTypeCaseWhen(caseArray2[0].Whens[num11].Match, typeBinding));
                            }
                            return this.factory.TypeCase(caseArray2[0].ClrType, caseArray2[0].RowType, discriminator,
                                                         list7, caseArray2[0].SourceExpression);
                        }
                    case SqlNodeType.Value:
                        {
                            SqlValue value2 = (SqlValue) exprs[0];
                            for (int num39 = 1; num39 < exprs.Count; num39++)
                            {
                                SqlValue value3 = (SqlValue) exprs[num39];
                                if (!object.Equals(value3.Value, value2.Value))
                                {
                                    return this.ExpandIntoExprSet(exprs);
                                }
                            }
                            return value2;
                        }
                }
                return this.ExpandIntoExprSet(exprs);
            }

            private SqlExpression ExpandUnion(SqlUnion union)
            {
                IList<SqlExpression> exprs = new List<SqlExpression>(2);
                this.GatherUnionExpressions(union, exprs);
                this.sourceExpression = union.SourceExpression;
                return this.ExpandTogether(exprs);
            }

            private void GatherUnionExpressions(SqlNode node, IList<SqlExpression> exprs)
            {
                SqlUnion union = node as SqlUnion;
                if (union != null)
                {
                    this.GatherUnionExpressions(union.Left, exprs);
                    this.GatherUnionExpressions(union.Right, exprs);
                }
                else
                {
                    SqlSelect select = node as SqlSelect;
                    if (select != null)
                    {
                        SqlAliasRef selection = select.Selection as SqlAliasRef;
                        if (selection != null)
                        {
                            this.GatherUnionExpressions(selection.Alias.Node, exprs);
                        }
                        else
                        {
                            exprs.Add(select.Selection);
                        }
                    }
                }
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
            {
                SqlNode node = aref.Alias.Node;
                if ((node is SqlTable) || (node is SqlTableValuedFunctionCall))
                {
                    return aref;
                }
                SqlUnion union = node as SqlUnion;
                if (union != null)
                {
                    return this.ExpandUnion(union);
                }
                SqlSelect select = node as SqlSelect;
                if (select != null)
                {
                    return this.VisitExpression(select.Selection);
                }
                SqlExpression exp = node as SqlExpression;
                if (exp == null)
                {
                    throw Error.CouldNotHandleAliasRef(node.NodeType);
                }
                return this.VisitExpression(exp);
            }

            internal override SqlExpression VisitColumn(SqlColumn col)
            {
                return new SqlColumnRef(col);
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                return cref;
            }

            internal override SqlNode VisitLink(SqlLink link)
            {
                SqlExpression expansion = this.VisitExpression(link.Expansion);
                SqlExpression[] keyExpressions = new SqlExpression[link.KeyExpressions.Count];
                int index = 0;
                int length = keyExpressions.Length;
                while (index < length)
                {
                    keyExpressions[index] = this.VisitExpression(link.KeyExpressions[index]);
                    index++;
                }
                return new SqlLink(link.Id, link.RowType, link.ClrType, link.SqlType, link.Expression, link.Member,
                                   keyExpressions, expansion, link.SourceExpression);
            }

            internal override SqlExpression VisitSharedExpression(SqlSharedExpression shared)
            {
                return this.VisitExpression(shared.Expression);
            }

            internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref)
            {
                return this.VisitExpression(sref.SharedExpression.Expression);
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                return (SqlExpression) new SqlDuplicator().Duplicate(ss);
            }
        }
    }
}