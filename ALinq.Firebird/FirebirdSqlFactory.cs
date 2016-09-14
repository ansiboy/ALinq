using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.Firebird
{
    class FirebirdSqlFactory : SqlFactory
    {
        internal FirebirdSqlFactory(ITypeSystemProvider typeProvider, MetaModel model)
            : base(typeProvider, model)
        {
        }

        internal override SqlExpression Math_Truncate(SqlMethodCall mc)
        {
            return FunctionCall(mc.Method.ReturnType, "TRUNC", mc.Arguments, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_Add(SqlMethodCall mc)
        {
            SqlExpression arg;
            if (mc.Arguments[0].NodeType == SqlNodeType.Value)
                arg = ValueFromObject(((TimeSpan)((SqlValue)mc.Arguments[0]).Value).Ticks / 10000000, true, mc.SourceExpression);
            else
                arg = Binary(SqlNodeType.Div, mc.Arguments[0],
                             ValueFromObject(10000000, mc.SourceExpression));
            //var arg1 = new FbFunctionArgument(mc.SourceExpression, arg, "SECOND", "TO", mc.Object);
            var expr = mc.SourceExpression;
            var args = new[] { arg, VariableFromName("SECOND", expr), VariableFromName("TO", expr), mc.Object };
            return FbFunctionCall(mc.ClrType, "DATEADD", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_Subtract(SqlMethodCall mc)
        {
            SqlExpression arg;
            if (mc.Arguments[0].NodeType == SqlNodeType.Value)
                arg = ValueFromObject(((TimeSpan)((SqlValue)mc.Arguments[0]).Value).Ticks / 10000000, true, mc.SourceExpression);
            else
                arg = Binary(SqlNodeType.Div, mc.Arguments[0],
                             ValueFromObject(10000000, mc.SourceExpression));
            arg = Binary(SqlNodeType.Sub, ValueFromObject(0, mc.SourceExpression), arg);
            var expr = mc.SourceExpression;
            var args = new[] { arg, VariableFromName("SECOND", expr), VariableFromName("TO", expr), mc.Object };
            return FbFunctionCall(mc.ClrType, "DATEADD", args, mc.SourceExpression);
        }

        internal SqlFunctionCall FbFunctionCall(Type clrType, string name, IEnumerable<SqlExpression> args, Expression source)
        {
            return new SqlFunctionCall(clrType, Default(clrType), name, args, source) { Comma = false };
        }

        //internal static SqlFunctionCall FbFunctionCall(Type clrType, ProviderType sqlType, string name, IEnumerable<SqlExpression> args, Expression source)
        //{
        //    return new SqlFunctionCall(clrType, sqlType, name, args, source);
        //}

        internal override SqlExpression DateTime_AddDays(SqlMethodCall mc)
        {
            //var arg = new FbFunctionArgument(mc.SourceExpression, mc.Arguments[0], "DAY", "TO", mc.Object);
            //return FunctionCall(mc.ClrType, "DATEADD", new[] { arg }, mc.SourceExpression);
            var expr = mc.SourceExpression;
            var args = new[] { mc.Arguments[0], VariableFromName("DAY", expr), 
                               VariableFromName("TO", expr), mc.Object };
            return FbFunctionCall(mc.ClrType, "DATEADD", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddHours(SqlMethodCall mc)
        {
            //var arg = new FbFunctionArgument(mc.SourceExpression, mc.Arguments[0], "HOUR", "TO", mc.Object);
            var expr = mc.SourceExpression;
            var args = new[]{ mc.Arguments[0], VariableFromName("HOUR",expr),
                              VariableFromName("TO",expr), mc.Object};
            return FbFunctionCall(mc.ClrType, "DATEADD", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddMinutes(SqlMethodCall mc)
        {
            var expr = mc.SourceExpression;
            var args = new[]{ mc.Arguments[0], VariableFromName("MINUTE",expr),
                              VariableFromName("TO",expr), mc.Object};
            return FbFunctionCall(mc.ClrType, "DATEADD", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddMonths(SqlMethodCall mc)
        {
            var expr = mc.SourceExpression;
            var args = new[]{ mc.Arguments[0], VariableFromName("MONTH",expr),
                              VariableFromName("TO",expr), mc.Object};
            return FbFunctionCall(mc.ClrType, "DATEADD", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddSeconds(SqlMethodCall mc)
        {
            var expr = mc.SourceExpression;
            var args = new[]{ mc.Arguments[0], VariableFromName("SECOND",expr),
                              VariableFromName("TO",expr), mc.Object};
            return FbFunctionCall(mc.ClrType, "DATEADD", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddYears(SqlMethodCall mc)
        {
            var expr = mc.SourceExpression;
            var args = new[]{ mc.Arguments[0], VariableFromName("YEAR",expr),
                              VariableFromName("TO",expr), mc.Object};
            return FbFunctionCall(mc.ClrType, "DATEADD", args, mc.SourceExpression);
        }

        internal override SqlExpression String_Substring(SqlMethodCall mc)
        {
            SqlExpression[] args;

            if (mc.Arguments.Count != 1)
            {
                if (mc.Arguments.Count == 2)
                {
                    var value1 = (int)((SqlValue)mc.Arguments[0]).Value + 1;
                    var value2 = (int)((SqlValue)mc.Arguments[1]).Value;
                    args = new[] { mc.Object, ValueFromObject(value1,mc.SourceExpression),
                                       ValueFromObject(value2, mc.SourceExpression) };
                    //args = new[] { mc.Object, sql.Add(mc.Arguments[0], 1), mc.Arguments[1] };
                    return FunctionCall(typeof(string), "SUBSTRING", args, mc.SourceExpression);
                }
                throw SqlClient.Error.MethodHasNoSupportConversionToSql(mc);//GetMethodSupportException(mc);
            }
            var arg = (SqlValue)mc.Arguments[0];
            args = new[] { mc.Object, Add(ValueFromObject(arg.Value, arg.SourceExpression), 1), CLRLENGTH(mc.Object) };
            return FunctionCall(typeof(string), "SUBSTRING", args, mc.SourceExpression);
        }

        internal override SqlExpression String_Insert(SqlMethodCall mc)
        {
            var clrType = mc.Method.ReturnType;
            var startIndex = (int)((SqlValue)mc.Arguments[0]).Value;//(int)((ConstantExpression)mc.Arguments[0]).Value;
            var insertString = (string)((SqlValue)mc.Arguments[1]).Value;//(string)((ConstantExpression)mc.Arguments[1]).Value;

            Debug.Assert(clrType == typeof(string));
            Debug.Assert(startIndex >= 0);

            var arg1 = mc.Object;//VisitExpression(mc.Object);
            var arg2 = ValueFromObject(startIndex, mc.SourceExpression);//VisitExpression(Expression.Constant(startIndex));
            var left = new SqlFunctionCall(clrType, TypeProvider.From(clrType), "LEFT",
                                           new[] { arg1, arg2 }, mc.SourceExpression);
            //return left;
            var result = new SqlBinary(SqlNodeType.Add, typeof(string), TypeProvider.From(typeof(string)),
                                       left, ValueFromObject(insertString, mc.SourceExpression));

            var len = new SqlFunctionCall(typeof(int), TypeProvider.From(typeof(int)), "CHAR_LENGTH",
                                          new[] { (mc.Object) }, mc.SourceExpression);
            var binary = new SqlBinary(SqlNodeType.Sub, typeof(int), TypeProvider.From(typeof(int)), len, arg2);
            var right = new SqlFunctionCall(typeof(string), TypeProvider.From(typeof(string)), "RIGHT",
                                            new[] { arg1, binary }, mc.SourceExpression);
            result = new SqlBinary(SqlNodeType.Add, typeof(string), TypeProvider.From(typeof(string)), result, right);
            return result;
        }

        internal override SqlExpression Math_Max(SqlMethodCall mc)
        {
            return FunctionCall(mc.ClrType, "MAXVALUE", mc.Arguments, mc.SourceExpression);
        }

        internal override SqlExpression Math_Min(SqlMethodCall mc)
        {
            return FunctionCall(mc.ClrType, "MINVALUE", mc.Arguments, mc.SourceExpression);
        }

        internal override SqlExpression String_Trim(SqlMethodCall mc)
        {
            return FunctionCall(mc.ClrType, "TRIM", new[] { mc.Object }, mc.SourceExpression);
        }

        internal override SqlExpression String_TrimEnd(SqlMethodCall mc)
        {
            //var arg0 = new FbFunctionArgument(mc.SourceExpression, "TRAILING", "From", mc.Object);
            var expr = mc.SourceExpression;
            var args = new[] { VariableFromName("TRAILING", expr), VariableFromName("FROM", expr), mc.Object };
            return FbFunctionCall(mc.ClrType, "TRIM", args, mc.SourceExpression);
        }

        internal override SqlExpression String_TrimStart(SqlMethodCall mc)
        {
            var expr = mc.SourceExpression;
            var args = new[] { VariableFromName("LEADING", expr), VariableFromName("FROM", expr), mc.Object };
            return FbFunctionCall(mc.ClrType, "TRIM", args, mc.SourceExpression);
        }

        internal override SqlExpression DATEPART(string partName, SqlExpression expr)
        {
            //var arg = new FbFunctionArgument(expr.SourceExpression, partName.ToUpper(), "FROM", expr);
            //return FunctionCall(typeof(int), "EXTRACT", new[] { arg }, expr.SourceExpression);
            //var args = new[] { new SqlVariable(typeof(void), null, partName.ToUpper(), expr.SourceExpression), expr };
            if (partName == "DayOfYear")
            {
                partName = "YEARDAY";
                var args = new[] { VariableFromName(partName.ToUpper(), expr.SourceExpression), 
                               VariableFromName("FROM",expr.SourceExpression) ,expr };
                var func = FunctionCall(typeof(int), "EXTRACT", args, expr.SourceExpression);
                func.Comma = false;
                return Add(func, 1);
            }
            else
            {
                var args = new[] { VariableFromName(partName.ToUpper(), expr.SourceExpression), 
                               VariableFromName("FROM",expr.SourceExpression) ,expr };
                var func = FunctionCall(typeof(int), "EXTRACT", args, expr.SourceExpression);
                func.Comma = false;
                return func;
            }

        }

        internal override SqlExpression UNICODE(Type clrType, SqlUnary uo)
        {
            return FunctionCall(clrType, TypeProvider.From(typeof(int)), "ASCII_VAL", new[] { uo.Operand }, uo.SourceExpression);
        }

        internal override SqlExpression DateTime_DayOfWeek(SqlMember m, SqlExpression expr)
        {
            var args = new[] { VariableFromName("WEEKDAY", expr.SourceExpression), 
                               VariableFromName("FROM",expr.SourceExpression) ,expr };
            var func = FunctionCall(typeof(DayOfWeek), TypeProvider.From(typeof(DayOfWeek)), "EXTRACT", args, expr.SourceExpression);
            func.Comma = false;
            return func;
        }

        internal override SqlNode DateTime_TimeOfDay(SqlMember member, SqlExpression expr)
        {
            var h = DATEPART("Hour", expr);
            var m = DATEPART("Minute", expr);
            var s = DATEPART("Second", expr);
            //var expression11 = DATEPART("MILLISECOND", expr);
            h = Multiply(h, 0x861c46800L);
            m = Multiply(m, 0x23c34600L);
            s = Multiply(s, 0x989680L);
            //var expression15 = Multiply(ConvertToBigint(expression11), 0x2710L);
            return ConvertTo(typeof(TimeSpan), Add(new[] { h, m, s }));

        }

        internal override SqlExpression MakeCoalesce(SqlExpression left, SqlExpression right, Type type, Expression sourceExpression)
        {
            return FunctionCall(type, "COALESCE", new[] { left, right }, sourceExpression);
        }

        internal override SqlExpression Math_Log10(SqlMethodCall mc)
        {
            Debug.Assert(mc.Arguments.Count == 1);
            var value = ValueFromObject(10, mc.SourceExpression);
            mc.Arguments.Insert(0, value);
            return CreateFunctionCallStatic2(typeof(double), "LOG", mc.Arguments, mc.SourceExpression);

        }

        internal override SqlExpression Math_Log(SqlMethodCall mc)
        {
            Debug.Assert(mc.Method.Name == "Log");
            if (mc.Arguments.Count == 1)
            {
                var value = ValueFromObject(Math.E, mc.SourceExpression);
                mc.Arguments.Insert(0, value);
            }
            return CreateFunctionCallStatic2(typeof(double), "LOG", mc.Arguments, mc.SourceExpression);
        }

        internal override MethodSupport GetSqlMethodsMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.Name.StartsWith("DateDiff"))
                return MethodSupport.None;

            return base.GetSqlMethodsMethodSupport(mc);
        }

        internal override MethodSupport GetDateTimeMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.Name == "Subtract")
                return MethodSupport.Method;

            return base.GetDateTimeMethodSupport(mc);
        }
    }
}
