using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using ALinq.Mapping;

namespace ALinq.SqlClient
{
    internal abstract class PostBindDotNetConverter
    {
        private SqlFactory sql;

        protected PostBindDotNetConverter(SqlFactory sqlFactory)
        {
            this.sql = sqlFactory;
        }


        // Methods
        internal bool CanConvert(SqlNode node)
        {
            var uo = node as SqlUnary;
            if ((uo != null) && IsSupportedUnary(uo))
            {
                return true;
            }
            var snew = node as SqlNew;
            if ((snew != null) && IsSupportedNew(snew))
            {
                return true;
            }
            var m = node as SqlMember;
            if ((m != null) && IsSupportedMember(m))
            {
                return true;
            }
            var mc = node as SqlMethodCall;
            return ((mc != null) && (GetMethodSupport(mc) == MethodSupport.Method));
        }

        internal SqlNode Convert(SqlNode node, SqlFactory sql, SqlProvider sqlProvider)
        {
            //SqlProvider.ProviderMode providerMode
            var visitor = CreateVisitor(sql, sqlProvider);
            visitor.PostBindDotNetConverter = this;
            return visitor.Visit(node);
        }

        internal abstract SqlVisitor CreateVisitor(SqlFactory factory, SqlProvider sqlProvider);

        private static MethodSupport GetCoercionMethodSupport(SqlMethodCall mc)
        {
            if ((!mc.Method.IsStatic || !mc.SqlType.CanBeColumn) ||
                (!(mc.Method.Name == "op_Implicit") && !(mc.Method.Name == "op_Explicit")))
            {
                return MethodSupport.None;
            }
            return MethodSupport.Method;
        }

        private static MethodSupport GetComparisonMethodSupport(SqlMethodCall mc)
        {
            if ((mc.Method.IsStatic && (mc.Method.Name == "Compare")) && (mc.Method.ReturnType == typeof(int)))
            {
                return MethodSupport.Method;
            }
            return MethodSupport.None;
        }

        private MethodSupport GetConvertMethodSupport(SqlMethodCall mc)
        {
            return sql.GetConvertMethodSupport(mc);
        }

        internal virtual string GetDatePart(string memberName)
        {
            switch (memberName)
            {
                case "Year":
                case "Month":
                case "Day":
                case "DayOfYear":
                //case "DayOfWeek":
                case "Hour":
                case "Minute":
                case "Second":
                case "Millisecond":
                    return memberName;
            }
            return null;
        }



