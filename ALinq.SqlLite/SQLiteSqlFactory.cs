using System;
using System.Diagnostics;
using System.Linq.Expressions;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.SQLite
{
    class SQLiteSqlFactory : SqlFactory
    {
        internal SQLiteSqlFactory(ITypeSystemProvider typeProvider, MetaModel model)
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

        internal override SqlExpression Math_Truncate(SqlMethodCall mc)
        {
            throw SqlClient.Error.MethodHasNoSupportConversionToSql(mc);
        }

        internal override SqlExpression String_Substring(SqlMethodCall mc)
        {
            SqlExpression[] args;

            if (mc.Arguments.Count != 1)
            {
                if (mc.Arguments.Count == 2)
                {
                    args = new[] { mc.Object, Add(mc.Arguments[0], 1), mc.Arguments[1] };
                    return this.FunctionCall(typeof(string), "SUBSTR", args, mc.SourceExpression);
                }
                throw SqlClient.Error.MethodHasNoSupportConversionToSql(mc);//GetMethodSupportException(mc);
            }
            args = new[] { mc.Object, this.Add(mc.Arguments[0], 1), this.CLRLENGTH(mc.Object) };
            return this.FunctionCall(typeof(string), "SUBSTR", args, mc.SourceExpression);
        }

        internal override SqlExpression String_Trim(SqlMethodCall mc)
        {
            return FunctionCall(typeof(string), "TRIM", new[] { mc.Object }, mc.SourceExpression);
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
                SqlExpression expression9 = this.Subtract(this.FunctionCall(typeof(int), "CHARINDEX", new[] { mc.Arguments[0], mc.Object }, sourceExpression), 1);
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
                SqlExpression expression11 = this.Subtract(this.FunctionCall(typeof(int), "CHARINDEX", new[] { mc.Arguments[0], mc.Object, this.Add(mc.Arguments[1], 1) }, sourceExpression), 1);
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
            SqlExpression @else = this.Subtract(this.FunctionCall(typeof(int), "CHARINDEX",
                                               new[] { mc.Arguments[0], expression13, this.Add(mc.Arguments[1], 1) }, sourceExpression), 1);
            return this.SearchedCase(new[] { when3 }, @else, sourceExpression);
        }

        internal override SqlExpression String_Replace(SqlMethodCall mc)
        {
            var clrType = mc.Method.ReturnType;
            var sqlType = TypeProvider.From(clrType);
            Debug.Assert(clrType == typeof(string));
            Debug.Assert(mc.Arguments.Count == 2);

            var sourceObject = mc.Object;
            var oldValue = mc.Arguments[0];
            var newValue = mc.Arguments[1];

            var result = new SqlFunctionCall(clrType, sqlType, "Replace",
                                             new[] { sourceObject, oldValue, newValue }, mc.SourceExpression);
            return result;
        }

        internal override SqlExpression String_Remove(SqlMethodCall mc)
        {
            var clrType = mc.Method.ReturnType;
            Debug.Assert(clrType == typeof(string));
            var startIndex = (int)((SqlValue)mc.Arguments[0]).Value;
            var sourceObject = (mc.Object);
            //var sourceString = 

            var arg1 = (mc.Object);
            var arg2 = (ValueFromObject(startIndex, mc.SourceExpression));
            var left = new SqlFunctionCall(typeof(string), TypeProvider.From(typeof(string)), "LEFTSTR",
                                           new[] { arg1, arg2 }, mc.SourceExpression);

            if (mc.Arguments.Count == 2)
            {
                var count = (int)((SqlValue)mc.Arguments[1]).Value;
                SqlExpression len1 = (ValueFromObject(startIndex + count, mc.SourceExpression));
                SqlExpression len2 = new SqlFunctionCall(typeof(int), TypeProvider.From(typeof(int)), "LENGTH",
                                                         new[] { sourceObject }, mc.SourceExpression);
                SqlExpression len = new SqlBinary(SqlNodeType.Sub, typeof(int), TypeProvider.From(typeof(int)),
                                                  len2, len1);

                SqlExpression right = new SqlFunctionCall(typeof(string), TypeProvider.From(typeof(string)),
                                                          "RIGHTSTR", new[] { sourceObject, len }, mc.SourceExpression);
                var result = new SqlBinary(SqlNodeType.Add, clrType, TypeProvider.From(clrType), left, right);
                return result;
            }
            Debug.Assert(mc.Arguments.Count == 1);
            return left;
        }

        internal override SqlExpression UNICODE(Type clrType, SqlUnary uo)
        {
            return FunctionCall(clrType, TypeProvider.From(typeof(int)), "ASCII", new[] { uo.Operand }, uo.SourceExpression);
        }

        internal override SqlExpression Math_Max(SqlMethodCall mc)
        {
            return FunctionCall(mc.ClrType, "MAX", mc.Arguments, mc.SourceExpression);
            //return base.Math_Max(mc);
        }

        internal override SqlExpression Math_Min(SqlMethodCall mc)
        {
            return FunctionCall(mc.ClrType, "MIN", mc.Arguments, mc.SourceExpression);
        }

        internal override MethodSupport GetConvertMethodSupport(SqlMethodCall mc)
        {
            return MethodSupport.None;
        }

        internal override SqlExpression DATEPART(string partName, SqlExpression expr)
        {
            SqlFunctionCall result;
            SqlExpression format;
            var sqlType = TypeProvider.From(typeof(string));
            var clrType = typeof(int);
            switch (partName)
            {
                case "Year":
                    format = ValueFromObject("%Y", expr.SourceExpression);
                    result = FunctionCall(typeof(int), sqlType, "strftime",
                                          new[] { format, expr }, expr.SourceExpression);
                    break;
                case "Month":
                    format = ValueFromObject("%m", expr.SourceExpression);
                    //result = FunctionCall(typeof(int), sqlType, "strftime",
                    //                      new[] { format, expr }, expr.SourceExpression);
                    result = new SqlFunctionCall(clrType, sqlType, "strftime",
                                                        new[] { format, expr }, expr.SourceExpression);
                    break;
                case "Day":
                    format = ValueFromObject("%d", expr.SourceExpression);
                    result = FunctionCall(typeof(int), sqlType, "strftime",
                                          new[] { format, expr }, expr.SourceExpression);
                    break;
                case "Hour":
                    format = ValueFromObject("%H", expr.SourceExpression);
                    result = FunctionCall(typeof(int), sqlType, "strftime",
                                          new[] { format, expr }, expr.SourceExpression);
                    break;
                case "Minute":
                    format = ValueFromObject("%M", expr.SourceExpression);
                    result = FunctionCall(typeof(int), sqlType, "strftime",
                                          new[] { format, expr }, expr.SourceExpression);
                    break;
                case "Second":
                    format = ValueFromObject("%S", expr.SourceExpression);
                    result = FunctionCall(typeof(int), sqlType, "strftime",
                                          new[] { format, expr }, expr.SourceExpression);
                    break;
                default:
                    return base.DATEPART(partName, expr);
            }
            return result;
            //return base.DATEPART(partName, expr);
        }

        internal override SqlExpression DateTime_Date(SqlMember m, SqlExpression expr)
        {
            var result = FunctionCall(typeof(DateTime), "DATE",
                                      new[] { expr }, expr.SourceExpression);
            return result;
        }

        internal override SqlNode DateTime_TimeOfDay(SqlMember member, SqlExpression expr)
        {
            var expression8 = DATEPART("Hour", expr);
            var expression9 = DATEPART("Minute", expr);
            var expression10 = DATEPART("Second", expr);
            //var expression11 = DATEPART("MILLISECOND", expr);
            var expression12 = Multiply(ConvertToBigint(expression8), 0x861c46800L);
            var expression13 = Multiply(ConvertToBigint(expression9), 0x23c34600L);
            var expression14 = Multiply(ConvertToBigint(expression10), 0x989680L);
            //var expression15 = Multiply(ConvertToBigint(expression11), 0x2710L);
            return ConvertTo(typeof(TimeSpan), Add(new[] { expression12, expression13, expression14 }));
        }

        internal override SqlExpression DateTime_AddHours(SqlMethodCall mc)
        {
            var value = ((SqlValue)mc.Arguments[0]).Value;
            var clrType = typeof(DateTime);
            var sqlType = TypeProvider.From(typeof(string));
            var args = new[] { mc.Object,
                               ValueFromObject(string.Format("+{0} hours",value),mc.SourceExpression)};
            return FunctionCall(clrType, sqlType, "DATE", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddMinutes(SqlMethodCall mc)
        {
            //var value = ((SqlValue)mc.Arguments[0]).Value;
            var clrType = typeof(DateTime);
            var sqlType = TypeProvider.From(typeof(string));
            //var args = new[] { mc.Object,
            //                   ValueFromObject(string.Format("+{0} minutes",value),mc.SourceExpression)};
            //return FunctionCall(clrType, sqlType, "DATE", args, mc.SourceExpression);
            return new SqlBinary(SqlNodeType.Add, clrType, sqlType, mc.Object, mc.Arguments[0]);
        }

        internal override SqlExpression DateTime_AddSeconds(SqlMethodCall mc)
        {
            var value = ((SqlValue)mc.Arguments[0]).Value;
            var clrType = typeof(DateTime);
            var sqlType = TypeProvider.From(typeof(string));
            var args = new[] { mc.Object,
                               ValueFromObject(string.Format("+{0} seconds",value),mc.SourceExpression)};
            return FunctionCall(clrType, sqlType, "DATE", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddDays(SqlMethodCall mc)
        {
            //UNDONE:where mc.Arguments[0] is not sqlvalue, throw exception

            var value = ((SqlValue)mc.Arguments[0]).Value;
            var clrType = typeof(DateTime);
            var sqlType = TypeProvider.From(typeof(string));
            var args = new[] { mc.Object,
                               ValueFromObject(string.Format("+{0} days",value),mc.SourceExpression)};
            return FunctionCall(clrType, sqlType, "DATE", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddMonths(SqlMethodCall mc)
        {
            //UNDONE:where mc.Arguments[0] is not sqlvalue, throw exception

            var value = ((SqlValue)mc.Arguments[0]).Value;
            var clrType = typeof(DateTime);
            var sqlType = TypeProvider.From(typeof(string));
            var args = new[] { mc.Object,
                               ValueFromObject(string.Format("+{0} months",value),mc.SourceExpression)};
            return FunctionCall(clrType, sqlType, "DATE", args, mc.SourceExpression);
        }

        internal override SqlExpression DateTime_AddYears(SqlMethodCall mc)
        {
            //UNDONE:where mc.Arguments[0] is not sqlvalue, throw exception

            var value = ((SqlValue)mc.Arguments[0]).Value;
            var clrType = typeof(DateTime);
            var sqlType = TypeProvider.From(typeof(string));
            var args = new[] { mc.Object, ValueFromObject(string.Format("+{0} years", value), mc.SourceExpression) };
            return FunctionCall(clrType, sqlType, "DATE", args, mc.SourceExpression);
        }

        internal override bool IsSupportedDateTimeMember(SqlMember m)
        {
            Debug.Assert(m != null);
            Debug.Assert(m.Member != null);
            switch (m.Member.Name)
            {
                case "DayOfWeek":
                case "DayOfYear":
                    return false;
            }
            return true;
        }

        internal override MethodSupport GetStringMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.Name == "IndexOf")
                return MethodSupport.None;
            return base.GetStringMethodSupport(mc);
        }

        internal override SqlExpression MakeCoalesce(SqlExpression left, SqlExpression right, Type type, Expression sourceExpression)
        {
            return FunctionCall(type, "IFNULL", new[] { left, right }, sourceExpression);
        }

        internal override MethodSupport GetSqlMethodsMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.Name.StartsWith("DateDiff"))
                return MethodSupport.None;

            return base.GetSqlMethodsMethodSupport(mc);
        }
    }
}
