using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.Access
{
    class AccessSqlFactory : SqlFactory
    {
        internal AccessSqlFactory(ITypeSystemProvider typeProvider, MetaModel model)
            : base(typeProvider, model)
        {
        }

        internal override SqlExpression DATALENGTH(SqlExpression expr)
        {
            return this.FunctionCall(typeof(int), "LEN", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression Math_Truncate(SqlMethodCall mc)
        {
            //var args = new[] { mc.Arguments[0], ValueFromObject(0, false, mc.SourceExpression),
            //                   ValueFromObject(1, false, mc.SourceExpression) };
            //return FunctionCall(mc.Method.ReturnType, "TRUNC", mc.Arguments, mc.SourceExpression);
            throw SqlClient.Error.MethodHasNoSupportConversionToSql(mc);
        }

        internal override SqlExpression Math_Atan(SqlMethodCall mc, Expression sourceExpression)
        {
            return CreateFunctionCallStatic1(typeof(double), "ATN", mc.Arguments, sourceExpression);
        }

        internal override SqlExpression String_Substring(SqlMethodCall mc)
        {
            var clrType = mc.Method.ReturnType;
            var expressions = new List<SqlExpression>();
            var sqlExpression = mc.Object;
            expressions.Add(sqlExpression);

            var expression = this.Binary(SqlNodeType.Add, mc.Arguments[0], this.ValueFromObject(1, mc.SourceExpression));
            expressions.Add((expression));

            if (mc.Arguments.Count > 1)
                expressions.Add(mc.Arguments[1]);

            var node = new SqlFunctionCall(clrType, TypeProvider.From(clrType), "MID", expressions, mc.SourceExpression);
            return node;
        }

        internal override SqlExpression String_IndexOf(SqlMethodCall mc, Expression sourceExpression)
        {
            if (mc.Arguments.Count == 1)
            {
                if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
                {
                    throw Error.ArgumentNull("value");
                }
                var when = new SqlWhen(this.Binary(SqlNodeType.EQ, this.CLRLENGTH(mc.Arguments[0]), this.ValueFromObject(0, sourceExpression)), this.ValueFromObject(0, sourceExpression));
                SqlExpression expression9 = this.Subtract(this.FunctionCall(typeof(int), "INSTR", new[] { mc.Arguments[0], mc.Object }, sourceExpression), 1);
                return this.SearchedCase(new[] { when }, expression9, sourceExpression);
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
                SqlExpression expression10 = this.Binary(SqlNodeType.EQ, this.CLRLENGTH(mc.Arguments[0]), this.ValueFromObject(0, sourceExpression));
                var when2 = new SqlWhen(this.AndAccumulate(expression10, this.Binary(SqlNodeType.LE, this.Add(mc.Arguments[1], 1), this.CLRLENGTH(mc.Object))), mc.Arguments[1]);
                SqlExpression expression11 = this.Subtract(this.FunctionCall(typeof(int), "INSTR", new[] { mc.Arguments[0], mc.Object, this.Add(mc.Arguments[1], 1) }, sourceExpression), 1);
                return this.SearchedCase(new[] { when2 }, expression11, sourceExpression);
            }
            if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
            {
                throw Error.ArgumentNull("value");
            }
            if (mc.Arguments[2].ClrType == typeof(StringComparison))
            {
                throw SqlClient.Error.IndexOfWithStringComparisonArgNotSupported();
            }
            SqlExpression left = this.Binary(SqlNodeType.EQ, this.CLRLENGTH(mc.Arguments[0]), this.ValueFromObject(0, sourceExpression));
            var when3 = new SqlWhen(this.AndAccumulate(left, this.Binary(SqlNodeType.LE, this.Add(mc.Arguments[1], 1), this.CLRLENGTH(mc.Object))), mc.Arguments[1]);
            SqlExpression expression13 = this.FunctionCall(typeof(string), "SUBSTRING", new[] { mc.Object, this.ValueFromObject(1, false, sourceExpression), this.Add(new SqlExpression[] { mc.Arguments[1], mc.Arguments[2] }) }, sourceExpression);
            SqlExpression @else = this.Subtract(this.FunctionCall(typeof(int), "INSTR",
                                                    new[] { mc.Arguments[0], expression13, this.Add(mc.Arguments[1], 1) }, sourceExpression), 1);
            return this.SearchedCase(new SqlWhen[] { when3 }, @else, sourceExpression);
        }

        internal override SqlExpression String_Remove(SqlMethodCall mc)
        {
            var clrType = mc.Method.ReturnType;
            Debug.Assert(clrType == typeof(string));
            var startIndex = (int)((SqlValue)mc.Arguments[0]).Value;
            var sourceObject = mc.Object;
            //var sourceString = 

            var arg1 = (mc.Object);
            var arg2 = (ValueFromObject(startIndex, mc.SourceExpression));
            var left = new SqlFunctionCall(typeof(string), TypeProvider.From(typeof(string)), "Left",
                                           new[] { arg1, arg2 }, mc.SourceExpression);

            if (mc.Arguments.Count == 2)
            {
                var count = (int)((SqlValue)mc.Arguments[1]).Value;
                //SqlExpression len1 = new SqlFunctionCall(typeof(int), typeProvider.From(typeof(int)), "Len",
                //                      new[] { sourceObject }, dominatingExpression);
                //len1 = new SqlBinary(SqlNodeType.Sub, typeof(int), typeProvider.From(typeof(int)), len1,
                //                    VisitExpression(Expression.Constant(startIndex + count)));
                SqlExpression len1 = ValueFromObject(startIndex + count, mc.SourceExpression);
                SqlExpression len2 = new SqlFunctionCall(typeof(int), TypeProvider.From(typeof(int)), "Len",
                                                         new[] { sourceObject }, mc.SourceExpression);
                SqlExpression len = new SqlBinary(SqlNodeType.Sub, typeof(int), TypeProvider.From(typeof(int)),
                                                  len2, len1);

                SqlExpression right = new SqlFunctionCall(typeof(string), TypeProvider.From(typeof(string)),
                                                          "Right", new[] { sourceObject, len }, mc.SourceExpression);
                var result = new SqlBinary(SqlNodeType.Add, clrType, TypeProvider.From(clrType), left, right);
                return result;
            }
            Debug.Assert(mc.Arguments.Count == 1);
            return left;
        }

        //internal override SqlExpression String_Replace(SqlMethodCall mc)
        //{
        //    if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
        //    {
        //        throw Error.ArgumentNull("old");
        //    }
        //    if ((mc.Arguments[1] is SqlValue) && (((SqlValue)mc.Arguments[1]).Value == null))
        //    {
        //        throw Error.ArgumentNull("new");
        //    }
        //    var clrType = mc.Method.ReturnType;
        //    var sqlType = TypeProvider.From(clrType);
        //    Debug.Assert(clrType == typeof(string));
        //    Debug.Assert(mc.Arguments.Count == 2);

        //    var sourceObject = ValueFromObject(mc.Object, mc.SourceExpression);
        //    var oldValue = ValueFromObject(mc.Arguments[0], mc.SourceExpression);
        //    var newValue = ValueFromObject(mc.Arguments[1], mc.SourceExpression);

        //    var result = new SqlFunctionCall(clrType, sqlType, "Replace",
        //                                     new[] { sourceObject, oldValue, newValue }, mc.SourceExpression);
        //    return result;
        //}


        internal override SqlExpression DateTime_Add(SqlMethodCall mc)
        {
            SqlExpression arg = mc.Arguments[0].NodeType == SqlNodeType.Value
                ? ValueFromObject(((TimeSpan)((SqlValue)mc.Arguments[0]).Value).Ticks / 10000000, true, mc.SourceExpression)
                : Binary(SqlNodeType.Div, mc.Arguments[0],
                                                                                                                                                                                                    ValueFromObject(10000000, mc.SourceExpression));
            var expr = mc.SourceExpression;
            var args = new[] { ValueFromObject("s", expr), arg, mc.Object };
            return FunctionCall(mc.ClrType, "DATEADD", args, expr);
        }

        internal override SqlExpression DateTime_AddDays(SqlMethodCall mc)
        {
            var expr = mc.SourceExpression;
            var args = new[] { ValueFromObject("d", expr), mc.Arguments[0], mc.Object };
            return FunctionCall(mc.ClrType, "DATEADD", args, expr);
        }

        internal override SqlExpression DateTime_AddHours(SqlMethodCall mc)
        {
            var expr = mc.SourceExpression;
            var args = new[] { ValueFromObject("h", expr), mc.Arguments[0], mc.Object };
            return FunctionCall(mc.ClrType, "DATEADD", args, expr);
        }

        internal override SqlExpression DateTime_AddMinutes(SqlMethodCall mc)
        {
            var expr = mc.SourceExpression;
            var args = new[] { ValueFromObject("n", expr), mc.Arguments[0], mc.Object };
            return FunctionCall(mc.ClrType, "DATEADD", args, expr);
        }

        internal override SqlExpression DateTime_AddMonths(SqlMethodCall mc)
        {
            var expr = mc.SourceExpression;
            var args = new[] { ValueFromObject("m", expr), mc.Arguments[0], mc.Object };
            return FunctionCall(mc.ClrType, "DATEADD", args, expr);
        }

        internal override SqlExpression DateTime_AddSeconds(SqlMethodCall mc)
        {
            var expr = mc.SourceExpression;
            var args = new[] { ValueFromObject("s", expr), mc.Arguments[0], mc.Object };
            return FunctionCall(mc.ClrType, "DATEADD", args, expr);
        }

        internal override SqlExpression DateTime_AddYears(SqlMethodCall mc)
        {
            var expr = mc.SourceExpression;
            var args = new[] { ValueFromObject("yyyy", expr), mc.Arguments[0], mc.Object };
            return FunctionCall(mc.ClrType, "DATEADD", args, expr);
        }

        internal override SqlExpression DateTime_Date(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(expr.ClrType, "DateValue", new[] { expr }, expr.SourceExpression);
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


        internal override SqlExpression UNICODE(Type clrType, SqlUnary uo)
        {
            return FunctionCall(clrType, TypeProvider.From(typeof(int)), "ASC", new[] { uo.Operand }, uo.SourceExpression);
        }

        internal override SqlExpression DATEPART(string partName, SqlExpression expr)
        {
            var clrType = typeof(int);
            var sqlType = TypeProvider.From(clrType);
            SqlFunctionCall result;
            if (partName == "DayOfYear")
            {
                var interval = ValueFromObject("y", expr.SourceExpression);
                result = FunctionCall(clrType, "DatePart", new[] { interval, expr }, expr.SourceExpression);
            }
            //else if (partName == "DayOfWek")
            //{
            //    var interval = ValueFromObject("w", expr.SourceExpression);
            //    result = FunctionCall(clrType, "DatePart", new[] { interval, expr }, expr.SourceExpression);
            //}
            else
            {
                result = new SqlFunctionCall(typeof(int), sqlType, partName,//"Date",
                                            new[] { expr }, expr.SourceExpression);
            }

            return result;
        }

        internal override SqlExpression DateTime_DayOfWeek(SqlMember m, SqlExpression expr)
        {
            var clrType = typeof(DayOfWeek);
            var sqlType = TypeProvider.From(typeof(int));
            var interval = ValueFromObject("w", expr.SourceExpression);
            var func = FunctionCall(typeof(int), sqlType, "DatePart", new[] { interval, expr }, expr.SourceExpression);
            return new SqlBinary(SqlNodeType.Sub, clrType, sqlType, func, ValueFromObject(1, expr.SourceExpression));
            //return Add(func, ValueFromObject(1, expr.SourceExpression));
        }

        internal override SqlExpression TranslateConvertStaticMethod(SqlMethodCall mc)
        {
            if (mc.Arguments.Count != 1)
            {
                return null;
            }
            var expr = mc.Arguments[0];
            Type type;
            string funcName;
            switch (mc.Method.Name)
            {
                case "ToBoolean":
                    type = typeof(bool);
                    funcName = "CBool";
                    break;

                case "ToDecimal":
                    type = typeof(decimal);
                    funcName = "CDec";
                    break;

                case "ToByte":
                    type = typeof(byte);
                    funcName = "CInt";
                    break;

                case "ToChar":
                    type = typeof(char);
                    funcName = "CStr";
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
                        funcName = "CDate";
                        break;
                    }
                case "ToDouble":
                    type = typeof(double);
                    funcName = "CDbl";
                    break;

                case "ToInt16":
                    type = typeof(short);
                    funcName = "CInt";
                    break;

                case "ToInt32":
                    type = typeof(int);
                    funcName = "CInt";
                    break;

                case "ToInt64":
                    type = typeof(long);
                    funcName = "CLng";
                    break;

                case "ToSingle":
                    type = typeof(float);
                    funcName = "CSng";
                    break;

                case "ToString":
                    type = typeof(string);
                    funcName = "CStr";
                    break;

                case "ToSByte":
                    type = typeof(sbyte);
                    funcName = "CByte";
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

        internal override MethodSupport GetConvertMethodSupport(SqlMethodCall mc)
        {
            if ((mc.Method.IsStatic && (mc.Method.DeclaringType == typeof(Convert))) && (mc.Arguments.Count == 1))
            {
                switch (mc.Method.Name)
                {
                    //case "ToBoolean":
                    //case "ToDecimal":
                    //case "ToByte":
                    //case "ToChar":
                    //case "ToDouble":
                    //case "ToInt16":
                    //case "ToInt32":
                    //case "ToInt64":
                    //case "ToSingle":
                    //case "ToString":
                    //    return MethodSupport.Method;

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

        internal override MethodSupport GetStringMethodSupport(SqlMethodCall mc)
        {
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

                        //case "IndexOf":
                        //case "LastIndexOf":
                        //    if (mc.Arguments.Count == 2 || mc.Arguments[mc.Arguments.Count - 1].ClrType == typeof(StringComparison))
                        //        return MethodSupport.None;
                        //    if (((mc.Arguments.Count != 1) && (mc.Arguments.Count != 2)) && (mc.Arguments.Count != 3))
                        //    {
                        //        return MethodSupport.MethodGroup;
                        //    }
                        //    return MethodSupport.Method;

                        //if (((mc.Arguments.Count != 1) && (mc.Arguments.Count != 2)) && (mc.Arguments.Count != 3))
                        //{
                        //    return MethodSupport.MethodGroup;
                        //}
                        //return MethodSupport.Method;

                        case "Insert":
                            if (mc.Arguments.Count != 2)
                            {
                                return MethodSupport.MethodGroup;
                            }
                            return MethodSupport.Method;

                        case "PadLeft":
                        case "PadRight":
                        case "Remove":
                        case "Substring":
                            if ((mc.Arguments.Count != 1) && (mc.Arguments.Count != 2))
                            {
                                return MethodSupport.MethodGroup;
                            }
                            return MethodSupport.Method;

                        //case "Replace":
                        //    return MethodSupport.Method;

                        case "Trim":
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

        internal override SqlExpression MakeCoalesce(SqlExpression left, SqlExpression right, Type type, Expression sourceExpression)
        {
            return FunctionCall(type, "IIF", new[] { left, right, left }, sourceExpression);
        }

        internal override SqlExpression Math_Log10(SqlMethodCall mc)
        {
            Expression sourceExpression = mc.SourceExpression;
            SqlExpression first = this.FunctionCall(typeof(double), "LOG", new[] { mc.Arguments[0] }, sourceExpression);
            SqlExpression second = this.FunctionCall(typeof(double), "LOG", new[] { this.ValueFromObject(10, sourceExpression) }, sourceExpression);
            return this.Divide(first, second);
        }

        internal override MethodSupport GetSqlMethodsMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.Name == "DateDiffMillisecond")
                return MethodSupport.None;
            return base.GetSqlMethodsMethodSupport(mc);
        }

        // Fields
        private static readonly string[] dateParts = new[] { "Year", "Month", "Day", "Hour", "Minute", "Second", "Millisecond" };
        internal override SqlExpression TranslateSqlMethodsMethod(SqlMethodCall mc)
        {
            Expression sourceExpression = mc.SourceExpression;
            const SqlExpression expression2 = null;
            string name = mc.Method.Name;
            if (name.StartsWith("DateDiff", StringComparison.Ordinal) && (mc.Arguments.Count == 2))
            {
                foreach (string str2 in dateParts)
                {
                    if (mc.Method.Name == ("DateDiff" + str2))
                    {
                        SqlExpression expression3 = mc.Arguments[0];
                        SqlExpression expression4 = mc.Arguments[1];
                        var str3 = str2;
                        if (str3 == "Day")
                            str3 = "d";
                        else if (str3 == "Year")
                            str3 = "yyyy";
                        else if (str3 == "Month")
                            str3 = "m";
                        else if (str3 == "Hour")
                            str3 = "h";
                        else if (str3 == "Minute")
                            str3 = "n";
                        else if (str3 == "Second")
                            str3 = "s";
                        else if (str3 == "Millisecond")
                            str3 = "s";

                        SqlExpression expression5 = this.ValueFromObject(str3, sourceExpression); //new SqlValue(typeof(void), null, str3, sourceExpression);
                        SqlExpression result = this.FunctionCall(typeof(int), "DATEDIFF", new[] { expression5, expression3, expression4 }, sourceExpression);
                        if (mc.Method.Name == "DateDiffMillisecond")
                            result = this.Binary(SqlNodeType.Mul,result,ValueFromObject(1000,sourceExpression));
                        return result;
                    }
                }
                return expression2;
            }
            if (name == "Like")
            {
                if (mc.Arguments.Count == 2)
                {
                    return this.Like(mc.Arguments[0], mc.Arguments[1], null, sourceExpression);
                }
                if (mc.Arguments.Count != 3)
                {
                    return expression2;
                }
                return this.Like(mc.Arguments[0], mc.Arguments[1], this.ConvertTo(typeof(string), mc.Arguments[2]), sourceExpression);
            }
            if (name == "RawLength")
            {
                return DATALENGTH(mc.Arguments[0]);
            }
            return expression2;
        }
    }
}
