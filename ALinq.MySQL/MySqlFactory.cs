using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using ALinq.Mapping;
using ALinq.SqlClient;
using System.Reflection;

namespace ALinq.MySQL
{
    class MySqlFactory : SqlFactory
    {
        internal MySqlFactory(ITypeSystemProvider typeProvider, MetaModel model)
            : base(typeProvider, model)
        {
        }

        internal override SqlExpression DATALENGTH(SqlExpression expr)
        {
            return FunctionCall(typeof(int), "LENGTH", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression String_Length(SqlExpression expr)
        {
            return FunctionCall(typeof(int), "LENGTH", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_Add(SqlMethodCall mc)
        {
            var arg = Binary(SqlNodeType.Div, mc.Arguments[0],
                             ValueFromObject(10000000, mc.SourceExpression));
            //var args = new[] { mc.Object, mc.Arguments[0], arg };
            var args = new[] { mc.Object, arg, ValueFromObject("SECOND", mc.SourceExpression) };
            return FunctionCall(mc.ClrType, "AddSeconds", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_Subtract(SqlMethodCall mc)
        {
            var arg = Binary(SqlNodeType.Div, mc.Arguments[0],
                             ValueFromObject(10000000, mc.SourceExpression));
            //var args = new[] { mc.Object, mc.Arguments[0], arg };
            var fc = FunctionCall(typeof(string), "INTERVAL", new SqlExpression[] { arg, VariableFromName("SECOND", mc.SourceExpression) }, mc.SourceExpression);
            fc.Brackets = false;
            fc.Comma = false;
            var args = new[] { mc.Object, fc };//ValueFromObject("SECOND", mc.SourceExpression)
            return FunctionCall(mc.ClrType, "DATE_SUB", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddDays(SqlMethodCall mc)
        {
            var args = new[] { mc.Object, mc.Arguments[0], ValueFromObject("DAY", mc.SourceExpression) };
            return FunctionCall(mc.ClrType, "AddDays", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddHours(SqlMethodCall mc)
        {
            var args = new[] { mc.Object, mc.Arguments[0], ValueFromObject("HOUR", mc.SourceExpression) };
            return FunctionCall(mc.ClrType, "AddHours", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddMinutes(SqlMethodCall mc)
        {
            var args = new[] { mc.Object, mc.Arguments[0], ValueFromObject("MINUTE", mc.SourceExpression) };
            return FunctionCall(mc.ClrType, "AddMinutes", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddSeconds(SqlMethodCall mc)
        {
            var args = new[] { mc.Object, mc.Arguments[0], ValueFromObject("SECOND", mc.SourceExpression) };
            return FunctionCall(mc.ClrType, "AddSeconds", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddMonths(SqlMethodCall mc)
        {
            var args = new[] { mc.Object, mc.Arguments[0], ValueFromObject("MONTH", mc.SourceExpression) };
            return FunctionCall(mc.ClrType, "AddMonths", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddYears(SqlMethodCall mc)
        {
            var args = new[] { mc.Object, mc.Arguments[0], ValueFromObject("YEAR", mc.SourceExpression) };
            return FunctionCall(mc.ClrType, "AddYears", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_ToString(SqlMethodCall mc)
        {
            var arg0 = mc.Arguments[0];
            if (arg0.NodeType == SqlNodeType.Value && ((SqlValue)arg0).Value is String)
            {
                var regex = new Regex(@"[\w+]+");
                var str = (string)((SqlValue)arg0).Value;
                var s = regex.Replace(str, new MatchEvaluator(delegate(Match match)
                                          {
                                              switch (match.Value)
                                              {
                                                  case "yyyy":
                                                      return "%Y";
                                                  case "yy":
                                                      return "%y";
                                                  case "MM":
                                                      return "%m";
                                                  case "M":
                                                      return "%c";
                                                  case "dd":
                                                      return "%d";
                                                  case "d":
                                                      return "%e";
                                                  case "HH":
                                                      return "%H";
                                                  case "hh":
                                                      return "%h";
                                                  case "h":
                                                      return "%k";
                                                  case "mm":
                                                      return "%i";
                                                  case "m":
                                                      throw new FormatException();
                                                  case "ss":
                                                      return "%S";
                                                  case "s":
                                                      return "%s";
                                                  case "%":
                                                      return "%%";
                                              }
                                              return match.Value;
                                          }));
                mc.Arguments[0] = ValueFromObject(s, true, mc.SourceExpression);
            }
            var args = new[] { mc.Object, mc.Arguments[0] };
            return FunctionCall(mc.ClrType, "DATE_FORMAT", args, mc.SourceExpression);
        }

        internal override SqlExpression Math_Truncate(SqlMethodCall mc)
        {
            mc.Arguments.Add(ValueFromObject(0, mc.SourceExpression));
            return FunctionCall(mc.ClrType, "Truncate", mc.Arguments, mc.SourceExpression);
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

        internal override SqlExpression String_Remove(SqlMethodCall mc)
        {
            var clrType = mc.Method.ReturnType;
            Debug.Assert(clrType == typeof(string));
            var startIndex = (int)((SqlValue)mc.Arguments[0]).Value;
            var sourceObject = (mc.Object);
            //var sourceString = 

            var arg1 = mc.Object;
            var arg2 = ValueFromObject(startIndex, mc.SourceExpression);//Expression.Constant(startIndex));
            var left = new SqlFunctionCall(typeof(string), TypeProvider.From(typeof(string)), "Left",
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
                                                          "Right", new[] { sourceObject, len }, mc.SourceExpression);
                var result = new SqlBinary(SqlNodeType.Add, clrType, TypeProvider.From(clrType), left, right);
                return result;
            }
            Debug.Assert(mc.Arguments.Count == 1);
            return left;
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
            //return Add(new[] {expression12, expression13, expression14});
        }

        internal override SqlExpression DateTime_DayOfWeek(SqlMember m, SqlExpression expr)
        {
            return new SqlBinary(SqlNodeType.Sub, typeof(DayOfWeek), TypeProvider.From(typeof(int)),
                                 FunctionCall(typeof(int), "DayOfWeek", new[] { expr }, expr.SourceExpression),
                                 ValueFromObject(1, expr.SourceExpression));
            //return ConvertTo(typeof(DayOfWeek), Subtract(FunctionCall(typeof(int), "DayOfWeek", new[] { expr }, expr.SourceExpression), 1));
        }

        internal override SqlExpression DATEPART(string partName, SqlExpression expr)
        {
            var args = new[] { expr };
            return new SqlFunctionCall(typeof(int), TypeProvider.From(typeof(int)),
                                       partName, args, expr.SourceExpression);
        }

        internal override SqlExpression DateTime_Date(SqlMember m, SqlExpression expr)
        {
            return FunctionCall(expr.ClrType, "DATE", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression String_IndexOf(SqlMethodCall mc, Expression sourceExpression)
        {
            if (mc.Arguments.Count == 1)
            {
                if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
                {
                    throw Error.ArgumentNull("value");
                }
                //var when = new SqlWhen(Binary(SqlNodeType.EQ, CLRLENGTH(mc.Arguments[0]), ValueFromObject(0, sourceExpression)), this.ValueFromObject(0, sourceExpression));
                var funInStr = Subtract(FunctionCall(typeof(int), "InStr", new[] { mc.Object, mc.Arguments[0] }, sourceExpression), 1);
                //return SearchedCase(new[] { when }, expression9, sourceExpression);
                return funInStr;
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
                #region Delete
                //SqlExpression expression10 = Binary(SqlNodeType.EQ, CLRLENGTH(mc.Arguments[0]), ValueFromObject(0, sourceExpression));
                //var when2 = new SqlWhen(AndAccumulate(expression10, Binary(SqlNodeType.LE, Add(mc.Arguments[1], 1), CLRLENGTH(mc.Object))), mc.Arguments[1]);
                //SqlExpression expression11 = this.Subtract(this.FunctionCall(typeof(int), "LOCATE", new[] { mc.Arguments[0], mc.Object, Add(mc.Arguments[1], 1) }, sourceExpression), 1);
                //return this.SearchedCase(new[] { when2 }, expression11, sourceExpression); 
                #endregion
                return Subtract(FunctionCall(typeof(int), "LOCATE", new[] { mc.Arguments[0], mc.Object, Add(mc.Arguments[1], 1) },
                                    sourceExpression), 1);
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
            var when3 = new SqlWhen(this.AndAccumulate(left, Binary(SqlNodeType.LE, Add(mc.Arguments[1], 1), CLRLENGTH(mc.Object))), mc.Arguments[1]);
            SqlExpression expression13 = this.FunctionCall(typeof(string), "SUBSTRING", new[] { mc.Object, ValueFromObject(1, false, sourceExpression), this.Add(new SqlExpression[] { mc.Arguments[1], mc.Arguments[2] }) }, sourceExpression);
            SqlExpression @else = this.Subtract(this.FunctionCall(typeof(int), "LOCATE", new[] { mc.Arguments[0], expression13, Add(mc.Arguments[1], 1) }, sourceExpression), 1);
            return this.SearchedCase(new SqlWhen[] { when3 }, @else, sourceExpression);
            //var args = new[] { mc.Arguments[0], mc.Object, Add(mc.Arguments[1], 1), mc.Arguments[2] };
            //return Subtract(FunctionCall(typeof(int), "LOCATE", args, sourceExpression), 1);

        }

        internal override SqlExpression Math_Max(SqlMethodCall mc)
        {
            return FunctionCall(mc.ClrType, "GREATEST", mc.Arguments, mc.SourceExpression);
        }

        internal override SqlExpression Math_Min(SqlMethodCall mc)
        {
            return FunctionCall(mc.ClrType, "LEAST", mc.Arguments, mc.SourceExpression);
        }

        internal override SqlExpression String_Trim(SqlMethodCall mc)
        {
            return FunctionCall(typeof(string), "TRIM", new[] { mc.Object }, mc.SourceExpression);
        }

        internal override SqlExpression Concat(SqlExpression[] expressions, Expression sourceExpression)
        {
            SqlExpression right = expressions[expressions.Length - 1];
            Debug.Assert(right.SqlType.IsString || right.SqlType.IsChar);

            for (int i = expressions.Length - 2; i >= 0; i--)
            {
                Debug.Assert(expressions[i].SqlType.IsString || expressions[i].SqlType.IsChar);
                right = FunctionCall(typeof(string), "CONCAT", new[] { expressions[i], right }, sourceExpression); //this.Binary(SqlNodeType.Concat, expressions[i], right);
            }
            return right;
        }

        internal override SqlExpression String_GetChar(SqlMethodCall mc, Expression sourceExpression)
        {
            if (mc.Arguments.Count != 1)
            {
                //throw GetMethodSupportException(mc);
                SqlClient.Error.MethodHasNoSupportConversionToSql(mc);
            }
            var args = new[] { mc.Object, Add(mc.Arguments[0], 1), 
                               ValueFromObject(1, false, sourceExpression) };
            return new SqlFunctionCall(typeof(char), TypeProvider.From(typeof(char)), "SUBSTRING", args, sourceExpression);
        }

        internal override SqlExpression MakeCoalesce(SqlExpression left, SqlExpression right, Type type, Expression sourceExpression)
        {
            return FunctionCall(type, "IFNULL", new[] { left, right }, sourceExpression);
        }

        internal override MethodSupport GetSqlMethodsMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.Name == "DateDiffMillisecond")
                return MethodSupport.None;

            return base.GetSqlMethodsMethodSupport(mc);
        }

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
                        if (str3 == "Millisecond")
                            str3 = "Second";

                        SqlExpression expression5 = this.VariableFromName(str3, sourceExpression); //new SqlValue(typeof(void), null, str3, sourceExpression);
                        SqlExpression result = this.FunctionCall(typeof(int), "TIMESTAMPDIFF", new[] { expression5, expression3, expression4 }, sourceExpression);
                        if (mc.Method.Name == "DateDiffMillisecond")
                            result = this.Binary(SqlNodeType.Mul, result, ValueFromObject(1000, sourceExpression));
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

#if FIX_ZERO_DATETIME 
        //用来修正日期值为 0000:00:00 的情况
        internal override SqlBinary Binary(SqlNodeType nodeType, SqlExpression left, SqlExpression right,
                                           MethodInfo method, Type clrType, IProviderType sqlType)
        {
            var args = new[] { left, right };
            if (left.NodeType != right.NodeType)
            {

                var m = args.Where(o => o.NodeType != SqlNodeType.Value).FirstOrDefault();
                var n = args.OfType<SqlValue>().Select(o => o.Value).FirstOrDefault();
                var type = m.ClrType.IsGenericType ? m.ClrType.GetGenericArguments()[0] : m.ClrType;
                if (type == typeof(DateTime) && n == null)
                {
                    if (nodeType == SqlNodeType.EQ)
                    {
                        var b1 = new SqlBinary(nodeType, clrType, sqlType, left, right, method);
                        var b2 = new SqlBinary(SqlNodeType.LT, clrType, sqlType, m,
                                              ValueFromObject(DateTime.MinValue, m.SourceExpression));
                    
                        return new SqlBinary(SqlNodeType.Or, typeof(bool), TypeProvider.From(typeof(bool)), b1, b2);
                  
                    }
                    if (nodeType == SqlNodeType.NE)
                    {
                        var b1 = new SqlBinary(nodeType, clrType, sqlType, left, right, method);
                        var b2 = new SqlBinary(SqlNodeType.GT, clrType, sqlType, m,
                                               ValueFromObject(DateTime.MinValue, m.SourceExpression));

                        return new SqlBinary(SqlNodeType.And, typeof(bool), TypeProvider.From(typeof(bool)), b1, b2);
                    }
                }
            }
            return base.Binary(nodeType, left, right, method, clrType, sqlType);
        }
#endif

    }
}

