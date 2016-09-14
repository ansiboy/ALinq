using System;
using System.Collections.Generic;
using System.Linq;

namespace ALinq.SqlClient
{
    internal static class SqlExpressionNullability
    {
        internal static bool? CanBeNull(SqlExpression expr)
        {
            #region MyRegion
            if (true)
            {
                bool? nullable, nullable2, nullable3 = null, nullable4, nullable5, nullable6 = null;
            L_0000:
                var type = (int)expr.NodeType;
                if (type > 0x22)
                    goto L_007d;
                if (type > 0x15)
                    goto L_0050;
                if (type == 0)
                    goto L_01c7;
            //L_0017
            L_001C:
                switch (type - 7)
                {
                    case 0:
                        goto L_01c7;
                    case 1:
                        goto L_022a;
                    case 2:
                        goto L_01c7;
                    case 3:
                        goto L_01c7;
                }
            //L_0020
            L_0035:
                switch (type - 0x13)
                {
                    case 0:
                        goto L_015a;
                    case 1:
                        goto L_0199;
                    case 2:
                        goto L_01c7;
                    default:
                        goto L_025b;    //L_004b
                }
            //L_004b
            L_0050:
                switch (type - 0x1a)
                {
                    case 0:
                        goto L_01c0;
                    case 1:
                        goto L_025b;
                    case 2:
                        goto L_01c7;
                }
            //L_0055

            L_0066:
                if (type == 0x1f)
                    goto L_0112;    //L_006a
            L_006f:
                if (type == 0x22)
                    goto L_01c0;    //L_0078
            L_007d:
                if (type > 0x43)
                    goto L_00e1;    //L_0081
            L_0083:
                switch (type - 0x26)
                {
                    case 0:
                        goto L_01c0;
                    case 1:
                        goto L_01c0;
                    case 2:
                        goto L_025b;
                    case 3:
                        goto L_023f;
                }//L_0088
            L_009d:
                if (type == 0x2f)
                    goto L_01c0;//L_00a1
            L_00a6:
                switch (type - 0x39)
                {
                    case 0:
                        goto L_01c7;
                    case 1:
                        goto L_01c7;
                    case 2:
                        goto L_01c0;
                    case 3:
                        goto L_025b;
                    case 4:
                        goto L_025b;
                    case 5:
                        goto L_022a;
                    case 6:
                        goto L_01c0;
                    case 7:
                        goto L_025b;
                    case 8:
                        goto L_025b;
                    case 9:
                        goto L_025b;
                    case 10:
                        goto L_01c0;
                    default:
                        goto L_025b;
                }//L_00dc
            L_00e1:
                if (type > 80)
                    goto L_00fb;    //L_00e5
            L_00e7:
                if (type == 70)
                    goto L_0254;    //L_00eb
            L_00e3:
                if (type == 80)
                    goto L_0125;    //L_00f4
                goto L_025b;        //L_00f6
            L_00fb:
                if (type == 0x54)
                    goto L_01c7;    //L_00ff
            L_0104:
                if (type == 0x60)
                    goto L_01ac;    //L_0108
                goto L_025b;        //L_010d
            L_0112:
                return CanBeNull(((SqlExprSet)expr).Expressions); //L_0124
            L_0125:
                return CanBeNull(((SqlSimpleCase)expr).Whens.Select(o => o.Value)); //L_0159
            L_015a:
                //???????????????????????????????????????//
                if (((SqlColumn)expr).MetaMember == null)
                    goto L_017a;
                //???????????????????????????????????????//
                return ((SqlColumn)expr).MetaMember.CanBeNull;  //L_0179
            L_017a:
                if (((SqlColumn)expr).Expression == null)
                    goto L_018e;
                return CanBeNull(((SqlColumn)expr).Expression);     //L_018d
            L_018e:
                return nullable3;   //L_0198       
            L_0199:
                return CanBeNull(((SqlColumnRef)expr).Column);  //L_01ab
            //L_01ab
            L_01ac:
                if (((SqlValue)expr).Value == null)
                    return true;
                else
                    return false;
            //L_01bf
            L_01c0:
                return false;
            L_01c7:
                var binary = (SqlBinary)expr;
                nullable = CanBeNull(binary.Left);
                nullable2 = CanBeNull(binary.Right);
                nullable4 = nullable;// nullable.GetValueOrDefault();
                //if (nullable4.GetValueOrDefault())
                //    goto L_0204;    //L_01f6
                if (nullable.HasValue && nullable2.HasValue)
                    return nullable.Value && nullable2.Value;
                return null;
            //TODO:L_01f8
            L_01f8:
            //nullable4 = false;
            //nullable4 =
            L_0204:
                if (true)
                    goto L_0223;    //L_0205
            L_0207:
                nullable5 = nullable2;
                if (nullable5.GetValueOrDefault())
                    goto L_0220;    //L_0212
            //if(nullable5.HasValue)
            L_0220:
                goto L_0224;
            L_0223:
                return true;
            L_0224:
                return true;    //L_0229
            L_022a:
                return CanBeNull(((SqlUnary)expr).Operand);     //L_023e
            L_023f:
                var lift = ((SqlLift)expr);
                return CanBeNull(lift.Expression);      //L_0253
            L_0254:
                return true;
            L_025b:
                return nullable6;
            }
            #endregion
            ///TODO:
            switch (expr.NodeType)
            {
                case SqlNodeType.ExprSet:
                    return CanBeNull(((SqlExprSet)expr).Expressions);
                case SqlNodeType.SimpleCase:
                    var items = ((SqlSimpleCase)expr).Whens.Select(o => o.Value);
                    return CanBeNull(items);
                case SqlNodeType.Column:
                    var member = ((SqlColumn)expr).MetaMember;
                    return member != null ? member.CanBeNull : true;
                case SqlNodeType.ColumnRef:
                    var column = ((SqlColumnRef)expr).Column;
                    return CanBeNull(column);
                case SqlNodeType.Value:
                    return true;
                #region case SqlBinary
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                case SqlNodeType.And:
                case SqlNodeType.Add:
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
                #endregion
                    var binary = (SqlBinary)expr;
                    var left = binary.Left;
                    var nullable = CanBeNull(left);
                    var right = binary.Right;
                    var nullable2 = CanBeNull(right);
                    if (nullable.HasValue && nullable2.HasValue)
                        return nullable.Value && nullable2.Value;

                    //TODO:
                    //return nullable4;
                    break;
                #region case SqlUnary
                case SqlNodeType.Avg:
                case SqlNodeType.BitNot:
                case SqlNodeType.Cast:
                case SqlNodeType.IsNotNull:
                case SqlNodeType.IsNull:
                case SqlNodeType.LongCount:
                case SqlNodeType.Convert:
                case SqlNodeType.Count:
                case SqlNodeType.Covar:
                case SqlNodeType.ClrLength:
                case SqlNodeType.Negate:
                case SqlNodeType.Not:
                case SqlNodeType.Not2V:
                case SqlNodeType.OuterJoinedValue:
                case SqlNodeType.Max:
                case SqlNodeType.Min:
                case SqlNodeType.Stddev:
                case SqlNodeType.Sum:
                case SqlNodeType.Treat:
                case SqlNodeType.ValueOf:
                #endregion
                    var unary = (SqlUnary)expr;
                    var operand = unary.Operand;
                    return CanBeNull(operand);
                case SqlNodeType.Lift:
                    var sqlLift = (SqlLift)expr;
                    return CanBeNull(sqlLift.Expression);
                    break;
                default:
                    return true;
            }
            return null;
        }

        private static bool? CanBeNull(IEnumerable<SqlExpression> exprs)
        {
            bool flag = false;
            foreach (SqlExpression expression in exprs)
            {
                bool? nullable = CanBeNull(expression);
                if (nullable == true)
                {
                    return true;
                }
                if (!nullable.HasValue)
                {
                    flag = true;
                }
            }
            if (flag)
            {
                return null;
            }
            return false;
        }





    }
}