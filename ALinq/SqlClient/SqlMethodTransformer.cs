using System;

namespace ALinq.SqlClient
{
    internal class SqlMethodTransformer : SqlVisitor
    {
        // Fields
        protected SqlFactory sql;

        // Methods
        internal SqlMethodTransformer(SqlFactory sql)
        {
            this.sql = sql;
        }

        internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
        {
            SqlExpression first = base.VisitFunctionCall(fc);
            if (first is SqlFunctionCall)
            {
                var expr = (SqlFunctionCall)first;
                if (expr.Name == "LEN")
                {
                    SqlExpression expression2 = expr.Arguments[0];
                    if (expression2.SqlType.IsLargeType && !expression2.SqlType.SupportsLength)
                    {
                        first = this.sql.DATALENGTH(expression2);
                        if (expression2.SqlType.IsUnicodeType)
                        {
                            first = sql.ConvertToInt(sql.Divide(first, sql.ValueFromObject(2, expression2.SourceExpression)));
                        }
                    }
                }

                Type closestRuntimeType = expr.SqlType.GetClosestRuntimeType();
                //if (expr.ClrType != closestRuntimeType)
                if (Type.GetTypeCode(expr.ClrType) != Type.GetTypeCode(closestRuntimeType))
                {
                    first = sql.ConvertTo(expr.ClrType, expr);
                }
            }
            return first;
        }

        internal override SqlExpression VisitUnaryOperator(SqlUnary fc)
        {
            SqlExpression first = base.VisitUnaryOperator(fc);
            if (!(first is SqlUnary))
            {
                return first;
            }
            SqlUnary unary = (SqlUnary)first;
            if (unary.NodeType != SqlNodeType.ClrLength)
            {
                return first;
            }
            SqlExpression operand = unary.Operand;
            first = this.sql.DATALENGTH(operand);
            if (operand.SqlType.IsUnicodeType)
            {
                first = this.sql.Divide(first, this.sql.ValueFromObject(2, operand.SourceExpression));
            }
            return this.sql.ConvertToInt(first);
        }
    }
}