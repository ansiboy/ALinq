using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using ALinq.Mapping;
using System.ComponentModel;
using ALinq.SqlClient;

namespace ALinq
{
    /// <summary>
    /// Represents the main entry point for the ALINQ framework.
    /// </summary>
    public partial class DataContext : IDisposable
    {
        private Dictionary<MetaTable, ITable> tables;
        private ChangeProcessor changeProcessor;
        private CommonDataServices services;
        private Type providerType;
        private bool disposed;
        private MethodInfo _miExecuteQuery;
        private bool objectTrackingEnabled;
        private bool deferredLoadingEnabled;
        private bool isInSubmitChanges;
        private ChangeConflictCollection conflicts;
        private DataLoadOptions loadOptions;
        private ModuleBuilder moduleBuilder;
        private ExtendTableTypes extendTableTypes;

        internal DataContext(DataContext context, Type providerType)
        {
            this.providerType = providerType;
            objectTrackingEnabled = true;
            deferredLoadingEnabled = true;
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }
            Init(context.Connection, context.Mapping.MappingSource);
            LoadOptions = context.LoadOptions;
            Transaction = context.Transaction;
            Log = context.Log;
            CommandTimeout = context.CommandTimeout;
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.DataContext class by referencing the connection used by the .NET Framework.
        /// </summary>
        /// <param name="connection">The connection used by the .NET Framework.</param>
        public DataContext(IDbConnection connection)
        {
            objectTrackingEnabled = true;
            deferredLoadingEnabled = true;

            if (connection == null)
            {
                throw Error.ArgumentNull("connection");
            }
            InitWithDefaultMapping(connection);
        }


