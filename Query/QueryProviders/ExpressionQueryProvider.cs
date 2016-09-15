using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ALinq.Dynamic.Parsers;

namespace ALinq.Dynamic
{
    internal class ExpressionQueryProvider : IQueryProvider
    {
        private Dictionary<string, object> parameters;

        public ExpressionQueryProvider(params ObjectParameter[] parameters)
        {
            if (parameters != null)
                this.parameters = parameters.ToDictionary(o => o.Name ?? string.Empty, o => o.Value);
        }

        public ExpressionQueryProvider(Dictionary<string, object> parameters)
        {
            this.parameters = parameters;
        }

        #region Source Replacement
        class SourceReplacement : ExpressionVisitor
        {
            protected override Expression VisitConstant(ConstantExpression c)
            {
                if (c.Type.IsGenericType && c.Type.GetGenericTypeDefinition() == typeof(ExpressionQueryable<>))
                {
                    Debug.Assert(c.Value != null);

                    var t = ((IQueryable)c.Value).GetEnumerator();
                    var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(((IQueryable)c.Value).ElementType));
                    while (t.MoveNext())
                    {
                        list.Add(t.Current);

                    }

                    var q = TypeUtility.CreateGenericEnumerableQueryInstance(list, ((IQueryable)c.Value).ElementType);
                    return Expression.Constant(q);
                }

                return base.VisitConstant(c);
            }
        }
        #endregion

        private class ExpressionQueryable
        {
            internal Expression expr;

            protected ExpressionQueryable(ExpressionQueryProvider provider, Expression expr)
            {
                this.Provider = provider;
                this.expr = expr;
            }

            public IQueryProvider Provider
            {
                get;
                private set;
            }
        }

        class ExpressionQueryable<T> : ExpressionQueryable, IOrderedQueryable<T>
        {
            public ExpressionQueryable(ExpressionQueryProvider provider, Expression expr)
                : base(provider, expr)
            {
                this.ElementType = typeof(T);

            }

            public IEnumerator<T> GetEnumerator()
            {
                var source = ExpressionCalculater.Eval(expr, null);
                if (source == null)
                {
                    yield return default(T);
                }
                else
                {
                    var items = source as IEnumerable;

                    Type elementType = null;
                    if (items != null)
                        elementType = GetElementType(items.GetType());

                    if (items == null || elementType == typeof(string) || elementType == typeof(byte))
                    {
                        var arr = Array.CreateInstance(typeof(object), 1);
                        arr.SetValue(source, 0);
                        items = arr;
                    }

                    foreach (var item in items)
                    {
                        T value;
                        if (item is T)
                            value = (T)item;
                        else
                            value = (T)Convert.ChangeType(item, typeof(T));

                        yield return value;
                    }
                }

            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public Expression Expression
            {
                get { return Expression.Constant(this); }
            }

            public Type ElementType { get; private set; }


        }

        #region Implementation of IQueryProvider



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

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = GetElementType(expression.Type);
            if (elementType == typeof(byte))
                elementType = expression.Type;

            var type = typeof(ExpressionQueryable<>).MakeGenericType(elementType);
            var queryable = Activator.CreateInstance(type, new object[] { this, expression });
            return (IQueryable)queryable;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var q = this.CreateQuery(expression);
            if (typeof(TElement) == q.ElementType)
                return (IQueryable<TElement>)q;

            if (typeof(ExpressionQueryProvider).IsInstanceOfType(q.Provider))
            {
                var q1 = ((ExpressionQueryProvider)q.Provider).Cast<TElement>(q);
                return q1;
            }

            return q.Cast<TElement>();
        }

        public object Execute(Expression expression)
        {
            object source = null;
            if (expression.NodeType == ExpressionType.Call)
            {
                expression = new SourceReplacement().Visit(expression);

                Debug.Assert(expression.NodeType == ExpressionType.Call);
                var method = ((MethodCallExpression)expression).Method;
                //Debug.Assert(method.IsGenericMethod);
                var args = ((MethodCallExpression)expression).Arguments.Select(o => ExpressionCalculater.Eval(o, null)).ToArray();
                source = method.Invoke(null, args);
            }
            else
            {
                source = ExpressionCalculater.Eval(expression);
                if (source is IQueryable)
                {
                    var type = source.GetType();
                    Debug.Assert(type.IsGenericType);
                    Debug.Assert(type.GetGenericArguments().Length == 1);

                    var t = typeof(List<>).MakeGenericType(source.GetType().GetGenericArguments()[0]);
                    var list = (IList)Activator.CreateInstance(t);
                    foreach (var item in (IQueryable)source)
                        list.Add(item);

                    source = list;
                }
            }
            return source;

        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

        #endregion

        public IQueryable<T> Cast<T>(IQueryable queryable)
        {
            if (queryable == null)
                throw new ArgumentNullException("queryable");

            Debug.Assert(queryable.GetType().IsGenericType);
            Debug.Assert(queryable.GetType().GetGenericTypeDefinition() == typeof(ExpressionQueryable<>));

            var q = new ExpressionQueryable<T>(this, ((ExpressionQueryable)queryable).expr);
            return q;
        }
    }
}
