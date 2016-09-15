using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ALinq.Dynamic
{
    #region ExpressionVisitor
    internal class ExpressionVisitor
    {
        // Methods

        public virtual Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return exp;
            }
            switch (exp.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return VisitBinary((BinaryExpression)exp);

                case ExpressionType.ArrayLength:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.UnaryPlus:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary((UnaryExpression)exp);

                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)exp);

                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression)exp);

                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)exp);

                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression)exp);

                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)exp);

                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression)exp);

                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)exp);

                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)exp);

                case ExpressionType.New:
                    return VisitNew((NewExpression)exp);

                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)exp);

                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)exp);

                case ExpressionType.TypeIs:
                    return VisitTypeIs((TypeBinaryExpression)exp);
            }
            throw new Exception(exp.NodeType.ToString());//Error.UnhandledExpressionType(exp.NodeType);
        }

        protected virtual Expression VisitBinary(BinaryExpression b)
        {
            Expression left = Visit(b.Left);
            Expression right = Visit(b.Right);
            Expression expression3 = Visit(b.Conversion);
            if (((left == b.Left) && (right == b.Right)) && (expression3 == b.Conversion))
            {
                return b;
            }
            if ((b.NodeType == ExpressionType.Coalesce) && (b.Conversion != null))
            {
                return Expression.Coalesce(left, right, expression3 as LambdaExpression);
            }
            return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
        }

        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)binding);

                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)binding);

                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)binding);
            }
            throw new Exception(binding.BindingType.ToString());//Error.UnhandledBindingType(binding.BindingType);
        }

        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            int num = 0;
            int count = original.Count;
            while (num < count)
            {
                MemberBinding item = VisitBinding(original[num]);
                if (list != null)
                {
                    list.Add(item);
                }
                else if (item != original[num])
                {
                    list = new List<MemberBinding>(count);
                    for (int i = 0; i < num; i++)
                    {
                        list.Add(original[i]);
                    }
                    list.Add(item);
                }
                num++;
            }
            if (list != null)
            {
                return list;
            }
            return original;
        }

        protected virtual Expression VisitConditional(ConditionalExpression c)
        {
            Expression test = Visit(c.Test);
            Expression ifTrue = Visit(c.IfTrue);
            Expression ifFalse = Visit(c.IfFalse);
            if (((test == c.Test) && (ifTrue == c.IfTrue)) && (ifFalse == c.IfFalse))
            {
                return c;
            }
            return Expression.Condition(test, ifTrue, ifFalse);
        }

        protected virtual Expression VisitConstant(ConstantExpression c)
        {
            return c;
        }

        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            ReadOnlyCollection<Expression> arguments = VisitExpressionList(initializer.Arguments);
            if (arguments != initializer.Arguments)
            {
                return Expression.ElementInit(initializer.AddMethod, arguments);
            }
            return initializer;
        }

        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            int num = 0;
            int count = original.Count;
            while (num < count)
            {
                ElementInit item = VisitElementInitializer(original[num]);
                if (list != null)
                {
                    list.Add(item);
                }
                else if (item != original[num])
                {
                    list = new List<ElementInit>(count);
                    for (int i = 0; i < num; i++)
                    {
                        list.Add(original[i]);
                    }
                    list.Add(item);
                }
                num++;
            }
            if (list != null)
            {
                return list;
            }
            return original;
        }

        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression> sequence = null;
            int num = 0;
            int count = original.Count;
            while (num < count)
            {
                Expression item = Visit(original[num]);
                if (sequence != null)
                {
                    sequence.Add(item);
                }
                else if (item != original[num])
                {
                    sequence = new List<Expression>(count);
                    for (int i = 0; i < num; i++)
                    {
                        sequence.Add(original[i]);
                    }
                    sequence.Add(item);
                }
                num++;
            }
            if (sequence != null)
            {
                return new ReadOnlyCollection<Expression>(sequence);//sequence.ToReadOnlyCollection<Expression>();
            }
            return original;
        }

        protected virtual Expression VisitInvocation(InvocationExpression iv)
        {
            IEnumerable<Expression> arguments = VisitExpressionList(iv.Arguments);
            Expression expression = Visit(iv.Expression);
            if ((arguments == iv.Arguments) && (expression == iv.Expression))
            {
                return iv;
            }
            return Expression.Invoke(expression, arguments);
        }

        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            Expression body = Visit(lambda.Body);
            if (body != lambda.Body)
            {
                return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }
            return lambda;
        }

        protected virtual Expression VisitListInit(ListInitExpression init)
        {
            NewExpression newExpression = (NewExpression)VisitNew(init.NewExpression);
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(init.Initializers);
            if ((newExpression == init.NewExpression) && (initializers == init.Initializers))
            {
                return init;
            }
            return Expression.ListInit(newExpression, initializers);
        }

        protected virtual Expression VisitMemberAccess(MemberExpression m)
        {
            Expression expression = Visit(m.Expression);
            if (expression != m.Expression)
            {
                return Expression.MakeMemberAccess(expression, m.Member);
            }
            return m;
        }

        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Expression expression = Visit(assignment.Expression);
            if (expression != assignment.Expression)
            {
                return Expression.Bind(assignment.Member, expression);
            }
            return assignment;
        }

        protected virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression newExpression = (NewExpression)VisitNew(init.NewExpression);
            IEnumerable<MemberBinding> bindings = VisitBindingList(init.Bindings);
            if ((newExpression == init.NewExpression) && (bindings == init.Bindings))
            {
                return init;
            }
            return Expression.MemberInit(newExpression, bindings);
        }

        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(binding.Initializers);
            if (initializers != binding.Initializers)
            {
                return Expression.ListBind(binding.Member, initializers);
            }
            return binding;
        }

        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            IEnumerable<MemberBinding> bindings = VisitBindingList(binding.Bindings);
            if (bindings != binding.Bindings)
            {
                return Expression.MemberBind(binding.Member, bindings);
            }
            return binding;
        }

        protected virtual Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression instance = Visit(m.Object);
            IEnumerable<Expression> arguments = VisitExpressionList(m.Arguments);
            if ((instance == m.Object) && (arguments == m.Arguments))
            {
                return m;
            }
            return Expression.Call(instance, m.Method, arguments);
        }

        protected virtual Expression VisitNew(NewExpression nex)
        {
            var arguments = VisitExpressionList(nex.Arguments);
            if (arguments == nex.Arguments)
            {
                return nex;
            }

            var args = arguments.ToArray();
            if (nex.Members != null)
            {
                Debug.Assert(arguments.Count == nex.Members.Count);
                var members = nex.Members.ToArray();
                //for (var i = 0; i < members.Length; i++)
                //{
                //    var memberType = TypeUtility.GetMemberReturnType(members[i]);
                    //if (memberType != args[i].Type)
                    //{
                    //    args[i] = Expression.Convert(args[i], memberType);
                    //}
                //}
                return Expression.New(nex.Constructor, args, members);
            }
            return Expression.New(nex.Constructor, arguments);
        }

        protected virtual Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<Expression> initializers = VisitExpressionList(na.Expressions);
            if (initializers == na.Expressions)
            {
                return na;
            }
            if (na.NodeType == ExpressionType.NewArrayInit)
            {
                return Expression.NewArrayInit(na.Type.GetElementType(), initializers);
            }
            return Expression.NewArrayBounds(na.Type.GetElementType(), initializers);
        }

        protected virtual Expression VisitParameter(ParameterExpression p)
        {
            return p;
        }

        protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            Expression expression = Visit(b.Expression);
            if (expression != b.Expression)
            {
                return Expression.TypeIs(expression, b.TypeOperand);
            }
            return b;
        }

        protected virtual Expression VisitUnary(UnaryExpression u)
        {
            Expression operand = Visit(u.Operand);
            if (operand != u.Operand)
            {
                return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
            }
            return u;
        }
    }
    #endregion
}
