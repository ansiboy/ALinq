using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ALinq.Dynamic.Parsers;
#if L2S
using System.Data.Linq;
#endif

namespace ALinq.Dynamic
{
    static class ExpressionUtility
    {
        public static Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right)
        {
            return Expression.Call(null, GetStaticMethod(methodName, left, right), new Expression[]
			{
				left, 
				right
			});
        }

        public static Expression PromoteExpression(Expression expr, Type type, bool exact)
        {
            Expression result;
            if (expr.Type == type)
            {
                result = expr;
            }
            else
            {
                if (expr is ConstantExpression)
                {
                    ConstantExpression ce = (ConstantExpression)expr;
                    if (ce == Literals.Null)
                    {
                        if (!type.IsValueType || TypeUtility.IsNullableType(type))
                        {
                            result = Expression.Constant(null, type);
                            return result;
                        }
                    }
                }
                if (TypeUtility.IsCompatibleWith(expr.Type, type))
                {
                    if (type.IsValueType || exact)
                    {
                        result = Expression.Convert(expr, type);
                    }
                    else
                    {
                        result = expr;
                    }
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }

        public static void CheckAndPromoteOperand(Type signatures, string opName, ref Expression expr, int errorPos)
        {
            Expression[] args = new Expression[]
            {
                expr
            };


            MethodBase method;
            if (FindMethod(signatures, "F", false, args, out method) != 1)
            {
                throw Error.ParseError(errorPos, "Operator '{0}' incompatible with operand type '{1}'", new object[]
                {
                    opName, 
                    TypeUtility.GetTypeName(args[0].Type)
                });
            }
            expr = args[0];
        }

        public static void CheckAndPromoteArgument(Type signatures, Function function, ref Expression arg, Token errorToken)
        {
            var args = new[] { arg };

            var methodName = function.ToString();
            BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;
            int result;
            MethodBase method = null;

            foreach (Type t in TypeUtility.SelfAndBaseTypes(signatures))
            {
                MemberInfo[] members = t.FindMembers(MemberTypes.Method, flags, Type.FilterNameIgnoreCase, methodName);
                Debug.Assert(members.Length > 0);

                int count = FindBestMethod(members.Cast<MethodBase>(), args, out method);
                if (count == 0)
                {
                    var exc = IsAggregateFunction(function) ?
                                    Error.NoCanonicalAggrFunctionOverloadMatch(errorToken, methodName, args.Select(o => o.Type)) :
                                    Error.NoCanonicalFunctionOverloadMatch(errorToken, methodName, args.Select(o => o.Type));

                    throw exc;
                }

            }

            arg = args[0];

        }

        public static void CheckAndPromoteOperands(Type signatures, string opName, ref Expression left, ref Expression right, int errorPos)
        {
            Debug.Assert(left != null);
            Debug.Assert(right != null);

            if (right.NodeType == ExpressionType.Constant && right.Type != left.Type && ((ConstantExpression)right).Value == null)
                right = Expression.Constant(null, left.Type);

            if (left.NodeType == ExpressionType.Constant && left.Type != right.Type && ((ConstantExpression)left).Value == null)
                left = Expression.Constant(null, right.Type);

            Expression[] args = new Expression[]
			{
				left, 
				right
			};
            MethodBase method;
            if (FindMethod(signatures, "F", false, args, out method) != 1)
            {
                throw Error.IncompatibleOperandsError(opName, left, right, errorPos);
            }
            left = args[0];
            right = args[1];
        }

        public static Expression GenerateEqual(Expression left, Expression right)
        {
            return Expression.Equal(left, right);
        }

        public static Expression GenerateNotEqual(Expression left, Expression right)
        {
            return Expression.NotEqual(left, right);
        }

        public static Expression GenerateGreaterThan(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                return Expression.GreaterThan(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                    );
            }
            return Expression.GreaterThan(left, right);
        }

        public static Expression GenerateGreaterThanEqual(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                return Expression.GreaterThanOrEqual(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                    );
            }
            return Expression.GreaterThanOrEqual(left, right);
        }

