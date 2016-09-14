using System;
using System.Collections.Generic;

namespace ALinq.SqlClient
{
    internal class SqlCaseSimplifier
    {
        // Methods
        internal static SqlNode Simplify(SqlNode node, SqlFactory sql)
        {
            return new Visitor(sql).Visit(node);
        }

        // Nested Types
        private class Visitor : SqlVisitor
        {
            // Fields
            private SqlFactory sql;

            // Methods
            internal Visitor(SqlFactory sql)
            {
                this.sql = sql;
            }

            internal bool AreCaseWhenValuesConstant(SqlSimpleCase sc)
            {
                foreach (SqlWhen when in sc.Whens)
                {
                    if (when.Value.NodeType != SqlNodeType.Value)
                    {
                        return false;
                    }
                }
                return true;
            }

            private SqlExpression DistributeOperatorIntoCase(SqlNodeType nt, SqlSimpleCase sc, SqlExpression expr)
            {
                if (((nt != SqlNodeType.EQ) && (nt != SqlNodeType.NE)) && ((nt != SqlNodeType.EQ2V) && (nt != SqlNodeType.NE2V)))
                {
                    throw Error.ArgumentOutOfRange("nt");
                }
                object obj2 = Eval(expr);
                var values = new List<SqlExpression>();
                var matches = new List<SqlExpression>();
                foreach (SqlWhen when in sc.Whens)
                {
                    matches.Add(when.Match);
                    object obj3 = Eval(when.Value);
                    bool flag = when.Value.SqlType.AreValuesEqual(obj3, obj2);
                    values.Add(sql.ValueFromObject(((nt == SqlNodeType.EQ) || (nt == SqlNodeType.EQ2V)) == flag, false, sc.SourceExpression));
                }
                return this.VisitExpression(this.sql.Case(typeof(bool), sc.Expression, matches, values, sc.SourceExpression));
            }

            private SqlExpression TryToConsolidateAllValueExpressions(int valueCount, SqlExpression value)
            {
                if (valueCount == 1)
                {
                    return value;
                }
                return null;
            }

            private SqlExpression TryToWriteAsReducedCase(Type caseType, SqlExpression discriminator, List<SqlWhen> newWhens, SqlExpression elseCandidate, int originalWhenCount)
            {
                if ((newWhens.Count != originalWhenCount) && (elseCandidate == null))
                {
                    return new SqlSimpleCase(caseType, discriminator, newWhens, discriminator.SourceExpression);
                }
                return null;
            }

            private SqlExpression TryToWriteAsSimpleBooleanExpression(Type caseType, SqlExpression discriminator, List<SqlWhen> newWhens, bool allValuesLiteral)
            {
                SqlExpression left = null;
                if ((caseType != typeof(bool)) || !allValuesLiteral)
                {
                    return left;
                }
                bool? nullable = SqlExpressionNullability.CanBeNull(discriminator);
                bool? nullable2 = null;
                for (int i = 0; i < newWhens.Count; i++)
                {
                    SqlValue value2 = (SqlValue)newWhens[i].Value;
                    bool flag = (bool)value2.Value;
                    if (newWhens[i].Match != null)
                    {
                        if (flag)
                        {
                            left = this.sql.OrAccumulate(left, this.sql.Binary(SqlNodeType.EQ, discriminator, newWhens[i].Match));
                        }
                        else
                        {
                            left = this.sql.AndAccumulate(left, this.sql.Binary(SqlNodeType.NE, discriminator, newWhens[i].Match));
                        }
                    }
                    else
                    {
                        nullable2 = new bool?(flag);
                    }
                }
                if ((nullable == false) || !nullable2.HasValue)
                {
                    return left;
                }
                if (nullable2 == true)
                {
                    return this.sql.OrAccumulate(left, this.sql.Unary(SqlNodeType.IsNull, discriminator, discriminator.SourceExpression));
                }
                return this.sql.AndAccumulate(left, this.sql.Unary(SqlNodeType.IsNotNull, discriminator, discriminator.SourceExpression));
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                switch (bo.NodeType)
                {
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                        if (((bo.Left.NodeType == SqlNodeType.SimpleCase) && (bo.Right.NodeType == SqlNodeType.Value)) && this.AreCaseWhenValuesConstant((SqlSimpleCase)bo.Left))
                        {
                            return this.DistributeOperatorIntoCase(bo.NodeType, (SqlSimpleCase)bo.Left, bo.Right);
                        }
                        if (((bo.Right.NodeType == SqlNodeType.SimpleCase) && (bo.Left.NodeType == SqlNodeType.Value)) && this.AreCaseWhenValuesConstant((SqlSimpleCase)bo.Right))
                        {
                            return this.DistributeOperatorIntoCase(bo.NodeType, (SqlSimpleCase)bo.Right, bo.Left);
                        }
                        break;
                }
                return base.VisitBinaryOperator(bo);
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c)
            {
                c.Expression = this.VisitExpression(c.Expression);
                int num = 0;
                int num2 = 0;
                int count = c.Whens.Count;
                while (num2 < count)
                {
                    if (c.Whens[num2].Match == null)
                    {
                        num = num2;
                        break;
                    }
                    num2++;
                }
                c.Whens[num].Match = this.VisitExpression(c.Whens[num].Match);
                c.Whens[num].Value = this.VisitExpression(c.Whens[num].Value);
                List<SqlWhen> newWhens = new List<SqlWhen>();
                bool allValuesLiteral = true;
                int num4 = 0;
                int num5 = c.Whens.Count;
                while (num4 < num5)
                {
                    if (num != num4)
                    {
                        SqlWhen item = c.Whens[num4];
                        item.Match = this.VisitExpression(item.Match);
                        item.Value = this.VisitExpression(item.Value);
                        if (!SqlComparer.AreEqual(c.Whens[num].Value, item.Value))
                        {
                            newWhens.Add(item);
                        }
                        allValuesLiteral = allValuesLiteral && (item.Value.NodeType == SqlNodeType.Value);
                    }
                    num4++;
                }
                newWhens.Add(c.Whens[num]);
                SqlExpression expression = this.TryToConsolidateAllValueExpressions(newWhens.Count, c.Whens[num].Value);
                if (expression != null)
                {
                    return expression;
                }
                expression = this.TryToWriteAsSimpleBooleanExpression(c.ClrType, c.Expression, newWhens, allValuesLiteral);
                if (expression != null)
                {
                    return expression;
                }
                expression = this.TryToWriteAsReducedCase(c.ClrType, c.Expression, newWhens, c.Whens[num].Match, c.Whens.Count);
                if (expression != null)
                {
                    return expression;
                }
                return c;
            }
        }
    }


}