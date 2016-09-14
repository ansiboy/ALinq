using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using ALinq.SqlClient;

namespace ALinq.Access
{
    class AccessPostBindDotNetConverter : PostBindDotNetConverter
    {
        public AccessPostBindDotNetConverter(SqlFactory sqlFactory)
            : base(sqlFactory)
        {
        }

        internal override MethodSupport GetMathMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.IsStatic && (mc.Method.DeclaringType == typeof(Math)))
            {
                switch (mc.Method.Name)
                {
                    case "Abs":
                    //case "Acos":
                    //case "Asin":
                    case "Atan":
                    //case "Ceiling":
                    case "Cos":
                    case "Cosh":
                    case "Exp":
                    //case "Floor":
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
                    //case "Pow":
                    //case "Atan2":
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
                        //case "Truncate":
                        if (mc.Arguments.Count != 1)
                        {
                            return MethodSupport.MethodGroup;
                        }
                        return MethodSupport.Method;
                }
            }
            return MethodSupport.None;
        }

        internal override SqlVisitor CreateVisitor(SqlFactory factory, SqlProvider providerMode)
        {
            return new MyVisitor(factory, providerMode);
        }

        protected override MethodSupport GetDecimalMethodSupport(SqlMethodCall mc)
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
                    if (((str2 = mc.Method.Name) != null) && (((str2 == "Negate") /*|| (str2 == "Floor")*/) || (/*(str2 == "Truncate") ||*/ (str2 == "Round"))))
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

        private class MyVisitor : Visitor
        {
            private readonly ITypeSystemProvider typeProvider;

            public MyVisitor(SqlFactory sql, SqlProvider providerMode)
                : base(sql, providerMode)
            {
                typeProvider = sql.TypeProvider;
            }

            protected override SqlExpression TranslateSignMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                return sql.FunctionCall(typeof(int), "SGN", new[] { mc.Arguments[0] }, sourceExpression);
            }

            protected override SqlExpression TranslateSqrtMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                return this.CreateFunctionCallStatic1(typeof(double), "SQR", mc.Arguments, sourceExpression);
            }

            protected override SqlExpression TranslateStringInsertMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                var clrType = mc.Method.ReturnType;
                var startIndex = (int)((SqlValue)mc.Arguments[0]).Value;//(int)((ConstantExpression)mc.Arguments[0]).Value;
                var insertString = (string)((SqlValue)mc.Arguments[1]).Value;//(string)((ConstantExpression)mc.Arguments[1]).Value;

                Debug.Assert(clrType == typeof(string));
                Debug.Assert(startIndex >= 0);

                var arg1 = mc.Object;//VisitExpression(mc.Object);
                var arg2 = sql.ValueFromObject(startIndex, sourceExpression);//VisitExpression(Expression.Constant(startIndex));
                var left = new SqlFunctionCall(clrType, typeProvider.From(clrType), "Left",
                                               new[] { arg1, arg2 }, sourceExpression);
                //return left;
                var result = new SqlBinary(SqlNodeType.Add, typeof(string), typeProvider.From(typeof(string)),
                                           left, sql.ValueFromObject(insertString, sourceExpression));

                var len = new SqlFunctionCall(typeof(int), typeProvider.From(typeof(int)), "Len",
                                              new[] { VisitExpression(mc.Object) }, sourceExpression);
                var binary = new SqlBinary(SqlNodeType.Sub, typeof(int), typeProvider.From(typeof(int)), len, arg2);
                var right = new SqlFunctionCall(typeof(string), typeProvider.From(typeof(string)), "Right",
                                                new[] { arg1, binary }, sourceExpression);
                result = new SqlBinary(SqlNodeType.Add, typeof(string), typeProvider.From(typeof(string)), result, right);
                return result;
            }

            //protected override SqlExpression TranslateStringSubstringMethod(SqlMethodCall mc, Expression sourceExpression)
            //{
            //    var clrType = mc.Method.ReturnType;
            //    var expressions = new List<SqlExpression>();
            //    var sqlExpression = mc.Object;
            //    expressions.Add(sqlExpression);

            //    var expression = sql.Binary(SqlNodeType.Add, mc.Arguments[0], sql.ValueFromObject(1, sourceExpression));
            //    expressions.Add(VisitExpression(expression));

            //    if (mc.Arguments.Count > 1)
            //        expressions.Add(mc.Arguments[1]);

            //    var node = new SqlFunctionCall(clrType, typeProvider.From(clrType), "MID", expressions, sourceExpression);
            //    return node;
            //}

            protected override SqlExpression TranslateStringToLowerMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                return sql.FunctionCall(typeof(string), "LCASE", new[] { mc.Object }, sourceExpression);
            }

            protected override SqlExpression TranslateStringToUpperMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                return sql.FunctionCall(typeof(string), "UCASE", new[] { mc.Object }, sourceExpression);
            }

            protected override SqlExpression TranslateStringGetCharsMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                if (mc.Arguments.Count != 1)
                {
                    throw GetMethodSupportException(mc);
                }
                return sql.FunctionCall(typeof(char), "MID", new[] { mc.Object, sql.Add(mc.Arguments[0], 1), 
                                        sql.ValueFromObject(1, false, sourceExpression) }, sourceExpression);
            }
        }
    }
}