        public static Expression GenerateLessThan(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                return Expression.LessThan(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                    );
            }
            return Expression.LessThan(left, right);
        }

        public static Expression GenerateLessThanEqual(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                return Expression.LessThanOrEqual(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0)
                    );
            }
            return Expression.LessThanOrEqual(left, right);
        }

        public static Expression GenerateConditional(Expression test, Expression expr1, Expression expr2, int errorPos)
        {
            if (test.Type != typeof(bool))
                throw Error.ParseError(errorPos, Res.FirstExprMustBeBool);
            if (expr1.Type != expr2.Type)
            {
                Expression expr1as2 = expr2 != Literals.Null ? PromoteExpression(expr1, expr2.Type, true) : null;
                Expression expr2as1 = expr1 != Literals.Null ? PromoteExpression(expr2, expr1.Type, true) : null;
                if (expr1as2 != null && expr2as1 == null)
                {
                    expr1 = expr1as2;
                }
                else if (expr2as1 != null && expr1as2 == null)
                {
                    expr2 = expr2as1;
                }
                else
                {
                    string type1 = expr1 != Literals.Null ? expr1.Type.Name : "null";
                    string type2 = expr2 != Literals.Null ? expr2.Type.Name : "null";
                    if (expr1as2 != null && expr2as1 != null)
                        throw Error.ParseError(errorPos, Res.BothTypesConvertToOther, type1, type2);
                    throw Error.ParseError(errorPos, Res.NeitherTypeConvertsToOther, type1, type2);
                }
            }
            return Expression.Condition(test, expr1, expr2);
        }

        private class MethodData
        {
            public MethodBase MethodBase;
            public ParameterInfo[] Parameters;
            public Expression[] Args;
        }


        public static int FindMethod(Type type, string methodName, bool staticAccess, Expression[] args, out MethodBase method)
        {
            BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public | (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            int result;
            foreach (Type t in TypeUtility.SelfAndBaseTypes(type))
            {
                MemberInfo[] members = t.FindMembers(MemberTypes.Method, flags, Type.FilterNameIgnoreCase, methodName);
                int count = FindBestMethod(members.Cast<MethodBase>(), args, out method);
                if (count != 0)
                {
                    result = count;
                    return result;
                }
            }
            method = null;
            result = 0;
            return result;
        }

        public static int FindMethod(Type type, string methodName, bool staticAccess, Func<Expression[]> getArgs, out MethodBase method)
        {
            BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public | (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            int result;
            foreach (Type t in TypeUtility.SelfAndBaseTypes(type))
            {
                MemberInfo[] members = t.FindMembers(MemberTypes.Method, flags, Type.FilterNameIgnoreCase, methodName);
                if (members.Length == 0)
                    continue;

                var args = getArgs();
                int count = FindBestMethod(members.Cast<MethodBase>(), args, out method);
                if (count != 0)
                {
                    result = count;
                    return result;
                }
            }
            method = null;
            result = 0;
            return result;
        }

        public static ConstructorInfo FindConstructor(Type type, Expression[] args)
        {
            var argTypes = args.Select(o => o.Type).ToArray();
            var cons = type.GetConstructor(argTypes);
            return cons;
        }

        public static int FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args, out MethodBase method)
        {
            MethodData[] applicable = (
                from m in methods
                select new MethodData
                {
                    MethodBase = m,
                    Parameters = m.GetParameters()
                } into m
                where IsApplicable(m, args)
                select m).ToArray();

            if (applicable.Length > 1)
            {
                applicable = (
                    from m in applicable
                    where applicable.All(n => m == n || IsBetterThan(args, m, n))
                    select m).ToArray();
            }
            if (applicable.Length == 1)
            {
                MethodData md = applicable[0];
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = md.Args[i];
                }
                method = md.MethodBase;
            }
            else
            {
                method = null;
            }
            return applicable.Length;
        }

        private static bool IsBetterThan(Expression[] args, MethodData m1, MethodData m2)
        {
            bool better = false;
            bool result;
            for (int i = 0; i < args.Length; i++)
            {
                int c = TypeUtility.CompareConversions(args[i].Type, m1.Parameters[i].ParameterType, m2.Parameters[i].ParameterType);
                if (c < 0)
                {
                    result = false;
                    return result;
                }
                if (c > 0)
                {
                    better = true;
                }
            }
            result = better;
            return result;
        }


        private static bool IsApplicable(MethodData method, Expression[] args)
        {
            bool result;
            if (method.Parameters.Length != args.Length)
            {
                result = false;
            }
            else
            {
                Expression[] promotedArgs = new Expression[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    ParameterInfo pi = method.Parameters[i];
                    if (pi.IsOut)
                    {
                        result = false;
                        return result;
                    }
                    Expression promoted = PromoteExpression(args[i], pi.ParameterType, false);
                    if (promoted == null)
                    {
                        result = false;
                        return result;
                    }
                    promotedArgs[i] = promoted;
                }
                method.Args = promotedArgs;
                result = true;
            }
            return result;
        }


        public static MethodInfo GetStaticMethod(string methodName, Expression left, Expression right)
        {
            return left.Type.GetMethod(methodName, new Type[]
			{
				left.Type, 
				right.Type
			});
        }

        public static Type ElementType(Expression expr)
        {
            Type elementType;
            Debug.Assert(typeof(IQueryable).IsAssignableFrom(expr.Type) ||
                         typeof(IEnumerable).IsAssignableFrom(expr.Type));


            if (typeof(Array).IsAssignableFrom(expr.Type))
            {
                elementType = expr.Type.GetElementType();
            }
            else
            {
                Debug.Assert(expr.Type.IsGenericType == true);
                var args = expr.Type.GetGenericArguments();
                if (expr.Type.GetGenericTypeDefinition() == typeof(IGrouping<,>))
                {
                    elementType = args[1];
                }
                else
                {
                    Debug.Assert(args.Length == 1);
                    elementType = args[0];
                }


            }

            return elementType;
        }

        #region Class MemberFinder
        class MemberFinder : ExpressionVisitor
        {
            private MemberInfo member;

            protected override Expression VisitMemberAccess(MemberExpression m)
            {
                this.member = m.Member;
                return m;
            }

            public MemberInfo Member
            {
                get { return this.member; }
            }
        }
        #endregion

        #region Class ParameterFetch
        class ParameterFetch : ExpressionVisitor
        {
            private ParameterExpression parameer;
            private Type elementType;

            private ParameterFetch(Type elementType)
            {
                this.elementType = elementType;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node.Type == elementType || elementType == null)
                    this.parameer = node;

                return node;
            }

            public static ParameterExpression GetParameter(Expression expr, Type elementType)
            {
                var visitor = new ParameterFetch(elementType);
                visitor.Visit(expr);
                return visitor.parameer;
            }
        }

        #endregion

        public static MemberInfo FindMember(Expression expr)
        {
            var visitor = new MemberFinder();
            visitor.Visit(expr);
            return visitor.Member;
        }

        public static ParameterExpression FindParameter(Expression expr)
        {
            return ParameterFetch.GetParameter(expr, null);
        }

        public static ParameterExpression FindParameter(Expression expr, Type parameterType)
        {
            return ParameterFetch.GetParameter(expr, parameterType);
        }

        //說明：關斷一個 select 的查詢是否為分組：
        //例句1：
        //select p.Id from Products as p group by p.CategoryId
        //其中例句1返回 True 。
        public static bool IsGroupQuery(Expression query)
        {
            var result = IsGroupQueryVisitor.IsGroupQuery(query);
            return result;
        }

        #region IsGroupQueryVisitor
        class IsGroupQueryVisitor : ExpressionVisitor
        {
            private bool Result = false;

            protected override Expression VisitMethodCall(MethodCallExpression m)
            {
                if (m.Method.Name == "GroupBy")
                {
                    Result = true;
                    return m;
                }

                return base.VisitMethodCall(m);
            }

            public static bool IsGroupQuery(Expression expr)
            {
                var instance = new IsGroupQueryVisitor();
                instance.Visit(expr);
                return instance.Result;
            }
        }
        #endregion



        public static ParameterExpression CreateParameter(Type type, string name)
        {
            var p = Expression.Parameter(type, name);
            return p;
        }

        public static ParameterExpression CreateParameter(Type type)
        {
            var p = Expression.Parameter(type, "");
            return p;
        }

        public static ParameterExpression CreateParameter(Expression query)
        {
            return CreateParameter(query, string.Empty);
        }

        public static ParameterExpression CreateParameter(Expression query, string name)
        {
            var elementType = ElementType(query);
            var p = Expression.Parameter(elementType, name);
            return p;
        }

        public static MethodCallExpression GenerateAggregateExpression(Function function, ParameterExpression it, Expression[] args, Token errorToken)
        {
            Debug.Assert(IsAggregateFunction(function));
            Debug.Assert(it != null);

            var methodName = function.ToString();

            //NextToken();
            switch (function)
            {
                case Function.Count:
                    {
                        Debug.Assert(it.Type.IsGenericType);
                        Debug.Assert(it.Type.IsGenericType);
                        Debug.Assert(it.Type.GetGenericTypeDefinition() == typeof(IGrouping<,>));

                        var elementType = it.Type.GetGenericArguments()[1];
                        var expr = Expression.Call(typeof(Enumerable), methodName, new[] { elementType }, it);
                        return expr;

                    }
                    break;
                case Function.Max:
                case Function.Min:
                    {
                        Debug.Assert(args.Length > 0);
                        if (args.Length == 1)
                        {
                            ParameterExpression p = FindParameter(args[0]); //ParameterFetch.GetParameter(funcArgs[0]);
                            if (p == null)
                                p = it;

                            var lambda = Expression.Lambda(args[0], p);
                            args = new Expression[] { it, lambda };
                        }
                        else
                        {
                            throw new NotSupportedException(); //TODO:显示具体的异常信息
                        }

                        var elementType = it.Type.GetGenericArguments()[1];
                        var expr = Expression.Call(typeof(Enumerable), methodName, new[] { elementType }, args);

                        return expr;
                    }
                case Function.Sum:
                case Function.Average:
                    {
                        Debug.Assert(args.Length > 0);

                        ParameterExpression p = FindParameter(args[0]);
                        Debug.Assert(p != null);

                        var arg0 = args[0];
                        CheckAndPromoteArgument(typeof(IEnumerableSignatures), function, ref arg0, errorToken);

                        var lambda = Expression.Lambda(arg0, p);
                        var expr = Expression.Call(typeof(System.Linq.Enumerable), methodName, new[] { p.Type }, it, lambda);
                        return expr;

                    }
                default:
                    throw new NotSupportedException(function.ToString());
            }
        }

        static bool IsAggregateFunction(string function)
        {
            var f = (Function)Enum.Parse(typeof(Function), function);
            return IsAggregateFunction(f);
        }

        static bool IsAggregateFunction(Function function)
        {
            return function == Function.BigCount || function == Function.Count || function == Function.Max ||
                   function == Function.Min || function == Function.Sum || function == Function.Average;
        }

        public static Expression GenerateSelect(Expression[] args, Type[] types)
        {
            var source = args[0];
            Type metodType;
            if (typeof(IQueryable).IsAssignableFrom(source.Type))
            {
                metodType = typeof(Queryable);
            }
            else
            {
                Debug.Assert(typeof(IEnumerable).IsAssignableFrom(source.Type));
                metodType = typeof(Enumerable);
            }

            MethodCallExpression q;
            q = Expression.Call(metodType, "Select", types, args);
            return q;
        }

        public static MethodCallExpression GenerateMethodCall(Function function, Expression[] args, Type[] types)
        {
            var methodName = function == Function.BigCount ? "LongCount" : function.ToString();
            return Call(methodName, types, args);
        }

        public static MethodCallExpression Call(string methodName, Type[] types, params Expression[] args)
        {


            var source = args[0];
            Type metodType;
            if (typeof(IQueryable).IsAssignableFrom(source.Type))
            {
                metodType = typeof(Queryable);
            }
            else
            {
                Debug.Assert(typeof(IEnumerable).IsAssignableFrom(source.Type));
                metodType = typeof(Enumerable);
            }

            MethodCallExpression q;
            q = Expression.Call(metodType, methodName, types, args);
            return q;
        }



        public static Expression GenerateQueryMethod(string methodName, Expression[] args)
        {
            return GenerateQueryMethod(methodName, args, null);
        }

        public static Expression GenerateQueryMethod(string methodName, Expression[] args, Type resultType)
        {
            Debug.Assert(args != null && args.Length > 0);

            var source = args[0];
            var parameterType = ElementType(source);

            Type[] types;
            if (resultType != null)
                types = new[] { parameterType, resultType };
            else
                types = new[] { parameterType };

            Type metodType;
            if (typeof(IQueryable).IsAssignableFrom(source.Type))
            {
                metodType = typeof(Queryable);
            }
            else
            {
                Debug.Assert(typeof(IEnumerable).IsAssignableFrom(source.Type));
                metodType = typeof(Enumerable);
            }

            MethodCallExpression q;
            q = Expression.Call(metodType, methodName, types, args);
            return q;
        }

        //public static bool IsMultiValue(Expression expr)
        //{
        //    if (!typeof(IEnumerable).IsAssignableFrom(expr.Type))
        //        return false;

        //    if (expr.Type == typeof(string))
        //        return false;

        //    var elementType = ElementType(expr);
        //    return elementType != typeof(string);
        //}

        //public static bool IsSingleValue(Expression expr)
        //{
        //    return !IsMultiValue(expr);
        //}

        #region QueryProviderFinder
        class QueryProviderFinder : ExpressionVisitor
        {
            private Expression NOOP = Expression.Constant(null);
            public static IQueryProvider FindProvider(Expression expr)
            {
                var instance = new QueryProviderFinder();
                instance.Visit(expr);
                return instance.queryProvider;
            }

            private IQueryProvider queryProvider;

            protected override Expression VisitConstant(ConstantExpression c)
            {
                if (c.Value is IQueryable)
                {
                    this.queryProvider = ((IQueryable)c.Value).Provider;
                }

                if (this.queryProvider != null)
                    return c;

                return base.VisitConstant(c);
            }

            protected override Expression VisitMemberAccess(MemberExpression m)
            {
                if (typeof(IQueryable).IsAssignableFrom(m.Type))
                {
                    Debug.Assert(m.Expression.NodeType == ExpressionType.Constant);
                    this.queryProvider = (IQueryProvider)ExpressionCalculater.Eval(m);

                }

                if (this.queryProvider != null)
                    return m;

                return base.VisitMemberAccess(m);
            }

        }
        #endregion

        public static IQueryProvider FindQueryProvider(Expression expr)
        {
            return QueryProviderFinder.FindProvider(expr);
        }

        #region TableFinder
        class ContextFinder : ExpressionVisitor
        {

            public static DataContext FindContext(Expression expr)
            {
                var instance = new ContextFinder();
                instance.Visit(expr);
                return instance.result;
            }

            private DataContext result;

            protected override Expression VisitConstant(ConstantExpression c)
            {
                if (c.Value is ITable)
                {
                    this.result = ((ITable)c.Value).Context;
                }
                else if (c.Value is DataContext)
                {
                    this.result = (DataContext)c.Value;
                }
                return base.VisitConstant(c);
            }
        }
        #endregion



        public static DataContext FindContext(Expression expr)
        {
            return ContextFinder.FindContext(expr);
        }


    }
}
