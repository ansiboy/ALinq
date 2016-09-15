using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace ALinq.Dynamic
{
    //=================================================================================
    // 說明：包裝原來的 IQueryProvider，用來攔截原來 IQueryProvider 的方法。 
    //=================================================================================
    [DebuggerDisplay("{source.ToString()}")]
    internal class ProxyQueryProvider : IQueryProvider
    {
        private IQueryProvider source;

        public ProxyQueryProvider(IQueryProvider source)
        {
            this.source = source;
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            var elementType = ExpressionUtility.ElementType(expression);
            var queryable = CreateQuery(expression, elementType);
            return queryable;
        }

        IQueryable CreateQuery(Expression expression, Type elementType)
        {
            var queryable = this.source.CreateQuery(expression);
            var t = typeof(ProxyQueryable<>).MakeGenericType(elementType);
            queryable = (IQueryable)Activator.CreateInstance(t, this, queryable);
            return queryable;
        }

        IQueryable<T> IQueryProvider.CreateQuery<T>(Expression expression)
        {
            var queryable = this.source.CreateQuery<T>(expression);
            if (queryable is ProxyQueryable == false)
            {
                queryable = new ProxyQueryable<T>(this, queryable);
            }

            return queryable;
        }

        //============================= 更新表达式 =============================
        // 例如 
        // var q = db.CreateQuery<IDataRecord>("select p from Products as p");
        // 由于序列的实际类型即是Product，将变为：
        // db.CreateQuery<Product>("select p from Products as p")
        //=====================================================================
        Expression UpdateExpression(Expression expression)
        {
            var visitor1 = new TypePaireVisitor();
            visitor1.Visit(expression);
            var typePaires = visitor1.Result;

            foreach (var item in typePaires)
            {
                var visitor = new ActualTypeReplacer(item.Value, item.Key);
                expression = visitor.Visit(expression);
            }
            return expression;
        }

        object IQueryProvider.Execute(Expression expression)
        {
            expression = UpdateExpression(expression);

            var isSingleValue = !typeof(IEnumerable).IsAssignableFrom(expression.Type) || expression.Type == typeof(string);
            if (isSingleValue)
            {
                var obj = this.source.Execute(expression);
                return obj;
            }

            var items = this.source.Execute<IEnumerable>(expression);
            return items;
        }

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            expression = UpdateExpression(expression);

            var isSingleValue = !typeof(IEnumerable).IsAssignableFrom(expression.Type) || expression.Type == typeof(string);
            if (isSingleValue)
            {
                var obj = this.source.Execute<TResult>(expression);
                return obj;
            }

            var castSourceType = ExpressionUtility.ElementType(expression);
            var castTargetType = GetElementType(typeof(TResult));
            if (castSourceType != castTargetType)
                expression = Expression.Call(typeof(Queryable), "Cast", new[] { castTargetType }, expression);

            var items = this.source.Execute<TResult>(expression);
            return items;
        }

        static Type GetElementType(Type collectionType)
        {
            Type elementType = collectionType.GetElementType();
            if (elementType != null)//&& elementType != typeof(char) && elementType != typeof(byte))
            {
                return elementType;
            }

            if (collectionType.IsGenericType)
            {
                var t = collectionType.GetGenericArguments()[0];
                var e = typeof(IEnumerable<>).MakeGenericType(t);
                if (e.IsAssignableFrom(collectionType))
                    return t;

            }


            return collectionType;

        }




        class TypePaireVisitor : ExpressionVisitor
        {
            private Dictionary<Type, Type> typePaires;

            public TypePaireVisitor()
            {
                this.typePaires = new Dictionary<Type, Type>();
            }

            protected override Expression VisitMethodCall(MethodCallExpression m)
            {
                var anonymousType = typeof(DataRow);

                if (m.Method.Name == "Cast")
                {
                    var castSourceType = ExpressionUtility.ElementType(m.Arguments[0]);
                    var castTargetType = ExpressionUtility.ElementType(m);
                    if (castTargetType.IsAssignableFrom(anonymousType))
                    {
                        this.typePaires.Add(castSourceType, castTargetType);
                    }
                }
                return base.VisitMethodCall(m);
            }

            public Dictionary<Type, Type> Result
            {
                get { return this.typePaires; }
            }
        }

        /// <summary>
        /// 将接口类型替换为实际类型，同时，接口的属性、字段亦相应在替换。
        /// <remart>
        /// <![CDATA[
        /// 例如 
        /// var q = db.CreateQuery<IDataRecord>("select p from Products as p");
        /// 由于序列的实际类型即是Product，将变为：
        /// db.CreateQuery<Product>("select p from Products as p")
        /// ]]>
        /// </remart>
        /// </summary>
        class ActualTypeReplacer : ExpressionVisitor
        {
            private Type newType;
            private Type oldType;
            private ParameterExpression parameter;

            public ActualTypeReplacer(Type oldType, Type newType)
            {
                this.newType = newType;
                this.oldType = oldType;
                this.parameter = ExpressionUtility.CreateParameter(newType);
            }


            protected override Expression VisitMethodCall(MethodCallExpression m)
            {
                //==============================================================
                // 说明：将索引器字段转换为类的属性或字段
                if (m.Method.Name == "get_Item" && m.Object != null && m.Object.Type == this.oldType)
                {
                    Debug.Assert(m.Arguments.Count == 1);
                    var arg = m.Arguments[0];

                    Debug.Assert(arg.NodeType == ExpressionType.Constant);
                    Debug.Assert(arg.Type == typeof(string));

                    var name = (string)((ConstantExpression)arg).Value;
                    var pf = TypeUtility.FindPropertyOrField(this.newType, name, false);
                    if (pf == null)
                        throw Error.NoPublicPropertyOrField(name, newType);

                    Expression expr = Expression.MakeMemberAccess(this.parameter, pf);
                    if (m.Type != expr.Type)
                        expr = Expression.Convert(expr, m.Type);

                    return expr;
                }
                //==============================================================



                //==============================================================
                // 说明：如果序列的实际类型与 Cast 的目标类型一致，则直接返回序列，而不进行转换。
                // 例如：
                // var q = db.CreateQuery("select p from Products as p").Cast<Product>();
                // 由于序列的实际类型即是Product，因为，Cast 是多余的，直接除。即变为：
                // db.CreateQuery("select p from Products as p")
                //--------------------------------------------------------------
                if (m.Method.Name == "Cast")
                {
                    var castSourceType = ExpressionUtility.ElementType(m.Arguments[0]);
                    var castTargetType = ExpressionUtility.ElementType(m);
                    if (this.newType == castSourceType && this.oldType == castTargetType)
                    {
                        return m.Arguments[0];
                    }
                }
                //==============================================================


                Expression instance = Visit(m.Object);
                IEnumerable<Expression> arguments = VisitExpressionList(m.Arguments);
                if ((instance == m.Object) && (arguments == m.Arguments))
                {
                    return m;
                }

                //==============================================================
                // 说明：替换掉原来的类型
                // 例如：
                // var q = db.CreateQuery<IDataRecord>("select p from Products as p")
                // 变为
                // q = db.CreateQuery<Product>()
                if (m.Method.IsGenericMethod)
                {
                    var argTypes = m.Method.GetGenericArguments();
                    if (argTypes[0] == oldType)
                    {
                        var methodName = m.Method.Name;
                        var declareType = m.Method.DeclaringType;
                        argTypes[0] = newType;
                        var expr = Expression.Call(declareType, methodName, argTypes, arguments.ToArray());
                        return expr;
                    }
                }
                //==============================================================



                return Expression.Call(instance, m.Method, arguments);
            }


            protected override Expression VisitUnary(UnaryExpression u)
            {
                Expression operand = Visit(u.Operand);
                if (operand.Type == u.Type)
                    return operand;

                if (operand != u.Operand)
                    return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);

                return u;
            }

            protected override Expression VisitLambda(LambdaExpression lambda)
            {
                Expression body = Visit(lambda.Body);
                if (body != lambda.Body)
                {
                    var parameters = lambda.Parameters
                                           .Select(o => VisitParameter(o)).Cast<ParameterExpression>()
                                           .ToArray();

                    return Expression.Lambda(body, parameters);
                }
                return lambda;
            }

            protected override Expression VisitParameter(ParameterExpression p)
            {
                if (p.Type == this.oldType)
                    return this.parameter;

                return base.VisitParameter(p);
            }


        }

        #region ProxyQueryable
        [DebuggerDisplay("{source.ToString()}")]
        class ProxyQueryable : IQueryable
        {
            private IQueryable source;
            private ProxyQueryProvider provider;

            protected ProxyQueryable(ProxyQueryProvider provider, IQueryable source)
            {
                this.provider = provider;
                this.source = source;
            }



            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.source.GetEnumerator();
            }

            Expression IQueryable.Expression
            {
                get { return source.Expression; }
            }

            Type IQueryable.ElementType
            {
                get { return this.source.ElementType; }
            }

            IQueryProvider IQueryable.Provider
            {
                get { return this.provider; }
            }
        }

        class ProxyQueryable<T> : ProxyQueryable, IQueryable<T>
        {
            private IQueryable<T> typedSource;
            //private IQueryable untypedSource;

            public ProxyQueryable(ProxyQueryProvider provider, IQueryable<T> source)
                : base(provider, source)
            {
                this.typedSource = source;
            }

            //public ProxyQueryable(ProxyQueryProvider provider, IQueryable source)
            //    : base(provider, source)
            //{
            //    this.untypedSource = source;
            //}

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                //if (this.typedSource != null)
                return typedSource.GetEnumerator();

                //var e = CreateEnumerator(this.untypedSource.GetEnumerator());
                //return e;
            }

            IEnumerator<T> CreateEnumerator(IEnumerator enumerator)
            {
                while (enumerator.MoveNext())
                {
                    var value = enumerator.Current;
                    if (value is T)
                        yield return (T)value;
                    else
                        yield return (T)Convert.ChangeType(enumerator.Current, typeof(T));
                }
            }
        }
        #endregion


    }


}
