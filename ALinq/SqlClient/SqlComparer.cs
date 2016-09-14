namespace ALinq.SqlClient
{
    internal class SqlComparer
    {
        // Methods
        internal SqlComparer()
        {
        }

        internal static bool AreEqual(SqlNode node1, SqlNode node2)
        {
            if (node1 == node2)
            {
                return true;
            }
            if ((node1 != null) && (node2 != null))
            {
                if (node1.NodeType == SqlNodeType.SimpleCase)
                {
                    node1 = UnwrapTrivialCaseExpression((SqlSimpleCase) node1);
                }
                if (node2.NodeType == SqlNodeType.SimpleCase)
                {
                    node2 = UnwrapTrivialCaseExpression((SqlSimpleCase) node2);
                }
                if (node1.NodeType != node2.NodeType)
                {
                    if (node1.NodeType == SqlNodeType.ExprSet)
                    {
                        SqlExprSet set = (SqlExprSet) node1;
                        int num = 0;
                        int count = set.Expressions.Count;
                        while (num < count)
                        {
                            if (AreEqual(set.Expressions[num], node2))
                            {
                                return true;
                            }
                            num++;
                        }
                    }
                    else if (node2.NodeType == SqlNodeType.ExprSet)
                    {
                        SqlExprSet set2 = (SqlExprSet) node2;
                        int num3 = 0;
                        int num4 = set2.Expressions.Count;
                        while (num3 < num4)
                        {
                            if (AreEqual(node1, set2.Expressions[num3]))
                            {
                                return true;
                            }
                            num3++;
                        }
                    }
                    return false;
                }
                if (node1.Equals(node2))
                {
                    return true;
                }
                switch (node1.NodeType)
                {
                    case SqlNodeType.Add:
                    case SqlNodeType.And:
                    case SqlNodeType.BitAnd:
                    case SqlNodeType.BitOr:
                    case SqlNodeType.BitXor:
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
                        {
                            SqlBinary binary = (SqlBinary) node1;
                            SqlBinary binary2 = (SqlBinary) node2;
                            if (!AreEqual(binary.Left, binary2.Left))
                            {
                                return false;
                            }
                            return AreEqual(binary.Right, binary2.Right);
                        }
                    case SqlNodeType.Alias:
                        return AreEqual(((SqlAlias) node1).Node, ((SqlAlias) node2).Node);

                    case SqlNodeType.AliasRef:
                        return AreEqual(((SqlAliasRef) node1).Alias, ((SqlAliasRef) node2).Alias);

                    case SqlNodeType.Avg:
                    case SqlNodeType.BitNot:
                    case SqlNodeType.ClrLength:
                    case SqlNodeType.Count:
                    case SqlNodeType.Covar:
                    case SqlNodeType.IsNotNull:
                    case SqlNodeType.IsNull:
                    case SqlNodeType.Max:
                    case SqlNodeType.Min:
                    case SqlNodeType.Negate:
                    case SqlNodeType.Not:
                    case SqlNodeType.Not2V:
                    case SqlNodeType.OuterJoinedValue:
                    case SqlNodeType.Stddev:
                    case SqlNodeType.Sum:
                    case SqlNodeType.ValueOf:
                        return AreEqual(((SqlUnary) node1).Operand, ((SqlUnary) node2).Operand);

                    case SqlNodeType.Between:
                        {
                            SqlBetween between = (SqlBetween) node1;
                            SqlBetween between2 = (SqlBetween) node1;
                            if (!AreEqual(between.Expression, between2.Expression) ||
                                !AreEqual(between.Start, between2.Start))
                            {
                                return false;
                            }
                            return AreEqual(between.End, between2.End);
                        }
                    case SqlNodeType.Cast:
                    case SqlNodeType.Convert:
                    case SqlNodeType.Treat:
                        {
                            SqlUnary unary = (SqlUnary) node1;
                            SqlUnary unary2 = (SqlUnary) node2;
                            if ((unary.ClrType != unary2.ClrType) || !(unary.SqlType == unary2.SqlType))
                            {
                                return false;
                            }
                            return AreEqual(unary.Operand, unary2.Operand);
                        }
                    case SqlNodeType.ClientCase:
                        {
                            SqlClientCase case5 = (SqlClientCase) node1;
                            SqlClientCase case6 = (SqlClientCase) node2;
                            if (case5.Whens.Count == case6.Whens.Count)
                            {
                                int num9 = 0;
                                int num10 = case5.Whens.Count;
                                while (num9 < num10)
                                {
                                    if (!AreEqual(case5.Whens[num9].Match, case6.Whens[num9].Match) ||
                                        !AreEqual(case5.Whens[num9].Value, case6.Whens[num9].Value))
                                    {
                                        return false;
                                    }
                                    num9++;
                                }
                                return true;
                            }
                            return false;
                        }
                    case SqlNodeType.Column:
                        {
                            SqlColumn column = (SqlColumn) node1;
                            SqlColumn column2 = (SqlColumn) node2;
                            return ((column == column2) ||
                                    (((column.Expression != null) && (column2.Expression != null)) &&
                                     AreEqual(column.Expression, column2.Expression)));
                        }
                    case SqlNodeType.ColumnRef:
                        {
                            SqlColumnRef cref = (SqlColumnRef) node1;
                            SqlColumnRef ref3 = (SqlColumnRef) node2;
                            return (GetBaseColumn(cref) == GetBaseColumn(ref3));
                        }
                    case SqlNodeType.DiscriminatedType:
                        {
                            SqlDiscriminatedType type = (SqlDiscriminatedType) node1;
                            SqlDiscriminatedType type2 = (SqlDiscriminatedType) node2;
                            return AreEqual(type.Discriminator, type2.Discriminator);
                        }
                    case SqlNodeType.ExprSet:
                        {
                            SqlExprSet set3 = (SqlExprSet) node1;
                            SqlExprSet set4 = (SqlExprSet) node2;
                            if (set3.Expressions.Count == set4.Expressions.Count)
                            {
                                int num17 = 0;
                                int num18 = set3.Expressions.Count;
                                while (num17 < num18)
                                {
                                    if (!AreEqual(set3.Expressions[num17], set4.Expressions[num17]))
                                    {
                                        return false;
                                    }
                                    num17++;
                                }
                                return true;
                            }
                            return false;
                        }
                    case SqlNodeType.FunctionCall:
                        {
                            SqlFunctionCall call = (SqlFunctionCall) node1;
                            SqlFunctionCall call2 = (SqlFunctionCall) node2;
                            if (!(call.Name != call2.Name))
                            {
                                if (call.Arguments.Count != call2.Arguments.Count)
                                {
                                    return false;
                                }
                                int num13 = 0;
                                int num14 = call.Arguments.Count;
                                while (num13 < num14)
                                {
                                    if (!AreEqual(call.Arguments[num13], call2.Arguments[num13]))
                                    {
                                        return false;
                                    }
                                    num13++;
                                }
                                return true;
                            }
                            return false;
                        }
                    case SqlNodeType.Link:
                        {
                            SqlLink link = (SqlLink) node1;
                            SqlLink link2 = (SqlLink) node2;
                            if (MetaPosition.AreSameMember(link.Member.Member, link2.Member.Member))
                            {
                                if (!AreEqual(link.Expansion, link2.Expansion))
                                {
                                    return false;
                                }
                                if (link.KeyExpressions.Count != link2.KeyExpressions.Count)
                                {
                                    return false;
                                }
                                int num15 = 0;
                                int num16 = link.KeyExpressions.Count;
                                while (num15 < num16)
                                {
                                    if (!AreEqual(link.KeyExpressions[num15], link2.KeyExpressions[num15]))
                                    {
                                        return false;
                                    }
                                    num15++;
                                }
                                return true;
                            }
                            return false;
                        }
                    case SqlNodeType.Like:
                        {
                            SqlLike like = (SqlLike) node1;
                            SqlLike like2 = (SqlLike) node2;
                            if (!AreEqual(like.Expression, like2.Expression) || !AreEqual(like.Pattern, like2.Pattern))
                            {
                                return false;
                            }
                            return AreEqual(like.Escape, like2.Escape);
                        }
                    case SqlNodeType.Member:
                        if (((SqlMember) node1).Member != ((SqlMember) node2).Member)
                        {
                            return false;
                        }
                        return AreEqual(((SqlMember) node1).Expression, ((SqlMember) node2).Expression);

                    case SqlNodeType.OptionalValue:
                        {
                            SqlOptionalValue value2 = (SqlOptionalValue) node1;
                            SqlOptionalValue value3 = (SqlOptionalValue) node2;
                            return AreEqual(value2.Value, value3.Value);
                        }
                    case SqlNodeType.Parameter:
                        return (node1 == node2);

                    case SqlNodeType.SearchedCase:
                        {
                            SqlSearchedCase case3 = (SqlSearchedCase) node1;
                            SqlSearchedCase case4 = (SqlSearchedCase) node2;
                            if (case3.Whens.Count == case4.Whens.Count)
                            {
                                int num7 = 0;
                                int num8 = case3.Whens.Count;
                                while (num7 < num8)
                                {
                                    if (!AreEqual(case3.Whens[num7].Match, case4.Whens[num7].Match) ||
                                        !AreEqual(case3.Whens[num7].Value, case4.Whens[num7].Value))
                                    {
                                        return false;
                                    }
                                    num7++;
                                }
                                return AreEqual(case3.Else, case4.Else);
                            }
                            return false;
                        }
                    case SqlNodeType.SimpleCase:
                        {
                            SqlSimpleCase case7 = (SqlSimpleCase) node1;
                            SqlSimpleCase case8 = (SqlSimpleCase) node2;
                            if (case7.Whens.Count == case8.Whens.Count)
                            {
                                int num11 = 0;
                                int num12 = case7.Whens.Count;
                                while (num11 < num12)
                                {
                                    if (!AreEqual(case7.Whens[num11].Match, case8.Whens[num11].Match) ||
                                        !AreEqual(case7.Whens[num11].Value, case8.Whens[num11].Value))
                                    {
                                        return false;
                                    }
                                    num11++;
                                }
                                return true;
                            }
                            return false;
                        }
                    case SqlNodeType.Table:
                        return (((SqlTable) node1).MetaTable == ((SqlTable) node2).MetaTable);

                    case SqlNodeType.TypeCase:
                        {
                            SqlTypeCase @case = (SqlTypeCase) node1;
                            SqlTypeCase case2 = (SqlTypeCase) node2;
                            if (AreEqual(@case.Discriminator, case2.Discriminator))
                            {
                                if (@case.Whens.Count != case2.Whens.Count)
                                {
                                    return false;
                                }
                                int num5 = 0;
                                int num6 = @case.Whens.Count;
                                while (num5 < num6)
                                {
                                    if (!AreEqual(@case.Whens[num5].Match, case2.Whens[num5].Match))
                                    {
                                        return false;
                                    }
                                    if (!AreEqual(@case.Whens[num5].TypeBinding, case2.Whens[num5].TypeBinding))
                                    {
                                        return false;
                                    }
                                    num5++;
                                }
                                return true;
                            }
                            return false;
                        }
                    case SqlNodeType.Variable:
                        {
                            SqlVariable variable = (SqlVariable) node1;
                            SqlVariable variable2 = (SqlVariable) node2;
                            return (variable.Name == variable2.Name);
                        }
                    case SqlNodeType.Value:
                        return object.Equals(((SqlValue) node1).Value, ((SqlValue) node2).Value);
                }
            }
            return false;
        }

        private static SqlColumn GetBaseColumn(SqlColumnRef cref)
        {
            while ((cref != null) && (cref.Column.Expression != null))
            {
                SqlColumnRef expression = cref.Column.Expression as SqlColumnRef;
                if (expression == null)
                {
                    break;
                }
                cref = expression;
            }
            return cref.Column;
        }

        private static SqlExpression UnwrapTrivialCaseExpression(SqlSimpleCase sc)
        {
            if (sc.Whens.Count != 1)
            {
                return sc;
            }
            if (!AreEqual(sc.Expression, sc.Whens[0].Match))
            {
                return sc;
            }
            SqlExpression expression = sc.Whens[0].Value;
            if (expression.NodeType == SqlNodeType.SimpleCase)
            {
                return UnwrapTrivialCaseExpression((SqlSimpleCase) expression);
            }
            return expression;
        }
    }
}