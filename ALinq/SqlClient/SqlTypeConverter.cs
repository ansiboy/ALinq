using System;
using ALinq.SqlClient;

namespace ALinq.SqlClient
{
    internal class SqlTypeConverter : SqlVisitor
    {
        private readonly SqlFactory sql;

        internal SqlTypeConverter(SqlFactory sql)
        {
            this.sql = sql;
        }

        private bool BothTypesAreStrings(IProviderType oldSqlType, IProviderType newSqlType)
        {
            var type = sql.TypeProvider.From(typeof (string));
            var result = (oldSqlType.IsSameTypeFamily(type) && newSqlType.IsSameTypeFamily(type));
            return result;
        }

        private SqlExpression ConvertBitToString(SqlExpression expr, Type resultClrType)
        {
            var whens = new SqlWhen[1];
            const bool flag = true;
            whens[0] = new SqlWhen(expr, sql.ValueFromObject(flag.ToString(), false, expr.SourceExpression));
            const bool flag2 = false;
            return new SqlSearchedCase(resultClrType, whens, sql.ValueFromObject(flag2.ToString(), false, expr.SourceExpression), expr.SourceExpression);
        }

        private SqlExpression ConvertDoubleToString(SqlExpression expr, Type resultClrType)
        {
            SqlExpression expression = sql.FunctionCall(typeof(void), "NVARCHAR", new[] { sql.ValueFromObject(30, false, expr.SourceExpression) }, expr.SourceExpression);
            return sql.FunctionCall(resultClrType, "CONVERT", new[] { expression, expr, sql.ValueFromObject(2, false, expr.SourceExpression) }, expr.SourceExpression);
        }

        private static bool OldWillFitInNew(IProviderType oldSqlType, IProviderType newSqlType)
        {
            if (newSqlType.IsLargeType)
                return true;
            if (!newSqlType.HasSizeOrIsLarge)
                return true;

            if (oldSqlType.IsLargeType)
                return false;
            if (!oldSqlType.HasSizeOrIsLarge)
                return false;

            var nullable = newSqlType.Size;
            var nullable2 = oldSqlType.Size;

            if (nullable.GetValueOrDefault() < nullable2.GetValueOrDefault())
                return false;

            return (nullable.HasValue && nullable2.HasValue);
        }

        private bool StringConversionIsNeeded(IProviderType oldSqlType, IProviderType newSqlType)
        {
            if (!BothTypesAreStrings(oldSqlType, newSqlType))
            {
                return true;
            }
            var flag = oldSqlType.IsFixedSize || newSqlType.IsFixedSize;
            return (!newSqlType.HasSizeOrIsLarge || (OldWillFitInNew(oldSqlType, newSqlType) && flag));
        }

        private bool StringConversionIsSafe(IProviderType oldSqlType, IProviderType newSqlType)
        {
            if (BothTypesAreStrings(oldSqlType, newSqlType) && newSqlType.HasSizeOrIsLarge)
            {
                return OldWillFitInNew(oldSqlType, newSqlType);
            }
            return true;
        }

        internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
        {
            uo.Operand = VisitExpression(uo.Operand);
            if (uo.NodeType == SqlNodeType.Convert)
            {
                IProviderType sqlType = uo.Operand.SqlType;
                IProviderType newSqlType = uo.SqlType;
                Type nonNullableType = TypeSystem.GetNonNullableType(uo.Operand.ClrType);
                Type clrType = TypeSystem.GetNonNullableType(uo.ClrType);
                if (clrType == typeof(char))
                {
                    if (nonNullableType == typeof(bool))
                    {
                        throw Error.ConvertToCharFromBoolNotSupported();
                    }
                    if (sqlType.IsNumeric)
                    {
                        return sql.FunctionCall(uo.ClrType, "NCHAR", new[] { uo.Operand }, uo.SourceExpression);
                    }
                    if (!StringConversionIsSafe(sqlType, newSqlType))
                    {
                        throw Error.UnsafeStringConversion(sqlType.ToQueryString(), newSqlType.ToQueryString());
                    }
                    if (StringConversionIsNeeded(sqlType, newSqlType))
                    {
                        uo.SetSqlType(sql.TypeProvider.From(uo.ClrType, sqlType.HasSizeOrIsLarge ? sqlType.Size : null));
                    }
                    return uo;
                }
                if (((nonNullableType == typeof(char)) && (sqlType.IsChar || sqlType.IsString)) && newSqlType.IsNumeric)
                {
                    //return sql.FunctionCall(clrType, sql.TypeProvider.From(typeof(int)), "UNICODE", new[] { uo.Operand }, uo.SourceExpression);
                    return sql.UNICODE(clrType, uo);
                }
                if (clrType != typeof(string))
                {
                    return uo;
                }
                if (nonNullableType == typeof(double))
                {
                    return ConvertDoubleToString(uo.Operand, uo.ClrType);
                }
                if (nonNullableType == typeof(bool))
                {
                    return ConvertBitToString(uo.Operand, uo.ClrType);
                }
                if (!StringConversionIsSafe(sqlType, newSqlType))
                {
                    throw Error.UnsafeStringConversion(sqlType.ToQueryString(), newSqlType.ToQueryString());
                }
                if (StringConversionIsNeeded(sqlType, newSqlType))
                {
                    uo.SetSqlType(sql.TypeProvider.From(uo.ClrType, sqlType.HasSizeOrIsLarge ? sqlType.Size : null));
                }
            }
            return uo;
        }





















    }
}