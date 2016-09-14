using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ALinq.Mapping;
using ALinq.SqlClient;
using System.Linq.Expressions;

namespace ALinq.EffiProz
{
    class EfzSqlFactory : SqlFactory
    {
        internal EfzSqlFactory(ITypeSystemProvider typeProvider, MetaModel model)
            : base(typeProvider, model)
        {
        }

        internal override SqlExpression TranslateConvertStaticMethod(SqlMethodCall mc)
        {
            return base.TranslateConvertStaticMethod(mc);
        }

        internal override SqlExpression DateTime_Date(SqlMember m, SqlExpression expr)
        {
            //var expression = DATEPART("MILLISECOND", expr);
            var expression4 = DATEPART("SECOND", expr);
            var expression5 = DATEPART("MINUTE", expr);
            var expression6 = DATEPART("HOUR", expr);
            var expression7 = expr;
            //expression7 = DATEADD("MILLISECOND", Unary(SqlNodeType.Negate, expression), expression7);
            expression7 = DATEADD("SECOND", Unary(SqlNodeType.Negate, expression4), expression7);
            expression7 = DATEADD("MINUTE", Unary(SqlNodeType.Negate, expression5), expression7);
            return DATEADD("HOUR", Unary(SqlNodeType.Negate, expression6), expression7);
        }

        internal override SqlExpression DATEPART(string partName, SqlExpression expr)
        {
            SqlFunctionCall result;
            var sqlType = TypeProvider.From(typeof(string));
            var clrType = typeof(int);
            switch (partName.ToUpper())
            {
                case "YEAR":
                    result = FunctionCall(typeof(int), sqlType, "Year",
                                          new[] { expr }, expr.SourceExpression);
                    break;
                case "MONTH":
                    result = new SqlFunctionCall(clrType, sqlType, "Month",
                                                 new[] { expr }, expr.SourceExpression);
                    break;
                case "DAY":
                    result = FunctionCall(typeof(int), sqlType, "Day",
                                          new[] { expr }, expr.SourceExpression);
                    break;
                case "HOUR":
                    result = FunctionCall(typeof(int), sqlType, "Hour",
                                          new[] { expr }, expr.SourceExpression);
                    break;
                case "MINUTE":
                    result = FunctionCall(typeof(int), sqlType, "Minute",
                                          new[] { expr }, expr.SourceExpression);
                    break;
                case "SECOND":
                    result = FunctionCall(typeof(int), sqlType, "Second",
                                          new[] { expr }, expr.SourceExpression);
                    break;
                default:
                    return base.DATEPART(partName, expr);
            }
            return result;
        }

        internal override SqlExpression String_Length(SqlExpression expr)
        {
            return FunctionCall(typeof(int), "CHAR_LENGTH", new[] { expr }, expr.SourceExpression);
        }

        internal override SqlExpression String_Substring(SqlMethodCall mc)
        {
            SqlExpression[] args;

            if (mc.Arguments.Count != 1)
            {
                if (mc.Arguments.Count == 2)
                {
                    args = new[] { mc.Object, Add(mc.Arguments[0], 1), mc.Arguments[1] };
                    return FunctionCall(typeof(string), "SUBSTRING", args, mc.SourceExpression);
                }
                throw SqlClient.Error.MethodHasNoSupportConversionToSql(mc.Method);//GetMethodSupportException(mc);
            }
            args = new[] { mc.Object, Add(mc.Arguments[0], 1) };
            //args = new[] { mc.Object, mc.Arguments[0] };
            return FunctionCall(typeof(string), "SUBSTRING", args, mc.SourceExpression);
        }

        internal override SqlExpression String_IndexOf(SqlMethodCall mc, System.Linq.Expressions.Expression sourceExpression)
        {
            if (mc.Arguments.Count == 1)
            {
                if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
                {
                    throw Error.ArgumentNull("value");
                }
                var when = new SqlWhen(Binary(SqlNodeType.EQ, String_Length(mc.Arguments[0]), ValueFromObject(0, sourceExpression)), ValueFromObject(0, sourceExpression));
                SqlExpression expression9 = Subtract(FunctionCall(typeof(int), "POSITION", new[] { mc.Arguments[0], mc.Object }, sourceExpression), 1);
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
                SqlExpression expression11 = Subtract(FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { mc.Arguments[0], mc.Object, Add(mc.Arguments[1], 1) }, sourceExpression), 1);
                return SearchedCase(new SqlWhen[] { when2 }, expression11, sourceExpression);
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
            SqlExpression left = Binary(SqlNodeType.EQ, CLRLENGTH(mc.Arguments[0]), ValueFromObject(0, sourceExpression));
            SqlWhen when3 = new SqlWhen(AndAccumulate(left, Binary(SqlNodeType.LE, Add(mc.Arguments[1], 1), CLRLENGTH(mc.Object))), mc.Arguments[1]);
            SqlExpression expression13 = FunctionCall(typeof(string), "SUBSTRING", new SqlExpression[] { mc.Object, ValueFromObject(1, false, sourceExpression), Add(new SqlExpression[] { mc.Arguments[1], mc.Arguments[2] }) }, sourceExpression);
            SqlExpression @else = Subtract(FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { mc.Arguments[0], expression13, Add(mc.Arguments[1], 1) }, sourceExpression), 1);
            return SearchedCase(new SqlWhen[] { when3 }, @else, sourceExpression);
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
