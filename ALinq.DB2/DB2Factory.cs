using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.DB2
{
    class DB2Factory : SqlFactory
    {
        internal DB2Factory(ITypeSystemProvider typeProvider, MetaModel model)
            : base(typeProvider, model)
        {
        }

        internal override SqlExpression UNICODE(Type clrType, SqlUnary uo)
        {
            return FunctionCall(clrType, TypeProvider.From(typeof(int)), "ASCII", new[] { uo.Operand }, uo.SourceExpression);
        }

        internal override SqlExpression DateTime_Add(SqlMethodCall mc)
        {
            var arg = mc.Arguments[0];
            var t = (TimeSpan)(((SqlValue)arg).Value);

            var args = new[]{
                               mc.Object,
                               ValueFromObject(t.Hours, mc.SourceExpression),
                               ValueFromObject(t.Minutes, mc.SourceExpression),
                               ValueFromObject(t.Seconds, mc.SourceExpression)
                            };

            return FunctionCall(typeof(DateTime), "AddTimeSpan", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddDays(SqlMethodCall mc)
        {
            return CreateDateTimeFunction(mc);
        }

        internal override SqlExpression DateTime_AddHours(SqlMethodCall mc)
        {
            return CreateDateTimeFunction(mc);
        }

        internal override SqlExpression DateTime_AddMinutes(SqlMethodCall mc)
        {
            return CreateDateTimeFunction(mc);
        }

        internal override SqlExpression DateTime_AddMonths(SqlMethodCall mc)
        {
            return CreateDateTimeFunction(mc);
        }

        internal override SqlExpression DateTime_AddSeconds(SqlMethodCall mc)
        {
            return CreateDateTimeFunction(mc);
        }

        internal override SqlExpression DateTime_AddYears(SqlMethodCall mc)
        {
            return CreateDateTimeFunction(mc);
        }

        SqlExpression CreateDateTimeFunction(SqlMethodCall mc)
        {
            var s = mc.SourceExpression;
            var args = new[] { mc.Object, mc.Arguments[0] };
            var func = FunctionCall(typeof(DateTime), mc.Method.Name, args, s);
            return func;
        }

        internal override SqlExpression DateTime_Date(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(expr.ClrType, "DATE", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_Day(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(typeof(int), "DAY", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_DayOfWeek(SqlMember m, SqlExpression expr)
        {
            var func = FunctionCall(typeof(int), "DAYOFWEEK", new[] { expr }, expr.SourceExpression);
            return Binary(SqlNodeType.Sub, func, ValueFromObject(1, expr.SourceExpression), typeof(DayOfWeek));
        }

        internal override SqlExpression DateTime_DayOfYear(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(typeof(int), "DAYOFYEAR", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_Hour(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(typeof(int), "HOUR", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_Minute(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(typeof(int), "MINUTE", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_Month(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(typeof(int), "MONTH", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_Second(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(typeof(int), "SECOND", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlNode DateTime_TimeOfDay(SqlMember member, SqlExpression expr)
        {
            var expression8 = DateTime_Hour(member, expr); //this.DATEPART("HOUR", expr);
            var expression9 = DateTime_Minute(member, expr); //this.DATEPART("MINUTE", expr);
            var expression10 = DateTime_Second(member, expr); //this.DATEPART("SECOND", expr);
            var expression11 = FunctionCall(typeof(int), "MICROSECOND", new[] { expr }, expr.SourceExpression); //this.DATEPART("MILLISECOND", expr);
            SqlExpression expression12 = this.Multiply(this.ConvertToBigint(expression8), 0x861c46800L);
            SqlExpression expression13 = this.Multiply(this.ConvertToBigint(expression9), 0x23c34600L);
            SqlExpression expression14 = this.Multiply(this.ConvertToBigint(expression10), 0x989680L);
            SqlExpression expression15 = this.Multiply(this.ConvertToBigint(expression11), 0x2710L);
            return this.ConvertTo(typeof(TimeSpan), this.Add(new SqlExpression[] { expression12, expression13, expression14, expression15 }));
        }

        internal override SqlNode DateTime_Year(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(typeof(int), "Year", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression String_GetChar(SqlMethodCall mc, Expression sourceExpression)
        {
            if (mc.Arguments.Count != 1)
            {
                SqlClient.Error.MethodHasNoSupportConversionToSql(mc);
            }
            var args = new[] { mc.Object, Add(mc.Arguments[0], 1), 
                               ValueFromObject(1, false, sourceExpression) };
            return FunctionCall(typeof(char), "SUBSTR", args, sourceExpression);
        }

        internal override SqlExpression String_Remove(SqlMethodCall mc)
        {
            var sourceExpression = mc.SourceExpression;
            if (mc.Arguments.Count != 1)
            {
                if (mc.Arguments.Count == 2)
                {
                    return this.FunctionCall(typeof(string), "STUFF", new[] { mc.Object, this.Add(mc.Arguments[0], 1), mc.Arguments[1], this.ValueFromObject("", false, sourceExpression) }, sourceExpression);
                }
                throw SqlClient.Error.MethodHasNoSupportConversionToSql(mc.Method);
            }
            return FunctionCall(typeof(string), "STUFF", new[] { mc.Object, this.Add(mc.Arguments[0], 1), this.CLRLENGTH(mc.Object), this.ValueFromObject("", false, sourceExpression) }, sourceExpression);
        }

        internal override SqlExpression String_Substring(SqlMethodCall mc)
        {
            var clrType = mc.Method.ReturnType;
            Debug.Assert(clrType == typeof(string));
            var startIndex = (int)((SqlValue)mc.Arguments[0]).Value + 1;
            var sourceObject = (mc.Object);
            //var sourceString = 

            var arg1 = mc.Object;
            var arg2 = ValueFromObject(startIndex, mc.SourceExpression);//Expression.Constant(startIndex));
            var left = new SqlFunctionCall(typeof(string), TypeProvider.From(typeof(string)), "SUBSTR",
                                           new[] { arg1, arg2 }, mc.SourceExpression);

            if (mc.Arguments.Count == 2)
            {
                var count = (int)((SqlValue)mc.Arguments[1]).Value;
                SqlExpression len1 = ValueFromObject(startIndex + count, mc.SourceExpression);
                SqlExpression len2 = new SqlFunctionCall(typeof(int), TypeProvider.From(typeof(int)), "Length",
                                                         new[] { sourceObject }, mc.SourceExpression);
                SqlExpression len = new SqlBinary(SqlNodeType.Sub, typeof(int), TypeProvider.From(typeof(int)),
                                                  len2, len1);

                SqlExpression right = new SqlFunctionCall(typeof(string), TypeProvider.From(typeof(string)),
                                                          "SUBSTR", new[] { sourceObject, len }, mc.SourceExpression);
                var result = new SqlBinary(SqlNodeType.Add, clrType, TypeProvider.From(clrType), left, right);
                return result;
            }
            Debug.Assert(mc.Arguments.Count == 1);
            return left;
        }

        internal override SqlExpression String_Length(SqlExpression expr)
        {
            var f = this.FunctionCall(typeof(int), "LENGTH", new[] { expr }, expr.SourceExpression);
            return f;
        }

        internal override SqlExpression String_Insert(SqlMethodCall mc)
        {
            var clrType = mc.Method.ReturnType;
            //var insertString = (string)((SqlValue)mc.Arguments[1]).Value;
            return FunctionCall(clrType, "INSERT", new[] { mc.Object, Binary(SqlNodeType.Add, mc.Arguments[0], ValueFromObject(1, false, mc.SourceExpression)), ValueFromObject(0, mc.SourceExpression), mc.Arguments[1] }, mc.SourceExpression);
        }

        internal override SqlExpression MakeCoalesce(SqlExpression left, SqlExpression right, Type type, Expression sourceExpression)
        {
            return FunctionCall(type, "NVL", new[] { left, right }, sourceExpression);
        }

        internal override SqlExpression String_IndexOf(SqlMethodCall mc, Expression sourceExpression)
        {
            var arg1 = mc.Arguments[0];
            SqlExpression arg2;
            arg2 = mc.Arguments.Count == 1 ? ValueFromObject(1, false, sourceExpression) : mc.Arguments[1];

            var f = FunctionCall(mc.ClrType, "LOCATE", new[] { arg1, mc.Object, Binary(SqlNodeType.Add, arg2, ValueFromObject(1, false, sourceExpression)) }, sourceExpression);
            return Binary(SqlNodeType.Sub, f, ValueFromObject(1, false, sourceExpression));
        }

        internal override SqlExpression Math_Atan2(SqlMethodCall mc, Expression sourceExpression)
        {
            Debug.Assert(mc.Method.Name == "Atan2");
            return FunctionCall(typeof(double), "ATAN2", new[] { mc.Arguments[1], mc.Arguments[0] }, sourceExpression);
        }

        internal override SqlExpression Math_Truncate(SqlMethodCall mc)
        {
            var args = new[] { mc.Arguments[0], ValueFromObject(0, false, mc.SourceExpression) };
            return FunctionCall(mc.Method.ReturnType, "TRUNC", args, mc.SourceExpression);
        }

        internal override SqlSearchedCase SearchedCase(SqlWhen[] whens, SqlExpression @else, Expression sourceExpression)
        {
            SqlValue sqlValue = @else as SqlValue;
            if (sqlValue != null)
                sqlValue.IsClientSpecified = false;
            for (var i = 0; i < whens.Length; i++)
            {
                sqlValue = whens[i].Value as SqlValue;
                if (sqlValue != null)
                {
                    sqlValue.IsClientSpecified = false;
                }
            }
            return new SqlSearchedCase(whens[0].Value.ClrType, whens, @else, sourceExpression);
        }

        internal override MethodSupport GetStringMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.Name == "Insert" || mc.Method.Name == "Substring")
                return MethodSupport.None;

            if (mc.Method.DeclaringType == typeof(string))
            {
                if (mc.Method.IsStatic)
                {
                    if (mc.Method.Name == "Concat")
                    {
                        return MethodSupport.Method;
                    }
                }
                else
                {
                    switch (mc.Method.Name)
                    {
                        case "Contains":
                        case "StartsWith":
                        case "EndsWith":
                            if (mc.Arguments.Count != 1)
                            {
                                return MethodSupport.MethodGroup;
                            }
                            return MethodSupport.Method;

                        case "IndexOf":
                        case "LastIndexOf":
                            if (mc.Arguments[mc.Arguments.Count - 1].ClrType == typeof(StringComparison))
                                return MethodSupport.None;
                            if (((mc.Arguments.Count != 1) && (mc.Arguments.Count != 2)) && (mc.Arguments.Count != 3))
                            {
                                return MethodSupport.MethodGroup;
                            }
                            return MethodSupport.Method;
                        case "Insert":
                            if (mc.Arguments.Count != 2)
                            {
                                return MethodSupport.MethodGroup;
                            }
                            return MethodSupport.Method;

                        case "PadLeft":
                        case "PadRight":
                        //case "Remove":
                        case "Substring":
                            if ((mc.Arguments.Count != 1) && (mc.Arguments.Count != 2))
                            {
                                return MethodSupport.MethodGroup;
                            }
                            return MethodSupport.Method;

                        case "Replace":
                            return MethodSupport.Method;

                        case "Trim":
                        case "TrimEnd":
                        case "ToLower":
                        case "ToUpper":
                            if (mc.Arguments.Count == 0)
                            {
                                return MethodSupport.Method;
                            }
                            return MethodSupport.MethodGroup;

                        case "get_Chars":
                        case "CompareTo":
                            if (mc.Arguments.Count != 1)
                            {
                                return MethodSupport.MethodGroup;
                            }
                            return MethodSupport.Method;
                    }
                }
            }
            return MethodSupport.None;
        }

        internal override MethodSupport GetSqlMethodsMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.Name.StartsWith("DateDiff"))
                return MethodSupport.None;

            return base.GetSqlMethodsMethodSupport(mc);
        }

    }
}
