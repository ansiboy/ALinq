using System;
using System.Diagnostics;
using System.Linq.Expressions;
using ALinq.SqlClient;

namespace ALinq.Oracle
{
    class OraclePostBindDotNetConverter : PostBindDotNetConverter
    {
        private static OraclePostBindDotNetConverter instance;

        public OraclePostBindDotNetConverter(SqlFactory sqlFactory)
            : base(sqlFactory)
        {
        }

        internal override SqlVisitor CreateVisitor(SqlFactory factory, SqlProvider providerMode)
        {
            return new MyVisitor(factory, providerMode);
        }

        private class MyVisitor : Visitor
        {
            private readonly ITypeSystemProvider typeProvider;

            public MyVisitor(SqlFactory sql, SqlProvider providerMode)
                : base(sql, providerMode)
            {
                typeProvider = sql.TypeProvider;
            }

            protected override SqlExpression TranslateAtan2Method(SqlMethodCall mc, System.Linq.Expressions.Expression sourceExpression)
            {
                return CreateFunctionCallStatic2(typeof(double), "ATAN2", mc.Arguments, sourceExpression);
            }

            protected override SqlExpression TranslateCeilingMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                Debug.Assert(mc.Method.Name == "Ceiling");
                return this.CreateFunctionCallStatic1(mc.Arguments[0].ClrType, "CEIL", mc.Arguments, sourceExpression);
            }

            protected override SqlExpression TranslateLogMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                Debug.Assert(mc.Method.Name == "Log");
                if (mc.Arguments.Count == 1)
                {
                    //mc.Arguments.Add(sql.Value(typeof(int),))
                    var value = sql.ValueFromObject(Math.E, sourceExpression);
                    mc.Arguments.Insert(0, value);
                }
                return CreateFunctionCallStatic2(typeof(double), "LOG", mc.Arguments, sourceExpression);
            }


            protected override SqlExpression TranslateStringInsertMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                var clrType = mc.Method.ReturnType;
                var startIndex = (int)((SqlValue)mc.Arguments[0]).Value;
                var insertString = (string)((SqlValue)mc.Arguments[1]).Value;

                Debug.Assert(clrType == typeof(string));
                Debug.Assert(startIndex >= 0);

                var arg1 = VisitExpression(mc.Object);
                var arg2 = sql.ValueFromObject(1, sourceExpression);//VisitExpression(Expression.Constant(1));
                var arg3 = sql.ValueFromObject(startIndex, sourceExpression);//VisitExpression(Expression.Constant(startIndex));
                var left = new SqlFunctionCall(clrType, typeProvider.From(clrType), "SUBSTR",
                                               new[] { arg1, arg2, arg3 }, sourceExpression);
                //return left;
                var result = new SqlBinary(SqlNodeType.Add, typeof(string), typeProvider.From(typeof(string)),
                                           left, sql.ValueFromObject(insertString, sourceExpression));//VisitExpression(Expression.Constant(insertString)));

                var len = new SqlFunctionCall(typeof(int), typeProvider.From(typeof(int)), "LENGTH",
                                              new[] { VisitExpression(mc.Object) }, sourceExpression);
                var binary = new SqlBinary(SqlNodeType.Sub, typeof(int), typeProvider.From(typeof(int)), len, arg2);
                arg2 = sql.ValueFromObject(startIndex + 1, sourceExpression);//VisitExpression(Expression.Constant(startIndex + 1));
                arg3 = len;//binary;
                var right = new SqlFunctionCall(typeof(string), typeProvider.From(typeof(string)), "SUBSTR",
                                                new[] { arg1, arg2, arg3 }, sourceExpression);
                result = new SqlBinary(SqlNodeType.Add, typeof(string), typeProvider.From(typeof(string)), result, right);
                return result;
            }
        }
    }
}
