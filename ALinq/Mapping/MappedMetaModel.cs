using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ALinq.SqlClient;
using LinqToSqlShared.Mapping;

namespace ALinq.Mapping
{
    internal class MappedMetaModel : MetaModel
    {
        // Fields
        private Type contextType;
        private bool fullyLoaded;
        private ReaderWriterLock @lock = new ReaderWriterLock();
        private DatabaseMapping mapping;
        private MappingSource mappingSource;
        private Dictionary<MetaPosition, MetaFunction> metaFunctions;
        private Dictionary<Type, MetaTable> metaTables;
        private Dictionary<Type, MetaType> metaTypes;
        private HashSet<Module> modules;
        private Type providerType;
        private Dictionary<string, Type> types;

        // Methods
        internal MappedMetaModel(MappingSource mappingSource, Type contextType, DatabaseMapping mapping)
        {
            this.mappingSource = mappingSource;
            this.contextType = contextType;
            this.mapping = mapping;
            this.modules = new HashSet<Module>();
            this.modules.Add(this.contextType.Module);
            this.metaTypes = new Dictionary<Type, MetaType>();
            this.metaTables = new Dictionary<Type, MetaTable>();
            this.types = new Dictionary<string, Type>();
            if ((this.providerType == null) && !string.IsNullOrEmpty(this.mapping.Provider))
            {
                //var namespaces = new[] { "ALinq.Access", "ALinq.SQLite", "ALinq.MySQL", "ALinq.Oracle", "ALinq.Oracle.Odp", "ALinq.Firebird", typeof(SqlProvider).Namespace };
                //foreach (var ns in namespaces)
                //{
                var ns = typeof(SqlProvider).Namespace;
                this.providerType = this.FindType(this.mapping.Provider, ns);
                //if (providerType != null)
                //    break;
                //}
                if (this.providerType == null)
                {
                    throw Error.ProviderTypeNotFound(this.mapping.Provider);
                }
            }
            //else if (this.providerType == null)
            //{
            //    this.providerType = typeof(SqlProvider);
            //}
            this.Init();
        }

        internal Type FindType(string name)
        {
            return this.FindType(name, this.contextType.Namespace);
        }

        internal Type FindType(string name, string defaultNamespace)
        {
            Type type = null;
            string key = null;
            Type type2;
            this.@lock.AcquireReaderLock(-1);
            try
            {
                if (!this.types.TryGetValue(name, out type))
                {
                    key = name.Contains(".") ? null : (defaultNamespace + "." + name);
                    if ((key == null) || !this.types.TryGetValue(key, out type))
                    {
                        goto Label_0069;
                    }
                }
                return type;
            }
            finally
            {
                this.@lock.ReleaseReaderLock();
            }
        Label_0069:
            type2 = this.SearchForType(name);
            if ((type2 == null) && (key != null))
            {
                type2 = this.SearchForType(key);
            }
            if (type2 != null)
            {
                this.@lock.AcquireWriterLock(-1);
                try
                {
                    if (this.types.TryGetValue(name, out type))
                    {
                        return type;
                    }
                    this.types.Add(name, type2);
                    return type2;
                }
                finally
                {
                    this.@lock.ReleaseWriterLock();
                }
            }
            return null;
        }

        public override MetaFunction GetFunction(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            MetaFunction function = null;
            this.metaFunctions.TryGetValue(new MetaPosition(method), out function);
            return function;
        }

        public override IEnumerable<MetaFunction> GetFunctions()
        {
            return this.metaFunctions.Values;
        }

        public override MetaType GetMetaType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
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

