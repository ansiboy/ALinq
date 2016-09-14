using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ALinq.Provider;
using System.Reflection;
using ALinq.Mapping;

namespace ALinq
{
    partial class Table<TEntity>
    {
        public int Delete(Expression<Func<TEntity, bool>> predicate)
        {
            return EntityManipulation.Delete(this, predicate);
        }

        public int Update<TResult>(Expression<Func<TEntity, TResult>> entity,
                                   Expression<Func<TEntity, bool>> predicate)
        {
            return EntityManipulation.Update<TEntity, TResult>(this, entity, predicate);
        }

        public int Insert<TResult>(Expression<Func<TEntity, TResult>> entity)
        {
            return EntityManipulation.Insert(this, entity);
        }

        //this Table<TEntity> table,
        //public T Insert<T>(Expression<Func<TEntity, TEntity>> entity)
        //{
        //    var table = this;
        //    return EntityManipulation.Insert<T>(table, entity);
        //}

    }

    internal static class EntityManipulation
    {
        internal static int Delete(this ITable table, Expression predicate)
        {
            var entityType = table.ElementType;

            var Context = table.Context;
            var inheritanceType = Context.Mapping.GetMetaType(entityType);
            var inheritanceRoot = inheritanceType.InheritanceRoot;
            var exp = Expression.Call(typeof(DataManipulation), "Delete", new[] { inheritanceRoot.Type },
                                      Expression.Constant(entityType), predicate);

            var result = (int)Context.Provider.Execute(exp).ReturnValue;
            return result;
        }

        internal static int Update<TEntity, TResult>(this ITable table,
                                                     Expression<Func<TEntity, TResult>> entity,
                                                     Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            var Context = table.Context;
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            var entityType = table.ElementType;
            var resutlType = typeof(object);

            var inheritanceType = Context.Mapping.GetMetaType(entityType);
            var inheritanceRoot = inheritanceType.InheritanceRoot;

            Expression<Func<int>> l = () => DataManipulation.Update<TEntity, TResult>(table, entity, predicate);
            var m = ((MethodCallExpression)l.Body).Method;
            //var exp = Expression.Call(typeof(DataManipulation), "Update", new[] { inheritanceRoot.Type, resutlType },
            //                          Expression.Constant(table), entity, predicate);
            var exp = Expression.Call(m, Expression.Constant(table), entity, predicate);
            var result = (int)Context.Provider.Execute(exp).ReturnValue;
            return result;
        }

        internal static int Insert(this ITable table, Expression entity)
        {
            return Insert<int>(table, entity);
        }


        internal static T Insert<T>(this ITable table, Expression entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            var entityType = table.ElementType;
            var resutlType = typeof(object);

            var Context = table.Context;
            var inheritanceType = Context.Mapping.GetMetaType(entityType);
            var inheritanceRoot = inheritanceType.InheritanceRoot;

            object result;
            if (entity.NodeType == ExpressionType.Lambda && ((LambdaExpression)entity).Body.Type == entityType)
            {
                var exp = Expression.Call(typeof(DataManipulation), "Insert", new[] { inheritanceRoot.Type, resutlType }, entity);
                result = (int)Context.Provider.Execute(exp).ReturnValue;
            }
            else
            {

                var exp = Expression.Call(typeof(DataManipulation), "Insert",
                                          new[] { inheritanceRoot.Type, resutlType },
                                          Expression.Constant(table), entity, Expression.Constant(null));
                var obj = Context.Provider.Execute(exp).ReturnValue;
                object abc = null;
                if (obj != null)
                    abc = ((IEnumerable<object>)obj).FirstOrDefault();

                result = abc;
            }
            if (result == null)
                return default(T);

            return (T)Convert.ChangeType(result, typeof(T));
        }

        private static MethodInfo GetMethod<T>(Func<T, object> func)
        {
            return null;
        }

    }



}
