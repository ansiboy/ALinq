using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using ALinq.SqlClient;

namespace ALinq.Firebird
{
    class FbPostBindDotNetConverter : PostBindDotNetConverter
    {
        public FbPostBindDotNetConverter(SqlFactory sqlFactory)
            : base(sqlFactory)
        {
        }

        internal override MethodSupport GetMethodSupport(SqlMethodCall mc)
        {
            if (mc.Object != null && mc.Object.ClrType == typeof(string))
                switch (mc.Method.Name)
                {
                    //case "Trim":
                    //case "TrimEnd":
                    case "IndexOf":
                        return MethodSupport.None;
                }
            return base.GetMethodSupport(mc);
        }

        protected override MethodSupport GetStringMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.Name == "TrimEnd")
            {
                return MethodSupport.Method;
            }
            return base.GetStringMethodSupport(mc);
        }

        internal override SqlVisitor CreateVisitor(SqlFactory factory, SqlProvider providerMode)
        {
            return new MyVisitor(factory, providerMode);
        }

        private class MyVisitor : Visitor
        {
            private readonly ITypeSystemProvider typeProvider;

            internal MyVisitor(SqlFactory sql, SqlProvider providerMode)
                : base(sql, providerMode)
            {
                typeProvider = sql.TypeProvider;
            }

            internal override SqlExpression TranslateDateTimeBinary(SqlBinary bo)
            {
                bool asNullable = TypeSystem.IsNullableType(bo.ClrType);
                var nonNullableType = TypeSystem.GetNonNullableType(bo.Right.ClrType);
                var nodeType = bo.NodeType;
                if (nodeType != SqlNodeType.Add)
                {
                    if (nodeType != SqlNodeType.Sub)
                    {
                        return bo;
                    }
                    if (nonNullableType == typeof(DateTime))
                    {
                        var clrType = bo.ClrType;
                        var left = bo.Left;
                        var right = bo.Right;
                        var expression3 = new SqlVariable(typeof(void), null, "DAY", bo.SourceExpression);
                        var expression4 = new SqlVariable(typeof(void), null, "MILLISECOND", bo.SourceExpression);
                        var expr = sql.FunctionCall(typeof(int), "DATEDIFF", new[] { expression3, right, left }, bo.SourceExpression);
                        var expression6 = sql.FunctionCall(typeof(DateTime), "DATEADD", new[] { expression3, expr, right }, bo.SourceExpression);
                        var expression7 = sql.FunctionCall(typeof(int), "DATEDIFF", new[] { expression4, expression6, left }, bo.SourceExpression);
                        var expression8 = sql.Multiply(sql.Add(new[] { sql.Multiply(sql.ConvertToBigint(expr), 0x5265c00L), expression7 }), 0x2710L);
                        return sql.ConvertTo(clrType, expression8);
                    }
                    if (nonNullableType != typeof(TimeSpan))
                    {
                        return bo;
                    }
                    return CreateDateTimeFromDateAndTicks(bo.Left, sql.Unary(SqlNodeType.Negate, bo.Right, bo.SourceExpression), bo.SourceExpression, asNullable);
                }
                if (nonNullableType == typeof(TimeSpan))
                {
                    return CreateDateTimeFromDateAndTicks(bo.Left, bo.Right, bo.SourceExpression, asNullable);
                }
                return bo;
            }

            protected override SqlExpression TranslateAtan2Method(SqlMethodCall mc, Expression sourceExpression)
            {
                return this.CreateFunctionCallStatic2(typeof(double), "ATAN2", mc.Arguments, sourceExpression);
            }

            protected override SqlExpression TranslateLogMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                Debug.Assert(mc.Method.Name == "Log");
                if (mc.Arguments.Count == 1)
                {
                    var value = sql.ValueFromObject(Math.E, sourceExpression);
                    mc.Arguments.Insert(0, value);
                }
                return CreateFunctionCallStatic2(typeof(double), "LOG", mc.Arguments, sourceExpression);
            }

            //protected override SqlExpression TranslateLog10Method(SqlMethodCall mc, Expression sourceExpression)
            //{
            //    Debug.Assert(mc.Arguments.Count == 1);
            //    var value = sql.ValueFromObject(10, sourceExpression);
            //    mc.Arguments.Insert(0, value);
            //    return CreateFunctionCallStatic2(typeof(double), "LOG", mc.Arguments, sourceExpression);
            //}

            //protected override SqlExpression TranslateStringSubstringMethod(SqlMethodCall mc, Expression sourceExpression)
            //{

            //}
        }
    }
}
