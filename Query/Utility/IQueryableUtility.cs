#if L2S
using System.Data.Linq;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using ALinq.Dynamic.Parsers;


namespace ALinq.Dynamic
{
    public static class IQueryableUtility
    {
        /// <summary>
        /// Executes the untyped query.
        /// </summary>
        /// <param name="query">The query to be executed.</param>
        /// <returns>A IEnumerable that contains a collection of entity objects returned by the query.</returns>
        public static IEnumerable Execute(this IQueryable query)
        {
            if (query.Expression.NodeType == ExpressionType.Constant)
            {
                var value = ((ConstantExpression)query.Expression).Value;
                if (value is IQueryable)
                {
                    return (IEnumerable)((IQueryable)value).Provider.Execute(query.Expression);
                }
                return (IEnumerable)((ConstantExpression)query.Expression).Value;
            }

            return (IEnumerable)query.Provider.Execute(query.Expression);
        }

        /// <summary>
        /// Executes the typed query.
        /// </summary>
        /// <typeparam name="T">The entity type of the query.</typeparam>
        /// <param name="query">he query to be executed.</param>
        /// <returns>A IEnumerable&lt;T&gt; that contains a collection of entity objects returned by the query.</returns>
        public static IEnumerable<T> Execute<T>(this IQueryable<T> query)
        {
            if (TypeUtility.EnumerableQueryType.IsInstanceOfType(query))
                return query.ToArray();

            return query.Provider.Execute<IEnumerable<T>>(query.Expression);
        }

        /// <summary>
        /// Creates a IQueryable&lt;T&gt; in the current object context by using the specified query string.
        /// </summary>
        /// <typeparam name="T">The entity type of the returned IQueryable&lt;T&gt;</typeparam>
        /// <param name="db">The dataContext execute the query string.</param>
        /// <param name="esql">The query string to be executed.</param>
        /// <param name="parameters">Parameters to pass to the query.</param>
        /// <returns>A IQueryable&lt;T&gt; of the specified type.</returns>
        public static IQueryable<T> CreateQuery<T>(this DataContext db, string esql, params ObjectParameter[] parameters)
        {
            var c = new ObjectParameterCollection();
            if (parameters != null)
                foreach (var p in parameters)
                    c.Add(p);

            return CreateQuery<T>(db, esql, c);
        }

        /// <summary>
        /// Creates a IQueryable&lt;T&gt; in the current object context by using the specified query string.
        /// </summary>
        /// <param name="db">The dataContext execute the query string.</param>
        /// <param name="esql">The query string to be executed.</param>
        /// <param name="parameters">Parameters to pass to the query.</param>
        /// <returns></returns>
        public static IQueryable CreateQuery(this DataContext db, string esql, params ObjectParameter[] parameters)
        {
            var c = new ObjectParameterCollection();
            if (parameters != null)
                foreach (var p in parameters)
                    c.Add(p);

            return CreateQuery(db, esql, c);
        }

        private static IQueryable CreateQuery(DataContext db, string esql, IEnumerable<ObjectParameter> parameters)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            if (string.IsNullOrEmpty(esql))
                throw new ArgumentNullException("esql");

            var parser = new QueryParser(db, esql,  parameters);
            var q = parser.Parse(null);
            return q;
        }

        private static IQueryable<T> CreateQuery<T>(this DataContext db, string esql, IEnumerable<ObjectParameter> parameters)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            if (string.IsNullOrEmpty(esql))
                throw new ArgumentNullException("esql");

            var parser = new QueryParser(db, esql, parameters);
            var q = parser.Parse(typeof(T));

            Debug.Assert(q != null);

            if (q.ElementType == typeof(T))
                return (IQueryable<T>)q;

            if (TypeUtility.EnumerableQueryType.IsInstanceOfType(q))
            {
                var items = Enumerable.Cast<T>(q);
                var q1 = TypeUtility.CreateGenericEnumerableQueryInstance<T>(items);
                return q1;
            }

            if (typeof(ExpressionQueryProvider).IsInstanceOfType(q.Provider))
            {
                var q1 = ((ExpressionQueryProvider)q.Provider).Cast<T>(q);
                return q1;
            }

            if (!TypeUtility.IsCompatibleWith(q.ElementType, typeof(T)))
                throw Error.InvalidQueryCast(q.ElementType, typeof(T));

            return q.Cast<T>();
        }

        /// <summary>
        /// Creates a IQueryable&lt;T&gt; in the current object context by using the specified query string.
        /// </summary>
        /// <typeparam name="T">The entity type of the returned IQueryable&lt;T&gt;</typeparam>
        /// <param name="db">The dataContext execute the query string.</param>
        /// <param name="esql">The query string to be executed.</param>
        /// <param name="parameters">Parameters to pass to the query.</param>
        /// <returns>A IQueryable&lt;T&gt; of the specified type.</returns>
        public static IQueryable<T> CreateQuery<T>(this DataContext db, string esql, params object[] parameters)
        {
            var c = new ObjectParameterCollection();
            if (parameters != null)
                foreach (var p in parameters)
                    c.Add(p);

            return CreateQuery<T>(db, esql, c);
        }

        /// <summary>
        /// Creates a IQueryable&lt;T&gt; in the current object context by using the specified query string.
        /// </summary>
        /// <param name="db">The dataContext execute the query string.</param>
        /// <param name="esql">The query string to be executed.</param>
        /// <param name="parameters">Parameters to pass to the query.</param>
        /// <returns>A IQueryable of objects.</returns>
        public static IQueryable CreateQuery(this DataContext db, string esql, params object[] parameters)
        {
            var c = new ObjectParameterCollection();
            if (parameters != null)
                foreach (var p in parameters)
                    c.Add(p);

            return CreateQuery(db, esql, c);
        }

        /// <summary>
        /// Creates a IQueryable&lt;T&gt; in the current object context by using the specified query string.
        /// </summary>
        /// <param name="db">The dataContext execute the query string.</param>
        /// <param name="esql">The query string to be executed.</param>
        /// <returns>A IQueryable&lt;T&gt; of the specified type.</returns>
        public static IQueryable<T> CreateQuery<T>(this DataContext db, string esql)
        {
            return CreateQuery<T>(db, esql, null);
        }

        /// <summary>
        /// Creates a IQueryable&lt;T&gt; in the current object context by using the specified query string.
        /// </summary>
        /// <param name="db">The dataContext execute the query string.</param>
        /// <param name="esql">The query string to be executed.</param>
        /// <returns>A IQueryable of objects.</returns>
        public static IQueryable CreateQuery(this DataContext db, string esql)
        {
            return CreateQuery(db, esql, null);
        }

        private static Type[] ScalarTypes = new[]
        {
            typeof(bool), typeof(char), typeof(string), typeof(sbyte), 
			typeof(byte), typeof(short), typeof(ushort), typeof(int), 
			typeof(uint), typeof(long), typeof(ulong), typeof(float), 
			typeof(double), typeof(decimal), typeof(Guid)
        };

        internal static T Treat<T>(object o)
        {
            if (o is T)
                return (T)o;

            return default(T);
        }

    }
}