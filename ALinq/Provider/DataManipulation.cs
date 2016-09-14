using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ALinq.Mapping;
using System.Linq;
using System.Text;

namespace ALinq.Provider
{
    internal static partial class DataManipulation
    {
        // Methods
        public static int Delete<TEntity>(Type type, Func<TEntity, bool> check) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public static int Delete<TEntity>(TEntity item)
        {
            throw new NotImplementedException();
        }


        public static int Delete<TEntity>(TEntity item, Func<TEntity, bool> check)
        {
            throw new NotImplementedException();
        }

        //public static int Delete<TEntity>(TEntity item, Func<TEntity, bool> check, bool disablePrimaryKey)
        //{
        //    throw new NotImplementedException();
        //}

        public static int Insert<TEntity, TResult>(Func<TEntity, TResult> entity, Expression resultSelector)
        {
            throw new NotImplementedException();
        }

        public static int Insert<TEntity, TResult>(Func<TEntity, TResult> entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public static int Insert<TEntity, TResult>(ITable table, Func<TEntity, TResult> entity, object EMPTY) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public static int Insert<TEntity>(TEntity item)
        {
            throw new NotImplementedException();
        }


        public static TResult Insert<TEntity, TResult>(TEntity item, Func<TEntity, TResult> resultSelector)
        {
            throw new NotImplementedException();
        }

        public static int Update<TEntity>(TEntity item)
        {
            throw new NotImplementedException();
        }

        public static int Update<TEntity>(TEntity item, Func<TEntity, bool> check)
        {
            throw new NotImplementedException();
        }

        public static int Update<TEntity, TResult>(Func<TEntity, TResult> entity, Func<TEntity, bool> check)
        {
            throw new NotImplementedException();
        }

        public static int Update<TEntity, TResult>(ITable table, Expression<Func<TEntity, TResult>> entity, Expression<Func<TEntity, bool>> check)
            where TEntity : class
        {
            throw new NotImplementedException();
        }


        public static int Update<TEntity>(TEntity item, Func<TEntity, bool> check, bool disablePrimaryKey)
        {
            throw new NotImplementedException();
        }

        public static TResult Update<TEntity, TResult>(TEntity item, Func<TEntity, TResult> resultSelector)
        {
            throw new NotImplementedException();
        }

        public static TResult Update<TEntity, TResult>(TEntity item, Func<TEntity, bool> check)
        {
            throw new NotImplementedException();
        }

        public static TResult Update<TEntity, TResult>(TEntity item, Func<TEntity, bool> check, Func<TEntity, TResult> resultSelector)
        {
            throw new NotImplementedException();
        }

        public static int Update<TEntity>(Expression<Func<TEntity, TEntity>> item)
        {
            throw new NotImplementedException();
        }

    }
}