        private MethodInfo GetMethod(string name)
        {
            string str;
            string str2;
            this.GetTypeAndMethod(name, out str, out str2);
            Type type = this.FindType(str);
            if (type != null)
            {
                return type.GetMethod(str2, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            }
            return null;
        }

        private Type GetRootType(Type type, TypeMapping rootMapping)
        {
            if (((string.Compare(rootMapping.Name, type.Name, StringComparison.Ordinal) == 0) || (string.Compare(rootMapping.Name, type.FullName, StringComparison.Ordinal) == 0)) || (string.Compare(rootMapping.Name, type.AssemblyQualifiedName, StringComparison.Ordinal) == 0))
            {
                return type;
            }
            if (type.BaseType == typeof(object))
            {
                throw Error.UnableToResolveRootForType(type);
            }
            return this.GetRootType(type.BaseType, rootMapping);
        }

        public override MetaTable GetTable(Type rowType)
        {
            if (rowType == null)
            {
                throw Error.ArgumentNull("rowType");
            }
            MetaTable table = null;
            this.metaTables.TryGetValue(rowType, out table);
            return table;
        }

        public override IEnumerable<MetaTable> GetTables()
        {
            return this.metaTables.Values.Where(x => (x != null)).Distinct();
        }

        private void GetTypeAndMethod(string name, out string typeName, out string methodName)
        {
            int length = name.LastIndexOf(".", StringComparison.CurrentCulture);
            if (length > 0)
            {
                typeName = name.Substring(0, length);
                methodName = name.Substring(length + 1);
            }
            else
            {
                typeName = this.contextType.FullName;
                methodName = name;
            }
        }

        private void Init()
        {
            if (!this.fullyLoaded)
            {
                this.@lock.AcquireWriterLock(-1);
                try
                {
                    if (!this.fullyLoaded)
                    {
                        this.InitStaticTables();
                        this.InitFunctions();
                        this.fullyLoaded = true;
                    }
                }
                finally
                {
                    this.@lock.ReleaseWriterLock();
                }
            }
        }

        private void InitFunctions()
        {
            this.metaFunctions = new Dictionary<MetaPosition, MetaFunction>();
            if (this.contextType != typeof(DataContext))
            {
                foreach (FunctionMapping mapping in this.mapping.Functions)
                {
                    MethodInfo method = this.GetMethod(mapping.MethodName);
                    if (method == null)
                    {
                        throw Error.MethodCannotBeFound(mapping.MethodName);
                    }
                    MappedFunction function = new MappedFunction(this, mapping, method);
                    this.metaFunctions.Add(new MetaPosition(method), function);
                    foreach (MetaType type in function.ResultRowTypes)
                    {
                        foreach (MetaType type2 in type.InheritanceTypes)
                        {
                            if (!this.metaTypes.ContainsKey(type2.Type))
                            {
                                this.metaTypes.Add(type2.Type, type2);
                            }
                        }
                    }
                }
            }
        }

        private void InitStaticTables()
        {
            this.InitStaticTableTypes();
            foreach (TableMapping mapping in this.mapping.Tables)
            {
                Type type = FindType(mapping.RowType.Name);
                if (type == null)
                {
                    throw Error.CouldNotFindTypeFromMapping(mapping.RowType.Name);
                }
                Type rootType = this.GetRootType(type, mapping.RowType);
                MetaTable table = new MappedTable(this, mapping, rootType);
                foreach (MetaType type3 in table.RowType.InheritanceTypes)
                {
                    metaTypes.Add(type3.Type, type3);
                    metaTables.Add(type3.Type, table);
                }
            }
        }

        private void InitStaticTableTypes()
        {
            for (Type type = this.contextType; type != typeof(DataContext); type = type.BaseType)
            {
                foreach (FieldInfo info in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    Type fieldType = info.FieldType;
                    if (fieldType.IsGenericType && (fieldType.GetGenericTypeDefinition() == typeof(Table<>)))
                    {
                        Type type3 = fieldType.GetGenericArguments()[0];
                        if (!this.types.ContainsKey(type3.Name))
                        {
                            this.types.Add(type3.FullName, type3);
                            if (!this.types.ContainsKey(type3.Name))
                            {
                                this.types.Add(type3.Name, type3);
                            }
                            this.modules.Add(type3.Module);
                        }
                    }
                }
                foreach (PropertyInfo info2 in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    Type propertyType = info2.PropertyType;
                    if (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(Table<>)))
                    {
                        Type type5 = propertyType.GetGenericArguments()[0];
                        if (!this.types.ContainsKey(type5.Name))
                        {
                            this.types.Add(type5.FullName, type5);
                            if (!this.types.ContainsKey(type5.Name))
                            {
                                this.types.Add(type5.Name, type5);
                            }
                            this.modules.Add(type5.Module);
                        }
                    }
                }
            }
        }

        private Type SearchForType(string name)
        {
            Type type = this.SearchForType(name, false);
            if (type != null)
            {
                return type;
            }
            return this.SearchForType(name, true);
        }

        private Type SearchForType(string name, bool ignoreCase)
        {
            Type type = Type.GetType(name, false, ignoreCase);
            if (type != null)
            {
                return type;
            }
            foreach (Module module in this.modules)
            {
                type = module.GetType(name, false, ignoreCase);
                if (type != null)
                {
                    return type;
                }
            }
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Module module2 in assembly.GetLoadedModules())
                {
                    type = module2.GetType(name, false, ignoreCase);
                    if (type != null)
                    {
                        return type;
                    }
                }
            }
            return null;
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
                return this.mapping.DatabaseName;
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
    }
}