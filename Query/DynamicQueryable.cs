using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using ALinq.Dynamic.Parsers;
#if L2S
using System.Data.Linq;
#endif

namespace ALinq.Dynamic
{
    public static class DynamicQueryable
    {
        class Parser : QueryParser
        {
            private IQueryable source;
            private ParameterExpression p;

            public Parser(IQueryable source, string predicate, params object[] parameters)
                : base(GetDataContext(source), predicate, ConvertToObjectParameters(parameters))
            {
                p = ExpressionUtility.CreateParameter(source.ElementType);
                this.source = source;
            }

            static DataContext GetDataContext(IQueryable source)
            {
                var dc = ExpressionUtility.FindContext(source.Expression);
                return dc;
            }

            static IEnumerable<ObjectParameter> ConvertToObjectParameters(object[] values)
            {
                var parameters = new ObjectParameterCollection();
                foreach (var value in values)
                    parameters.Add(value);

                return parameters;
            }



            public IQueryable ParseWhere()
            {
                var expr = base.ParseWhere(source.Expression, this.p);
                return source.Provider.CreateQuery(expr);
            }

            public IQueryable ParseSelect()
            {
                var expr = base.ParseSelection(source.Expression, this.p);
                return source.Provider.CreateQuery(expr);
            }

            public IQueryable ParseOrderBy()
            {
                var expr = base.ParseOrder(source.Expression, this.p);
                return source.Provider.CreateQuery(expr);
            }

            public IQueryable Skip()
            {
                var expr = base.ParseSkip(source.Expression, this.p);
                return source.Provider.CreateQuery(expr);
            }

            public IQueryable Take()
            {
                var expr = base.ParseTake(source.Expression, this.p);
                return source.Provider.CreateQuery(expr);
            }

            protected override Expression ParseIdentifier()
            {
                if (string.Equals("it", Token.Text, StringComparison.CurrentCultureIgnoreCase))
                {
                    NextToken();
                    return p;
                }

                Expression expr;
                if ((expr = this.ParseMemberAccess(p)) != null)
                    return expr;

                return base.ParseIdentifier();
            }



        }

        public static IQueryable<T> Where<T>(this IQueryable<T> source, string predicate, params object[] values)
        {
            return (IQueryable<T>)Where((IQueryable)source, predicate, values);
        }

        public static IQueryable Where(this IQueryable source, string predicate, params object[] values)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (predicate == null)
                throw new ArgumentNullException("predicate");

            var parser = new Parser(source, predicate, values);
            var result = parser.ParseWhere();

            return result;
        }


        public static IQueryable<IDataRecord> Select(this IQueryable source, string selector, params object[] parameters)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (selector == null)
                throw new ArgumentNullException("selector");

            var parser = new Parser(source, selector, parameters);
            var result = parser.ParseSelect();