        protected virtual MethodSupport GetDecimalMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.IsStatic)
            {
                if (mc.Arguments.Count == 2)
                {
                    string str;
                    if (((str = mc.Method.Name) != null) && ((((str == "Multiply") || (str == "Divide")) || ((str == "Subtract") || (str == "Add"))) || ((str == "Remainder") || (str == "Round"))))
                    {
                        return MethodSupport.Method;
                    }
                }
                else if (mc.Arguments.Count == 1)
                {
                    string str2;
                    if (((str2 = mc.Method.Name) != null) && (((str2 == "Negate") || (str2 == "Floor")) || ((str2 == "Truncate") || (str2 == "Round"))))
                    {
                        return MethodSupport.Method;
                    }
                    if (mc.Method.Name.StartsWith("To", StringComparison.Ordinal))
                    {
                        return MethodSupport.Method;
                    }
                }
            }
            return MethodSupport.None;
        }

        internal virtual MethodSupport GetMathMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.IsStatic && (mc.Method.DeclaringType == typeof(Math)))
            {
                switch (mc.Method.Name)
                {
                    case "Abs":
                    case "Acos":
                    case "Asin":
                    case "Atan":
                    case "Ceiling":
                    case "Cos":
                    case "Cosh":
                    case "Exp":
                    case "Floor":
                    case "Log10":
                        if (mc.Arguments.Count != 1)
                        {
                            return MethodSupport.MethodGroup;
                        }
                        return MethodSupport.Method;

                    case "Log":
                        if ((mc.Arguments.Count != 1) && (mc.Arguments.Count != 2))
                        {
                            return MethodSupport.MethodGroup;
                        }
                        return MethodSupport.Method;

                    case "Max":
                    case "Min":
                    case "Pow":
                    case "Atan2":
                    case "BigMul":
                        if (mc.Arguments.Count != 2)
                        {
                            return MethodSupport.MethodGroup;
                        }
                        return MethodSupport.Method;

                    case "Round":
                        if ((mc.Arguments[mc.Arguments.Count - 1].ClrType != typeof(MidpointRounding)) || ((mc.Arguments.Count != 2) && (mc.Arguments.Count != 3)))
                        {
                            return MethodSupport.MethodGroup;
                        }
                        return MethodSupport.Method;

                    case "Sign":
                    case "Sin":
                    case "Sinh":
                    case "Sqrt":
                    case "Tan":
                    case "Tanh":
                    case "Truncate":
                        if (mc.Arguments.Count != 1)
                        {
                            return MethodSupport.MethodGroup;
                        }
                        return MethodSupport.Method;
                }
            }
            return MethodSupport.None;
        }

        internal virtual MethodSupport GetMethodSupport(SqlMethodCall mc)
        {
            MethodSupport none = MethodSupport.None;
            MethodSupport sqlMethodsMethodSupport = sql.GetSqlMethodsMethodSupport(mc);
            if (sqlMethodsMethodSupport > none)
            {
                none = sqlMethodsMethodSupport;
            }
            sqlMethodsMethodSupport = sql.GetDateTimeMethodSupport(mc);
            if (sqlMethodsMethodSupport > none)
            {
                none = sqlMethodsMethodSupport;
            }
            sqlMethodsMethodSupport = GetTimeSpanMethodSupport(mc);
            if (sqlMethodsMethodSupport > none)
            {
                none = sqlMethodsMethodSupport;
            }
            sqlMethodsMethodSupport = GetConvertMethodSupport(mc);
            if (sqlMethodsMethodSupport > none)
            {
                none = sqlMethodsMethodSupport;
            }
            sqlMethodsMethodSupport = GetDecimalMethodSupport(mc);
            if (sqlMethodsMethodSupport > none)
            {
                none = sqlMethodsMethodSupport;
            }
            sqlMethodsMethodSupport = GetMathMethodSupport(mc);
            if (sqlMethodsMethodSupport > none)
            {
                none = sqlMethodsMethodSupport;
            }
            sqlMethodsMethodSupport = GetStringMethodSupport(mc);
            if (sqlMethodsMethodSupport > none)
            {
                none = sqlMethodsMethodSupport;
            }
            sqlMethodsMethodSupport = GetComparisonMethodSupport(mc);
            if (sqlMethodsMethodSupport > none)
            {
                none = sqlMethodsMethodSupport;
            }
            sqlMethodsMethodSupport = GetNullableMethodSupport(mc);
            if (sqlMethodsMethodSupport > none)
            {
                none = sqlMethodsMethodSupport;
            }
            sqlMethodsMethodSupport = GetCoercionMethodSupport(mc);
            if (sqlMethodsMethodSupport > none)
            {
                none = sqlMethodsMethodSupport;
            }
            sqlMethodsMethodSupport = GetObjectMethodSupport(mc);
            if (sqlMethodsMethodSupport > none)
            {
                none = sqlMethodsMethodSupport;
            }
            sqlMethodsMethodSupport = GetVbHelperMethodSupport(mc);
            if (sqlMethodsMethodSupport > none)
            {
                none = sqlMethodsMethodSupport;
            }
            return none;
        }

        private static MethodSupport GetNullableMethodSupport(SqlMethodCall mc)
        {
            if ((mc.Method.Name == "GetValueOrDefault") && TypeSystem.IsNullableType(mc.Object.ClrType))
            {
                return MethodSupport.Method;
            }
            return MethodSupport.None;
        }

        private static MethodSupport GetObjectMethodSupport(SqlMethodCall mc)
        {
            string str;
            if (!mc.Method.IsStatic && ((str = mc.Method.Name) != null))
            {
                if (str == "Equals")
                {
                    if (mc.Arguments.Count == 2 && mc.Arguments[0].ClrType == typeof(string) &&
                        mc.Arguments[1].ClrType == typeof(StringComparison))
                        return MethodSupport.None;
                    return MethodSupport.Method;
                }
                if (str == "ToString")
                {
                    //====My Code====
                    if (mc.Object.ClrType == typeof(DateTime))
                    {
                        return MethodSupport.None;
                    }
                    //===============
                    if (mc.Object.SqlType.CanBeColumn)
                    {
                        return MethodSupport.Method;
                    }
                    return MethodSupport.None;
                }
                if (str == "GetType")
                {
                    if (mc.Arguments.Count == 0)
                    {
                        return MethodSupport.Method;
                    }
                    return MethodSupport.None;
                }
            }
            return MethodSupport.None;
        }

        // Fields
        protected virtual MethodSupport GetStringMethodSupport(SqlMethodCall mc)
        {
            return sql.GetStringMethodSupport(mc);
        }

        private static MethodSupport GetTimeSpanMethodSupport(SqlMethodCall mc)
        {
            string str;
            if (((mc.Method.IsStatic || (mc.Method.DeclaringType != typeof(TimeSpan))) || ((str = mc.Method.Name) == null)) || ((!(str == "Add") && !(str == "Subtract")) && ((!(str == "CompareTo") && !(str == "Duration")) && !(str == "Negate"))))
            {
                return MethodSupport.None;
            }
            return MethodSupport.Method;
        }

        private static MethodSupport GetVbHelperMethodSupport(SqlMethodCall mc)
        {
            if ((!IsVbConversionMethod(mc) && !IsVbCompareString(mc)) && !IsVbLike(mc))
            {
                return MethodSupport.None;
            }
            return MethodSupport.Method;
        }

        private static bool IsSupportedBinaryMember(SqlMember m)
        {
            return ((m.Expression.ClrType == typeof(Binary)) && (m.Member.Name == "Length"));
        }

        private bool IsSupportedDateTimeMember(SqlMember m)
        {
            return sql.IsSupportedDateTimeMember(m);
        }

        private static bool IsSupportedDateTimeNew(SqlNew sox)
        {
            if ((((sox.ClrType == typeof(DateTime)) && (sox.Args.Count >= 3)) && ((sox.Args[0].ClrType == typeof(int)) && (sox.Args[1].ClrType == typeof(int)))) && (sox.Args[2].ClrType == typeof(int)))
            {
                if (sox.Args.Count == 3)
                {
                    return true;
                }
                if (((sox.Args.Count >= 6) && (sox.Args[3].ClrType == typeof(int))) && ((sox.Args[4].ClrType == typeof(int)) && (sox.Args[5].ClrType == typeof(int))))
                {
                    if (sox.Args.Count == 6)
                    {
                        return true;
                    }
                    if ((sox.Args.Count == 7) && (sox.Args[6].ClrType == typeof(int)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsSupportedMember(SqlMember m)
        {
            if ((!IsSupportedStringMember(m) && !IsSupportedBinaryMember(m)) && !IsSupportedDateTimeMember(m))
            {
                return IsSupportedTimeSpanMember(m);
            }
            return true;
        }

        private static bool IsSupportedNew(SqlNew snew)
        {
            if (snew.ClrType == typeof(string))
            {
                return IsSupportedStringNew(snew);
            }
            if (snew.ClrType == typeof(TimeSpan))
            {
                return IsSupportedTimeSpanNew(snew);
            }
            return ((snew.ClrType == typeof(DateTime)) && IsSupportedDateTimeNew(snew));
        }

        private static bool IsSupportedStringMember(SqlMember m)
        {
            return ((m.Expression.ClrType == typeof(string)) && (m.Member.Name == "Length"));
        }

        private static bool IsSupportedStringNew(SqlNew snew)
        {
            return (((snew.Args.Count == 2) && (snew.Args[0].ClrType == typeof(char))) && (snew.Args[1].ClrType == typeof(int)));
        }

        private static bool IsSupportedTimeSpanMember(SqlMember m)
        {
            if (m.Expression.ClrType == typeof(TimeSpan))
            {
                switch (m.Member.Name)
                {
                    case "Ticks":
                    case "TotalMilliseconds":
                    case "TotalSeconds":
                    case "TotalMinutes":
                    case "TotalHours":
                    case "TotalDays":
                    case "Milliseconds":
                    case "Seconds":
                    case "Minutes":
                    case "Hours":
                    case "Days":
                        return true;
                }
            }
            return false;
        }

        private static bool IsSupportedTimeSpanNew(SqlNew sox)
        {
            return ((sox.Args.Count == 1) || ((sox.Args.Count == 3) || ((sox.Args.Count == 4) || (sox.Args.Count == 5))));
        }

        private static bool IsSupportedUnary(SqlUnary uo)
        {
            return (((uo.NodeType == SqlNodeType.Convert) && (uo.ClrType == typeof(char))) || (uo.Operand.ClrType == typeof(char)));
        }

        private static bool IsVbCompareString(SqlMethodCall call)
        {
            return ((call.Method.IsStatic && (call.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators")) && (call.Method.Name == "CompareString"));
        }

        private static bool IsVbConversionMethod(SqlMethodCall mc)
        {
            if (mc.Method.IsStatic && (mc.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Conversions"))
            {
                switch (mc.Method.Name)
                {
                    case "ToBoolean":
                    case "ToSByte":
                    case "ToByte":
                    case "ToChar":
                    case "ToCharArrayRankOne":
                    case "ToDate":
                    case "ToDecimal":
                    case "ToDouble":
                    case "ToInteger":
                    case "ToUInteger":
                    case "ToLong":
                    case "ToULong":
                    case "ToShort":
                    case "ToUShort":
                    case "ToSingle":
                    case "ToString":
                        return true;
                }
            }
            return false;
        }

        private static bool IsVbLike(SqlMethodCall mc)
        {
            return (((mc.Method.IsStatic && (mc.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.LikeOperator")) && (mc.Method.Name == "LikeString")) || ((mc.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators") && (mc.Method.Name == "LikeString")));
        }

        // Nested Types

        private class SqlSelectionSkipper : SqlVisitor
        {
            // Fields
            private SqlVisitor parent;

            // Methods
            internal SqlSelectionSkipper(SqlVisitor parent)
            {
                this.parent = parent;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                return parent.VisitClientQuery(cq);
            }

            internal override SqlExpression VisitColumn(SqlColumn col)
            {
                return parent.VisitColumn(col);
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                return parent.VisitSubSelect(ss);
            }
        }

        protected abstract class Visitor : SqlVisitor
        {
            // Fields
            private readonly SqlProvider sqlProvider;
            private readonly SqlSelectionSkipper skipper;
            protected SqlFactory sql;

            // Methods
            protected Visitor(SqlFactory sql, SqlProvider sqlProvider)
            {
                this.sql = sql;
                this.sqlProvider = sqlProvider;
                skipper = new SqlSelectionSkipper(this);
            }

            private SqlExpression CreateComparison(SqlExpression a, SqlExpression b, Expression source)
            {
                SqlExpression match = sql.Binary(SqlNodeType.LT, a, b);
                SqlExpression expression2 = sql.Binary(SqlNodeType.EQ2V, a, b);
                return sql.SearchedCase(new[] { new SqlWhen(match, sql.ValueFromObject(-1, false, source)), 
                                        new SqlWhen(expression2, sql.ValueFromObject(0, false, source)) },
                                        sql.ValueFromObject(1, false, source), source);
            }

            private SqlExpression CreateDateTimeFromDateAndMs(SqlExpression sqlDate, SqlExpression ms, Expression source)
            {
                return CreateDateTimeFromDateAndMs(sqlDate, ms, source, false);
            }

            private SqlExpression CreateDateTimeFromDateAndMs(SqlExpression sqlDate, SqlExpression ms, Expression source, bool asNullable)
            {
                SqlExpression expr = sql.ConvertToBigint(ms);
                SqlExpression expression2 = sql.DATEADD("day", sql.Divide(expr, 0x5265c00L), sqlDate, source, asNullable);
                return sql.DATEADD("ms", sql.Mod(expr, 0x5265c00L), expression2, source, asNullable);
            }

            protected SqlExpression CreateDateTimeFromDateAndTicks(SqlExpression sqlDate, SqlExpression sqlTicks, Expression source)
            {
                return CreateDateTimeFromDateAndTicks(sqlDate, sqlTicks, source, false);
            }

            protected SqlExpression CreateDateTimeFromDateAndTicks(SqlExpression sqlDate, SqlExpression sqlTicks, Expression source, bool asNullable)
            {
                SqlExpression expr = sql.DATEADD("day", this.sql.Divide(sqlTicks, (long)0xc92a69c000L), sqlDate, source, asNullable);
                return this.sql.DATEADD("ms", this.sql.Mod(this.sql.Divide(sqlTicks, (long)0x2710L), 0x5265c00L), expr, source, asNullable);
            }

            protected SqlExpression CreateFunctionCallStatic1(Type type, string functionName, List<SqlExpression> arguments, Expression source)
            {
                return this.sql.FunctionCall(type, functionName, new[] { arguments[0] }, source);
            }

            protected SqlExpression CreateFunctionCallStatic2(Type type, string functionName, List<SqlExpression> arguments, Expression source)
            {
                return sql.FunctionCall(type, functionName, new[] { arguments[0], arguments[1] }, source);
            }

            internal Exception GetMethodSupportException(SqlMethodCall mc)
            {
                if (sqlProvider.PostBindDotNetConverter.GetMethodSupport(mc) == MethodSupport.MethodGroup)
                {
                    return Error.MethodFormHasNoSupportConversionToSql(mc.Method.Name, mc.Method);
                }

                string str = null;
                if (mc.Method.Name == "get_Item" && mc.Arguments.Count > 0)
                {
                    if (mc.Arguments[0] is SqlValue && ((SqlValue)mc.Arguments[0]).Value is string)
                        str = string.Format("{0}.{1}('{2}'))", mc.Object.ClrType, mc.Method.Name, ((SqlValue)mc.Arguments[0]).Value);
                }
                if (str == null)
                    str = mc.Method.ToString();

                return Error.MethodHasNoSupportConversionToSql(str);
            }

            private SqlExpression TranslateConvertStaticMethod(SqlMethodCall mc)
            {
                return sql.TranslateConvertStaticMethod(mc);
            }

            internal virtual SqlExpression TranslateDateTimeBinary(SqlBinary bo)
            {
                bool asNullable = TypeSystem.IsNullableType(bo.ClrType);
                Type nonNullableType = TypeSystem.GetNonNullableType(bo.Right.ClrType);
                SqlNodeType nodeType = bo.NodeType;
                if (nodeType != SqlNodeType.Add)
                {
                    if (nodeType != SqlNodeType.Sub)
                    {
                        return bo;
                    }
                    if (nonNullableType == typeof(DateTime))
                    {
                        Type clrType = bo.ClrType;
                        SqlExpression left = bo.Left;
                        SqlExpression right = bo.Right;
                        SqlExpression expression3 = new SqlVariable(typeof(void), null, "DAY", bo.SourceExpression);
                        SqlExpression expression4 = new SqlVariable(typeof(void), null, "MILLISECOND", bo.SourceExpression);
                        SqlExpression expr = sql.FunctionCall(typeof(int), "DATEDIFF", new[] { expression3, right, left }, bo.SourceExpression);
                        SqlExpression expression6 = sql.FunctionCall(typeof(DateTime), "DATEADD", new[] { expression3, expr, right }, bo.SourceExpression);
                        SqlExpression expression7 = sql.FunctionCall(typeof(int), "DATEDIFF", new[] { expression4, expression6, left }, bo.SourceExpression);
                        SqlExpression expression8 = sql.Multiply(sql.Add(new[] { sql.Multiply(sql.ConvertToBigint(expr), 0x5265c00L), expression7 }), 0x2710L);
                        return sql.ConvertTo(clrType, expression8);
                    }
                    if (nonNullableType != typeof(TimeSpan))
                    {
                        return bo;
                    }
                    return CreateDateTimeFromDateAndTicks(bo.Left, this.sql.Unary(SqlNodeType.Negate, bo.Right, bo.SourceExpression), bo.SourceExpression, asNullable);
                }
                if (nonNullableType == typeof(TimeSpan))
                {
                    return CreateDateTimeFromDateAndTicks(bo.Left, bo.Right, bo.SourceExpression, asNullable);
                }
                return bo;
            }

            private SqlExpression TranslateDateTimeInstanceMethod(SqlMethodCall mc)
            {
                SqlExpression expression = null;
                Expression sourceExpression = mc.SourceExpression;
                if (mc.Method.Name == "CompareTo")
                {
                    return this.CreateComparison(mc.Object, mc.Arguments[0], sourceExpression);
                }
                if ((mc.Method.Name == "Add" && mc.Arguments.Count == 1 && mc.Arguments[0].ClrType == typeof(TimeSpan)) ||
                    (mc.Method.Name == "AddTicks"))
                {
                    return sql.DateTime_Add(mc);
                }
                if (mc.Method.Name == "AddMonths")
                {
                    return sql.DateTime_AddMonths(mc);
                    //return this.sql.DATEADD("MONTH", mc.Arguments[0], mc.Object);
                }
                if (mc.Method.Name == "AddYears")
                {
                    return sql.DateTime_AddYears(mc);
                    //return this.sql.DATEADD("YEAR", mc.Arguments[0], mc.Object);
                }
                if (mc.Method.Name == "AddMilliseconds")
                {
                    return this.CreateDateTimeFromDateAndMs(mc.Object, mc.Arguments[0], sourceExpression);
                }
                if (mc.Method.Name == "AddSeconds")
                {
                    return sql.DateTime_AddSeconds(mc);
                    //SqlExpression ms = this.sql.Multiply(mc.Arguments[0], 0x3e8L);
                    //return this.CreateDateTimeFromDateAndMs(mc.Object, ms, sourceExpression);
                }
                if (mc.Method.Name == "AddMinutes")
                {
                    return sql.DateTime_AddMinutes(mc);
                    //SqlExpression expression4 = this.sql.Multiply(mc.Arguments[0], 0xea60L);
                    //return this.CreateDateTimeFromDateAndMs(mc.Object, expression4, sourceExpression);
                }
                if (mc.Method.Name == "AddHours")
                {
                    return sql.DateTime_AddHours(mc);
                    //SqlExpression expression5 = this.sql.Multiply(mc.Arguments[0], 0x36ee80L);
                    //return this.CreateDateTimeFromDateAndMs(mc.Object, expression5, sourceExpression);
                }
                if (mc.Method.Name == "AddDays")
                {
                    //return sql.DATEADD("AddDays", mc.Arguments[0], mc.Object);
                    return sql.DateTime_AddDays(mc);
                }
                if ((mc.Method.Name == "Subtract" && mc.Arguments.Count == 1 && mc.Arguments[0].ClrType == typeof(TimeSpan)) ||
                     (mc.Method.Name == "SubtractTicks"))
                {
                    return sql.DateTime_Subtract(mc);
                }
                return expression;
            }

            internal SqlExpression TranslateDecimalMethod(SqlMethodCall mc)
            {
                Expression sourceExpression = mc.SourceExpression;
                if (mc.Method.IsStatic)
                {
                    if (mc.Arguments.Count == 2)
                    {
                        switch (mc.Method.Name)
                        {
                            case "Multiply":
                                return this.sql.Binary(SqlNodeType.Mul, mc.Arguments[0], mc.Arguments[1]);

                            case "Divide":
                                return this.sql.Binary(SqlNodeType.Div, mc.Arguments[0], mc.Arguments[1]);

                            case "Subtract":
                                return this.sql.Binary(SqlNodeType.Sub, mc.Arguments[0], mc.Arguments[1]);

                            case "Add":
                                return this.sql.Binary(SqlNodeType.Add, mc.Arguments[0], mc.Arguments[1]);

                            case "Remainder":
                                return this.sql.Binary(SqlNodeType.Mod, mc.Arguments[0], mc.Arguments[1]);

                            case "Round":
                                return this.sql.FunctionCall(mc.Method.ReturnType, "ROUND", mc.Arguments, mc.SourceExpression);
                        }
                    }
                    else if (mc.Arguments.Count == 1)
                    {
                        switch (mc.Method.Name)
                        {
                            case "Negate":
                                return this.sql.Unary(SqlNodeType.Negate, mc.Arguments[0], sourceExpression);

                            case "Floor":
                            case "Truncate":
                                return this.sql.FunctionCall(mc.Method.ReturnType, "ROUND", new SqlExpression[] { mc.Arguments[0], this.sql.ValueFromObject(0, false, mc.SourceExpression), this.sql.ValueFromObject(1, false, mc.SourceExpression) }, mc.SourceExpression);

                            case "Round":
                                return this.sql.FunctionCall(mc.Method.ReturnType, "ROUND", new SqlExpression[] { mc.Arguments[0], this.sql.ValueFromObject(0, false, mc.SourceExpression) }, mc.SourceExpression);
                        }
                        if (mc.Method.Name.StartsWith("To", StringComparison.Ordinal))
                        {
                            return this.TranslateConvertStaticMethod(mc);
                        }
                    }
                }
                throw GetMethodSupportException(mc);
            }

            private SqlExpression TranslateGetValueOrDefaultMethod(SqlMethodCall mc)
            {
                if (mc.Arguments.Count == 0)
                {
                    Type type = mc.Object.ClrType.GetGenericArguments()[0];
                    return this.sql.Binary(SqlNodeType.Coalesce, mc.Object, this.sql.ValueFromObject(Activator.CreateInstance(type), mc.SourceExpression));
                }
                return this.sql.Binary(SqlNodeType.Coalesce, mc.Object, mc.Arguments[0]);
            }

            protected virtual SqlExpression TranslateAtan2Method(SqlMethodCall mc, Expression sourceExpression)
            {
                //Debug.Assert(mc.Method.Name == "Atan2");
                //return this.CreateFunctionCallStatic2(typeof(double), "ATN2", mc.Arguments, sourceExpression);
                return this.sql.Math_Atan2(mc, sourceExpression);
            }

            protected virtual SqlExpression TranslateCeilingMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                Debug.Assert(mc.Method.Name == "Ceiling");
                return this.CreateFunctionCallStatic1(mc.Arguments[0].ClrType, "CEILING", mc.Arguments, sourceExpression);
            }

            protected virtual SqlExpression TranslateLogMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                Debug.Assert(mc.Method.Name == "Log");
                return CreateFunctionCallStatic1(typeof(double), "LOG", mc.Arguments, sourceExpression);
            }

            //protected virtual SqlExpression TranslateLog10Method(SqlMethodCall mc, Expression sourceExpression)
            //{
            //    Debug.Assert(mc.Method.Name == "Log10");
            //    return CreateFunctionCallStatic1(typeof(double), "LOG10", mc.Arguments, sourceExpression);
            //}

            protected virtual SqlExpression TranslateSignMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                return sql.FunctionCall(typeof(int), "SIGN", new[] { mc.Arguments[0] }, sourceExpression);
            }

            protected virtual SqlExpression TranslateSqrtMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                return this.CreateFunctionCallStatic1(typeof(double), "SQRT", mc.Arguments, sourceExpression);
            }

            #region MyRegion
            //protected virtual SqlExpression TranslateRoundMethod(SqlMethodCall mc, Expression sourceExpression)
            //{
            //    var expr = mc.Arguments[0];
            //    Type clrType = expr.ClrType;

            //    //if (mc.Arguments.Count == 1)
            //    //    return sql.FunctionCall(clrType, "round", new[] { expr }, sourceExpression);

            //    int count = mc.Arguments.Count;
            //    if (mc.Arguments[count - 1].ClrType != typeof(MidpointRounding))
            //    {
            //        throw Error.MathRoundNotSupported();
            //    }

            //    SqlExpression expression15;
            //    if (count == 2)
            //    {
            //        expression15 = sql.ValueFromObject(0, false, sourceExpression);
            //    }
            //    else
            //    {
            //        expression15 = mc.Arguments[1];
            //    }
            //    SqlExpression expression16 = mc.Arguments[count - 1];
            //    if (expression16.NodeType != SqlNodeType.Value)
            //    {
            //        throw Error.NonConstantExpressionsNotSupportedForRounding();
            //    }
            //    if (((MidpointRounding)Eval(expression16)) == MidpointRounding.AwayFromZero)
            //    {
            //        return this.sql.FunctionCall(expr.ClrType, "round", new[] { expr, expression15 }, sourceExpression);
            //    }

            //    SqlExpression expression17 = sql.FunctionCall(clrType, "round", new[] { expr, expression15 }, sourceExpression);
            //    SqlExpression expression18 = sql.Multiply(expr, 2L);
            //    SqlExpression expression19 = sql.FunctionCall(clrType, "round", new[] { expression18, expression15 }, sourceExpression);
            //    SqlExpression expression20 = sql.AndAccumulate(sql.Binary(SqlNodeType.EQ, expression18, expression19),
            //                                                   sql.Binary(SqlNodeType.NE, expr, expression17));
            //    SqlExpression expression21 = sql.Multiply(sql.FunctionCall(clrType, "round", new[] { sql.Divide(expr, 2L), expression15 },
            //                                              sourceExpression), 2L);
            //    return this.sql.SearchedCase(new[] { new SqlWhen(expression20, expression21) }, expression17, sourceExpression);
            //} 
            #endregion

            private SqlExpression TranslateMathMethod(SqlMethodCall mc)
            {
                Expression sourceExpression = mc.SourceExpression;
                switch (mc.Method.Name)
                {
                    case "Abs":
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        return sql.FunctionCall(mc.Arguments[0].ClrType, "ABS", new[] { mc.Arguments[0] }, sourceExpression);

                    case "Acos":
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        return CreateFunctionCallStatic1(typeof(double), "ACOS", mc.Arguments, sourceExpression);

                    case "Asin":
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        return this.CreateFunctionCallStatic1(typeof(double), "ASIN", mc.Arguments, sourceExpression);

                    case "Atan":
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        return sql.Math_Atan(mc, sourceExpression);

                    case "Atan2":
                        if (mc.Arguments.Count != 2)
                        {
                            break;
                        }
                        //return this.CreateFunctionCallStatic2(typeof(double), "ATN2", mc.Arguments, sourceExpression);
                        return TranslateAtan2Method(mc, sourceExpression);

                    case "BigMul":
                        if (mc.Arguments.Count != 2)
                        {
                            break;
                        }
                        return this.sql.Multiply(new[] { this.sql.ConvertToBigint(mc.Arguments[0]), this.sql.ConvertToBigint(mc.Arguments[1]) });

                    case "Ceiling":
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        //return this.CreateFunctionCallStatic1(mc.Arguments[0].ClrType, "CEILING", mc.Arguments, sourceExpression);
                        return TranslateCeilingMethod(mc, sourceExpression);

                    case "Cos":
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        return this.CreateFunctionCallStatic1(typeof(double), "COS", mc.Arguments, sourceExpression);

                    case "Cosh":
                        {
                            if (mc.Arguments.Count != 1)
                            {
                                break;
                            }
                            return this.sql.Math_Cosh(mc, sourceExpression);
                        }
                    case "Exp":
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        return this.CreateFunctionCallStatic1(typeof(double), "EXP", mc.Arguments, sourceExpression);

                    case "Floor":
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        return sql.Math_Floor(mc);

                    case "Log":
                        if (mc.Arguments.Count != 1)
                        {
                            if (mc.Arguments.Count != 2)
                            {
                                break;
                            }
                            SqlExpression first = sql.FunctionCall(typeof(double), "LOG", new[] { mc.Arguments[0] }, sourceExpression);
                            SqlExpression second = sql.FunctionCall(typeof(double), "LOG", new[] { mc.Arguments[1] }, sourceExpression);
                            return sql.Divide(first, second);
                        }
                        return sql.Math_Log(mc);

                    case "Log10":
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        return sql.Math_Log10(mc);

                    case "Max":
                        {
                            if (mc.Arguments.Count != 2)
                            {
                                break;
                            }
                            return sql.Math_Max(mc);
                        }
                    case "Min":
                        {
                            if (mc.Arguments.Count != 2)
                            {
                                break;
                            }
                            return sql.Math_Min(mc);
                        }
                    case "Pow":
                        if (mc.Arguments.Count != 2)
                        {
                            break;
                        }
                        return this.CreateFunctionCallStatic2(mc.ClrType, "POWER", mc.Arguments, sourceExpression);

                    case "Round":
                        {
                            return sql.Math_Round(mc);
                            //return TranslateRoundMethod(mc, sourceExpression);
                        }

                    case "Sign":
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        //return this.sql.FunctionCall(typeof(int), "SIGN", new SqlExpression[] { mc.Arguments[0] }, sourceExpression);
                        return TranslateSignMethod(mc, sourceExpression);

                    case "Sin":
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        return this.CreateFunctionCallStatic1(typeof(double), "SIN", mc.Arguments, sourceExpression);

                    case "Sinh":
                        {
                            if (mc.Arguments.Count != 1)
                            {
                                break;
                            }
                            return sql.Math_Sinh(mc, sourceExpression);
                        }
                    case "Sqrt":
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        //return this.CreateFunctionCallStatic1(typeof(double), "SQRT", mc.Arguments, sourceExpression);
                        return TranslateSqrtMethod(mc, sourceExpression);

                    case "Tan":
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        return this.CreateFunctionCallStatic1(typeof(double), "TAN", mc.Arguments, sourceExpression);

                    case "Tanh":
                        {
                            if (mc.Arguments.Count != 1)
                            {
                                break;
                            }
                            SqlExpression expression26 = mc.Arguments[0];
                            SqlExpression expression27 = this.sql.FunctionCall(typeof(double), "EXP", new SqlExpression[] { expression26 }, sourceExpression);
                            SqlExpression expression28 = this.sql.Unary(SqlNodeType.Negate, expression26, sourceExpression);
                            SqlExpression expression29 = this.sql.FunctionCall(typeof(double), "EXP", new SqlExpression[] { expression28 }, sourceExpression);
                            return this.sql.Divide(this.sql.Subtract(expression27, expression29), this.sql.Add(new SqlExpression[] { expression27, expression29 }));
                        }
                    case "Truncate":
                        {
                            if (mc.Arguments.Count != 1)
                            {
                                break;
                            }
                            return sql.Math_Truncate(mc);
                        }
                }
                throw GetMethodSupportException(mc);
            }




            private SqlExpression TranslateNewDateTime(SqlNew sox)
            {
                Expression sourceExpression = sox.SourceExpression;
                if ((((sox.ClrType == typeof(DateTime)) && (sox.Args.Count >= 3)) && ((sox.Args[0].ClrType == typeof(int)) && (sox.Args[1].ClrType == typeof(int)))) && (sox.Args[2].ClrType == typeof(int)))
                {
                    SqlExpression expression2 = this.sql.FunctionCall(typeof(void), "NCHAR", new SqlExpression[] { this.sql.ValueFromObject(2, false, sourceExpression) }, sourceExpression);
                    SqlExpression expression3 = this.sql.FunctionCall(typeof(void), "NCHAR", new SqlExpression[] { this.sql.ValueFromObject(4, false, sourceExpression) }, sourceExpression);
                    SqlExpression expression4 = this.sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[] { expression3, sox.Args[0] }, sourceExpression);
                    SqlExpression expression5 = this.sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[] { expression2, sox.Args[1] }, sourceExpression);
                    SqlExpression expression6 = this.sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[] { expression2, sox.Args[2] }, sourceExpression);
                    SqlExpression expression7 = new SqlVariable(typeof(void), null, "DATETIME", sourceExpression);
                    if (sox.Args.Count == 3)
                    {
                        SqlExpression expression8 = this.sql.Concat(new SqlExpression[] { expression5, this.sql.ValueFromObject("/", false, sourceExpression), expression6, this.sql.ValueFromObject("/", false, sourceExpression), expression4 }, sourceExpression);
                        return this.sql.FunctionCall(typeof(DateTime), "CONVERT", new SqlExpression[] { expression7, expression8, this.sql.ValueFromObject(0x65, false, sourceExpression) }, sourceExpression);
                    }
                    if (((sox.Args.Count >= 6) && (sox.Args[3].ClrType == typeof(int))) && ((sox.Args[4].ClrType == typeof(int)) && (sox.Args[5].ClrType == typeof(int))))
                    {
                        SqlExpression expression9 = this.sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[] { expression2, sox.Args[3] }, sourceExpression);
                        SqlExpression expression10 = this.sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[] { expression2, sox.Args[4] }, sourceExpression);
                        SqlExpression expression11 = this.sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[] { expression2, sox.Args[5] }, sourceExpression);
                        SqlExpression expression12 = this.sql.Concat(new SqlExpression[] { expression4, this.sql.ValueFromObject("-", false, sourceExpression), expression5, this.sql.ValueFromObject("-", false, sourceExpression), expression6 }, sourceExpression);
                        SqlExpression expression13 = this.sql.Concat(new SqlExpression[] { expression9, this.sql.ValueFromObject(":", false, sourceExpression), expression10, this.sql.ValueFromObject(":", false, sourceExpression), expression11 }, sourceExpression);
                        SqlExpression expression14 = this.sql.Concat(new SqlExpression[] { expression12, this.sql.ValueFromObject(' ', false, sourceExpression), expression13 }, sourceExpression);
                        if (sox.Args.Count == 6)
                        {
                            return this.sql.FunctionCall(typeof(DateTime), "CONVERT", new SqlExpression[] { expression7, expression14, this.sql.ValueFromObject(120, false, sourceExpression) }, sourceExpression);
                        }
                        if ((sox.Args.Count == 7) && (sox.Args[6].ClrType == typeof(int)))
                        {
                            SqlExpression expression16;
                            SqlExpression expression15 = this.sql.FunctionCall(typeof(string), "CONVERT", new SqlExpression[] { expression3, this.sql.Add(new SqlExpression[] { this.sql.ValueFromObject(0x3e8, false, sourceExpression), sox.Args[6] }) }, sourceExpression);
                            if (sqlProvider.Mode == SqlProvider.ProviderMode.SqlCE)
                            {
                                SqlExpression left = this.sql.FunctionCall(typeof(int), "LEN", new SqlExpression[] { expression15 }, sourceExpression);
                                SqlExpression expression18 = this.sql.Binary(SqlNodeType.Sub, left, this.sql.ValueFromObject(2, false, sourceExpression));
                                expression16 = this.sql.FunctionCall(typeof(string), "SUBSTRING", new SqlExpression[] { expression15, expression18, this.sql.ValueFromObject(3, false, sourceExpression) }, sourceExpression);
                            }
                            else
                            {
                                expression16 = this.sql.FunctionCall(typeof(string), "RIGHT", new SqlExpression[] { expression15, this.sql.ValueFromObject(3, false, sourceExpression) }, sourceExpression);
                            }
                            expression14 = this.sql.Concat(new SqlExpression[] { expression14, this.sql.ValueFromObject('.', false, sourceExpression), expression16 }, sourceExpression);
                            return this.sql.FunctionCall(typeof(DateTime), "CONVERT", new SqlExpression[] { expression7, expression14, this.sql.ValueFromObject(0x79, false, sourceExpression) }, sourceExpression);
                        }
                    }
                }
                throw Error.UnsupportedDateTimeConstructorForm();
            }

            private SqlExpression TranslateNewString(SqlNew sox)
            {
                if (((sox.ClrType != typeof(string)) || (sox.Args.Count != 2)) || ((sox.Args[0].ClrType != typeof(char)) || (sox.Args[1].ClrType != typeof(int))))
                {
                    throw Error.UnsupportedStringConstructorForm();
                }
                return this.sql.FunctionCall(typeof(string), "REPLICATE", new SqlExpression[] { sox.Args[0], sox.Args[1] }, sox.SourceExpression);
            }

            private SqlExpression TranslateNewTimeSpan(SqlNew sox)
            {
                if (sox.Args.Count == 1)
                {
                    return this.sql.ConvertTo(typeof(TimeSpan), sox.Args[0]);
                }
                if (sox.Args.Count == 3)
                {
                    SqlExpression expression = this.sql.ConvertToBigint(sox.Args[0]);
                    SqlExpression expression2 = this.sql.ConvertToBigint(sox.Args[1]);
                    SqlExpression expression3 = this.sql.ConvertToBigint(sox.Args[2]);
                    SqlExpression expression4 = this.sql.Multiply(expression, 0x861c46800L);
                    SqlExpression expression5 = this.sql.Multiply(expression2, 0x23c34600L);
                    SqlExpression expression6 = this.sql.Multiply(expression3, 0x989680L);
                    return this.sql.ConvertTo(typeof(TimeSpan), this.sql.Add(new SqlExpression[] { expression4, expression5, expression6 }));
                }
                SqlExpression expr = this.sql.ConvertToBigint(sox.Args[0]);
                SqlExpression expression8 = this.sql.ConvertToBigint(sox.Args[1]);
                SqlExpression expression9 = this.sql.ConvertToBigint(sox.Args[2]);
                SqlExpression expression10 = this.sql.ConvertToBigint(sox.Args[3]);
                SqlExpression expression11 = this.sql.Multiply(expr, 0xc92a69c000L);
                SqlExpression expression12 = this.sql.Multiply(expression8, 0x861c46800L);
                SqlExpression expression13 = this.sql.Multiply(expression9, 0x23c34600L);
                SqlExpression expression14 = this.sql.Multiply(expression10, 0x989680L);
                SqlExpression expression15 = this.sql.Add(new SqlExpression[] { expression11, expression12, expression13, expression14 });
                if (sox.Args.Count == 4)
                {
                    return this.sql.ConvertTo(typeof(TimeSpan), expression15);
                }
                if (sox.Args.Count != 5)
                {
                    throw Error.UnsupportedTimeSpanConstructorForm();
                }
                SqlExpression expression16 = this.sql.ConvertToBigint(sox.Args[4]);
                SqlExpression expression17 = this.sql.Multiply(expression16, 0x2710L);
                return this.sql.ConvertTo(typeof(TimeSpan), this.sql.Add(new SqlExpression[] { expression15, expression17 }));
            }



            private SqlExpression TranslateStringMethod(SqlMethodCall mc)
            {
                SqlExpression expression2;
                SqlExpression expression3;
                bool flag;
                Expression sourceExpression = mc.SourceExpression;
                switch (mc.Method.Name)
                {
                    case "Contains":
                        {
                            if (mc.Arguments.Count != 1)
                            {
                                goto Label_1B30;
                            }
                            expression2 = mc.Arguments[0];
                            expression3 = null;
                            flag = true;
                            if (expression2.NodeType != SqlNodeType.Value)
                            {
                                if (expression2.NodeType != SqlNodeType.ClientParameter)
                                {
                                    throw Error.NonConstantExpressionsNotSupportedFor("String.Contains");
                                }
                                var parameter = (SqlClientParameter)expression2;
                                expression2 = new SqlClientParameter(parameter.ClrType, parameter.SqlType, Expression.Lambda(Expression.Call(typeof(SqlHelpers), "GetStringContainsPattern", Type.EmptyTypes, new Expression[] { parameter.Accessor.Body, Expression.Constant('~') }), new ParameterExpression[] { parameter.Accessor.Parameters[0] }), parameter.SourceExpression);
                                break;
                            }
                            string stringContainsPattern = SqlHelpers.GetStringContainsPattern((string)((SqlValue)expression2).Value, '~');
                            flag = stringContainsPattern.Contains("~");
                            expression2 = this.sql.ValueFromObject(stringContainsPattern, true, expression2.SourceExpression);
                            break;
                        }
                    case "StartsWith":
                        {
                            if (mc.Arguments.Count != 1)
                            {
                                goto Label_1B30;
                            }
                            SqlExpression pattern = mc.Arguments[0];
                            SqlExpression escape = null;
                            bool flag2 = true;
                            if (pattern.NodeType != SqlNodeType.Value)
                            {
                                if (pattern.NodeType != SqlNodeType.ClientParameter)
                                {
                                    throw Error.NonConstantExpressionsNotSupportedFor("String.StartsWith");
                                }
                                var parameter2 = (SqlClientParameter)pattern;
                                pattern = new SqlClientParameter(parameter2.ClrType, parameter2.SqlType, Expression.Lambda(Expression.Call(typeof(SqlHelpers), "GetStringStartsWithPattern", Type.EmptyTypes, new Expression[] { parameter2.Accessor.Body, Expression.Constant('~') }), new ParameterExpression[] { parameter2.Accessor.Parameters[0] }), parameter2.SourceExpression);
                            }
                            else
                            {
                                string stringStartsWithPattern = SqlHelpers.GetStringStartsWithPattern((string)((SqlValue)pattern).Value, '~');
                                flag2 = stringStartsWithPattern.Contains("~");
                                pattern = this.sql.ValueFromObject(stringStartsWithPattern, true, pattern.SourceExpression);
                            }
                            if (flag2)
                            {
                                escape = this.sql.ValueFromObject("~", false, sourceExpression);
                            }
                            return this.sql.Like(mc.Object, pattern, escape, sourceExpression);
                        }
                    case "EndsWith":
                        {
                            if (mc.Arguments.Count != 1)
                            {
                                goto Label_1B30;
                            }
                            SqlExpression expression6 = mc.Arguments[0];
                            SqlExpression expression7 = null;
                            bool flag3 = true;
                            if (expression6.NodeType != SqlNodeType.Value)
                            {
                                if (expression6.NodeType != SqlNodeType.ClientParameter)
                                {
                                    throw Error.NonConstantExpressionsNotSupportedFor("String.EndsWith");
                                }
                                var parameter3 = (SqlClientParameter)expression6;
                                expression6 = new SqlClientParameter(parameter3.ClrType, parameter3.SqlType, Expression.Lambda(Expression.Call(typeof(SqlHelpers), "GetStringEndsWithPattern", Type.EmptyTypes, new Expression[] { parameter3.Accessor.Body, Expression.Constant('~') }), new ParameterExpression[] { parameter3.Accessor.Parameters[0] }), parameter3.SourceExpression);
                            }
                            else
                            {
                                string stringEndsWithPattern = SqlHelpers.GetStringEndsWithPattern((string)((SqlValue)expression6).Value, '~');
                                flag3 = stringEndsWithPattern.Contains("~");
                                expression6 = this.sql.ValueFromObject(stringEndsWithPattern, true, expression6.SourceExpression);
                            }
                            if (flag3)
                            {
                                expression7 = this.sql.ValueFromObject("~", false, sourceExpression);
                            }
                            return sql.Like(mc.Object, expression6, expression7, sourceExpression);
                        }
                    case "IndexOf":
                        {
                            return sql.String_IndexOf(mc, sourceExpression); //
                        }
                    case "LastIndexOf":
                        {
                            if (mc.Arguments.Count == 1)
                            {
                                SqlExpression expression15 = mc.Arguments[0];
                                if ((expression15 is SqlValue) && (((SqlValue)expression15).Value == null))
                                {
                                    throw Error.ArgumentNull("value");
                                }
                                SqlExpression expression16 = mc.Object;
                                SqlExpression expression17 = this.sql.FunctionCall(typeof(string), "REVERSE", new SqlExpression[] { expression16 }, sourceExpression);
                                SqlExpression expression18 = this.sql.FunctionCall(typeof(string), "REVERSE", new SqlExpression[] { expression15 }, sourceExpression);
                                SqlExpression expression19 = this.sql.FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { expression15, expression16 }, sourceExpression);
                                SqlExpression expression20 = this.sql.FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { expression18, expression17 }, sourceExpression);
                                SqlExpression match = this.sql.Binary(SqlNodeType.EQ, expression19, this.sql.ValueFromObject(0, false, sourceExpression));
                                SqlExpression expression22 = this.sql.CLRLENGTH(expression16);
                                SqlExpression expression23 = this.sql.CLRLENGTH(expression15);
                                SqlExpression expression24 = this.sql.Add(new SqlExpression[] { this.sql.ValueFromObject(1, false, sourceExpression), this.sql.Subtract(expression22, this.sql.Add(new SqlExpression[] { expression23, expression20 })) });
                                SqlWhen when4 = new SqlWhen(match, this.sql.ValueFromObject(-1, false, sourceExpression));
                                SqlWhen when5 = new SqlWhen(this.sql.Binary(SqlNodeType.EQ, this.sql.CLRLENGTH(mc.Arguments[0]), this.sql.ValueFromObject(0, sourceExpression)), this.sql.Subtract(this.sql.CLRLENGTH(expression16), 1));
                                return this.sql.SearchedCase(new SqlWhen[] { when5, when4 }, expression24, sourceExpression);
                            }
                            if (mc.Arguments.Count == 2)
                            {
                                if (mc.Arguments[1].ClrType == typeof(StringComparison))
                                {
                                    throw Error.LastIndexOfWithStringComparisonArgNotSupported();
                                }
                                SqlExpression expression26 = mc.Object;
                                SqlExpression expression27 = mc.Arguments[0];
                                if ((expression27 is SqlValue) && (((SqlValue)expression27).Value == null))
                                {
                                    throw Error.ArgumentNull("value");
                                }
                                SqlExpression expression28 = mc.Arguments[1];
                                SqlExpression expression29 = this.sql.FunctionCall(typeof(string), "LEFT", new[] { expression26, this.sql.Add(expression28, 1) }, sourceExpression);
                                SqlExpression expression30 = this.sql.FunctionCall(typeof(string), "REVERSE", new[] { expression29 }, sourceExpression);
                                SqlExpression expression31 = this.sql.FunctionCall(typeof(string), "REVERSE", new[] { expression27 }, sourceExpression);
                                SqlExpression expression32 = this.sql.FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { expression27, expression29 }, sourceExpression);
                                SqlExpression expression33 = this.sql.FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { expression31, expression30 }, sourceExpression);
                                SqlExpression expression34 = this.sql.Binary(SqlNodeType.EQ, expression32, this.sql.ValueFromObject(0, false, sourceExpression));
                                SqlExpression expression35 = this.sql.CLRLENGTH(expression29);
                                SqlExpression expression36 = this.sql.CLRLENGTH(expression27);
                                SqlExpression expression37 = this.sql.Add(new SqlExpression[] { this.sql.ValueFromObject(1, false, sourceExpression), this.sql.Subtract(expression35, this.sql.Add(new SqlExpression[] { expression36, expression33 })) });
                                var when6 = new SqlWhen(expression34, this.sql.ValueFromObject(-1, false, sourceExpression));
                                SqlExpression expression38 = this.sql.Binary(SqlNodeType.EQ, this.sql.CLRLENGTH(mc.Arguments[0]), this.sql.ValueFromObject(0, sourceExpression));
                                var when7 = new SqlWhen(this.sql.AndAccumulate(expression38, this.sql.Binary(SqlNodeType.LE, this.sql.Add(mc.Arguments[1], 1), this.sql.CLRLENGTH(expression26))), mc.Arguments[1]);
                                return this.sql.SearchedCase(new SqlWhen[] { when7, when6 }, expression37, sourceExpression);
                            }
                            if (mc.Arguments.Count != 3)
                            {
                                goto Label_1B30;
                            }
                            if (mc.Arguments[2].ClrType == typeof(StringComparison))
                            {
                                throw Error.LastIndexOfWithStringComparisonArgNotSupported();
                            }
                            SqlExpression expr = mc.Object;
                            SqlExpression expression40 = mc.Arguments[0];
                            if ((expression40 is SqlValue) && (((SqlValue)expression40).Value == null))
                            {
                                throw Error.ArgumentNull("value");
                            }
                            SqlExpression expression41 = mc.Arguments[1];
                            SqlExpression second = mc.Arguments[2];
                            SqlExpression expression43 = sql.FunctionCall(typeof(string), "LEFT", new SqlExpression[] { expr, this.sql.Add(expression41, 1) }, sourceExpression);
                            SqlExpression expression44 = sql.FunctionCall(typeof(string), "REVERSE", new SqlExpression[] { expression43 }, sourceExpression);
                            SqlExpression expression45 = sql.FunctionCall(typeof(string), "REVERSE", new SqlExpression[] { expression40 }, sourceExpression);
                            SqlExpression expression46 = sql.FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { expression40, expression43 }, sourceExpression);
                            SqlExpression expression47 = sql.FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { expression45, expression44 }, sourceExpression);
                            SqlExpression first = this.sql.CLRLENGTH(expression43);
                            SqlExpression expression49 = sql.CLRLENGTH(expression40);
                            SqlExpression expression50 = sql.Add(new SqlExpression[] { this.sql.ValueFromObject(1, false, sourceExpression), this.sql.Subtract(first, this.sql.Add(new SqlExpression[] { expression49, expression47 })) });
                            SqlExpression expression51 = sql.Binary(SqlNodeType.EQ, expression46, this.sql.ValueFromObject(0, false, sourceExpression));
                            var when8 = new SqlWhen(this.sql.OrAccumulate(expression51, this.sql.Binary(SqlNodeType.LE, expression50, this.sql.Subtract(expression41, second))), this.sql.ValueFromObject(-1, false, sourceExpression));
                            SqlExpression expression52 = sql.Binary(SqlNodeType.EQ, this.sql.CLRLENGTH(mc.Arguments[0]), this.sql.ValueFromObject(0, sourceExpression));
                            var when9 = new SqlWhen(this.sql.AndAccumulate(expression52, this.sql.Binary(SqlNodeType.LE, this.sql.Add(mc.Arguments[1], 1), this.sql.CLRLENGTH(expr))), mc.Arguments[1]);
                            return this.sql.SearchedCase(new SqlWhen[] { when9, when8 }, expression50, sourceExpression);
                        }
                    case "Insert":
                        if (mc.Arguments.Count == 2)
                        {
                            var result = sql.String_Insert(mc);
                            return result ?? TranslateStringInsertMethod(mc, sourceExpression);
                        }
                        goto Label_1B30;

                    case "PadLeft":
                        {
                            if (mc.Arguments.Count != 1)
                            {
                                if (mc.Arguments.Count == 2)
                                {
                                    SqlExpression expression62 = mc.Object;
                                    SqlExpression expression63 = mc.Arguments[0];
                                    SqlExpression expression64 = mc.Arguments[1];
                                    SqlExpression expression65 = this.sql.Binary(SqlNodeType.GE, this.sql.CLRLENGTH(expression62), expression63);
                                    SqlExpression expression66 = this.sql.CLRLENGTH(expression62);
                                    SqlExpression expression67 = this.sql.Subtract(expression63, expression66);
                                    SqlExpression expression68 = this.sql.FunctionCall(typeof(string), "REPLICATE", new SqlExpression[] { expression64, expression67 }, sourceExpression);
                                    SqlExpression expression69 = this.sql.Concat(new SqlExpression[] { expression68, expression62 }, sourceExpression);
                                    return this.sql.SearchedCase(new SqlWhen[] { new SqlWhen(expression65, expression62) }, expression69, sourceExpression);
                                }
                                goto Label_1B30;
                            }
                            SqlExpression expression55 = mc.Object;
                            SqlExpression right = mc.Arguments[0];
                            SqlExpression expression57 = this.sql.CLRLENGTH(expression55);
                            SqlExpression expression58 = this.sql.Binary(SqlNodeType.GE, expression57, right);
                            SqlExpression expression59 = this.sql.Subtract(right, expression57);
                            SqlExpression expression60 = this.sql.FunctionCall(typeof(string), "SPACE", new SqlExpression[] { expression59 }, sourceExpression);
                            SqlExpression expression61 = this.sql.Concat(new SqlExpression[] { expression60, expression55 }, sourceExpression);
                            return this.sql.SearchedCase(new SqlWhen[] { new SqlWhen(expression58, expression55) }, expression61, sourceExpression);
                        }
                    case "PadRight":
                        {
                            if (mc.Arguments.Count != 1)
                            {
                                if (mc.Arguments.Count == 2)
                                {
                                    SqlExpression expression77 = mc.Object;
                                    SqlExpression expression78 = mc.Arguments[0];
                                    SqlExpression expression79 = mc.Arguments[1];
                                    SqlExpression expression80 = this.sql.Binary(SqlNodeType.GE, this.sql.CLRLENGTH(expression77), expression78);
                                    SqlExpression expression81 = this.sql.CLRLENGTH(expression77);
                                    SqlExpression expression82 = this.sql.Subtract(expression78, expression81);
                                    SqlExpression expression83 = this.sql.FunctionCall(typeof(string), "REPLICATE", new SqlExpression[] { expression79, expression82 }, sourceExpression);
                                    SqlExpression expression84 = this.sql.Concat(new SqlExpression[] { expression77, expression83 }, sourceExpression);
                                    return this.sql.SearchedCase(new SqlWhen[] { new SqlWhen(expression80, expression77) }, expression84, sourceExpression);
                                }
                                goto Label_1B30;
                            }
                            SqlExpression expression70 = mc.Object;
                            SqlExpression expression71 = mc.Arguments[0];
                            SqlExpression expression72 = this.sql.Binary(SqlNodeType.GE, this.sql.CLRLENGTH(expression70), expression71);
                            SqlExpression expression73 = this.sql.CLRLENGTH(expression70);
                            SqlExpression expression74 = this.sql.Subtract(expression71, expression73);
                            SqlExpression expression75 = this.sql.FunctionCall(typeof(string), "SPACE", new SqlExpression[] { expression74 }, sourceExpression);
                            SqlExpression expression76 = this.sql.Concat(new SqlExpression[] { expression70, expression75 }, sourceExpression);
                            return this.sql.SearchedCase(new SqlWhen[] { new SqlWhen(expression72, expression70) }, expression76, sourceExpression);
                        }
                    case "Remove":
                        return sql.String_Remove(mc);
                    case "Replace":
                        return sql.String_Replace(mc);
                    case "Substring":
                        return sql.String_Substring(mc);
                    //return TranslateStringSubstringMethod(mc, sourceExpression);
                    case "Trim":
                        if (mc.Arguments.Count != 0)
                        {
                            goto Label_1B30;
                        }
                        return sql.String_Trim(mc);
                    case "TrimEnd":
                        return sql.String_TrimEnd(mc);
                    case "TrimStart":
                        return sql.String_TrimStart(mc);
                    case "ToLower":
                        return TranslateStringToLowerMethod(mc, sourceExpression);

                    case "ToUpper":
                        return TranslateStringToUpperMethod(mc, sourceExpression);

                    case "get_Chars":
                        return TranslateStringGetCharsMethod(mc, sourceExpression);

                    case "CompareTo":
                        if (mc.Arguments.Count == 1)
                        {
                            if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
                            {
                                throw Error.ArgumentNull("value");
                            }
                            return this.CreateComparison(mc.Object, mc.Arguments[0], sourceExpression);
                        }
                        goto Label_1B30;

                    default:
                        goto Label_1B30;
                }
                if (flag)
                {
                    expression3 = this.sql.ValueFromObject("~", false, sourceExpression);
                }
                return this.sql.Like(mc.Object, expression2, expression3, sourceExpression);
            Label_1B30:
                throw GetMethodSupportException(mc);
            }

            [System.Obsolete]
            protected virtual SqlExpression TranslateStringGetCharsMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                return sql.String_GetChar(mc, sourceExpression);

            }

            protected virtual SqlExpression TranslateStringToUpperMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                if (mc.Arguments.Count != 0)
                    throw GetMethodSupportException(mc);

                return sql.FunctionCall(typeof(string), "UPPER", new[] { mc.Object }, sourceExpression);
            }

            protected virtual SqlExpression TranslateStringToLowerMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                if (mc.Arguments.Count != 0)
                {
                    throw GetMethodSupportException(mc);
                }
                return this.sql.FunctionCall(typeof(string), "LOWER", new[] { mc.Object }, sourceExpression);
            }

            //protected virtual SqlExpression TranslateStringSubstringMethod(SqlMethodCall mc, Expression sourceExpression)
            //{
            //    SqlExpression[] args;

            //    if (mc.Arguments.Count != 1)
            //    {
            //        if (mc.Arguments.Count == 2)
            //        {
            //            args = new[] { mc.Object, sql.Add(mc.Arguments[0], 1), mc.Arguments[1] };
            //            return sql.FunctionCall(typeof(string), "SUBSTRING", args, sourceExpression);
            //        }
            //        throw GetMethodSupportException(mc);
            //    }
            //    args = new[] { mc.Object, sql.Add(mc.Arguments[0], 1), sql.CLRLENGTH(mc.Object) };
            //    return sql.FunctionCall(typeof(string), "SUBSTRING", args, sourceExpression);

            //}

            protected virtual SqlExpression TranslateStringInsertMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                if ((mc.Arguments[1] is SqlValue) && (((SqlValue)mc.Arguments[1]).Value == null))
                {
                    throw Error.ArgumentNull("value");
                }
                SqlFunctionCall call = sql.FunctionCall(typeof(string), "STUFF", new[] { mc.Object, sql.Add(mc.Arguments[0], 1), sql.ValueFromObject(0, false, sourceExpression), mc.Arguments[1] }, sourceExpression);
                SqlExpression expression53 = sql.Binary(SqlNodeType.EQ, sql.CLRLENGTH(mc.Object), mc.Arguments[0]);
                SqlExpression expression54 = sql.Concat(new[] { mc.Object, mc.Arguments[1] }, sourceExpression);
                return this.sql.SearchedCase(new[] { new SqlWhen(expression53, expression54) }, call, sourceExpression);
            }

            private SqlExpression TranslateStringStaticMethod(SqlMethodCall mc)
            {
                var sourceExpression = mc.SourceExpression;
                if (mc.Method.Name == "Concat")
                {
                    var array = mc.Arguments[0] as SqlClientArray;
                    List<SqlExpression> expressions = null;
                    expressions = array != null ? array.Expressions : mc.Arguments;
                    if (expressions.Count == 0)
                    {
                        sql.ValueFromObject("", false, sourceExpression);
                    }
                    else
                    {
                        SqlExpression expression2;
                        if (expressions[0].SqlType.IsString || expressions[0].SqlType.IsChar)
                        {
                            expression2 = expressions[0];
                        }
                        else
                        {
                            expression2 = this.sql.ConvertTo(typeof(string), expressions[0]);
                        }
                        for (int i = 1; i < expressions.Count; i++)
                        {
                            if (expressions[i].SqlType.IsString || expressions[i].SqlType.IsChar)
                            {
                                expression2 = sql.Concat(new[] { expression2, expressions[i] }, sourceExpression);
                            }
                            else
                            {
                                expression2 = sql.Concat(new[] { expression2, this.sql.ConvertTo(typeof(string), expressions[i]) }, sourceExpression);
                            }
                        }
                    }
                }
                else if ((mc.Method.Name == "Equals") && (mc.Arguments.Count == 2))
                {
                    this.sql.Binary(SqlNodeType.EQ2V, mc.Arguments[0], mc.Arguments[1]);
                }
                else if ((mc.Method.Name == "Compare") && (mc.Arguments.Count == 2))
                {
                    this.CreateComparison(mc.Arguments[0], mc.Arguments[1], sourceExpression);
                }
                throw GetMethodSupportException(mc);
            }

            private SqlExpression TranslateTimeSpanInstanceMethod(SqlMethodCall mc)
            {
                SqlExpression expression = null;
                Expression sourceExpression = mc.SourceExpression;
                if (mc.Method.Name == "Add")
                {
                    return this.sql.Add(new[] { mc.Object, mc.Arguments[0] });
                }
                if (mc.Method.Name == "Subtract")
                {
                    return this.sql.Subtract(mc.Object, mc.Arguments[0]);
                }
                if (mc.Method.Name == "CompareTo")
                {
                    return this.CreateComparison(mc.Object, mc.Arguments[0], sourceExpression);
                }
                if (mc.Method.Name == "Duration")
                {
                    return sql.FunctionCall(typeof(TimeSpan), "ABS", new[] { mc.Object }, sourceExpression);
                }
                if (mc.Method.Name == "Negate")
                {
                    expression = sql.Unary(SqlNodeType.Negate, mc.Object, sourceExpression);
                }
                return expression;
            }

            private SqlExpression TranslateVbCompareString(SqlMethodCall mc)
            {
                if (mc.Arguments.Count < 2)
                {
                    throw GetMethodSupportException(mc);
                }
                return this.CreateComparison(mc.Arguments[0], mc.Arguments[1], mc.SourceExpression);
            }

            private SqlExpression TranslateVbConversionMethod(SqlMethodCall mc)
            {
                Expression sourceExpression = mc.SourceExpression;
                if (mc.Arguments.Count == 1)
                {
                    SqlExpression discriminator = mc.Arguments[0];
                    Type clrType = null;
                    switch (mc.Method.Name)
                    {
                        case "ToBoolean":
                            clrType = typeof(bool);
                            break;

                        case "ToSByte":
                            clrType = typeof(sbyte);
                            break;

                        case "ToByte":
                            clrType = typeof(byte);
                            break;

                        case "ToChar":
                            clrType = typeof(char);
                            break;

                        case "ToCharArrayRankOne":
                            clrType = typeof(char[]);
                            break;

                        case "ToDate":
                            clrType = typeof(DateTime);
                            break;

                        case "ToDecimal":
                            clrType = typeof(decimal);
                            break;

                        case "ToDouble":
                            clrType = typeof(double);
                            break;

                        case "ToInteger":
                            clrType = typeof(int);
                            break;

                        case "ToUInteger":
                            clrType = typeof(uint);
                            break;

                        case "ToLong":
                            clrType = typeof(long);
                            break;

                        case "ToULong":
                            clrType = typeof(ulong);
                            break;

                        case "ToShort":
                            clrType = typeof(short);
                            break;

                        case "ToUShort":
                            clrType = typeof(ushort);
                            break;

                        case "ToSingle":
                            clrType = typeof(float);
                            break;

                        case "ToString":
                            clrType = typeof(string);
                            break;
                    }
                    if (clrType != null)
                    {
                        if (((clrType == typeof(int)) || (clrType == typeof(float))) && (discriminator.ClrType == typeof(bool)))
                        {
                            var matches = new List<SqlExpression>();
                            var values = new List<SqlExpression>();
                            matches.Add(this.sql.ValueFromObject(true, false, sourceExpression));
                            values.Add(this.sql.ValueFromObject(-1, false, sourceExpression));
                            matches.Add(this.sql.ValueFromObject(false, false, sourceExpression));
                            values.Add(this.sql.ValueFromObject(0, false, sourceExpression));
                            return this.sql.Case(clrType, discriminator, matches, values, sourceExpression);
                        }
                        if (mc.ClrType != mc.Arguments[0].ClrType)
                        {
                            return this.sql.ConvertTo(clrType, discriminator);
                        }
                        return mc.Arguments[0];
                    }
                }
                throw GetMethodSupportException(mc);
            }

            private SqlExpression TranslateVbLikeString(SqlMethodCall mc)
            {
                bool flag = true;
                Expression sourceExpression = mc.SourceExpression;
                SqlExpression pattern = mc.Arguments[1];
                if (pattern.NodeType == SqlNodeType.Value)
                {
                    string str = SqlHelpers.TranslateVBLikePattern((string)((SqlValue)pattern).Value, '~');
                    pattern = this.sql.ValueFromObject(str, typeof(string), true, sourceExpression);
                    flag = str.Contains("~");
                }
                else
                {
                    if (pattern.NodeType != SqlNodeType.ClientParameter)
                    {
                        throw Error.NonConstantExpressionsNotSupportedFor("LIKE");
                    }
                    var parameter = (SqlClientParameter)pattern;
                    pattern = new SqlClientParameter(parameter.ClrType, parameter.SqlType, Expression.Lambda(Expression.Call(typeof(SqlHelpers), "TranslateVBLikePattern", Type.EmptyTypes, new Expression[] { parameter.Accessor.Body, Expression.Constant('~') }), new ParameterExpression[] { parameter.Accessor.Parameters[0] }), parameter.SourceExpression);
                }
                SqlExpression escape = flag ? this.sql.ValueFromObject("~", false, mc.SourceExpression) : null;
                return this.sql.Like(mc.Arguments[0], pattern, escape, sourceExpression);
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                bo = (SqlBinary)base.VisitBinaryOperator(bo);
                if (TypeSystem.GetNonNullableType(bo.Left.ClrType) == typeof(DateTime))
                {
                    return TranslateDateTimeBinary(bo);
                }
                return bo;
            }

            protected override SqlNode VisitMember(SqlMember m)
            {
                SqlExpression expr = VisitExpression(m.Expression);
                MemberInfo member = m.Member;
                Expression sourceExpression = m.SourceExpression;
                if ((expr.ClrType == typeof(string)) && (member.Name == "Length"))
                {
                    return this.sql.String_Length(expr);
                }
                if ((expr.ClrType == typeof(Binary)) && (member.Name == "Length"))
                {
                    return this.sql.DATALENGTH(expr);
                }
                if (expr.ClrType == typeof(DateTime))
                {

                    switch (member.Name)
                    {
                        case "Day":
                            return sql.DateTime_Day(m, expr);
                        case "DayOfWeek":
                            return sql.DateTime_DayOfWeek(m, expr);
                        case "DayOfYear":
                            return sql.DateTime_DayOfYear(m, expr);
                        case "Date":
                            return sql.DateTime_Date(m, expr);
                        case "Hour":
                            return sql.DateTime_Hour(m, expr);
                        case "Minute":
                            return sql.DateTime_Minute(m, expr);
                        case "Month":
                            return sql.DateTime_Month(m, expr);
                        case "Second":
                            return sql.DateTime_Second(m, expr);
                        case "TimeOfDay":
                            return sql.DateTime_TimeOfDay(m, expr);
                        case "Year":
                            return sql.DateTime_Year(m, expr);
                    }
                    string datePart = PostBindDotNetConverter.GetDatePart(member.Name);
                    if (datePart != null)
                    {
                        return sql.DATEPART(datePart, expr);
                    }
                }
                else if (expr.ClrType == typeof(TimeSpan))
                {
                    switch (member.Name)
                    {
                        case "Ticks":
                            return this.sql.ConvertToBigint(expr);

                        case "TotalMilliseconds":
                            return this.sql.Divide(this.sql.ConvertToDouble(expr), (long)0x2710L);

                        case "TotalSeconds":
                            return this.sql.Divide(this.sql.ConvertToDouble(expr), (long)0x989680L);

                        case "TotalMinutes":
                            return this.sql.Divide(this.sql.ConvertToDouble(expr), (long)0x23c34600L);

                        case "TotalHours":
                            return this.sql.Divide(this.sql.ConvertToDouble(expr), (long)0x861c46800L);

                        case "TotalDays":
                            return this.sql.Divide(this.sql.ConvertToDouble(expr), (long)0xc92a69c000L);

                        case "Milliseconds":
                            return this.sql.ConvertToInt(this.sql.Mod(this.sql.ConvertToBigint(this.sql.Divide(expr, (long)0x2710L)), 0x3e8L));

                        case "Seconds":
                            return this.sql.ConvertToInt(this.sql.Mod(this.sql.ConvertToBigint(this.sql.Divide(expr, (long)0x989680L)), 60L));

                        case "Minutes":
                            return this.sql.ConvertToInt(this.sql.Mod(this.sql.ConvertToBigint(this.sql.Divide(expr, (long)0x23c34600L)), 60L));

                        case "Hours":
                            return this.sql.ConvertToInt(this.sql.Mod(this.sql.ConvertToBigint(this.sql.Divide(expr, (long)0x861c46800L)), 0x18L));

                        case "Days":
                            return this.sql.ConvertToInt(this.sql.Divide(expr, (long)0xc92a69c000L));
                    }
                    throw Error.MemberCannotBeTranslated(member.DeclaringType, member.Name);
                }
                throw Error.MemberCannotBeTranslated(member.DeclaringType, member.Name);
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
            {
                Type declaringType = mc.Method.DeclaringType;
                Expression sourceExpression = mc.SourceExpression;
                SqlExpression expression = null;
                mc.Object = this.VisitExpression(mc.Object);
                int num = 0;
                int count = mc.Arguments.Count;
                while (num < count)
                {
                    mc.Arguments[num] = this.VisitExpression(mc.Arguments[num]);
                    num++;
                }
                if (mc.Method.IsStatic)
                {
                    if ((mc.Method.Name == "op_Explicit") || (mc.Method.Name == "op_Implicit"))
                    {
                        if (mc.SqlType.CanBeColumn && mc.Arguments[0].SqlType.CanBeColumn)
                        {
                            expression = this.sql.ConvertTo(mc.ClrType, mc.Arguments[0]);
                        }
                    }
                    else if (((mc.Method.Name == "Compare") && (mc.Arguments.Count == 2)) && (mc.Method.ReturnType == typeof(int)))
                    {
                        expression = this.CreateComparison(mc.Arguments[0], mc.Arguments[1], mc.SourceExpression);
                    }
                    else if (declaringType == typeof(Math))
                    {
                        expression = this.TranslateMathMethod(mc);
                    }
                    else if (declaringType == typeof(string))
                    {
                        expression = this.TranslateStringStaticMethod(mc);
                    }
                    else if (declaringType == typeof(Convert))
                    {
                        expression = this.TranslateConvertStaticMethod(mc);
                    }
                    else if (declaringType == typeof(SqlMethods))
                    {
                        expression = this.sql.TranslateSqlMethodsMethod(mc); //this.TranslateSqlMethodsMethod(mc);
                    }
                    else if (declaringType == typeof(decimal))
                    {
                        expression = this.TranslateDecimalMethod(mc);
                    }
                    else
                    {
                        if (PostBindDotNetConverter.IsVbConversionMethod(mc))
                        {
                            return this.TranslateVbConversionMethod(mc);
                        }
                        if (PostBindDotNetConverter.IsVbCompareString(mc))
                        {
                            return this.TranslateVbCompareString(mc);
                        }
                        if (PostBindDotNetConverter.IsVbLike(mc))
                        {
                            return this.TranslateVbLikeString(mc);
                        }
                    }
                    if (expression != null)
                    {
                        return expression;
                    }
                }
                else
                {
                    if ((mc.Method.Name == "Equals") && (mc.Arguments.Count == 1))
                    {
                        return this.sql.Binary(SqlNodeType.EQ, mc.Object, mc.Arguments[0]);
                    }
                    if (((mc.Method.Name == "GetValueOrDefault") && mc.Method.DeclaringType.IsGenericType) && (mc.Method.DeclaringType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        return this.TranslateGetValueOrDefaultMethod(mc);
                    }
                    if ((mc.Method.Name == "ToString") && (mc.Arguments.Count == 0))
                    {
                        SqlExpression expr = mc.Object;
                        if (expr.SqlType.IsRuntimeOnlyType)
                        {
                            throw Error.ToStringOnlySupportedForPrimitiveTypes();
                        }
                        return this.sql.ConvertTo(typeof(string), expr);
                    }
                    if (declaringType == typeof(string))
                    {
                        return this.TranslateStringMethod(mc);
                    }
                    if (declaringType == typeof(TimeSpan))
                    {
                        expression = this.TranslateTimeSpanInstanceMethod(mc);
                    }
                    else if (declaringType == typeof(DateTime))
                    {
                        expression = this.TranslateDateTimeInstanceMethod(mc);
                    }
                    if (expression != null)
                    {
                        return expression;
                    }
                }

                //My Code
                if (Attribute.IsDefined(mc.Method, typeof(FunctionAttribute)))
                {
                    string methodName;
                    var attFunc = (FunctionAttribute)mc.Method.GetCustomAttributes(typeof(FunctionAttribute), true)
                                                       .SingleOrDefault();
                    if (attFunc != null && !string.IsNullOrEmpty(attFunc.Name))
                        methodName = attFunc.Name;
                    else
                        methodName = mc.Method.Name;
                    return sql.FunctionCall(mc.ClrType, methodName, mc.Arguments, mc.SourceExpression);
                }
                //=======================

                throw GetMethodSupportException(mc);
            }

            internal override SqlExpression VisitNew(SqlNew sox)
            {
                sox = (SqlNew)base.VisitNew(sox);
                if (sox.ClrType == typeof(string))
                {
                    return this.TranslateNewString(sox);
                }
                if (sox.ClrType == typeof(TimeSpan))
                {
                    return this.TranslateNewTimeSpan(sox);
                }
                if (sox.ClrType == typeof(DateTime))
                {
                    return this.TranslateNewDateTime(sox);
                }
                return sox;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                select = this.VisitSelectCore(select);
                select.Selection = this.skipper.VisitExpression(select.Selection);
                return select;
            }

            internal override SqlExpression VisitTypeCase(SqlTypeCase tc)
            {
                tc.Discriminator = base.VisitExpression(tc.Discriminator);
                var matches = new List<SqlExpression>();
                var values = new List<SqlExpression>();
                bool flag = true;
                foreach (SqlTypeCaseWhen when in tc.Whens)
                {
                    SqlExpression item = this.VisitExpression(when.Match);
                    SqlExpression expression2 = this.VisitExpression(when.TypeBinding);
                    flag = flag && (expression2 is SqlNew);
                    matches.Add(item);
                    values.Add(expression2);
                }
                if (!flag)
                {
                    return this.sql.Case(tc.ClrType, tc.Discriminator, matches, values, tc.SourceExpression);
                }
                int num = 0;
                int count = tc.Whens.Count;
                while (num < count)
                {
                    SqlTypeCaseWhen when2 = tc.Whens[num];
                    when2.Match = matches[num];
                    when2.TypeBinding = (SqlNew)values[num];
                    num++;
                }
                return tc;
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                if (uo.NodeType == SqlNodeType.Convert)
                {
                    Type clrType = uo.ClrType;
                    SqlExpression operand = uo.Operand;
                    if ((clrType == typeof(char)) || (operand.ClrType == typeof(char)))
                    {
                        operand = this.VisitExpression(uo.Operand);
                        uo.Operand = operand;
                        return this.sql.ConvertTo(clrType, uo.SqlType, operand);
                    }
                }
                return base.VisitUnaryOperator(uo);
            }
        }
    }

    internal enum MethodSupport
    {
        None,
        MethodGroup,
        Method
    }

    class SqlPostBindDotNetConverter : PostBindDotNetConverter
    {
        private static PostBindDotNetConverter instance;



        //internal static PostBindDotNetConverter Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //            instance = new SqlPostBindDotNetConverter();
        //        return instance;
        //    }
        //}

        public SqlPostBindDotNetConverter(SqlFactory sqlFactory)
            : base(sqlFactory)
        {
        }

        internal override SqlVisitor CreateVisitor(SqlFactory factory, SqlProvider sqlProvider)
        {
            return new MyVisitor(factory, sqlProvider);
        }

        class MyVisitor : PostBindDotNetConverter.Visitor
        {
            public MyVisitor(SqlFactory sql, SqlProvider sqlProvider)
                : base(sql, sqlProvider)
            {
            }

            //protected override SqlExpression TranslateRoundMethod(SqlMethodCall mc, Expression sourceExpression)
            //{
            //    if (mc.Arguments.Count == 1)
            //    {
            //        var expr = mc.Arguments[0];
            //        Type clrType = expr.ClrType;

            //        mc.Arguments.Insert(0, sql.ValueFromObject(0, sourceExpression));
            //        return sql.FunctionCall(clrType, "round", new[] { expr, sql.ValueFromObject(0, sourceExpression) }, sourceExpression);
            //    }
            //    return base.TranslateRoundMethod(mc, sourceExpression);
            //}
        }
    }
}