        ///<summary>
        /// Initializes a new instance of the ALinq.DataContext class.
        ///</summary>
        ///<param name="connection">The connection used by the .NET Framework.</param>
        ///<param name="providerType">The provider type.</param>
        public DataContext(IDbConnection connection, Type providerType)
        {
            objectTrackingEnabled = true;
            deferredLoadingEnabled = true;

            this.providerType = providerType;
            if (connection == null)
            {
                throw Error.ArgumentNull("connection");
            }
            InitWithDefaultMapping(connection);
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.DataContext class by referencing a connection and a mapping source.
        /// </summary>
        /// <param name="connection">The connection used by the .NET Framework.</param>
        /// <param name="mapping">The ALinq.Mapping.MappingSource.</param>
        public DataContext(IDbConnection connection, MappingSource mapping)
        {
            objectTrackingEnabled = true;
            deferredLoadingEnabled = true;

            if (connection == null)
            {
                throw Error.ArgumentNull("connection");
            }
            if (mapping == null)
            {
                throw Error.ArgumentNull("mapping");
            }
            Init(connection, mapping);
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.DataContext class by referencing a file source and a mapping source.
        /// </summary>
        /// <param name="fileOrServerOrConnection">The database file path or connection string.</param>
        /// <param name="mapping">The ALinq.Mapping.MappingSource.</param>
        public DataContext(string fileOrServerOrConnection, MappingSource mapping)
        {
            objectTrackingEnabled = true;
            deferredLoadingEnabled = true;
            if (fileOrServerOrConnection == null)
            {
                throw Error.ArgumentNull("fileOrServerOrConnection");
            }
            if (mapping == null)
            {
                throw Error.ArgumentNull("mapping");
            }
            Init(fileOrServerOrConnection, mapping);
        }


        ///<summary>
        /// Initializes a new instance of the ALinq.DataContext class.
        ///</summary>
        ///<param name="fileOrServerOrConnection">The database file path or connection string.</param>
        ///<param name="providerType">The provider type.</param>
        public DataContext(string fileOrServerOrConnection, Type providerType)
        {
            objectTrackingEnabled = true;
            deferredLoadingEnabled = true;

            this.providerType = providerType;
            if (fileOrServerOrConnection == null)
            {
                throw Error.ArgumentNull("fileOrServerOrConnection");
            }
            InitWithDefaultMapping(fileOrServerOrConnection);
        }

        /// <summary>
        /// Initializes a new instance of the ALinq.DataContext class by referencing a file source.
        /// </summary>
        /// <param name="fileOrServerOrConnection"></param>
        public DataContext(string fileOrServerOrConnection)
        {
            objectTrackingEnabled = true;
            deferredLoadingEnabled = true;

            fileOrServerOrConnection = fileOrServerOrConnection.TrimEnd();
            if (fileOrServerOrConnection.EndsWith(".mdb", StringComparison.OrdinalIgnoreCase) ||
                fileOrServerOrConnection.EndsWith(".accdb", StringComparison.OrdinalIgnoreCase))
                providerType = Type.GetType(string.Format("ALinq.Access.AccessDbProvider, ALinq.Access, Version={0}, Culture=neutral, PublicKeyToken=2b23f34316d38f3a", Constants.AccessVersion), true);

            if (fileOrServerOrConnection.EndsWith(".db", StringComparison.OrdinalIgnoreCase) ||
                fileOrServerOrConnection.EndsWith(".db3", StringComparison.OrdinalIgnoreCase))
                providerType = Type.GetType(string.Format("ALinq.SQLite.SQLiteProvider, ALinq.SQLite, Version={0}, Culture=neutral, PublicKeyToken=2b23f34316d38f3a", Constants.SQLiteVersion), true);

            if (fileOrServerOrConnection.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase))
                providerType = typeof(ALinq.SqlClient.Sql2005Provider);

            if (fileOrServerOrConnection == null)
            {
                throw Error.ArgumentNull("fileOrServerOrConnection");
            }
            InitWithDefaultMapping(fileOrServerOrConnection);
        }

        private void InitWithDefaultMapping(object connection)
        {
            Init(connection, new AttributeMappingSource());
        }

        private void Init(object connection, MappingSource mapping)
        {
            //if (mapping is DynamicMappingSource == false)
            //    mapping = new DynamicMappingSource(mapping);

            MetaModel model = mapping.GetModel(GetType());
            if (providerType == null)
                providerType = model.ProviderType;

            if (providerType == null)
                throw Error.ProviderTypeNull();


            try
            {
                LicenseManager.Validate(providerType, this);
            }
            catch (Exception exc)
            {
                throw exc;
            }


            services = new CommonDataServices(this, model);
            conflicts = new ChangeConflictCollection();
            extendTableTypes = new ExtendTableTypes();




            if (!typeof(IProvider).IsAssignableFrom(providerType))
            {
                throw Error.ProviderDoesNotImplementRequiredInterface(providerType, typeof(IProvider));
            }


            var p = (SqlProvider)Activator.CreateInstance(providerType);
            Provider = p;

            Provider.Initialize(services, connection);
            tables = new Dictionary<MetaTable, ITable>();
        }

        /// <summary>
        /// Sends changes that were made to retrieved objects to the underlying database, and specifies the action to be taken if the submission fails.
        /// </summary>
        /// <param name="failureMode">The action to be taken if the submission fails. Valid arguments are as follows: ALinq.ConflictMode.FailOnFirstConflictSystem.Data.Linq.ConflictMode.ContinueOnConflict</param>
        public virtual void SubmitChanges(ConflictMode failureMode)
        {
            CheckDispose();
            CheckNotInSubmitChanges();
            VerifyTrackingEnabled();
            conflicts.Clear();
            try
            {
                changeProcessor = new ChangeProcessor(Services, this);
                isInSubmitChanges = true;
                if ((System.Transactions.Transaction.Current == null) && (Provider.Transaction == null))
                {
                    bool flag = false;
                    DbTransaction transaction = null;
                    try
                    {
                        if (Provider.Connection.State == ConnectionState.Open)
                        {
                            Provider.ClearConnection();
                        }
                        if (Provider.Connection.State == ConnectionState.Closed)
                        {
                            Provider.Connection.Open();
                            flag = true;
                        }
                        transaction = Provider.Connection.BeginTransaction(IsolationLevel.ReadCommitted);
                        Provider.Transaction = transaction;
                        changeProcessor.SubmitChanges(failureMode);
                        AcceptChanges();
                        Provider.ClearConnection();
                        transaction.Commit();
                        return;
                    }
                    catch (Exception exc)
                    {
                        if (transaction != null)
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch
                            {
                            }
                        }
                        throw exc;
                    }
                    finally
                    {
                        Provider.Transaction = null;
                        if (flag)
                        {
                            Provider.Connection.Close();
                        }
                    }
                }
                changeProcessor.SubmitChanges(failureMode);
                AcceptChanges();

            }
            finally
            {
                isInSubmitChanges = false;
            }
        }

