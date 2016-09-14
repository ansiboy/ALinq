using System;
using ALinq;
using System.Linq.Expressions;

namespace ALinq
{
    /// <summary>
    /// Provides for compilation and caching of queries for reuse.
    /// </summary>
    public sealed class CompiledQuery
    {
        // Fields
        private ICompiledQuery compiled;
        private readonly LambdaExpression query;

        // Methods
        private CompiledQuery(LambdaExpression query)
        {
            this.query = query;
        }

        /// <summary>
        /// Compiles the query.
        /// </summary>
        /// <typeparam name="TArg0">Represents the type of the parameter that has to be passed in when executing the delegate returned by the ALinq.CompiledQuery.Compile&lt;T0, T1&gt;(System.Linq.Expressions.Expression&lt;System.Func&lt;T0,T1&gt;&gt;) method.</typeparam>
        /// <typeparam name="TResult">The type of T in the System.Collections.Generic.IEnumerable&lt;T&gt; returned when executing the delegate returned by the ALinq.CompiledQuery.Compile&lt;T0, T1&gt;</typeparam>
        /// <param name="query">The query expression to be compiled.</param>
        /// <returns>A generic delegate that represents the compiled query.</returns>
        public static Func<TArg0, TResult> Compile<TArg0, TResult>(Expression<Func<TArg0, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
            {
                Error.ArgumentNull("query");
            }
            else if (UseExpressionCompile(query))
            {
                return query.Compile();
            }
            return new CompiledQuery(query).Invoke<TArg0, TResult>;
        }

        /// <summary>
        /// Compiles the query.
        /// </summary>
        /// <typeparam name="TArg0">Represents the type of the parameter that has to be passed in when executing the delegate returned by the ALinq.CompiledQuery.Compile&lt;T0, T1&gt;(System.Linq.Expressions.Expression&lt;System.Func&lt;T0,T1&gt;&gt;) method.</typeparam>
        /// <typeparam name="TArg1">Represents the type of the parameter that has to be passed in when executing the delegate returned by the ALinq.CompiledQuery.Compile&lt;T0, T1&gt;(System.Linq.Expressions.Expression&lt;System.Func&lt;T0,T1&gt;&gt;) method.</typeparam>
        /// <typeparam name="TResult">The type of T in the System.Collections.Generic.IEnumerable&lt;T&gt; returned when executing the delegate returned by the ALinq.CompiledQuery.Compile&lt;T0, T1&gt;(System.Linq.Expressions.Expression&lt;System.Func&lt;T0,T1&gt;&gt;) method.</typeparam>
        /// <param name="query">The query expression to be compiled.</param>
        /// <returns>A generic delegate that represents the compiled query.</returns>
        public static Func<TArg0, TArg1, TResult> Compile<TArg0, TArg1, TResult>(Expression<Func<TArg0, TArg1, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
            {
                Error.ArgumentNull("query");
            }
            else if (UseExpressionCompile(query))
            {
                return query.Compile();
            }
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TResult>;
        }

        /// <summary>
        /// Compiles the query.
        /// </summary>
        /// <typeparam name="TArg0">Represents the type of the parameter that has to be passed in when executing the delegate returned by the ALinq.CompiledQuery.Compile&lt;T0, T1&gt;(System.Linq.Expressions.Expression&lt;System.Func&lt;T0,T1&gt;&gt;) method.</typeparam>
        /// <typeparam name="TArg1">Represents the type of the parameter that has to be passed in when executing the delegate returned by the ALinq.CompiledQuery.Compile&lt;T0, T1&gt;(System.Linq.Expressions.Expression&lt;System.Func&lt;T0,T1&gt;&gt;) method.</typeparam>
        /// <typeparam name="TArg2">Represents the type of the parameter that has to be passed in when executing the delegate returned by the ALinq.CompiledQuery.Compile&lt;T0, T1&gt;(System.Linq.Expressions.Expression&lt;System.Func&lt;T0,T1&gt;&gt;) method.</typeparam>
        /// <typeparam name="TResult">The type of T in the System.Collections.Generic.IEnumerable&lt;T&gt; returned when executing the delegate returned by the ALinq.CompiledQuery.Compile&lt;T0, T1&gt;(System.Linq.Expressions.Expression&lt;System.Func&lt;T0,T1&gt;&gt;) method.</typeparam>
        /// <param name="query">The query expression to be compiled.</param>
        /// <returns>A generic delegate that represents the compiled query.</returns>
        public static Func<TArg0, TArg1, TArg2, TResult> Compile<TArg0, TArg1, TArg2, TResult>(Expression<Func<TArg0, TArg1, TArg2, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
            {
                Error.ArgumentNull("query");
            }
            else if (UseExpressionCompile(query))
            {
                return query.Compile();
            }
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TResult>;
        }

