using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.Mapping
{
    internal class AttributedMetaModel : MetaModel
    {
        // Fields
        private readonly Type contextType;
        private readonly string dbName;
        private bool initFunctions;
        private bool initStaticTables;
        private readonly ReaderWriterLock @lock = new ReaderWriterLock();
        private readonly MappingSource mappingSource;
        private readonly Dictionary<MetaPosition, MetaFunction> metaFunctions;
        private readonly Dictionary<Type, MetaTable> metaTables;
        private readonly Dictionary<Type, MetaType> metaTypes;
        private readonly Type providerType;
        private ReadOnlyCollection<MetaTable> staticTables;
        private IAttributeProvider attributeProvider;

        // Methods
        internal AttributedMetaModel(MappingSource mappingSource, Type contextType, IAttributeProvider attributeProvider)
        {
            this.attributeProvider = attributeProvider;
            this.mappingSource = mappingSource;
            this.contextType = contextType;
            this.metaTypes = new Dictionary<Type, MetaType>();
            this.metaTables = new Dictionary<Type, MetaTable>();
            this.metaFunctions = new Dictionary<MetaPosition, MetaFunction>();

            /*
            var customAttributes = (ProviderAttribute[])this.contextType.GetCustomAttributes(typeof(ProviderAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length == 1))
            {
                providerType = customAttributes[0].Type;
            }
            //else
            //{
            //    this.providerType = typeof(SqlProvider);
            //}
            */
            Debug.Assert(attributeProvider != null);

            var providerAttribute = this.attributeProvider.GetProviderAttribute(contextType);
            if (providerAttribute != null)
                this.providerType = providerAttribute.Type;

            /*
            var attributeArray2 = (DatabaseAttribute[])this.contextType.GetCustomAttributes(typeof(DatabaseAttribute), false);
            dbName = ((attributeArray2 != null) && (attributeArray2.Length > 0)) ? attributeArray2[0].Name : this.contextType.Name;
            */
            var databaseAttribute = this.attributeProvider.GetDatabaseAttribute(contextType);
            dbName = databaseAttribute != null ? databaseAttribute.Name : contextType.Name;
        }

        public override MetaFunction GetFunction(MethodInfo method)
        {
            if (method == null)
            {
                throw ALinq.Error.ArgumentNull("method");
            }
            InitFunctions();
            MetaFunction function;
            metaFunctions.TryGetValue(new MetaPosition(method), out function);
            return function;
        }

        public override IEnumerable<MetaFunction> GetFunctions()
        {
            InitFunctions();
            return metaFunctions.Values.ToList().AsReadOnly();
        }

        public override MetaType GetMetaType(Type type)
        {
            if (type == null)
            {
                throw ALinq.Error.ArgumentNull("type");
            }
            MetaType type2 = null;
            this.@lock.AcquireReaderLock(-1);
            try
            {
                if (this.metaTypes.TryGetValue(type, out type2))
                {
                    return type2;
                }
            }
            finally
            {
                this.@lock.ReleaseReaderLock();
            }
            MetaTable table = this.GetTable(type);
            if (table != null)
            {
                return table.RowType.GetInheritanceType(type);
            }
            this.InitFunctions();
            this.@lock.AcquireWriterLock(-1);
            try
            {
                if (!metaTypes.TryGetValue(type, out type2))
                {
                    type2 = new UnmappedType(this, type);
                    metaTypes.Add(type, type2);
                }
            }
            finally
            {
                this.@lock.ReleaseWriterLock();
            }
            return type2;
        }

        private Type GetRoot(Type derivedType)
        {
            while ((derivedType != null) && (derivedType != typeof(object)))
            {
                /*
                var customAttributes = (TableAttribute[])derivedType.GetCustomAttributes(typeof(TableAttribute), false);
                if (customAttributes.Length > 0)
                {
                    return derivedType;
                }
                */
                var tableAttribute = this.AttributeProvider.GetTableAttribute(derivedType);
                if (tableAttribute != null)
                    return derivedType;

                derivedType = derivedType.BaseType;
            }
            return null;
        }

        public override MetaTable GetTable(Type rowType)
        {
            MetaTable tableNoLocks;
            if (rowType == null)
            {
                throw ALinq.Error.ArgumentNull("rowType");
            }
            this.@lock.AcquireReaderLock(-1);
            try
            {
                if (this.metaTables.TryGetValue(rowType, out tableNoLocks))
                {
                    return tableNoLocks;
                }
            }
            finally
            {
                this.@lock.ReleaseReaderLock();
            }
            this.@lock.AcquireWriterLock(-1);
            try
            {
                tableNoLocks = this.GetTableNoLocks(rowType);
            }
            finally
            {
                this.@lock.ReleaseWriterLock();
            }
            return tableNoLocks;
        }

        internal MetaTable GetTableNoLocks(Type rowType)
        {
            MetaTable table;
            if (!metaTables.TryGetValue(rowType, out table))
            {
                Type key = GetRoot(rowType) ?? rowType;
                /*
                var customAttributes = (TableAttribute[])key.GetCustomAttributes(typeof(TableAttribute), true);
                if (customAttributes.Length == 0)
                {
                    metaTables.Add(rowType, null);
                    return table;
                }
                */
                var tableAttribute = AttributeProvider.GetTableAttribute(key);
                if (tableAttribute == null)
                {
                    metaTables.Add(rowType, null);
                    return table;
                }

                if (!metaTables.TryGetValue(key, out table))
                {
                    /* table = new AttributedMetaTable(this, customAttributes[0], key); */
                    table = new AttributedMetaTable(this, tableAttribute, key);

                    foreach (MetaType type2 in table.RowType.InheritanceTypes)
                    {
                        metaTables.Add(type2.Type, table);
                    }
                }
                if (table.RowType.GetInheritanceType(rowType) == null)
                {
                    metaTables.Add(rowType, null);
                    return null;
                }
            }
            return table;
        }

        public override IEnumerable<MetaTable> GetTables()
        {
            IEnumerable<MetaTable> enumerable;
            this.InitStaticTables();
            if (this.staticTables.Count > 0)
            {
                return this.staticTables;
            }
            this.@lock.AcquireReaderLock(-1);
            try
            {
                enumerable = this.metaTables.Values.Where(x => (x != null)).Distinct();
            }
            finally
            {
                this.@lock.ReleaseReaderLock();
            }
            return enumerable;
        }

        private void InitFunctions()
        {
            if (!initFunctions)
            {
                @lock.AcquireWriterLock(-1);
                try
                {
                    if (!initFunctions)
                    {
                        if (contextType != typeof(DataContext))
                        {
                            for (Type type = contextType; type != typeof(DataContext); type = type.BaseType)
                            {
                                foreach (MethodInfo info in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                                {
                                    if (IsUserFunction(info))
                                    {
                                        if (info.IsGenericMethodDefinition)
                                            throw Error.InvalidUseOfGenericMethodAsMappedFunction(info.Name);

                                        var key = new MetaPosition(info);
                                        if (!metaFunctions.ContainsKey(key))
                                        {
                                            MetaFunction function = new AttributedMetaFunction(this, info, attributeProvider);
                                            metaFunctions.Add(key, function);
                                            foreach (MetaType type2 in function.ResultRowTypes)
                                                foreach (MetaType type3 in type2.InheritanceTypes)
                                                    if (!metaTypes.ContainsKey(type3.Type))
                                                        metaTypes.Add(type3.Type, type3);
                                        }
                                    }
                                }
                            }
                        }
                        initFunctions = true;
                    }
                }
                finally
                {
                    @lock.ReleaseWriterLock();
                }
            }
        }

        private void InitStaticTables()
        {
            if (!this.initStaticTables)
            {
                this.@lock.AcquireWriterLock(-1);
                try
                {
                    if (!this.initStaticTables)
                    {
                        var collection = new HashSet<MetaTable>();
                        for (Type type = this.contextType; type != typeof(DataContext); type = type.BaseType)
                        {
                            foreach (FieldInfo info in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                            {
                                Type fieldType = info.FieldType;
                                if (fieldType.IsGenericType && (fieldType.GetGenericTypeDefinition() == typeof(Table<>)))
                                {
                                    Type rowType = fieldType.GetGenericArguments()[0];
                                    collection.Add(this.GetTableNoLocks(rowType));
                                }
                            }
                            foreach (PropertyInfo info2 in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                            {
                                Type propertyType = info2.PropertyType;
                                if (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(Table<>)))
                                {
                                    Type type5 = propertyType.GetGenericArguments()[0];
                                    collection.Add(this.GetTableNoLocks(type5));
                                }
                            }
                        }
                        this.staticTables = new List<MetaTable>(collection).AsReadOnly();
                        this.initStaticTables = true;
                    }
                }
                finally
                {
                    this.@lock.ReleaseWriterLock();
                }

            }
        }

        private bool IsUserFunction(MethodInfo mi)
        {
            //return (Attribute.GetCustomAttribute(mi, typeof(FunctionAttribute), false) != null);
            return this.attributeProvider.GetFunction(mi) != null;
        }

        // Properties
        public override Type ContextType
        {
            get
            {
                return this.contextType;
            }
        }

        public override string DatabaseName
        {
            get
            {
                return this.dbName;
            }
        }

        public override MappingSource MappingSource
        {
            get
            {
                return this.mappingSource;
            }
        }

        public override Type ProviderType
        {
            get
            {
                return this.providerType;
            }
        }

        internal IAttributeProvider AttributeProvider
        {
            get { return attributeProvider; }
        }
    }
}