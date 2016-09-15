using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.Dynamic
{
    static class ExpressionCalculater
    {
        class ExpressionCalculaterVisitor : ExpressionVisitor
        {
            #region Arithmetic（算术运算）
            class Arithmetic
            {
                public static object Add(object valueLeft, object valueRight, Type targetType)
                {
                    var value1 = Convert.ToDecimal(valueLeft);
                    var value2 = Convert.ToDecimal(valueRight);

                    var value = value1 + value2;
                    var result = Convert.ChangeType(value, targetType);
                    return result;
                }

                public static object Subtract(object valueLeft, object valueRight, Type targetType)
                {
                    var value1 = Convert.ToDecimal(valueLeft);
                    var value2 = Convert.ToDecimal(valueRight);

                    var value = value1 - value2;
                    var result = Convert.ChangeType(value, targetType);
                    return result;
                }

                public static object Multiply(object valueLeft, object valueRight, Type targetType)
                {
                    var value1 = Convert.ToDecimal(valueLeft);
                    var value2 = Convert.ToDecimal(valueRight);
                    var value = value1 * value2;
                    return Convert.ChangeType(value, targetType);
                }

                public static object Divide(object valueLeft, object valueRight, Type targetType)
                {
                    var value1 = Convert.ToDecimal(valueLeft);
                    var value2 = Convert.ToDecimal(valueRight);
                    var value = value1 / value2;
                    return Convert.ChangeType(value, targetType);
                }

                public static object Modulo(object valueLeft, object valueRight, Type targetType)
                {
                    var value1 = Convert.ToDecimal(valueLeft);
                    var value2 = Convert.ToDecimal(valueRight);
                    var value = value1 % value2;
                    return Convert.ChangeType(value, targetType);
                }

                public static object Negate(object operand, Type resultType)
                {
                    operand = 0 - Convert.ToDecimal(operand);
                    var value = Convert.ChangeType(operand, resultType);
                    return value;
                }
            }

            #endregion

            #region Logic（逻辑运算）
            static class Logic
            {

                internal static object And(object valueLeft, object valueRight)
                {
                    var value1 = (bool)valueLeft;
                    var value2 = (bool)valueRight;

                    var value = value1 && value2;
                    return value;
                }

                internal static object Or(object valueLeft, object valueRight)
                {
                    var value1 = (bool)valueLeft;
                    var value2 = (bool)valueRight;

                    var value = value1 || value2;
                    return value;
                }
            }
            #endregion

            #region Bit（位运算）
            static class Bit
            {
                internal static object And(object valueLeft, object valueRight, Type targetType)
                {
                    var left = Convert.ChangeType(valueLeft, targetType);
                    var right = Convert.ChangeType(valueRight, targetType);
                    switch (Type.GetTypeCode(targetType))
                    {
                        case TypeCode.Byte:
                            return (Byte)left & (Byte)right;

                        case TypeCode.Int16:
                            return (Int16)left & (Int16)right;

                        case TypeCode.Int32:
                            return (int)left & (int)right;

                        case TypeCode.Int64:
                            return (Int64)left & (Int64)right;

                        case TypeCode.SByte:
                            return (SByte)left & (SByte)right;

                        case TypeCode.UInt16:
                            return (UInt16)left & (UInt16)right;

                        case TypeCode.UInt32:
                            return (UInt32)left & (UInt32)right;

                        case TypeCode.UInt64:
                            return (UInt64)left & (UInt64)right;

                        default:
                            throw new NotImplementedException();
                    }

                }

                internal static object Not(object value, Type targetType)
                {
                    switch (Type.GetTypeCode(targetType))
                    {
                        case TypeCode.Byte:
                            return ~(Byte)value;

                        case TypeCode.Int16:
                            return ~(Int16)value;

                        case TypeCode.Int32:
                            return ~(Int32)value;

                        case TypeCode.Int64:
                            return ~(Int64)value;

                        case TypeCode.SByte:
                            return ~(SByte)value;

                        case TypeCode.UInt16:
                            return ~(UInt16)value;

                        case TypeCode.UInt32:
                            return ~(UInt32)value;

                        case TypeCode.UInt64:
                            return ~(UInt64)value;

                        default:
                            throw new NotImplementedException();
                    }

                }

                internal static object Or(object valueLeft, object valueRight, Type targetType)
                {
                    //return (int)valueLeft | (int)valueRight;
                    var left = Convert.ChangeType(valueLeft, targetType);
                    var right = Convert.ChangeType(valueRight, targetType);
                    switch (Type.GetTypeCode(targetType))
                    {
                        case TypeCode.Byte:
                            return (Byte)left | (Byte)right;

                        case TypeCode.Int16:
                            return (Int16)left | (Int16)right;

                        case TypeCode.Int32:
                            return (int)left | (int)right;

                        case TypeCode.Int64:
                            return (Int64)left | (Int64)right;

                        case TypeCode.SByte:
                            return (SByte)left | (SByte)right;

                        case TypeCode.UInt16:
                            return (UInt16)left | (UInt16)right;

                        case TypeCode.UInt32:
                            return (UInt32)left | (UInt32)right;

                        case TypeCode.UInt64:
                            return (UInt64)left | (UInt64)right;

                        default:
                            throw new NotImplementedException();
                    }
                }

                public static object Xor(object valueLeft, object valueRight, Type targetType)
                {
                    //return (int)valueLeft ^ (int)valueRight;
                    var left = Convert.ChangeType(valueLeft, targetType);
                    var right = Convert.ChangeType(valueRight, targetType);
                    switch (Type.GetTypeCode(targetType))
                    {
                        case TypeCode.Byte:
                            return (Byte)left ^ (Byte)right;

                        case TypeCode.Int16:
                            return (Int16)left ^ (Int16)right;

                        case TypeCode.Int32:
                            return (int)left ^ (int)right;

                        case TypeCode.Int64:
                            return (Int64)left ^ (Int64)right;

                        case TypeCode.SByte:
                            return (SByte)left ^ (SByte)right;

                        case TypeCode.UInt16:
                            return (UInt16)left ^ (UInt16)right;

                        case TypeCode.UInt32:
                            return (UInt32)left ^ (UInt32)right;

                        case TypeCode.UInt64:
                            return (UInt64)left ^ (UInt64)right;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            #endregion

            #region ValueComparer（数值比较）
            static class ValueComparer
            {
                public static bool Equal(object valueLeft, object valueRight)
                {
                    var value1 = Convert.ToDecimal(valueLeft);
                    var value2 = Convert.ToDecimal(valueRight);

                    var value = value1 == value2;
                    return value;
                }

                internal static object LessThan(object valueLeft, object valueRight)
                {
                    var value1 = Convert.ToDecimal(valueLeft);
                    var value2 = Convert.ToDecimal(valueRight);

                    var value = value1 < value2;
                    return value;
                }

                internal static object LessThanOrEqual(object valueLeft, object valueRight)
                {
                    var value1 = Convert.ToDecimal(valueLeft);
                    var value2 = Convert.ToDecimal(valueRight);

                    var value = value1 <= value2;
                    return value;
                }

                internal static object GreaterThan(object valueLeft, object valueRight)
                {
                    var value1 = Convert.ToDecimal(valueLeft);
                    var value2 = Convert.ToDecimal(valueRight);

                    var value = value1 > value2;
                    return value;
                }

                internal static object GreaterThanOrEqual(object valueLeft, object valueRight)
                {
                    var value1 = Convert.ToDecimal(valueLeft);
                    var value2 = Convert.ToDecimal(valueRight);

                    var value = value1 >= value2;
                    return value;
                }

                public static object NotEqual(object valueLeft, object valueRight)
                {
                    var value1 = Convert.ToDecimal(valueLeft);
                    var value2 = Convert.ToDecimal(valueRight);

                    var value = value1 != value2;
                    return value;
                }
            }
            #endregion



            private object _result;
            private ObjectParameter[] parameters;

            internal ExpressionCalculaterVisitor(params ObjectParameter[] parameters)
            {
                if (parameters == null)
                    this.parameters = new ObjectParameter[] { };
                else
                    this.parameters = parameters;
            }

            private void PushValue(object value)
            {
                _result = value;
            }

            internal object PopValue()
            {
                var obj = _result;
                _result = null;
                return obj;
            }



            public override Expression Visit(Expression node)
            {
                switch (node.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        VisitMemberAccess((MemberExpression)node);
                        break;
                    case ExpressionType.Lambda:
                        VisitLambda((LambdaExpression)node);
                        break;
                    case ExpressionType.Parameter:
                        VisitParameter((ParameterExpression)node);
                        break;
                    case ExpressionType.Add:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Divide:
                    case ExpressionType.Equal:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Multiply:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.Subtract:
                        Debug.Assert(node is BinaryExpression);
                        VisitBinary((BinaryExpression)node);
                        break;
                    case ExpressionType.Constant:
                        VisitConstant((ConstantExpression)node);
                        break;
                    case ExpressionType.Call:
                        VisitMethodCall((MethodCallExpression)node);
                        break;
                    case ExpressionType.Convert:
                        VisitUnary((UnaryExpression)node);
                        break;
                    case ExpressionType.MemberInit:
                        VisitMemberInit((MemberInitExpression)node);
                        break;
                    case ExpressionType.New:
                        VisitNew((NewExpression)node);
                        break;
                    case ExpressionType.Negate:
                        VisitNegate((UnaryExpression)node);
                        break;
                    case ExpressionType.Not:
                        this.Not((UnaryExpression)node);
                        break;
                    case ExpressionType.Quote:
                        this.VisitUnary((UnaryExpression)node);
                        break;
                    case ExpressionType.Modulo:
                        this.VisitBinary((BinaryExpression)node);
                        break;

                    default:
                        throw new NotImplementedException(node.NodeType.ToString());
                }


                return node;
            }



            private void Not(UnaryExpression node)
            {

                Visit(node.Operand);
                var value = PopValue();
                if (value is bool)
                    value = !(bool)value;
                else
                    value = Bit.Not(value, node.Type); //~(int)value;

                PushValue(value);
                return;
            }

            protected override Expression VisitNew(NewExpression nex)
            {
                var args = new object[nex.Arguments.Count];
                for (var i = 0; i < nex.Arguments.Count; i++)
                {
                    var arg = Eval(nex.Arguments[i], parameters);
                    args[i] = arg;
                }

                var obj = nex.Constructor.Invoke(args);
                this.PushValue(obj);

                return nex;
            }



            private void VisitNegate(UnaryExpression node)
            {
                var value = ExpressionCalculater.Eval(node.Operand, null);
                value = Arithmetic.Negate(value, node.Type);

                PushValue(value);
            }


            protected override Expression VisitMemberInit(MemberInitExpression node)
            {
                var args = node.NewExpression.Arguments.Cast<ConstantExpression>().Select(o => o.Value).ToArray();
                var obj = node.NewExpression.Constructor.Invoke(args);

                foreach (MemberAssignment binding in node.Bindings)
                {
                    var value = Eval(binding.Expression, parameters);
                    var member = binding.Member;
                    if (member.MemberType == MemberTypes.Property)
                    {
                        ((PropertyInfo)member).SetValue(obj, value, null);
                    }
                    else if (binding.Member.MemberType == MemberTypes.Field)
                    {
                        ((FieldInfo)member).SetValue(obj, value);
                    }
                    else
                    {
                        throw Error.NotPropertyOrField(member.Name);
                    }
                }

                PushValue(obj);

                return node;
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
               
                if (node.NodeType == ExpressionType.Convert)
                {
                    Visit(node.Operand);
                    var operand = PopValue();

                    var value = Convert.ChangeType(operand, node.Type);
                    PushValue(value);
                }
                else if (node.NodeType == ExpressionType.Quote)
                {
                    PushValue(node.Operand);
                }
                else
                {
                    throw new NotImplementedException(node.NodeType.ToString());
                }
                return node;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                object value;
                value = node.Value;

                PushValue(value);
                return node;
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                Visit(node.Left);
                var valueLeft = PopValue();
                Visit(node.Right);
                var valueRight = PopValue();

                object value = null;

                if (node.Method != null)
                {
                    value = node.Method.Invoke(null, new object[] { valueLeft, valueRight });
                    PushValue(value);
                    return node;
                }

                switch (node.NodeType)
                {
                    //四则运算
                    case ExpressionType.Add:
                        value = Arithmetic.Add(valueLeft, valueRight, node.Type);
                        break;
                    case ExpressionType.Subtract:
                        value = Arithmetic.Subtract(valueLeft, valueRight, node.Type);
                        break;
                    case ExpressionType.Multiply:
                        value = Arithmetic.Multiply(valueLeft, valueRight, node.Type);
                        break;
                    case ExpressionType.Divide:
                        value = Arithmetic.Divide(valueLeft, valueRight, node.Type);
                        break;
                    case ExpressionType.Modulo:
                        value = Arithmetic.Modulo(valueLeft, valueRight, node.Type);
                        break;

                    //数值比较
                    case ExpressionType.Equal:
                        value = ValueComparer.Equal(valueLeft, valueRight);
                        break;
                    case ExpressionType.NotEqual:
                        value = ValueComparer.NotEqual(valueLeft, valueRight);
                        break;
                    case ExpressionType.LessThan:
                        value = ValueComparer.LessThan(valueLeft, valueRight);
                        break;
                    case ExpressionType.LessThanOrEqual:
                        value = ValueComparer.LessThanOrEqual(valueLeft, valueRight);
                        break;
                    case ExpressionType.GreaterThan:
                        value = ValueComparer.GreaterThan(valueLeft, valueRight);
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        value = ValueComparer.GreaterThanOrEqual(valueLeft, valueRight);
                        break;

                    //逻辑运算
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                        if (valueLeft is bool && valueLeft is bool)
                            value = Logic.And(valueLeft, valueRight);
                        else
                            value = Bit.And(valueLeft, valueRight, node.Type);
                        break;

                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                        if (valueLeft is bool && valueLeft is bool)
                            value = Logic.Or(valueLeft, valueRight);
                        else
                            value = Bit.Or(valueLeft, valueRight, node.Type);
                        break;

                    case ExpressionType.ExclusiveOr:
                        value = Bit.Xor(valueLeft, valueRight, node.Type);
                        break;

                }

                if (value == null)
                    throw new NotImplementedException(node.NodeType.ToString());

                Debug.Assert(value != null);
                PushValue(value);

                return node;
            }

            protected override Expression VisitLambda(LambdaExpression lambda)
            {
                Expression body = Visit(lambda.Body);
                if (body != lambda.Body)
                {
                    return Expression.Lambda(lambda.Type, body, lambda.Parameters);
                }
                return lambda;
            }

            protected override Expression VisitMemberAccess(MemberExpression node)
            {
                object obj = null;
                if (node.Expression != null)
                {
                    Visit(node.Expression);
                    obj = PopValue();
                }


                var member = node.Member;
                if (member.MemberType == MemberTypes.Property)
                {
                    var value = ((PropertyInfo)member).GetValue(obj, null);
                    PushValue(value);
                }
                else if (member.MemberType == MemberTypes.Field)
                {
                    var value = ((FieldInfo)member).GetValue(obj);
                    PushValue(value);
                }
                else
                {
                    throw Error.NotPropertyOrField(member.Name);
                }
                return node;
            }

            #region  创建 Func
            static Func<T, TResult> CreateFunc1<T, TResult>(LambdaExpression lamb)
            {
                Debug.Assert(lamb.Parameters.Count == 1);
                Func<T, TResult> f = delegate(T arg)
                                   {
                                       var p = new ObjectParameter(lamb.Parameters[0].Name, arg);
                                       var result = ExpressionCalculater.Eval(lamb.Body, p);
                                       return (TResult)result;
                                   };
                return f;
            }

            static Func<T1, T2, TResult> CreateFunc2<T1, T2, TResult>(LambdaExpression lamb)
            {
                Debug.Assert(lamb.Parameters.Count == 2);
                Func<T1, T2, TResult> f = delegate(T1 arg0, T2 arg1)
                {
                    var p0 = new ObjectParameter(lamb.Parameters[0].Name, arg0);
                    var p1 = new ObjectParameter(lamb.Parameters[1].Name, arg1);

                    var result = ExpressionCalculater.Eval(lamb.Body, p0, p1);
                    return (TResult)result;
                };
                return f;
            }

            static Func<T1, T2, T3, TResult> CreateFunc3<T1, T2, T3, TResult>(LambdaExpression lamb)
            {
                Debug.Assert(lamb.Parameters.Count == 2);
                Func<T1, T2, T3, TResult> f = delegate(T1 arg0, T2 arg1, T3 arg2)
                {
                    var p0 = new ObjectParameter(lamb.Parameters[0].Name, arg0);
                    var p1 = new ObjectParameter(lamb.Parameters[1].Name, arg1);
                    var p2 = new ObjectParameter(lamb.Parameters[2].Name, arg2);

                    var result = ExpressionCalculater.Eval(lamb.Body, p0, p1, p2);
                    return (TResult)result;
                };
                return f;
            }
            #endregion

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var declaringType = node.Method.DeclaringType;
                if (declaringType == typeof(System.Linq.Enumerable) || declaringType == typeof(Queryable) ||
                    (declaringType.IsGenericType && declaringType.GetGenericTypeDefinition() == typeof(IQueryable<>)))
                {

                    var provider = QueryProviderFinder.FindProvider(node);
                    if (provider != null)
                    {
                        var value = provider.Execute(node);
                        PushValue(value);

                        return node;
                    }
                }

                var argValues = new object[node.Arguments.Count];
                for (var i = 0; i < node.Arguments.Count; i++)
                {
                    var arg = node.Arguments[i];
                    if (arg.NodeType == ExpressionType.Lambda)
                    {
                        var exp = ((LambdaExpression)arg);
                        Debug.Assert(exp.Parameters.Count == 1);

                        MethodInfo m;
                        var bf = BindingFlags.Static | BindingFlags.NonPublic;
                        var list = exp.Parameters.Select(o => o.Type).ToList();
                        list.Add(exp.Body.Type);
                        var types = list.ToArray();

                        switch (exp.Parameters.Count)
                        {
                            case 1:
                                m = this.GetType().GetMethod("CreateFunc1", bf)
                                                   .MakeGenericMethod(types);
                                break;
                            case 2:
                                m = this.GetType().GetMethod("CreateFunc2", bf)
                                                  .MakeGenericMethod(types);
                                break;
                            case 3:
                                m = this.GetType().GetMethod("CreateFunc3", bf)
                                         .MakeGenericMethod(types);
                                break;
                            default:
                                throw new NotImplementedException();

                        }
                        argValues[i] = m.Invoke(null, new object[] { exp });
                    }
                    else
                    {
                        Visit(arg);
                        argValues[i] = this.PopValue();
                    }

                }
                if (node.Method.IsStatic)
                {
                    var value = node.Method.Invoke(null, argValues);
                    PushValue(value);
                }
                else
                {
                    Visit(node.Object);
                    var instance = PopValue();
                    Debug.Assert(instance != null);

                    var value = node.Method.Invoke(instance, argValues);
                    PushValue(value);
                }
                return node;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                var p = this.parameters.SingleOrDefault(o => o.Name == (node.Name ?? string.Empty));
                if (p != null)
                {
                    object entity = p.Value;
                    PushValue(entity);
                }
                else
                {
                    var value = Activator.CreateInstance(node.Type);
                    PushValue(value);
                }
                return node;
            }

            class QueryProviderFinder : ExpressionVisitor
            {

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
                    return base.VisitConstant(c);
                }
            }


        }

        public static object Eval(Expression expr)
        {
            return Eval(expr, null);
        }

        public static object Eval(Expression expr, params ObjectParameter[] parameters)
        {
            var instance = new ExpressionCalculaterVisitor(parameters);
            instance.Visit(expr);
            return instance.PopValue();
        }
    }
}