        /// <summary>
        /// Refreshes an entity object according to the specified mode.
        /// </summary>
        /// <param name="mode">A value that specifies how optimistic concurrency conflicts are handled.</param>
        /// <param name="entities">The collection of entities to be refreshed.</param>
        public void Refresh(RefreshMode mode, IEnumerable entities)
        {
            CheckDispose();
            CheckNotInSubmitChanges();
            VerifyTrackingEnabled();
            if (entities == null)
            {
                throw Error.ArgumentNull("entities");
            }
            List<object> list = entities.Cast<object>().ToList();
            DataContext context = CreateRefreshContext();
            foreach (object obj2 in list)
            {
                MetaType inheritanceRoot = services.Model.GetMetaType(obj2.GetType()).InheritanceRoot;
                GetTable(inheritanceRoot.Type);
                TrackedObject trackedObject = services.ChangeTracker.GetTrackedObject(obj2);
                if (trackedObject == null)
                {
                    throw Error.UnrecognizedRefreshObject();
                }
                object[] keyValues = CommonDataServices.GetKeyValues(trackedObject.Type, trackedObject.Original);
                object objectByKey = context.Services.GetObjectByKey(trackedObject.Type, keyValues);
                if (objectByKey == null)
                {
                    throw Error.RefreshOfDeletedObject();
                }
                trackedObject.Refresh(mode, objectByKey);
            }
        }


        /// <summary>
        /// Refreshes an array of entity objects according to the specified mode.
        /// </summary>
        /// <param name="mode">A value that specifies how optimistic concurrency conflicts are handled.</param>
        /// <param name="entity">The array of entity objects to be refreshed.</param>
        public void Refresh(RefreshMode mode, object entity)
        {
            CheckDispose();
            CheckNotInSubmitChanges();
            VerifyTrackingEnabled();
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            Array array = Array.CreateInstance(entity.GetType(), 1);
            array.SetValue(entity, 0);
            Refresh(mode, array);
        }

        /// <summary>
        /// Refreshes a collection of entity objects according to the specified mode.
        /// </summary>
        /// <param name="mode">A value that specifies how optimistic concurrency conflicts are handled.</param>
        /// <param name="entities">The array of entity objects to be refreshed.</param>
        public void Refresh(RefreshMode mode, params object[] entities)
        {
            CheckDispose();
            if (entities == null)
            {
                throw Error.ArgumentNull("entities");
            }
            Refresh(mode, (IEnumerable)entities);
        }

        /// <summary>
        /// Converts an existing System.Data.Common.DbDataReader to objects.
        /// </summary>
        /// <typeparam name="TResult">The type of the System.Collections.Generic.IEnumerable&lt;T&gt; to be returned.</typeparam>
        /// <param name="reader">The System.Data.IDataReader to be converted.</param>
        /// <returns>A collection of objects returned by the conversion.</returns>
        public IEnumerable<TResult> Translate<TResult>(DbDataReader reader)
        {
            CheckDispose();
            return (IEnumerable<TResult>)Translate(typeof(TResult), reader);
        }

        /// <summary>
        /// Converts an existing System.Data.Common.DbDataReader to objects.
        /// </summary>
        /// <param name="reader">The System.Data.IDataReader to be converted.</param>
        /// <returns>A list of objects returned by the conversion.</returns>
        public IMultipleResults Translate(DbDataReader reader)
        {
            CheckDispose();
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }
            return Provider.Translate(reader);
        }

