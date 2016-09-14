using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.PostgreSQL
{
    class PgsqlFactory : SqlFactory
    {
        internal PgsqlFactory(ITypeSystemProvider typeProvider, MetaModel model)
            : base(typeProvider, model)
        {
        }

        internal override SqlExpression DateTime_AddDays(SqlMethodCall mc)
        {
            return DateTime_Interval(mc, " days");
        }

        internal override SqlExpression DateTime_AddHours(SqlMethodCall mc)
        {
            return DateTime_Interval(mc, " hours");
        }

        internal override SqlExpression DateTime_AddMinutes(SqlMethodCall mc)
        {
            return DateTime_Interval(mc, " minutes");
        }

        internal override SqlExpression DateTime_AddMonths(SqlMethodCall mc)
        {
            return DateTime_Interval(mc, " months");
        }

        internal override SqlExpression DateTime_AddSeconds(SqlMethodCall mc)
        {
            return DateTime_Interval(mc, " seconds");
        }

        internal override SqlExpression DateTime_AddYears(SqlMethodCall mc)
        {
            return DateTime_Interval(mc, " years");
        }

        internal override SqlExpression DateTime_Add(SqlMethodCall mc)
        {
            var t = (TimeSpan)((SqlValue)mc.Arguments[0]).Value;
            var sb = new StringBuilder();

            sb.Append(t.Days);
            sb.Append(" days");

            if (t.Hours > 0)
            {
                sb.Append(" ");
                sb.Append(t.Hours);
                sb.Append(" hours");
            }
            if (t.Minutes > 0)
            {
                sb.Append(" ");
                sb.Append(t.Minutes);
                sb.Append(" minutes");
            }
            if (t.Seconds > 0)
            {
                sb.Append(" ");
                sb.Append(t.Seconds);
                sb.Append(" seconds");
            }

            var f = FunctionCall(mc.ClrType, "INTERVAL", new[] { ValueFromObject(sb.ToString(), mc.SourceExpression) }, mc.SourceExpression);
            f.Comma = false;
            f.Brackets = false;
            return Binary(SqlNodeType.Add, mc.Object, f);
        }

        internal override SqlExpression DateTime_Date(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(typeof(DateTime), "date_trunc", new[] { ValueFromObject("day", false, expr.SourceExpression), expr }, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_Day(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(typeof(int), "date_part", new[] { ValueFromObject("day", false, expr.SourceExpression), expr }, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_Hour(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(typeof(int), "date_part", new[] { ValueFromObject("hour", false, expr.SourceExpression), expr }, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_Minute(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(typeof(int), "date_part", new[] { ValueFromObject("minute", false, expr.SourceExpression), expr }, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_Month(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(typeof(int), "date_part", new[] { ValueFromObject("month", false, expr.SourceExpression), expr }, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_Second(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(typeof(int), "date_part", new[] { ValueFromObject("second", false, expr.SourceExpression), expr }, expr.SourceExpression);
        }

        internal override SqlNode DateTime_Year(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(typeof(int), "date_part", new[] { ValueFromObject("year", false, expr.SourceExpression), expr }, expr.SourceExpression);
        }

        internal override SqlExpression DATALENGTH(SqlExpression expr)
        {
            return FunctionCall(typeof(int), "LENGTH", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_DayOfWeek(SqlMember m, SqlExpression expr)
        {
            var args = new[] { VariableFromName("DOW", expr.SourceExpression), VariableFromName("FROM", expr.SourceExpression), expr };
            var f = FunctionCall(typeof(DayOfWeek), "EXTRACT", args, expr.SourceExpression);
            f.Comma = false;
            return f;
        }

        internal override SqlExpression DateTime_DayOfYear(SqlMember m, SqlExpression expr)
        {
            var args = new[] { VariableFromName("DOY", expr.SourceExpression), VariableFromName("FROM", expr.SourceExpression), expr };
            var f = FunctionCall(typeof(int), "EXTRACT", args, expr.SourceExpression);
            f.Comma = false;
            return f;
        }

        internal override SqlNode DateTime_TimeOfDay(SqlMember member, SqlExpression expr)
        {
            var h = DateTime_Hour(member, expr);
            var m = DateTime_Minute(member, expr);//DATEPART("Minute", expr);
            var s = DateTime_Second(member, expr);//DATEPART("Second", expr);
            //var expression11 = DATEPART("MILLISECOND", expr);
            h = Multiply(h, 0x861c46800L);
            m = Multiply(m, 0x23c34600L);
            s = Multiply(s, 0x989680L);
            //var expression15 = Multiply(ConvertToBigint(expression11), 0x2710L);
            return ConvertTo(typeof(TimeSpan), Add(new[] { h, m, s }));
        }

        SqlExpression DateTime_Interval(SqlMethodCall mc, string str)
        {
            SqlExpression a = mc.Arguments[0].NodeType == SqlNodeType.Value ?
                  ValueFromObject(((SqlValue)mc.Arguments[0]).Value + str, mc.SourceExpression) :
                  Binary(SqlNodeType.Add, mc.Arguments[0], ValueFromObject(str, mc.SourceExpression), typeof(string));

            var f = FunctionCall(mc.ClrType, "INTERVAL", new[] { a }, mc.SourceExpression);
            f.Comma = false;
            f.Brackets = false;
            return Binary(SqlNodeType.Add, mc.Object, f);
        }

        internal override SqlExpression Math_Log10(SqlMethodCall mc)
        {
            return CreateFunctionCallStatic1(mc.ClrType, "log", mc.Arguments, mc.SourceExpression);
        }

        internal override SqlExpression Math_Atan2(SqlMethodCall mc, Expression sourceExpression)
        {
            return FunctionCall(mc.ClrType, "atan2", new[] { mc.Arguments[0], mc.Arguments[1] }, mc.SourceExpression);
        }

        internal override SqlExpression Math_Truncate(SqlMethodCall mc)
        {
            return FunctionCall(mc.ClrType, "trunc", new[] { mc.Arguments[0] }, mc.SourceExpression);
        }

        internal override SqlExpression String_Length(SqlExpression expr)
        {
            var f = this.FunctionCall(typeof(int), "LENGTH", new[] { expr }, expr.SourceExpression);
            return f;
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
            args = new[] { mc.Object, this.Add(mc.Arguments[0], 1) };//, this.String_Length(mc.Object) 
            var f = this.FunctionCall(typeof(string), "SUBSTR", args, mc.SourceExpression);
            return f;
        }

        internal override SqlExpression String_IndexOf(SqlMethodCall mc, Expression sourceExpression)
        {
            if (mc.Arguments.Count == 1)
            {
                if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
                {
                    throw Error.ArgumentNull("value");
                }
                var when = new SqlWhen(Binary(SqlNodeType.EQ, String_Length(mc.Arguments[0]), ValueFromObject(0, sourceExpression)), ValueFromObject(0, sourceExpression));
                SqlExpression expression9 = Subtract(FunctionCall(typeof(int), "STRPOS", new[] { mc.Arguments[0], mc.Object }, sourceExpression), 1);
                return SearchedCase(new SqlWhen[] { when }, expression9, sourceExpression);
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
                SqlExpression expression10 = Binary(SqlNodeType.EQ, String_Length(mc.Arguments[0]), ValueFromObject(0, sourceExpression));
                SqlWhen when2 = new SqlWhen(AndAccumulate(expression10, Binary(SqlNodeType.LE, Add(mc.Arguments[1], 1), String_Length(mc.Object))), mc.Arguments[1]);
                SqlExpression expression11 = Subtract(FunctionCall(typeof(int), "STRPOS", new[] { mc.Arguments[0], mc.Object, Add(mc.Arguments[1], 1) }, sourceExpression), 1);
                return SearchedCase(new[] { when2 }, expression11, sourceExpression);
            }
            if (mc.Arguments.Count != 3)
            {
                //throw GetMethodSupportException(mc);
                SqlClient.Error.MethodHasNoSupportConversionToSql(mc);
                //goto Label_1B30;
            }
            if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
            {
                throw Error.ArgumentNull("value");
            }
            if (mc.Arguments[2].ClrType == typeof(StringComparison))
            {
                throw SqlClient.Error.IndexOfWithStringComparisonArgNotSupported();
            }
            SqlExpression left = Binary(SqlNodeType.EQ, String_Length(mc.Arguments[0]), ValueFromObject(0, sourceExpression));
            SqlWhen when3 = new SqlWhen(AndAccumulate(left, Binary(SqlNodeType.LE, Add(mc.Arguments[1], 1), String_Length(mc.Object))), mc.Arguments[1]);
            SqlExpression expression13 = FunctionCall(typeof(string), "SUBSTRING", new[] { mc.Object, ValueFromObject(1, false, sourceExpression), Add(new[] { mc.Arguments[1], mc.Arguments[2] }) }, sourceExpression);
            SqlExpression @else = Subtract(FunctionCall(typeof(int), "STRPOS", new[] { mc.Arguments[0], expression13, Add(mc.Arguments[1], 1) }, sourceExpression), 1);
            return SearchedCase(new[] { when3 }, @else, sourceExpression);
        }

        internal override SqlExpression MakeCoalesce(SqlExpression left, SqlExpression right, Type type, Expression sourceExpression)
        {
            return FunctionCall(type, "COALESCE", new[] { left, right }, sourceExpression);
        }

        internal override SqlExpression String_Remove(SqlMethodCall mc)
        {
            var sourceExpression = mc.SourceExpression;
            if (mc.Arguments.Count != 1)
            {
                if (mc.Arguments.Count == 2)
                {
                    //return this.FunctionCall(typeof(string), "STUFF", new[] { mc.Object, this.Add(mc.Arguments[0], 1), mc.Arguments[1], this.ValueFromObject("", false, sourceExpression) }, sourceExpression);
                    var f1 = FunctionCall(typeof(string), "SUBSTR", new[] { mc.Object, ValueFromObject(1, sourceExpression), mc.Arguments[1] },
                                          sourceExpression);
                    var arg = Binary(SqlNodeType.Add, Binary(SqlNodeType.Add, mc.Arguments[0], mc.Arguments[1]), ValueFromObject(1, sourceExpression));
                    var f2 = FunctionCall(typeof(string), "SUBSTR", new[] { mc.Object, arg }, sourceExpression);
                    return Add(f1, f2);
                }
                throw SqlClient.Error.MethodHasNoSupportConversionToSql(mc.Method);
            }
            return FunctionCall(typeof(string), "SUBSTR", new[] { mc.Object, mc.Arguments[0] }, sourceExpression);

        }

        internal override SqlExpression UNICODE(Type clrType, SqlUnary uo)
        {
            return FunctionCall(clrType, TypeProvider.From(typeof(int)), "ASCII", new[] { uo.Operand }, uo.SourceExpression);
        }

        internal override MethodSupport GetStringMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.Name == "IndexOf" || mc.Method.Name == "Insert")
                return MethodSupport.None;

            return base.GetStringMethodSupport(mc);
        }

        internal override MethodSupport GetSqlMethodsMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.Name.StartsWith("DateDiff"))
                return MethodSupport.None;

            return base.GetSqlMethodsMethodSupport(mc);
        }
    }
}