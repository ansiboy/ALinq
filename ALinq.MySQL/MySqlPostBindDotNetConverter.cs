using System;
using System.Linq.Expressions;
using ALinq.SqlClient;

namespace ALinq.MySQL
{
    class MySqlPostBindDotNetConverter : PostBindDotNetConverter
    {
        private readonly SqlFactory sql;

        public MySqlPostBindDotNetConverter(SqlFactory sqlFactory)
            : base(sqlFactory)
        {
            this.sql = sqlFactory;
        }

        internal override SqlVisitor CreateVisitor(SqlFactory factory, SqlProvider providerMode)
        {
            return new MyVisitor(factory, providerMode);
        }

        internal override MethodSupport GetMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.ReflectedType == typeof(Convert))
                return sql.GetConvertMethodSupport(mc);
            return base.GetMethodSupport(mc);
        }



        private class MyVisitor : Visitor
        {
            private readonly ITypeSystemProvider typeProvider;

            public MyVisitor(SqlFactory sql, SqlProvider providerMode)
                : base(sql, providerMode)
            {
                typeProvider = sql.TypeProvider;
            }

            //protected override SqlNode VisitMember(SqlMember m)
            //{
            //    var expr = this.VisitExpression(m.Expression);
            //    var member = m.Member;
            //    var sourceExpression = m.SourceExpression;
            //    if ((expr.ClrType == typeof(string)) && (member.Name == "Length"))
            //    {
            //        return this.sql.LEN(expr);
            //    }
            //    if ((expr.ClrType == typeof(ALinq.Binary)) && (member.Name == "Length"))
            //    {
            //        return this.sql.DATALENGTH(expr);
            //    }
            //    if (expr.ClrType == typeof(DateTime))
            //    {
            //        string datePart = PostBindDotNetConverter.GetDatePart(member.Name);
            //        if (datePart != null)
            //        {
            //            //return this.sql.DATEPART(datePart, expr);
            //            var args = new[] { expr };
            //            return new SqlFunctionCall(typeof(int), typeProvider.From(typeof(int)), datePart, args, expr.SourceExpression);
            //        }

            //        if (member.Name == "Date" || member.Name == "Year" || member.Name == "Month" ||
            //            member.Name == "Hour" || member.Name == "Minute" || member.Name == "Second")
            //        {
            //            var args = new[] { expr };
            //            return new SqlFunctionCall(typeof(DateTime), typeProvider.From(typeof(string)),
            //                                       member.Name, args, expr.SourceExpression);
            //        }
            //    }
            //    throw SqlClient.Error.MemberCannotBeTranslated(member.DeclaringType, member.Name);
            //}

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
            {
                if (mc.Method.IsStatic == false)
                {
                    if (mc.Method.Name == "ToString" && mc.Arguments.Count == 1 &&
                        mc.Object.ClrType == typeof(DateTime))
                    {
                        return sql.DateTime_ToString(mc);
                    }
                }
                return base.VisitMethodCall(mc);
            }

            protected override SqlExpression TranslateAtan2Method(SqlMethodCall mc, Expression sourceExpression)
            {
                return CreateFunctionCallStatic2(typeof(double), "ATAN2", mc.Arguments, sourceExpression);
            }

            //protected override SqlExpression TranslateStringIndexOfMethod(SqlMethodCall mc, Expression sourceExpression)
            //{
            //    return sql.String_IndexOf(mc, sourceExpression);
            //}

            protected override SqlExpression TranslateStringInsertMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                if ((mc.Arguments[1] is SqlValue) && (((SqlValue)mc.Arguments[1]).Value == null))
                {
                    throw Error.ArgumentNull("value");
                }
                SqlFunctionCall call = sql.FunctionCall(typeof(string), "INSERT", new[] { mc.Object, sql.Add(mc.Arguments[0], 1), sql.ValueFromObject(0, false, sourceExpression), mc.Arguments[1] }, sourceExpression);
                SqlExpression expression53 = sql.Binary(SqlNodeType.EQ, sql.CLRLENGTH(mc.Object), mc.Arguments[0]);
                SqlExpression expression54 = sql.Concat(new[] { mc.Object, mc.Arguments[1] }, sourceExpression);
                return sql.SearchedCase(new[] { new SqlWhen(expression53, expression54) }, call, sourceExpression);

            }
        }
    }
}
