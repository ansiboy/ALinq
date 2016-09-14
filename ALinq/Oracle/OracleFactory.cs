using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.Oracle
{
    class OracleFactory : SqlFactory
    {
        internal OracleFactory(ITypeSystemProvider typeProvider, MetaModel model)
            : base(typeProvider, model)
        {
        }

        internal override SqlExpression DATALENGTH(SqlExpression expr)
        {
            return this.FunctionCall(typeof(int), "LENGTH", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression String_Length(SqlExpression expr)
        {
            return this.FunctionCall(typeof(int), "LENGTH", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_Add(SqlMethodCall mc)
        {
            var arg = Binary(SqlNodeType.Div, mc.Arguments[0],
                 ValueFromObject(10000000, mc.SourceExpression));
            var div = Binary(SqlNodeType.Div, arg, ValueFromObject(24 * 60 * 60, mc.SourceExpression));
            mc.Arguments[0] = div;
            return DateTime_AddDays(mc);
        }

        internal override SqlExpression DateTime_Subtract(SqlMethodCall mc)
        {
            var arg = Binary(SqlNodeType.Div, mc.Arguments[0],
                 ValueFromObject(10000000, mc.SourceExpression));
            var div = Binary(SqlNodeType.Div, arg, ValueFromObject(24 * 60 * 60, mc.SourceExpression));
            mc.Arguments[0] = div;
            return DateTime_SubtractDays(mc);
        }

        internal override SqlExpression DateTime_SubtractDays(SqlMethodCall mc)
        {
            var s = mc.SourceExpression;
            var args = new[] { mc.Object, VariableFromName("-", s), mc.Arguments[0] };
            var func = FunctionCall(typeof(DateTime), string.Empty, args, s);
            func.Comma = false;
            return func;
        }

        internal override SqlExpression DateTime_AddDays(SqlMethodCall mc)
        {
            var s = mc.SourceExpression;
            var args = new[] { mc.Object, VariableFromName("+", s), mc.Arguments[0] };
            var func = FunctionCall(typeof(DateTime), string.Empty, args, s);
            func.Comma = false;
            return func;
        }

        internal override SqlExpression DateTime_AddMonths(SqlMethodCall mc)
        {
            var args = new[] { mc.Object, mc.Arguments[0] };
            return FunctionCall(mc.ClrType, "ADD_MONTHS", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddYears(SqlMethodCall mc)
        {
            var mul = Binary(SqlNodeType.Mul, mc.Arguments[0], ValueFromObject(12, mc.SourceExpression));
            mc.Arguments[0] = mul;
            return DateTime_AddMonths(mc);
        }

        internal override SqlExpression DateTime_AddHours(SqlMethodCall mc)
        {
            var div = Binary(SqlNodeType.Div, mc.Arguments[0], ValueFromObject(24, mc.SourceExpression));
            mc.Arguments[0] = div;
            return DateTime_AddDays(mc);
        }

        internal override SqlExpression DateTime_AddMinutes(SqlMethodCall mc)
        {
            var div = Binary(SqlNodeType.Div, mc.Arguments[0], ValueFromObject(24 * 60, mc.SourceExpression));
            mc.Arguments[0] = div;
            return DateTime_AddDays(mc);
        }

        internal override SqlExpression DateTime_AddSeconds(SqlMethodCall mc)
        {
            var div = Binary(SqlNodeType.Div, mc.Arguments[0], ValueFromObject(24 * 60 * 60, mc.SourceExpression));
            mc.Arguments[0] = div;
            return DateTime_AddDays(mc);
        }

        internal override SqlExpression DateTime_ToString(SqlMethodCall mc)
        {
            var args = new[] { mc.Object, mc.Arguments[0] };
            return FunctionCall(mc.ClrType, "TO_CHAR", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_Date(SqlMember m, SqlExpression expr)
        {
            var s = expr.SourceExpression;
            var func = new SqlFunctionCall(typeof(string), TypeProvider.From(typeof(string)), "TO_CHAR",
                                          new[] { expr, ValueFromObject("YYYY-MM-DD", s) }, s);
            //return func;
            return new SqlFunctionCall(typeof(DateTime), TypeProvider.From(typeof(DateTime)),
                                       "TO_DATE", new[] { func, ValueFromObject("YYYY-MM-DD", s) }, s);
        }

        internal override SqlExpression Math_Truncate(SqlMethodCall mc)
        {
            var args = new[] { mc.Arguments[0], ValueFromObject(0, false, mc.SourceExpression),
                               ValueFromObject(1, false, mc.SourceExpression) };
            return FunctionCall(mc.Method.ReturnType, "TRUNC", mc.Arguments, mc.SourceExpression);
        }

        internal override SqlExpression Math_Cosh(SqlMethodCall mc, Expression sourceExpression)
        {
            return FunctionCall(typeof(double), "COSH", new[] { mc.Arguments[0] }, sourceExpression);
        }

        internal override SqlExpression Math_Sinh(SqlMethodCall mc, Expression sourceExpression)
        {
            return FunctionCall(typeof(double), "SINH", new[] { mc.Arguments[0] }, sourceExpression);
        }

        internal override SqlExpression String_Substring(SqlMethodCall mc)
        {
            SqlExpression[] args;

            if (mc.Arguments.Count != 1)
            {
                if (mc.Arguments.Count == 2)
                {
                    args = new[] { mc.Object, this.Add(mc.Arguments[0], 1), mc.Arguments[1] };
                    return this.FunctionCall(typeof(string), "SUBSTR", args, mc.SourceExpression);
                }
                throw SqlClient.Error.MethodHasNoSupportConversionToSql(mc);//GetMethodSupportException(mc);
            }
            args = new[] { mc.Object, this.Add(mc.Arguments[0], 1), this.CLRLENGTH(mc.Object) };
            return this.FunctionCall(typeof(string), "SUBSTR", args, mc.SourceExpression);
        }

        internal override SqlExpression String_GetChar(SqlMethodCall mc, Expression sourceExpression)
        {
            if (mc.Arguments.Count != 1)
            {
                throw SqlClient.Error.MethodHasNoSupportConversionToSql(mc);
            }
            var args = new[] { mc.Object, Add(mc.Arguments[0], 1), 
                               ValueFromObject(1, false, sourceExpression) };
            return FunctionCall(typeof(char), "SUBSTR", args, sourceExpression);
        }

        internal override SqlExpression String_IndexOf(SqlMethodCall mc, Expression sourceExpression)
        {
            if (mc.Arguments.Count == 1)
            {
                if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
                {
                    throw Error.ArgumentNull("value");
                }
                var when = new SqlWhen(Binary(SqlNodeType.EQ, CLRLENGTH(mc.Arguments[0]), ValueFromObject(0, sourceExpression)), ValueFromObject(0, sourceExpression));
                SqlExpression expression9 = Subtract(FunctionCall(typeof(int), "INSTR", new[] { mc.Arguments[0], mc.Object }, sourceExpression), 1);
                return SearchedCase(new[] { when }, expression9, sourceExpression);
            }
            if (mc.Arguments.Count == 2)
            {
                if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
                {
                    throw Error.ArgumentNull("value");
                }
                if (mc.Arguments[1].ClrType == typeof(StringComparison))
                {
                    throw SqlClient.Error.IndexOfWithStringComparisonArgNotSupported();
                }
                SqlExpression expression10 = Binary(SqlNodeType.EQ, CLRLENGTH(mc.Arguments[0]), ValueFromObject(0, sourceExpression));
                SqlWhen when2 = new SqlWhen(AndAccumulate(expression10, Binary(SqlNodeType.LE, Add(mc.Arguments[1], 1), CLRLENGTH(mc.Object))), mc.Arguments[1]);
                SqlExpression expression11 = Subtract(FunctionCall(typeof(int), "INSTR", new[] { mc.Arguments[0], mc.Object, Add(mc.Arguments[1], 1) }, sourceExpression), 1);
                return SearchedCase(new[] { when2 }, expression11, sourceExpression);
            }
            if (mc.Arguments.Count != 3)
            {
                SqlClient.Error.MethodHasNoSupportConversionToSql(mc);
            }
            if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
            {
                throw Error.ArgumentNull("value");
            }
            if (mc.Arguments[2].ClrType == typeof(StringComparison))
            {
                throw SqlClient.Error.IndexOfWithStringComparisonArgNotSupported();
            }
            SqlExpression left = Binary(SqlNodeType.EQ, CLRLENGTH(mc.Arguments[0]), ValueFromObject(0, sourceExpression));
            var when3 = new SqlWhen(AndAccumulate(left, Binary(SqlNodeType.LE, Add(mc.Arguments[1], 1), CLRLENGTH(mc.Object))), mc.Arguments[1]);
            var expression13 = FunctionCall(typeof(string), "SUBSTR", new[] { mc.Object, ValueFromObject(1, false, sourceExpression), Add(new[] { mc.Arguments[1], mc.Arguments[2] }) }, sourceExpression);
            var @else = Subtract(FunctionCall(typeof(int), "INSTR", new[] { mc.Arguments[0], expression13, Add(mc.Arguments[1], 1) }, sourceExpression), 1);
            return SearchedCase(new[] { when3 }, @else, sourceExpression);
        }

        internal override SqlExpression UNICODE(Type clrType, SqlUnary uo)
        {
            return FunctionCall(clrType, TypeProvider.From(typeof(int)), "ASCII", new[] { uo.Operand }, uo.SourceExpression);
        }

        internal override MethodSupport GetConvertMethodSupport(SqlMethodCall mc)
        {
            if ((mc.Method.IsStatic && (mc.Method.DeclaringType == typeof(Convert))) && (mc.Arguments.Count == 1))
            {
                switch (mc.Method.Name)
                {
                    case "ToBoolean":
                    case "ToDecimal":
                    //case "ToByte":
                    case "ToChar":
                    case "ToDouble":
                    case "ToInt16":
                    case "ToInt32":
                    case "ToInt64":
                    case "ToSingle":
                    case "ToString":
                        return MethodSupport.Method;

                    case "ToDateTime":
                        if ((mc.Arguments[0].ClrType != typeof(string)) && (mc.Arguments[0].ClrType != typeof(DateTime)))
                        {
                            return MethodSupport.MethodGroup;
                        }
                        return MethodSupport.Method;
                }
            }
            return MethodSupport.None;
        }

        internal override SqlExpression TranslateConvertStaticMethod(SqlMethodCall mc)
        {
            if (mc.Arguments.Count != 1)
            {
                return null;
            }
            var expr = mc.Arguments[0];
            Type type;
            switch (mc.Method.Name)
            {
                case "ToBoolean":
                    type = typeof(bool);
                    break;

                case "ToDecimal":
                    type = typeof(decimal);
                    break;

                case "ToByte":
                    type = typeof(byte);
                    break;

                case "ToChar":
                    type = typeof(char);
                    if (expr.SqlType.IsChar)
                    {
                        TypeProvider.From(type, 1);
                    }
                    break;

                case "ToDateTime":
                    {
                        var nonNullableType = TypeSystem.GetNonNullableType(expr.ClrType);
                        if ((nonNullableType != typeof(string)) && (nonNullableType != typeof(DateTime)))
                        {
                            throw SqlClient.Error.ConvertToDateTimeOnlyForDateTimeOrString();
                        }
                        type = typeof(DateTime);
                        break;
                    }
                case "ToDouble":
                    type = typeof(double);
                    break;

                case "ToInt16":
                    type = typeof(short);
                    break;

                case "ToInt32":
                    type = typeof(int);
                    break;

                case "ToInt64":
                    type = typeof(long);
                    break;

                case "ToSingle":
                    type = typeof(float);
                    break;

                case "ToString":
                    type = typeof(string);
                    break;

                case "ToSByte":
                    type = typeof(sbyte);
                    break;

                default:
                    throw SqlClient.Error.MethodHasNoSupportConversionToSql(mc);
            }

            if ((TypeProvider.From(type) != expr.SqlType) || ((expr.ClrType == typeof(bool)) && (type == typeof(int))))
            {
                if (type != typeof(DateTime))
                    return ConvertTo(type, TypeProvider.From(typeof(decimal)), expr);
                return ConvertTo(type, expr);
            }

            if ((type != expr.ClrType) && (TypeSystem.GetNonNullableType(type) == TypeSystem.GetNonNullableType(expr.ClrType)))
            {
                return new SqlLift(type, expr, expr.SourceExpression);
            }
            return expr;
        }

        internal override SqlExpression DATEPART(string partName, SqlExpression expr)
        {
            var args = new[] { expr };
            var s = expr.SourceExpression;
            SqlExpression arg;
            IProviderType sqlType = null;
            Type clrType = typeof(int);
            sqlType = TypeProvider.From(typeof(string));
            switch (partName.ToUpper())
            {
                case "HOUR":
                    arg = ValueFromObject("HH24", true, s);
                    break;
                case "MINUTE":
                    arg = ValueFromObject("MI", true, s);
                    break;
                case "SECOND":
                    arg = ValueFromObject("SS", true, s);
                    break;
                case "DAY":
                    args = new[] { VariableFromName("DAY FROM", s), expr };
                    return new SqlFunctionCall(clrType, sqlType, "EXTRACT", args, s) { Comma = false };
                case "MONTH":
                    args = new[] { VariableFromName("MONTH FROM", s), expr };
                    return new SqlFunctionCall(clrType, sqlType, "EXTRACT", args, s) { Comma = false };
                case "YEAR":
                    args = new[] { VariableFromName("YEAR FROM", s), expr };
                    return new SqlFunctionCall(clrType, sqlType, "EXTRACT", args, s) { Comma = false };
                case "DAYOFWEEK":
                    clrType = typeof(DayOfWeek);
                    sqlType = TypeProvider.From(typeof(int));
                    arg = ValueFromObject("D", true, s);
                    var func = new SqlFunctionCall(clrType, sqlType, "TO_CHAR", new[] { expr, arg }, s);
                    return new SqlBinary(SqlNodeType.Sub, clrType, sqlType, func, ValueFromObject(1, s));
                case "DAYOFYEAR":
                    arg = ValueFromObject("DDD", true, s);
                    break;
                case "TIMEOFDAY":
                    arg = ValueFromObject("DDD", true, s);
                    break;
                default:
                    throw SqlClient.Error.MethodHasNoSupportConversionToSql(partName);
            }
            return new SqlFunctionCall(clrType, sqlType, "TO_CHAR", new[] { expr, arg }, s);
        }

        internal override SqlExpression DateTime_DayOfWeek(SqlMember m, SqlExpression expr)
        {
            return this.DATEPART(m.Member.Name, expr);
        }

        internal override SqlNode DateTime_TimeOfDay(SqlMember member, SqlExpression expr)
        {
            var h = DATEPART("HOUR", expr);
            var m = DATEPART("MINUTE", expr);
            var s = DATEPART("SECOND", expr);
            h = Multiply(ConvertToBigint(h), 0x861c46800L);
            m = Multiply(ConvertToBigint(m), 0x23c34600L);
            s = Multiply(ConvertToBigint(s), 0x989680L);
            return ConvertTo(typeof(TimeSpan), Add(new[] { h, m, s }));

        }

        internal override SqlExpression MakeCoalesce(SqlExpression left, SqlExpression right, Type type, Expression sourceExpression)
        {
            return FunctionCall(type, "NVL", new[] { left, right }, sourceExpression);
        }

        internal override SqlExpression Math_Log(SqlMethodCall mc)
        {
            Debug.Assert(mc.Method.Name == "Log");
            if (mc.Arguments.Count == 1)
            {
                //mc.Arguments.Add(sql.Value(typeof(int),))
                var value = ValueFromObject(Math.E, mc.SourceExpression);
                mc.Arguments.Insert(0, value);
            }
            return CreateFunctionCallStatic2(typeof(double), "LOG", mc.Arguments, mc.SourceExpression);
        }

        internal override SqlExpression Math_Log10(SqlMethodCall mc)
        {
            Debug.Assert(mc.Arguments.Count == 1);
            var value = ValueFromObject(10, mc.SourceExpression);
            mc.Arguments.Insert(0, value);
            return CreateFunctionCallStatic2(mc.ClrType, "LOG", mc.Arguments, mc.SourceExpression);
        }

        internal override MethodSupport GetSqlMethodsMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.Name.StartsWith("DateDiff"))
                return MethodSupport.None;

            return base.GetSqlMethodsMethodSupport(mc);
        }

        internal override MethodSupport GetDateTimeMethodSupport(SqlMethodCall mc)
        {
            if (!mc.Method.IsStatic && (mc.Method.DeclaringType == typeof(DateTime)))
            {
                switch (mc.Method.Name)
                {
                    case "Subtract":
                        return MethodSupport.Method;
                        break;
                }
            }
            return base.GetDateTimeMethodSupport(mc);
        }
    }
}