            return result.Cast<IDataRecord>();
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering, params object[] parameters)
        {
            return (IQueryable<T>)OrderBy((IQueryable)source, ordering, parameters);
        }

        public static IQueryable OrderBy(this IQueryable source, string ordering, params object[] parameters)
        {
            var parser = new Parser(source, ordering, parameters);
            var result = parser.ParseOrderBy();
            return result;
        }

        public static IQueryable<IDataRecord> GroupBy
            (this IQueryable source, string key, string projection, params object[] parameters)
        {
            string itName = "it";
            var parameterNames = parameters.OfType<ObjectParameter>().Select(o => o.Name).ToArray();
            var num = 1;
            while (parameterNames.Contains(itName))
                itName = "it" + num;

            var esql = @"select {1} from @{2} as it
                         group by it.{0}"
                .Replace("{0}", key).Replace("{1}", projection).Replace("{2}", itName);

            var ps = new[] { new ObjectParameter("it", source) };
            var parser = new QueryParser(esql, ps);
            var result = parser.Parse();
            return result.Cast<IDataRecord>();


        }

        public static IQueryable Take(this IQueryable source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Take",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Constant(count)));
        }

        public static IQueryable Skip(this IQueryable source, string count, params object[] parameters)
        {
            var parser = new Parser(source, count, parameters);
            var result = parser.Skip();
            return result;
        }

        public static IQueryable Skip(this IQueryable source, int count, params object[] parameters)
        {
            return Skip(source, count.ToString(), parameters);
        }

        public static IQueryable Take(this IQueryable source, string count, params object[] parameters)
        {
            var parser = new Parser(source, count, parameters);
            var result = parser.Take();
            return result;
        }


        public static IQueryable Take(this IQueryable source, int count, params object[] parameters)
        {
            return Take(source, count.ToString(), parameters);
        }

        public static object SingleOrDefault(this IQueryable source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var types = new[] { source.ElementType };
            var mc = Expression.Call(typeof(Queryable), "SingleOrDefault", types, source.Expression);
            return source.Provider.Execute(mc);
        }

        public static int Count(this IQueryable source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return (int)source.Provider.Execute(Expression.Call(typeof(Queryable), "Count", new[] { source.ElementType }, source.Expression));
        }

        //public static T Single<T>(this IQueryable<T> source, string predicate)
        //{
        //    return (T)Single((IQueryable)source, predicate);
        //}

        #region MyRegion




        //public static object Single(this IQueryable source, string predicate)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    var lambda = DynamicExpression.ParseLambda(source.ElementType, null, predicate);
        //    var types = new[] { source.ElementType };
        //    var mc = Expression.Call(typeof(Queryable), "Single", types, source.Expression, Expression.Quote(lambda));
        //    return source.Provider.Execute(mc);
        //}

        //public static T SingleOrDefault<T>(this IQueryable<T> source, string predicate)
        //{
        //    return (T)SingleOrDefault((IQueryable)source, predicate);
        //}

        //public static object SingleOrDefault(this IQueryable source, string predicate)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    var lambda = DynamicExpression.ParseLambda(source.ElementType, null, predicate);
        //    var types = new[] { source.ElementType };
        //    var mc = Expression.Call(typeof(Queryable), "SingleOrDefault", types, source.Expression, Expression.Quote(lambda));
        //    return source.Provider.Execute(mc);
        //}

        //public static object Sum(this IQueryable source, string predicate)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    var lambda = DynamicExpression.ParseLambda(source.ElementType, null, predicate);
        //    var types = new[] { source.ElementType };
        //    var mc = Expression.Call(typeof(Queryable), "Sum", types, source.Expression, Expression.Quote(lambda));
        //    return source.Provider.Execute(mc);
        //}



        //public static bool Any(this IQueryable source)
        //{
        //    if (source == null) throw new ArgumentNullException("source");
        //    return (bool)source.Provider.Execute(
        //        Expression.Call(
        //            typeof(Queryable), "Any",
        //            new Type[] { source.ElementType }, source.Expression));
        //}

        //public static object Average(this IQueryable source, string predicate)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    var lambda = DynamicExpression.ParseLambda(source.ElementType, null, predicate);
        //    var types = new[] { source.ElementType };
        //    var mc = Expression.Call(typeof(Queryable), "Average", types, source.Expression, Expression.Quote(lambda));
        //    return source.Provider.Execute(mc);
        //}



        //public static object Count(this IQueryable source, string predicate)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    var lambda = DynamicExpression.ParseLambda(source.ElementType, null, predicate);
        //    var types = new[] { source.ElementType };
        //    var mc = Expression.Call(typeof(Queryable), "Count", types, source.Expression, Expression.Quote(lambda));
        //    return source.Provider.Execute(mc);
        //}

        //public static bool Contains<T>(this IQueryable source, T predicate)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    //var lambda = DynamicExpression.ParseLambda(source.ElementType, null, predicate);
        //    var types = new[] { source.ElementType };
        //    var mc = Expression.Call(typeof(Enumerable), "Contains", types, source.Expression, Expression.Constant(predicate));
        //    return (bool)source.Provider.Execute(mc);
        //}


        //public static T First<T>(this IQueryable<T> source, string predicate)
        //{
        //    return (T)First((IQueryable)source, predicate);
        //}

        //public static object First(this IQueryable source, string predicate)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    var lambda = DynamicExpression.ParseLambda(source.ElementType, null, predicate);
        //    var types = new[] { source.ElementType };
        //    var mc = Expression.Call(typeof(Queryable), "First", types, source.Expression, Expression.Quote(lambda));
        //    return source.Provider.Execute(mc);
        //}

        //public static object FirstOrDefault(this IQueryable source)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    var mc = Expression.Call(typeof(Queryable), "FirstOrDefault", new[] { source.ElementType }, source.Expression);
        //    return source.Provider.Execute(mc);
        //}

        //public static T FirstOrDefault<T>(this IQueryable<T> source, string predicate)
        //{
        //    return (T)FirstOrDefault((IQueryable)source, predicate);
        //}

        //public static object FirstOrDefault(this IQueryable source, string predicate)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    var lambda = DynamicExpression.ParseLambda(source.ElementType, null, predicate);
        //    var types = new[] { source.ElementType };
        //    var mc = Expression.Call(typeof(Queryable), "FirstOrDefault", types, source.Expression, Expression.Quote(lambda));
        //    return source.Provider.Execute(mc);
        //}

        //public static object Min(this IQueryable source)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");
        //    return source.Provider.Execute(Expression.Call(typeof(Queryable), "Min",
        //                                                   new[] { source.ElementType }, source.Expression));
        //}

        //public static object Min(this IQueryable source, string predicate)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    var lambda = DynamicExpression.ParseLambda(source.ElementType, null, predicate);
        //    var types = new[] { source.ElementType, lambda.Body.Type };
        //    var mc = Expression.Call(typeof(Queryable), "Min", types, source.Expression, Expression.Quote(lambda));
        //    return source.Provider.Execute(mc);
        //}

        //public static object Max(this IQueryable source)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");
        //    return source.Provider.Execute(Expression.Call(typeof(Queryable), "Max",
        //                                                   new[] { source.ElementType }, source.Expression));
        //}

        //public static object Max(this IQueryable source, string predicate)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    var lambda = DynamicExpression.ParseLambda(source.ElementType, null, predicate);
        //    var types = new[] { source.ElementType, lambda.Body.Type };
        //    var mc = Expression.Call(typeof(Queryable), "Max", types, source.Expression, Expression.Quote(lambda));
        //    return source.Provider.Execute(mc);
        //} 
        #endregion

    }
}