        /// <summary>
        /// Converts an existing System.Data.Common.DbDataReader to objects.
        /// </summary>
        /// <param name="elementType">The type of the System.Collections.Generic.IEnumerable&lt;T&gt; to be returned.  The algorithm for matching columns in the result to fields and properties in the object works as follows: If a field or property is mapped to a particular column name, that column name is expected in the resultset.  If a field or property is not mapped, a column with the same name as the field or property is expected in the resultset.  The comparison is performed by looking for a case-sensitive match first. If this match is not found, a subsequent search is occurs for a case-insensitive match.  The query must return all the tracked fields and properties of the object (except those that are loaded on a deferred basis) when all the following conditions are true: T is an entity explicitly tracked by the ALinq.DataContext.  ALinq.DataContext.ObjectTrackingEnabled is true.  The entity has a primary key.  Otherwise an exception is thrown.</param>
        /// <param name="reader">The System.Data.IDataReader to be converted.</param>
        /// <returns>A list of objects returned by the conversion.</returns>
        public IEnumerable Translate(Type elementType, DbDataReader reader)
        {
            CheckDispose();
            if (elementType == null)
            {
                throw Error.ArgumentNull("elementType");
            }
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }
            return Provider.Translate(elementType, reader);
        }

        /// <summary>
        /// Returns a collection of objects of a particular type, where the type is defined by the TEntity parameter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the objects to be returned.</typeparam>
        /// <returns>A collection of objects.</returns>
        public Table<TEntity> GetTable<TEntity>() where TEntity : class
        {
            CheckDispose();
            MetaTable metaTable = Services.Model.GetTable(typeof(TEntity));
            if (metaTable == null)
            {
                throw Error.TypeIsNotMarkedAsTable(typeof(TEntity));
            }
            ITable table = GetTable(metaTable);
            if (table.ElementType != typeof(TEntity))
            {
                throw Error.CouldNotGetTableForSubtype(typeof(TEntity), metaTable.RowType.Type);
            }
            return (Table<TEntity>)table;
        }


        //public ITable GetTable<TEntity>(string tableName)
        //{
        //    var t = extendTableTypes.GetType(tableName, typeof(TEntity));
        //    var table = GetTable(t);
        //    return table;
        //}



        private ITable GetTable(MetaTable metaTable)
        {
            ITable table;
            if (!tables.TryGetValue(metaTable, out table))
            {
                ValidateTable(metaTable);
                table = (ITable)Activator.CreateInstance(typeof(Table<>).MakeGenericType(new[] { metaTable.RowType.Type }), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new object[] { this, metaTable }, null);
                tables.Add(metaTable, table);
            }
            return table;
        }

        private static void ValidateTable(MetaTable metaTable)
        {
            foreach (MetaAssociation association in metaTable.RowType.Associations)
            {
                if (!association.ThisMember.DeclaringType.IsEntity)
                {
                    throw Error.NonEntityAssociationMapping(association.ThisMember.DeclaringType.Type, association.ThisMember.Name, association.ThisMember.DeclaringType.Type);
                }
                if (!association.OtherType.IsEntity)
                {
                    throw Error.NonEntityAssociationMapping(association.ThisMember.DeclaringType.Type, association.ThisMember.Name, association.OtherType.Type);
                }
            }
        }

        #region
        /// <summary>
        /// Specifies the destination to write the SQL query or command.
        /// </summary>
        /// <returns>
        /// The System.IO.TextReader to use for writing the command.
        /// </returns>
        public TextWriter Log
        {
            get
            {
                CheckDispose();
                return Provider.Log;
            }
            set
            {
                CheckDispose();
                if (Provider != null)
                    Provider.Log = value;
                //Source.Log = value;
            }
        }

        /// <summary>
        /// Computes the set of modified objects to be inserted, updated, or deleted, and executes the appropriate commands to implement the changes to the database.
        /// </summary>
        public void SubmitChanges()
        {
            SubmitChanges(ConflictMode.FailOnFirstConflict);
        }

        /// <summary>
        /// Returns a collection of objects that caused concurrency conflicts when ALinq.DataContext.SubmitChanges() was called.
        /// </summary>
        /// <returns>
        /// A collection of the objects.
        /// </returns>
        public ChangeConflictCollection ChangeConflicts
        {
            get
            {
                CheckDispose();
                return conflicts;
            }

        }

        /// <summary>
        /// Returns the ALinq.Mapping.MetaModel on which the mapping is based.
        /// </summary>
        /// <returns>
        /// The ALinq.Mapping.MetaModel.
        /// </returns>
        public MetaModel Mapping
        {
            get { return services.Model; }
        }

        /// <summary>
        /// Returns the connection used by the framework.
        /// </summary>
        public DbConnection Connection
        {
            get
            {
                CheckDispose();
                return Provider.Connection;
            }
        }

        /// <summary>
        /// Sets a local transaction for the .NET Framework to use to access the database.
        /// </summary>
        public DbTransaction Transaction
        {
            get
            {
                CheckDispose();
                return Provider.Transaction;
            }
            set
            {
                CheckDispose();
                Provider.Transaction = value;
            }
        }

        [DebuggerStepThrough]
        private void CheckDispose()
        {
            if (disposed)
            {
                throw Error.ProviderCannotBeUsedAfterDispose();
            }
            //SourceType.InvokeMember("CheckDispose", bf | BindingFlags.InvokeMethod, null, Source, null);
        }

        internal void CheckNotInSubmitChanges()
        {
            CheckDispose();
            if (isInSubmitChanges)
            {
                throw Error.CannotPerformOperationDuringSubmitChanges();
            }
        }

        internal void VerifyTrackingEnabled()
        {
            //SourceType.InvokeMember("VerifyTrackingEnabled", bf | BindingFlags.InvokeMethod, null, Source, null);
            CheckDispose();
            if (!ObjectTrackingEnabled)
            {
                throw Error.ObjectTrackingRequired();
            }
        }

        private void AcceptChanges()
        {
            CheckDispose();
            VerifyTrackingEnabled();
            services.ChangeTracker.AcceptChanges();
        }

        /// <summary>
        /// Returns a collection of objects of a particular type, where the type is defined by the type parameter.
        /// </summary>
        /// <param name="type">The type of the objects to be returned.</param>
        /// <returns>A collection of objects.</returns>
        public ITable GetTable(Type type)
        {
            CheckDispose();
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            MetaTable metaTable = services.Model.GetTable(type);
            if (metaTable == null)
            {
                throw Error.TypeIsNotMarkedAsTable(type);
            }
            if (metaTable.RowType.Type != type)
            {
                throw Error.CouldNotGetTableForSubtype(type, metaTable.RowType.Type);
            }
            return GetTable(metaTable);
        }
        #endregion

        /// <summary>
        /// Increases the time-out period for queries that would otherwise time out during the default time-out period.
        /// </summary>
        /// <returns>
        /// An integer value that represents seconds.
        /// </returns>
        public int CommandTimeout
        {
            get
            {
                CheckDispose();
                return Provider.CommandTimeout;
            }
            set
            {
                CheckDispose();
                Provider.CommandTimeout = value;
            }
        }

        internal CommonDataServices Services
        {
            get
            {
                Debug.Assert(services != null);
                return services;
            }
        }

        internal IProvider Provider { get; private set; }

        /// <summary>
        /// Gets or sets the ALinq.DataLoadOptions associated with this ALinq.DataContext.
        /// </summary>
        /// <returns>
        /// The prefetch load options for related data.
        /// </returns>
        public DataLoadOptions LoadOptions
        {
            get
            {
                CheckDispose();
                return loadOptions;
            }
            set
            {
                CheckDispose();
                if (services.HasCachedObjects && (value != loadOptions))
                {
                    throw Error.LoadOptionsChangeNotAllowedAfterQuery();
                }
                if (value != null)
                {
                    value.Freeze();
                }
                loadOptions = value;
            }
        }

        /// <summary>
        /// Specifies whether to delay-load one-to-many or one-to-one relationships.
        /// </summary>
        /// <returns>
        /// true if deferred loading is enabled; otherwise false.
        /// </returns>
        public bool DeferredLoadingEnabled
        {
            get
            {
                CheckDispose();
                return deferredLoadingEnabled;
            }
            set
            {
                CheckDispose();
                if (Services.HasCachedObjects)
                {
                    throw Error.OptionsCannotBeModifiedAfterQuery();
                }
                if (!ObjectTrackingEnabled && value)
                {
                    throw Error.DeferredLoadingRequiresObjectTracking();
                }
                deferredLoadingEnabled = value;
            }

        }

        [Obsolete]
        internal void CreateTable(Type entityType)
        {
            //CheckDispose();
            //((IProviderExtend)Provider).CreateTable(entityType);
            var metaTable = Mapping.GetTable(entityType);
            CreateTable(metaTable);
        }

        [Obsolete]
        internal void CreateTable(MetaTable metaTable)
        {
            CheckDispose();
            ((IProviderExtend)Provider).CreateTable(metaTable);
        }

        [Obsolete]
        internal bool TableExists(Type entityType)
        {
            //CheckDispose();
            //return ((IProviderExtend)Provider).TableExists(entityType);
            var metaTable = Mapping.GetTable(entityType);
            return TableExists(metaTable);
        }

        [Obsolete]
        internal bool TableExists(MetaTable metaTable)
        {
            CheckDispose();
            return ((IProviderExtend)Provider).TableExists(metaTable);
        }

        [Obsolete]
        internal void DeleteTable(Type entityType)
        {
            var metaTable = Mapping.GetTable(entityType);
            DeleteTable(metaTable);
        }

        [Obsolete]
        internal void DeleteTable(MetaTable metaTable)
        {
            CheckDispose();
            ((IProviderExtend)Provider).DeleteTable(metaTable);
        }

        [Obsolete]
        internal void CreateForeignKeys(MetaTable metaTable)
        {
            CheckDispose();
            ((IProviderExtend)Provider).CreateForeignKeys(metaTable);
        }

        [Obsolete]
        internal void CreateForeignKeys(Type entityType)
        {
            CheckDispose();
            var metaTable = Mapping.GetTable(entityType);
            ((IProviderExtend)Provider).CreateForeignKeys(metaTable);
        }

        /// <summary>
        /// Creates a database on the server.
        /// </summary>
        public void CreateDatabase()
        {
            CheckDispose();
            Provider.CreateDatabase();
        }

        /// <summary>
        /// Determines whether the associated database can be opened.
        /// </summary>
        /// <returns>true if the specified database can be opened; otherwise, false.</returns>
        public bool DatabaseExists()
        {
            CheckDispose();
            return Provider.DatabaseExists();
        }

        /// <summary>
        /// Deletes the associated database.
        /// </summary>
        public void DeleteDatabase()
        {
            CheckDispose();
            Provider.DeleteDatabase();
        }

        /// <summary>
        /// Executes SQL commands directly on the database.
        /// </summary>
        /// <param name="command">The SQL command to be executed.</param>
        /// <param name="parameters">The array of parameters to be passed to the command. Note the following behavior: If the number of objects in the array is less than the highest number identified in the command string, an exception is thrown.  If the array contains objects that are not referenced in the command string, no exception is thrown.  If any one of the parameters is null, it is converted to DBNull.Value.</param>
        /// <returns>An int representing the number of rows modified by the executed command.</returns>
        public int ExecuteCommand(string command, params object[] parameters)
        {
            CheckDispose();
            if (command == null)
            {
                throw Error.ArgumentNull("command");
            }
            if (parameters == null)
            {
                throw Error.ArgumentNull("parameters");
            }
            return (int)ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), new object[] { command, parameters }).ReturnValue;
        }

        internal void CheckInSubmitChanges()
        {
            this.CheckDispose();
            if (!this.isInSubmitChanges)
            {
                throw Error.CannotPerformOperationOutsideSubmitChanges();
            }
        }

        /// <summary>
        /// Called inside delete override methods to redelegate to LINQ to SQL the task of generating and executing dynamic SQL for delete operations.
        /// </summary>
        /// <param name="entity">The entity to be deleted.</param>
        protected internal void ExecuteDynamicDelete(object entity)
        {
            this.CheckDispose();
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            this.CheckInSubmitChanges();
            TrackedObject trackedObject = this.services.ChangeTracker.GetTrackedObject(entity);
            if (trackedObject == null)
            {
                throw Error.CannotPerformOperationForUntrackedObject();
            }
            if (this.services.ChangeDirector.DynamicDelete(trackedObject) == 0)
            {
                throw new ChangeConflictException();
            }
        }

        /// <summary>
        /// Called inside insert override methods to redelegate to LINQ to SQL the task of generating and executing dynamic SQL for insert operations.
        /// </summary>
        /// <param name="entity">The entity to be inserted.</param>
        protected internal void ExecuteDynamicInsert(object entity)
        {
            this.CheckDispose();
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            this.CheckInSubmitChanges();
            TrackedObject trackedObject = this.services.ChangeTracker.GetTrackedObject(entity);
            if (trackedObject == null)
            {
                throw Error.CannotPerformOperationForUntrackedObject();
            }
            this.services.ChangeDirector.DynamicInsert(trackedObject);
        }

        /// <summary>
        /// Called inside update override methods to redelegate to LINQ to SQL the task of generating and executing dynamic SQL for update operations.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        protected internal void ExecuteDynamicUpdate(object entity)
        {
            this.CheckDispose();
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            this.CheckInSubmitChanges();
            TrackedObject trackedObject = this.services.ChangeTracker.GetTrackedObject(entity);
            if (trackedObject == null)
            {
                throw Error.CannotPerformOperationForUntrackedObject();
            }
            if (this.services.ChangeDirector.DynamicUpdate(trackedObject) == 0)
            {
                throw new ChangeConflictException();
            }
        }

        /// <summary>
        /// Executes the stored database procedure or scalar function associated with the specified CLR method.
        /// </summary>
        /// <param name="instance">The instance of the method invocation (the current object).</param>
        /// <param name="methodInfo">Identifies the CLR method that corresponds to a database method.</param>
        /// <param name="parameters">The array of parameters to be passed to the command.</param>
        /// <returns>The result (the return value and output parameters) of executing the specified method.</returns>
        protected internal IExecuteResult ExecuteMethodCall(object instance, MethodInfo methodInfo, params object[] parameters)
        {
            CheckDispose();
            if (instance == null)
            {
                throw Error.ArgumentNull("instance");
            }
            if (methodInfo == null)
            {
                throw Error.ArgumentNull("methodInfo");
            }
            if (parameters == null)
            {
                throw Error.ArgumentNull("parameters");
            }
            return Provider.Execute(GetMethodCall(instance, methodInfo, parameters));
        }

        /// <summary>
        /// Executes SQL queries directly on the database and returns objects.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the returned collection.</typeparam>
        /// <param name="query">The SQL query to be executed.</param>
        /// <param name="parameters">The array of parameters to be passed to the command. Note the following behavior: If the number of objects in the array is less than the highest number identified in the command string, an exception is thrown.  If the array contains objects that are not referenced in the command string, no exception is thrown.  If a parameter is null, it is converted to DBNull.Value.</param>
        /// <returns>A collection of objects returned by the query.</returns>
        public IEnumerable<TResult> ExecuteQuery<TResult>(string query, params object[] parameters)
        {
            CheckDispose();
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }
            if (parameters == null)
            {
                throw Error.ArgumentNull("parameters");
            }
            return (IEnumerable<TResult>)ExecuteMethodCall(this, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(new[] { typeof(TResult) }), new object[] { query, parameters }).ReturnValue;
        }

        /// <summary>
        /// Executes SQL queries directly on the database.
        /// </summary>
        /// <param name="elementType">The type of the System.Collections.Generic.IEnumerable&lt;T&gt; to be returned.  The algorithm for matching columns in the result of the query to fields or properties in the object works as follows: If a field or property is mapped to a particular column name, that column name is expected in the resultset.  If a field or property is not mapped, a column with the same name as the field or property is expected in the resultset.  The comparison is performed by looking for a case-sensitive match first. If this match is not found, a subsequent search occurs for a case-insensitive match.  The query must return all the tracked fields and properties of the object (except those that are loaded on a deferred basis) when all the following conditions are true: T is an entity explicitly tracked by the ALinq.DataContext.  ALinq.DataContext.ObjectTrackingEnabled is true.  The entity has a primary key.  Otherwise an exception is thrown.</param>
        /// <param name="query">The SQL query to be executed.</param>
        /// <param name="parameters">The array of parameters to be passed to the command. Note the following behavior: If the number of objects in the array is less than the highest number identified in the command string, an exception is thrown.  If the array contains objects that are not referenced in the command string, no exception is thrown.  If a parameter is null, it is converted to DBNull.Value.</param>
        /// <returns>An System.Collections.Generic.IEnumerable&lt;T&gt; collection of objects returned by the query.</returns>
        public IEnumerable ExecuteQuery(Type elementType, string query, params object[] parameters)
        {
            CheckDispose();
            if (elementType == null)
            {
                throw Error.ArgumentNull("elementType");
            }
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }
            if (parameters == null)
            {
                throw Error.ArgumentNull("parameters");
            }
            if (_miExecuteQuery == null)
            {
                _miExecuteQuery = typeof(DataContext).GetMethods()
                                                     .Single(m => (m.Name == "ExecuteQuery") && (m.GetParameters().Length == 2));
            }
            return (IEnumerable)ExecuteMethodCall(this, _miExecuteQuery.MakeGenericMethod(new[] { elementType }), new object[] { query, parameters }).ReturnValue;
        }

        /// <summary>
        /// Provides information about SQL commands generated by LINQ to SQL.
        /// </summary>
        /// <param name="query">The query whose SQL command information is to be retrieved.</param>
        /// <returns>The requested command information object.</returns>
        public DbCommand GetCommand(IQueryable query)
        {
            CheckDispose();
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }
            return Provider.GetCommand(query.Expression);
        }

        /// <summary>
        /// Provides access to the modified objects tracked by ALinq.DataContext.
        /// </summary>
        /// <returns>The set of objects is returned as three read-only collections.</returns>
        public ChangeSet GetChangeSet()
        {
            CheckDispose();
            return new ChangeProcessor(services, this).GetChangeSet();
        }

        private Expression GetMethodCall(object instance, MethodInfo methodInfo, params object[] parameters)
        {
            CheckDispose();
            if (parameters.Length <= 0)
            {
                return Expression.Call(Expression.Constant(instance), methodInfo);
            }
            ParameterInfo[] infoArray = methodInfo.GetParameters();
            var arguments = new List<Expression>(parameters.Length);
            int index = 0;
            int length = parameters.Length;
            while (index < length)
            {
                Type parameterType = infoArray[index].ParameterType;
                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                }
                arguments.Add(Expression.Constant(parameters[index], parameterType));
                index++;
            }
            return Expression.Call(Expression.Constant(instance), methodInfo, arguments);
        }

        /// <summary>
        /// Releases resources used by the ALinq.DataContext.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Provider != null)
                {
                    Provider.Dispose();
                    Provider = null;
                }
                services = null;
                tables = null;
                //Source.Dispose();
            }
        }

        /// <summary>
        /// Releases all resources used by the ALinq.DataContext.
        /// </summary>
        public void Dispose()
        {
            disposed = true;
            Dispose(true);
        }

        /// <summary>
        /// Instructs the framework to track the original value and object identity for this ALinq.DataContext.
        /// </summary>
        /// <returns>
        /// true to enable object tracking; otherwise, false. The default is true.
        /// </returns>
        public bool ObjectTrackingEnabled
        {
            get
            {
                CheckDispose();
                return objectTrackingEnabled;
            }
            set
            {
                CheckDispose();
                if (Services.HasCachedObjects)
                {
                    throw Error.OptionsCannotBeModifiedAfterQuery();
                }
                objectTrackingEnabled = value;
                if (!objectTrackingEnabled)
                {
                    deferredLoadingEnabled = false;
                }
                services.ResetServices();
            }

        }

        internal DataContext CreateRefreshContext()
        {
            return new DataContext(this, providerType);
        }


    }
}