        /// <summary>
        /// Compiles the query.
        /// </summary>
        /// <typeparam name="TArg0">Represents the type of the parameter that has to be passed in when executing the delegate returned by the ALinq.CompiledQuery.Compile&lt;T0, T1&gt;(System.Linq.Expressions.Expression&lt;System.Func&lt;T0, T1&gt;&gt;) method.</typeparam>
        /// <typeparam name="TArg1">Represents the type of the parameter that has to be passed in when executing the delegate returned by the ALinq.CompiledQuery.Compile&lt;T0, T1&gt;(System.Linq.Expressions.Expression&lt;System.Func&lt;T0, T1&gt;&gt;) method.</typeparam>
        /// <typeparam name="TArg2">Represents the type of the parameter that has to be passed in when executing the delegate returned by the ALinq.CompiledQuery.Compile&lt;T0, T1&gt;(System.Linq.Expressions.Expression&lt;System.Func&lt;T0, T1&gt;&gt;) method.</typeparam>
        /// <typeparam name="TArg3">Represents the type of the parameter that has to be passed in when executing the delegate returned by the ALinq.CompiledQuery.Compile&lt;T0, T1&gt;(System.Linq.Expressions.Expression&lt;System.Func&lt;T0, T1&gt;&gt;) method.</typeparam>
        /// <typeparam name="TResult">The type of T in the System.Collections.Generic.IEnumerable<T> returned when executing the delegate returned by the ALinq.CompiledQuery.Compile&lt;T0, T1&gt;(System.Linq.Expressions.Expression&lt;System.Func&lt;T0, T1&gt;&gt;) method.</typeparam>
        /// <param name="query">The query expression to be compiled.</param>
        /// <returns>A generic delegate that represents the compiled query.</returns>
        public static Func<TArg0, TArg1, TArg2, TArg3, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
            {
                Error.ArgumentNull("query");
            }
            else if (UseExpressionCompile(query))
            {
                return query.Compile();
            }
            return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TResult>;
        }

        private object ExecuteQuery(DataContext context, object[] args)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }
            if (compiled == null)
            {
                lock (this)
                {
                    if (compiled == null)
                    {
                        compiled = context.Provider.Compile(query);
                    }
                }
            }
            return compiled.Execute(context.Provider, args).ReturnValue;
        }

        private TResult Invoke<TArg0, TResult>(TArg0 arg0) where TArg0 : DataContext
        {
            return (TResult)ExecuteQuery(arg0, new object[] { arg0 });
        }

        private TResult Invoke<TArg0, TArg1, TResult>(TArg0 arg0, TArg1 arg1) where TArg0 : DataContext
        {
            return (TResult)this.ExecuteQuery(arg0, new object[] { arg0, arg1 });
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2) where TArg0 : DataContext
        {
            return (TResult)this.ExecuteQuery(arg0, new object[] { arg0, arg1, arg2 });
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3) where TArg0 : DataContext
        {
            return (TResult)this.ExecuteQuery(arg0, new object[] { arg0, arg1, arg2, arg3 });
        }

        private static bool UseExpressionCompile(LambdaExpression query)
        {
            return typeof(ITable).IsAssignableFrom(query.Body.Type);
        }

        /// <summary>
        /// Returns the query as a lambda expression.
        /// </summary>    
        /// <returns>
        /// The lambda expression that represents the query.
        /// </returns>    
        public LambdaExpression Expression
        {
            get
            {
                return this.query;
            }
        }
    }

}
