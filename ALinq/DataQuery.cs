using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using ALinq.SqlClient;

namespace ALinq
{
    internal sealed class DataQuery<T> : IOrderedQueryable<T>, IQueryProvider, IListSource
    {
        // Fields
        private IBindingList cachedList;
        private readonly DataContext context;
        private readonly Expression queryExpression;

        // Methods
        public DataQuery(DataContext context, Expression expression)
        {
            this.context = context;
            queryExpression = expression;
        }

        internal IBindingList GetNewBindingList()
        {
            return BindingList.Create(context, this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            IExecuteResult executeResult = context.Provider.Execute(queryExpression);
            object returnValue = executeResult.ReturnValue;
            var result = ((IEnumerable<T>)returnValue).GetEnumerator();
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)context.Provider.Execute(queryExpression).ReturnValue).GetEnumerator();
        }

        IList IListSource.GetList()
        {
            if (cachedList == null)
            {
                cachedList = GetNewBindingList();
            }
            return cachedList;
        }

        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNull("expression");
            }
            if (!typeof(IQueryable<S>).IsAssignableFrom(expression.Type))
            {
                throw Error.ExpectedQueryableArgument("expression", typeof(IEnumerable<S>));
            }
            return new DataQuery<S>(context, expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNull("expression");
            }
            Type elementType = TypeSystem.GetElementType(expression.Type);
            Type type2 = typeof(IQueryable<>).MakeGenericType(new[] { elementType });
            if (!type2.IsAssignableFrom(expression.Type))
            {
                throw Error.ExpectedQueryableArgument("expression", type2);
            }
            return (IQueryable)Activator.CreateInstance(typeof(DataQuery<>).MakeGenericType(new[] { elementType }), new object[] { context, expression });
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return context.Provider.Execute(expression).ReturnValue;
        }

        S IQueryProvider.Execute<S>(Expression expression)
        {
            return (S)context.Provider.Execute(expression).ReturnValue;
        }

        public override string ToString()
        {
            return context.Provider.GetQueryText(queryExpression);
        }

        // Properties
        bool IListSource.ContainsListCollection
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return false; }
        }

        Type IQueryable.ElementType
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return typeof(T); }
        }

        Expression IQueryable.Expression
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return queryExpression;
            }
        }

        IQueryProvider IQueryable.Provider
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this;
            }
        }
    }

}