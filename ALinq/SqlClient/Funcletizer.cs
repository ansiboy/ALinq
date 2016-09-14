using System;
using System.Collections.Generic;
using ALinq.Mapping;
using System.Linq.Expressions;
using ALinq.Provider;

namespace ALinq.SqlClient
{
    internal static class Funcletizer
    {
        // Methods
        public static Expression Funcletize(Expression expression)
        {
            var mapLocals = new LocalMapper().MapLocals(expression);
            return new Localizer(mapLocals).Localize(expression);
        }

        // Nested Types
        private class DependenceChecker : ExpressionVisitor
        {
            // Fields
            private readonly HashSet<ParameterExpression> inScope = new HashSet<ParameterExpression>();
            private bool isIndependent = true;

            // Methods
            public static bool IsIndependent(Expression expression)
            {
                var checker = new DependenceChecker();
                checker.Visit(expression);
                return checker.isIndependent;
            }

            public override Expression VisitLambda(LambdaExpression lambda)
            {
                foreach (ParameterExpression expression in lambda.Parameters)
                {
                    inScope.Add(expression);
                }
                return base.VisitLambda(lambda);
            }

            public override Expression VisitParameter(ParameterExpression p)
            {
                isIndependent &= inScope.Contains(p);
                return p;
            }
        }

        private class Localizer : ExpressionVisitor
        {
            // Fields
            private readonly Dictionary<Expression, bool> locals;

            // Methods
            internal Localizer(Dictionary<Expression, bool> locals)
            {
                this.locals = locals;
            }

            internal Expression Localize(Expression expression)
            {
                return Visit(expression);
            }

            private static Expression MakeLocal(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant)
                {
                    return e;
                }
                if ((e.NodeType == ExpressionType.Convert) || (e.NodeType == ExpressionType.ConvertChecked))
                {
                    var expression = (UnaryExpression)e;
                    if (expression.Type == typeof(object))
                    {
                        Expression expression2 = MakeLocal(expression.Operand);
                        if (e.NodeType != ExpressionType.Convert)
                        {
                            return Expression.ConvertChecked(expression2, e.Type);
                        }
                        return Expression.Convert(expression2, e.Type);
                    }
                    if (expression.Operand.NodeType == ExpressionType.Constant)
                    {
                        var operand = (ConstantExpression)expression.Operand;
                        if (operand.Value == null)
                        {
                            return Expression.Constant(null, expression.Type);
                        }
                    }
                }
                return Expression.Invoke(
                    Expression.Constant(Expression.Lambda(e, new ParameterExpression[0]).Compile()), new Expression[0]);
            }

            public override Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return null;
                }
                if (locals.ContainsKey(exp))
                {
                    return MakeLocal(exp);
                }
                if (exp.NodeType == ((ExpressionType)0x7d0))
                {
                    return exp;
                }
                return base.Visit(exp);
            }
        }

        private class LocalMapper : ExpressionVisitor
        {
            // Fields
            private bool isRemote;
            private bool isUpdateInsertMethod = false;
            private Dictionary<Expression, bool> locals;

            // Methods
            internal Dictionary<Expression, bool> MapLocals(Expression expression)
            {
                locals = new Dictionary<Expression, bool>();
                isRemote = false;
                if(expression.NodeType == ExpressionType.Call)
                {
                    var methodName = ((MethodCallExpression) expression).Method.Name;
                    if (methodName == "Insert" || methodName == "Update")
                        isUpdateInsertMethod = true;
                }
                Visit(expression);
                return locals;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression == null)
                {
                    return null;
                }
                bool oldValue = isRemote;
                ExpressionType nodeType = expression.NodeType;

                if (nodeType != ExpressionType.Constant)
                {
                    if (nodeType == ((ExpressionType)0x7d0))
                    {
                        return expression;
                    }
                    isRemote = false;
                    base.Visit(expression);
                    if ((!isRemote && (expression.NodeType != ExpressionType.Lambda)) &&
                        ((expression.NodeType != ExpressionType.Quote) &&
                         IsIndependent(expression)))
                    {
                        locals[expression] = true;
                    }
                }


                if (typeof(ITable).IsAssignableFrom(expression.Type) ||
                    typeof(DataContext).IsAssignableFrom(expression.Type))
                {
                    isRemote = true;
                }

                //isRemote |= oldValue;
                isRemote = isRemote | oldValue;
                return expression;
            }

            private bool IsIndependent(Expression expression)
            {
                if (isUpdateInsertMethod && expression.NodeType == ExpressionType.MemberInit)
                    return false;
                return DependenceChecker.IsIndependent(expression);
            }

            public override Expression VisitMemberAccess(MemberExpression m)
            {
                base.VisitMemberAccess(m);
                isRemote |= (m.Expression != null) && typeof(ITable).IsAssignableFrom(m.Expression.Type);
                return m;
            }

            //private static readonly Type DataManipulationType =
            //    ReflectObject.GetType("ALinq.Provider.DataManipulation");

            public override Expression VisitMethodCall(MethodCallExpression m)
            {
                base.VisitMethodCall(m);
                isRemote |= ((m.Method.DeclaringType == typeof(DataManipulation) ||
                              m.Method.DeclaringType == typeof(DataManipulation)) ||
                             Attribute.IsDefined(m.Method, typeof(FunctionAttribute)));

                return m;
            }
        }
    }
}