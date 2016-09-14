using System;
using System.Collections.Generic;
using System.Reflection;

namespace ALinq.Mapping
{
    internal class DataContextMapping
    {
        private Dictionary<Type, EntityMapping> entityMappings = new Dictionary<Type, EntityMapping>();
        private Dictionary<MemberInfo, FunctionAttribute> functions = new Dictionary<MemberInfo, FunctionAttribute>();
        private Dictionary<MemberInfo, ResultTypeAttribute[]> resultTypeAttributes = new Dictionary<MemberInfo, ResultTypeAttribute[]>();

        public DataContextMapping(Type contextType)
        {
            if (contextType == null)
                throw Error.ArgumentNull("contextType");

            if (contextType != typeof(DataContext) && !contextType.IsSubclassOf(typeof(DataContext)))
                throw SqlClient.Error.ArgumentWrongType("contextType", typeof(DataContext), contextType);

            this.DataContextType = contextType;
        }

        public Type DataContextType { get; private set; }

        public DatabaseAttribute Database { get; set; }

        public ProviderAttribute Provider
        {
            get;
            set;
        }

        public EntityMapping GetEntityMapping(Type type)
        {
            EntityMapping entityMapping;
            if (!entityMappings.TryGetValue(type, out entityMapping))
                return null;

            return entityMapping;
        }

        public EntityMapping<TEntity> GetEntityMapping<TEntity>() where TEntity : class
        {
            EntityMapping entityMapping;
            if (!entityMappings.TryGetValue(typeof(TEntity), out entityMapping))
                return null;

            return (EntityMapping<TEntity>)entityMapping;
        }

        internal void Add(EntityMapping entityMapping)
        {
            entityMappings[entityMapping.EntityType] = entityMapping;
        }

        public void Add(MemberInfo mi, FunctionAttribute function)
        {
            functions[mi] = function;
        }

        public void Add(MemberInfo mi, FunctionAttribute function, ResultTypeAttribute[] resultTypes)
        {
            functions[mi] = function;
            this.resultTypeAttributes[mi] = resultTypes;
        }

        public FunctionAttribute GetFunction(MemberInfo mi)
        {
            FunctionAttribute function;
            if (functions.TryGetValue(mi, out function))
                return function;

            return null;
        }

        internal ResultTypeAttribute[] GetResultTypes(MemberInfo mi)
        {
            ResultTypeAttribute[] result;
            if (resultTypeAttributes.TryGetValue(mi, out result))
                return result;

            return null;
        }

        public IEnumerable<EntityMapping> EntityMappings
        {
            get { return entityMappings.Values; }
        }

        public IEnumerable<FunctionAttribute> Functions
        {
            get { return functions.Values; }
        }
    }

    internal class DataContextMapping<T> : DataContextMapping where T : DataContext
    {
        public DataContextMapping()
            : this(null)
        {
        }

        private DataContextMapping(Type providerType)
            : base(typeof(T))
        {
            if (providerType != null)
                base.Provider = new ProviderAttribute(providerType);
        }

        public void Add<TEntity>(EntityMapping<TEntity> entityMapping) where TEntity : class
        {
            base.Add(entityMapping);
        }
    }
}