using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq
{
    internal class CommonDataServices : IDataServices
    {

        // Fields
        private Dictionary<MetaDataMember, IDeferredSourceFactory> factoryMap;

        // Methods
        internal CommonDataServices(DataContext context, MetaModel model)
        {

            Context = context;
            Model = model;
            bool asReadOnly = !context.ObjectTrackingEnabled;
            IdentityManager = IdentityManager.CreateIdentityManager(asReadOnly);
            ChangeTracker = ChangeTracker.CreateChangeTracker(this, asReadOnly);
            ChangeDirector = ChangeDirector.CreateChangeDirector(context);
            factoryMap = new Dictionary<MetaDataMember, IDeferredSourceFactory>();
        }

        private static Expression[] BuildKeyExpressions(object[] keyValues, ReadOnlyCollection<MetaDataMember> keyMembers)
        {
            var expressionArray = new Expression[keyValues.Length];
            int index = 0;
            int count = keyMembers.Count;
            while (index < count)
            {
                MetaDataMember member = keyMembers[index];
                expressionArray[index] = Expression.Constant(keyValues[index], member.Type);
                index++;
            }
            return expressionArray;
        }

        public object GetCachedObject(Expression query)
        {
            if (query != null)
            {
                string str;
                var expression = query as MethodCallExpression;
                if ((expression == null) || (expression.Arguments.Count != 2))
                {
                    return null;
                }
                if ((expression.Method.DeclaringType != typeof(Queryable)) && (((str = expression.Method.Name) == null) || ((((str != "Where") && (str != "First")) && ((str != "FirstOrDefault") && (str != "Single"))) && (str != "SingleOrDefault"))))
                {
                    return null;
                }
                var expression2 = expression.Arguments[1] as UnaryExpression;
                if ((expression2 == null) || (expression2.NodeType != ExpressionType.Quote))
                {
                    return null;
                }
                var operand = expression2.Operand as LambdaExpression;
                if (operand != null)
                {
                    var expression4 = expression.Arguments[0] as ConstantExpression;
                    if (expression4 == null)
                    {
                        return null;
                    }
                    var table = expression4.Value as ITable;
                    if (table == null)
                    {
                        return null;
                    }
                    if (TypeSystem.GetElementType(query.Type) != table.ElementType)
                    {
                        return null;
                    }
                    MetaTable table2 = Model.GetTable(table.ElementType);
                    object[] keyValues = GetKeyValues(table2.RowType, operand);
                    if (keyValues != null)
                    {
                        return GetCachedObject(table2.RowType, keyValues);
                    }
                }
            }
            return null;
        }

        internal object GetCachedObject(MetaType type, object[] keyValues)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (!type.IsEntity)
            {
                return null;
            }
            return IdentityManager.Find(type, keyValues);
        }

        internal object GetCachedObjectLike(MetaType type, object instance)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (!type.IsEntity)
            {
                return null;
            }
            return IdentityManager.FindLike(type, instance);
        }

        internal IEnumerable<RelatedItem> GetChildren(MetaType type, object item)
        {
            return GetRelations(type, item, false);
        }

        internal Expression GetDataMemberQuery(MetaDataMember member, Expression[] keyValues)
        {
            if (member == null)
            {
                throw Error.ArgumentNull("member");
            }
            if (keyValues == null)
            {
                throw Error.ArgumentNull("keyValues");
            }
            if (member.IsAssociation)
            {
                MetaAssociation association = member.Association;
                Type type = association.ThisMember.DeclaringType.InheritanceRoot.Type;
                Expression source = Expression.Constant(Context.GetTable(type));
                if (type != association.ThisMember.DeclaringType.Type)
                {
                    source = Expression.Call(typeof(Enumerable), "Cast", new[] { association.ThisMember.DeclaringType.Type }, new[] { source });
                }
                Expression thisInstance = Expression.Call(typeof(Enumerable), "FirstOrDefault", new[] { association.ThisMember.DeclaringType.Type }, new[] { Translator.WhereClauseFromSourceAndKeys(source, association.ThisKey.ToArray(), keyValues) });
                Expression otherSource = Expression.Constant(Context.GetTable(association.OtherType.InheritanceRoot.Type));
                if (association.OtherType.Type != association.OtherType.InheritanceRoot.Type)
                {
                    otherSource = Expression.Call(typeof(Enumerable), "Cast", new[] { association.OtherType.Type }, new[] { otherSource });
                }
                return Translator.TranslateAssociation(Context, association, otherSource, keyValues, thisInstance);
            }
            Expression objectQuery = GetObjectQuery(member.DeclaringType, keyValues);
            Type elementType = TypeSystem.GetElementType(objectQuery.Type);
            ParameterExpression expression6 = Expression.Parameter(elementType, "p");
            Expression expression7 = expression6;
            if (elementType != member.DeclaringType.Type)
            {
                expression7 = Expression.Convert(expression7, member.DeclaringType.Type);
            }
            Expression body = (member.Member is PropertyInfo) ? Expression.Property(expression7, (PropertyInfo)member.Member) : Expression.Field(expression7, (FieldInfo)member.Member);
            LambdaExpression expression9 = Expression.Lambda(body, new[] { expression6 });
            return Expression.Call(typeof(Queryable), "Select", new[] { elementType, expression9.Body.Type }, new[] { objectQuery, expression9 });
        }

        public IDeferredSourceFactory GetDeferredSourceFactory(MetaDataMember member)
        {
            IDeferredSourceFactory factory;
            if (member == null)
            {
                throw Error.ArgumentNull("member");
            }
            if (!factoryMap.TryGetValue(member, out factory))
            {
                Type type = (member.IsAssociation && member.Association.IsMany) ? TypeSystem.GetElementType(member.Type) : member.Type;
                factory = (IDeferredSourceFactory)Activator.CreateInstance(typeof(DeferredSourceFactory<>).MakeGenericType(new[] { type }), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { member, this }, null);
                factoryMap.Add(member, factory);
            }
            return factory;
        }

        internal static object[] GetForeignKeyValues(MetaAssociation association, object instance)
        {
            var list = new List<object>();
            foreach (MetaDataMember member in association.ThisKey)
            {
                list.Add(member.MemberAccessor.GetBoxedValue(instance));
            }
            return list.ToArray();
        }

        private static bool GetKeyFromPredicate(MetaType type, IDictionary<MetaDataMember, object> keys, Expression mex, Expression vex)
        {
            var expression = mex as MemberExpression;
            if ((((expression != null) && (expression.Expression != null)) && ((expression.Expression.NodeType == ExpressionType.Parameter) && (expression.Expression.Type == type.Type))) && (type.Type.IsAssignableFrom(expression.Member.ReflectedType) || expression.Member.ReflectedType.IsAssignableFrom(type.Type)))
            {
                MetaDataMember dataMember = type.GetDataMember(expression.Member);
                if (!dataMember.IsPrimaryKey)
                {
                    return false;
                }
                if (keys.ContainsKey(dataMember))
                {
                    return false;
                }
                var expression2 = vex as ConstantExpression;
                if (expression2 != null)
                {
                    keys.Add(dataMember, expression2.Value);
                    return true;
                }
            }
            return false;
        }

        private static bool GetKeysFromPredicate(MetaType type, IDictionary<MetaDataMember, object> keys, Expression expr)
        {
            var expression = expr as BinaryExpression;
            if (expression == null)
            {
                var expression2 = expr as MethodCallExpression;
                if (((expression2 == null) || (expression2.Method.Name != "op_Equality")) || (expression2.Arguments.Count != 2))
                {
                    return false;
                }
                expression = Expression.Equal(expression2.Arguments[0], expression2.Arguments[1]);
            }
            ExpressionType nodeType = expression.NodeType;
            if (nodeType != ExpressionType.And)
            {
                if (nodeType != ExpressionType.Equal)
                {
                    return false;
                }
            }
            else
            {
                return (GetKeysFromPredicate(type, keys, expression.Left) && GetKeysFromPredicate(type, keys, expression.Right));
            }
            if (!GetKeyFromPredicate(type, keys, expression.Left, expression.Right))
            {
                return GetKeyFromPredicate(type, keys, expression.Right, expression.Left);
            }
            return true;
        }

        internal static object[] GetKeyValues(MetaType type, LambdaExpression predicate)
        {
            if (predicate == null)
            {
                throw Error.ArgumentNull("predicate");
            }
            if (predicate.Parameters.Count != 1)
            {
                return null;
            }
            var keys = new Dictionary<MetaDataMember, object>();
            if (!GetKeysFromPredicate(type, keys, predicate.Body) || (keys.Count != type.IdentityMembers.Count))
            {
                return null;
            }
            return keys.OrderBy(delegate(KeyValuePair<MetaDataMember, object> kv)
            {
                return kv.Key.Ordinal;
            }).Select(delegate(KeyValuePair<MetaDataMember, object> kv)
            {
                return kv.Value;
            }).ToArray();
        }

        internal static object[] GetKeyValues(MetaType type, object instance)
        {
            var list = new List<object>();
            foreach (MetaDataMember member in type.IdentityMembers)
            {
                list.Add(member.MemberAccessor.GetBoxedValue(instance));
            }
            return list.ToArray();
        }

        internal object GetObjectByKey(MetaType type, object[] keyValues)
        {
            object cachedObject = GetCachedObject(type, keyValues);
            if (cachedObject == null)
            {
                cachedObject = ((IEnumerable)Context.Provider.Execute(GetObjectQuery(type, keyValues)).ReturnValue).OfType<object>().SingleOrDefault();
            }
            return cachedObject;
        }

        internal Expression GetObjectQuery(MetaType type, Expression[] keyValues)
        {
            //TODO:当 keyValues.Count 为 0 ，抛出异常不存在主键

            ITable table = Context.GetTable(type.InheritanceRoot.Type);
            ParameterExpression expression = Expression.Parameter(table.ElementType, "p");
            Expression left = null;
            int index = 0;
            int count = type.IdentityMembers.Count;
            while (index < count)
            {
                MetaDataMember member = type.IdentityMembers[index];
                Expression expression3 = (member.Member is FieldInfo) ? Expression.Field(expression, (FieldInfo)member.Member) : Expression.Property(expression, (PropertyInfo)member.Member);
                Expression right = Expression.Equal(expression3, keyValues[index]);
                left = (left != null) ? Expression.And(left, right) : right;
                index++;
            }

            return Expression.Call(typeof(Queryable), "Where", new[] { table.ElementType }, new[] { table.Expression, Expression.Lambda(left, new[] { expression }) });
        }

        internal Expression GetObjectQuery(MetaType type, object[] keyValues)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (keyValues == null)
            {
                throw Error.ArgumentNull("keyValues");
            }
            return GetObjectQuery(type, BuildKeyExpressions(keyValues, type.IdentityMembers));
        }

        internal IEnumerable<RelatedItem> GetParents(MetaType type, object item)
        {
            return GetRelations(type, item, true);
        }

        private static IEnumerable<RelatedItem> GetRelations(MetaType type, object item, bool isForeignKey)
        {
            foreach (var mm in type.PersistentDataMembers)
            {
                if (mm.IsAssociation)
                {
                    var otherType = mm.Association.OtherType;
                    if (mm.Association.IsForeignKey == isForeignKey)
                    {
                        object value;
                        if (mm.IsDeferred)
                        {
                            value = mm.DeferredValueAccessor.GetBoxedValue(item);
                        }
                        else
                        {
                            value = mm.StorageAccessor.GetBoxedValue(item);
                        }
                        if (value != null)
                        {
                            if (mm.Association.IsMany)
                            {
                                foreach (var o in (IEnumerable)value)
                                {
                                    yield return new RelatedItem(otherType.GetInheritanceType(o.GetType()), o);
                                }
                            }
                            else
                            {
                                yield return new RelatedItem(otherType.GetInheritanceType(value.GetType()), value);
                            }
                        }
                    }
                }
            }
        }

        public object InsertLookupCachedObject(MetaType type, object instance)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            HasCachedObjects = true;
            if (!type.IsEntity)
            {
                return instance;
            }
            return IdentityManager.InsertLookup(type, instance);
        }

        public bool IsCachedObject(MetaType type, object instance)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (!type.IsEntity)
            {
                return false;
            }
            return (IdentityManager.FindLike(type, instance) == instance);
        }

        public void OnEntityMaterialized(MetaType type, object instance)
        {
#if DEBUG
            //Console.WriteLine("CommandDataServices.OnEntityMaterialized");
#endif
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            ChangeTracker.FastTrack(instance);
            if (type.HasAnySetDataContextMethod)
            {
                SendSetDataContext(type, instance, new object[] { Context });
            }
            if (type.HasAnyLoadMethod)
            {
                SendOnLoaded(type, instance);
            }
        }

        public bool RemoveCachedObjectLike(MetaType type, object instance)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (!type.IsEntity)
            {
                return false;
            }
            return IdentityManager.RemoveLike(type, instance);
        }

        internal void ResetServices()
        {
            HasCachedObjects = false;
            bool asReadOnly = !Context.ObjectTrackingEnabled;
            IdentityManager = IdentityManager.CreateIdentityManager(asReadOnly);
            ChangeTracker = ChangeTracker.CreateChangeTracker(this, asReadOnly);
            factoryMap = new Dictionary<MetaDataMember, IDeferredSourceFactory>();
        }

        private void SendOnLoaded(MetaType type, object item)
        {
            if (type != null)
            {
                SendOnLoaded(type.InheritanceBase, item);
                if (type.OnLoadedMethod != null)
                {
                    try
                    {
                        type.OnLoadedMethod.Invoke(item, new object[0]);
                    }
                    catch (TargetInvocationException exception)
                    {
                        if (exception.InnerException != null)
                        {
                            throw exception.InnerException;
                        }
                        throw;
                    }
                }
            }
        }

        public void SendSetDataContext(MetaType type, object item, object[] args)
        {
            if (type != null)
            {
                SendSetDataContext(type.InheritanceBase, item, args);
                if (type.SetDataContextMehod != null)
                {
                    try
                    {
                        type.SetDataContextMehod.Invoke(item, args);
                    }
                    catch (TargetInvocationException exception)
                    {
                        if (exception.InnerException != null)
                        {
                            throw exception.InnerException;
                        }
                        throw;
                    }
                }
            }
        }

        internal void SetModel(MetaModel model)
        {
            Model = model;
        }

        // Properties
        internal ChangeDirector ChangeDirector { get; private set; }

        internal ChangeTracker ChangeTracker { get; private set; }

        public DataContext Context { get; private set; }

        internal bool HasCachedObjects { get; private set; }

        internal IdentityManager IdentityManager { get; private set; }

        public MetaModel Model { get; private set; }

        // Nested Types


        private class DeferredSourceFactory<T> : IDeferredSourceFactory
        {
            // Fields
            private readonly T[] empty;
            private readonly MetaDataMember member;
            private ICompiledQuery query;
            private readonly bool refersToPrimaryKey;
            private readonly CommonDataServices services;

            // Methods
            internal DeferredSourceFactory(MetaDataMember member, CommonDataServices services)
            {
                this.member = member;
                this.services = services;
                refersToPrimaryKey = this.member.IsAssociation && this.member.Association.OtherKeyIsPrimaryKey;
                empty = new T[0];
            }

            public IEnumerable CreateDeferredSource(object instance)
            {
                if (instance == null)
                {
                    throw Error.ArgumentNull("instance");
                }
                return new DeferredSource<T>(this, instance);
            }

            public IEnumerable CreateDeferredSource(object[] keyValues)
            {
                if (keyValues == null)
                {
                    throw Error.ArgumentNull("keyValues");
                }
                return new DeferredSource<T>(this, keyValues);
            }

            private IEnumerator<T> Execute(object instance)
            {
                T local;
                var thisKey = member.IsAssociation ? member.Association.ThisKey : member.DeclaringType.IdentityMembers;
                var keyValues = new object[thisKey.Count];
                int index = 0;
                int count = thisKey.Count;
                while (index < count)
                {
                    keyValues[index] = thisKey[index].StorageAccessor.GetBoxedValue(instance);
                    index++;
                }
                if (HasNullForeignKey(keyValues))
                {
                    return ((IEnumerable<T>)empty).GetEnumerator();
                }
                if (TryGetCachedObject(keyValues, out local))
                {
                    return ((IEnumerable<T>)new[] { local }).GetEnumerator();
                }
                if (member.LoadMethod != null)
                {
                    try
                    {
                        object obj3 = member.LoadMethod.Invoke(services.Context, new[] { instance });
                        if (typeof(T).IsAssignableFrom(member.LoadMethod.ReturnType))
                        {
                            return ((IEnumerable<T>)new[] { ((T)obj3) }).GetEnumerator();
                        }
                        return ((IEnumerable<T>)obj3).GetEnumerator();
                    }
                    catch (TargetInvocationException exception)
                    {
                        if (exception.InnerException != null)
                        {
                            throw exception.InnerException;
                        }
                        throw;
                    }
                }
                return ExecuteKeyQuery(keyValues);
            }

            private IEnumerator<T> ExecuteKeyQuery(ICollection<object> keyValues)
            {
                if (query == null)
                {
                    ParameterExpression expression = null;
                    var expressionArray = new Expression[keyValues.Count];
                    ReadOnlyCollection<MetaDataMember> onlys = member.IsAssociation ? member.Association.OtherKey : member.DeclaringType.IdentityMembers;
                    int index = 0;
                    int length = keyValues.Count;
                    while (index < length)
                    {
                        MetaDataMember m = onlys[index];
                        expressionArray[index] = Expression.Convert(Expression.ArrayIndex(expression = Expression.Parameter(typeof(object[]), "keys"), Expression.Constant(index)), m.Type);
                        index++;
                    }
                    LambdaExpression q = Expression.Lambda(services.GetDataMemberQuery(member, expressionArray), new[] { expression });
                    query = services.Context.Provider.Compile(q);
                }
                return ((IEnumerable<T>)query.Execute(services.Context.Provider, new object[] { keyValues }).ReturnValue).GetEnumerator();
            }

            private IEnumerator<T> ExecuteKeys(object[] keyValues)
            {
                T local;
                if (HasNullForeignKey(keyValues))
                {
                    return ((IEnumerable<T>)empty).GetEnumerator();
                }
                if (TryGetCachedObject(keyValues, out local))
                {
                    return ((IEnumerable<T>)new[] { local }).GetEnumerator();
                }
                return ExecuteKeyQuery(keyValues);
            }

            private bool HasNullForeignKey(object[] keyValues)
            {
                if (refersToPrimaryKey)
                {
                    bool flag = false;
                    int index = 0;
                    int length = keyValues.Length;
                    while (index < length)
                    {
                        flag |= keyValues[index] == null;
                        index++;
                    }
                    if (flag)
                    {
                        return true;
                    }
                }
                return false;
            }

            private bool TryGetCachedObject(object[] keyValues, out T cached)
            {
                cached = default(T);
                if (refersToPrimaryKey)
                {
                    MetaType type = member.IsAssociation ? member.Association.OtherType : member.DeclaringType;
                    object cachedObject = services.GetCachedObject(type, keyValues);
                    if (cachedObject != null)
                    {
                        cached = (T)cachedObject;
                        return true;
                    }
                }
                return false;
            }

            // Nested Types
            private class DeferredSource<G> : IEnumerable<G>
            {
                // Fields
                private readonly DeferredSourceFactory<G> factory;
                private readonly object instance;

                // Methods
                internal DeferredSource(DeferredSourceFactory<G> factory, object instance)
                {
                    this.factory = factory;
                    this.instance = instance;
                }

                public IEnumerator<G> GetEnumerator()
                {
                    object[] objects = this.instance as object[];
                    if (objects != null)
                    {
                        return factory.ExecuteKeys(objects);
                    }
                    return factory.Execute(this.instance);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }
        }
    }
}
