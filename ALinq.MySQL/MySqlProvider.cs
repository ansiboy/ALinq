using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using ALinq;
using ALinq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MySql.Data.MySqlClient;
using ALinq.SqlClient;

namespace ALinq.MySQL
{
    public partial class MySqlProvider : SqlProvider, IProvider, IProviderExtend
    {
        private string dbName;
        private string connectionString;
        private bool deleted;
        private MySqlPostBindDotNetConverter postBindDotNetConverter;
        private List<MySqlProvider> subQueryProviders;

        public MySqlProvider()
            : base(ProviderMode.MySql)
        {
            subQueryProviders = new List<MySqlProvider>();
        }


        void IProvider.Initialize(IDataServices dataServices, object connection)
        {
            if (dataServices == null)
            {
                throw SqlClient.Error.ArgumentNull("dataServices");
            }
            DbConnection conn;
            if (connection is string)
                conn = new MySqlConnection((string)connection);
            else
                conn = ((DbConnection)connection);

            connectionString = conn.ConnectionString;
            var builder = new MySqlConnectionStringBuilder(connectionString);
            dbName = builder.Database;
            if (string.IsNullOrEmpty(dbName))
                dbName = dataServices.Model.DatabaseName;
            if (string.IsNullOrEmpty(dbName))
                throw SqlClient.Error.CouldNotDetermineCatalogName();

            Initialize(dataServices, conn);
            //var conn = new Connection((MySqlConnection)connection);
            //Debug.Assert(dataServices is ReflectObject);
            //InvokeIProviderMethod("Initialize", new[] { ((ReflectObject)dataServices).Source, conn });
            //services = dataServices;
            //this.dbName = (string)GetField("dbName");

            //conManager = new MySqlConnectionManager(this, conn, 100);
            //var type = typeof(DbDataReader);
            //this.readerCompiler = new ObjectReaderCompiler(type, this.services);
        }

        private string GetConnectionString(string dataBaseName)
        {
            var builder = new MySqlConnectionStringBuilder(connectionString) { Database = dataBaseName };
            return builder.ConnectionString;
        }

        internal override QueryInfo[] BuildQuery(Expression query, SqlNodeAnnotations annotations)
        {
            CheckDispose();
            query = Funcletizer.Funcletize(query);
            var converter = new MySqlQueryConverter(services, typeProvider, translator, sqlFactory)
                                {
                                    ConverterStrategy = ConverterStrategy.CanOutputFromInsert | ConverterStrategy.CanUseJoinOn |
                                                        ConverterStrategy.CanUseOuterApply | ConverterStrategy.SkipWithRowNumber
                                };
            SqlNode node = converter.ConvertOuter(query);
            var queryInfos = BuildQuery(GetResultShape(query), GetResultType(query), node, null, annotations);
            var formatter = new MySqlFormatter(this);
            for (int i = 0; i < queryInfos.Length; i++)
            {
                var queryInfo = queryInfos[i];
                queryInfo.CommandText = formatter.Format(queryInfo.Query);
            }
            return queryInfos;
        }

        internal override QueryConverter CreateQueryConverter(SqlFactory sql)
        {
            return new MySqlQueryConverter(services, typeProvider, translator, sql);
        }

        public new IExecuteResult Execute(Expression query)
        {
            InitializeProviderMode();

            var annotations = new SqlNodeAnnotations();
            var queries = BuildQuery(query, annotations);
            var queryInfo = queries[queries.Length - 1];

            //queryInfo.CommandText = new MySqlFormatter().Format(queryInfo.Query);

            IObjectReaderFactory readerFactory = null;
            ICompiledSubQuery[] subQueries = null;
            if (queryInfo.ResultShape == ResultShape.Singleton)
            {
                subQueries = CompileSubQueries(queryInfo.Query);
                readerFactory = GetReaderFactory(queryInfo.Query, queryInfo.ResultType);
            }
            else if (queryInfo.ResultShape == ResultShape.Sequence)
            {
                subQueries = CompileSubQueries(queryInfo.Query);
                readerFactory = GetReaderFactory(queryInfo.Query, TypeSystem.GetElementType(queryInfo.ResultType));
            }

            return ExecuteAll(query, queries, readerFactory, null, subQueries);
        }


        internal override IExecuteResult Execute(Expression query, QueryInfo queryInfo, IObjectReaderFactory factory, object[] parentArgs, object[] userArgs, ICompiledSubQuery[] subQueries, object lastResult)
        {
            IExecuteResult result3;
            InitializeProviderMode();
            DbConnection connection = conManager.UseConnection(this);
            try
            {
#if FREE
                var count = GetTabesCount(connection);
                Debug.Assert(count > 0);

                if (count > Constants.LimitedTablesCount)
                    throw ALinq.SqlClient.Error.TablesLimited();
#endif

                DbCommand cmd = connection.CreateCommand();

                cmd.CommandText = queryInfo.CommandText;
                cmd.Transaction = conManager.Transaction;
                cmd.CommandTimeout = CommandTimeout;
                cmd.CommandType = (queryInfo.Query is SqlStoredProcedureCall) ? CommandType.StoredProcedure : CommandType.Text;
                AssignParameters(cmd, queryInfo.Parameters, userArgs, lastResult);
                //if (queryInfo.Query is SqlStoredProcedureCall)

                LogCommand(Log, cmd);
                queryCount++;
                switch (queryInfo.ResultShape)
                {
                    case ResultShape.Singleton:
                        {
                            DbDataReader reader = CreateRereader(cmd.ExecuteReader());
                            IObjectReader reader2 = factory.Create(reader, true, this, parentArgs, userArgs, subQueries);
                            conManager.UseConnection(reader2.Session);
                            try
                            {
                                var objType = typeof(OneTimeEnumerable<>).MakeGenericType(new[] { queryInfo.ResultType });
                                var bf1 = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                                var args = new object[] { reader2 };
                                var sequence =
                                    (IEnumerable)Activator.CreateInstance(objType, bf1, null, args, null);
                                object obj2 = null;
                                var expression = query as MethodCallExpression;
                                MethodInfo info;
                                if ((expression != null) && ((expression.Method.DeclaringType == typeof(Queryable)) ||
                                                             (expression.Method.DeclaringType == typeof(Enumerable))))
                                {
                                    string name = expression.Method.Name;
                                    if (name != null)
                                    {
                                        if ((!(name == "First") && !(name == "FirstOrDefault")) &&
                                            !(name == "SingleOrDefault"))
                                        {
                                            if (name == "Single")
                                            {
                                            }
                                        }
                                        else
                                        {
                                            info = TypeSystem.FindSequenceMethod(expression.Method.Name, sequence);
                                            goto Label_01DE;
                                        }
                                    }
                                    info = TypeSystem.FindSequenceMethod("Single", sequence);
                                }
                                else
                                {
                                    info = TypeSystem.FindSequenceMethod("SingleOrDefault", sequence);
                                }
                            Label_01DE:
                                if (info != null)
                                {
                                    try
                                    {
                                        obj2 = info.Invoke(null, new object[] { sequence });
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
                                return CreateExecuteResult(cmd, queryInfo.Parameters, reader2.Session, obj2);
                            }
                            finally
                            {
                                reader2.Dispose();
                            }
                        }
                    case ResultShape.Sequence:
                        break;

                    case ResultShape.MultipleResults:
                        {
                            DbDataReader reader5 = CreateRereader(cmd.ExecuteReader());
                            IObjectReaderSession user = readerCompiler.CreateSession(reader5, this, parentArgs,
                                                                                          userArgs, subQueries);
                            conManager.UseConnection(user);
                            MetaFunction function2 = GetFunction(query);
                            var executeResult = CreateExecuteResult(cmd, queryInfo.Parameters, user);
                            executeResult.ReturnValue = new MultipleResults(this, function2, user, executeResult);
                            return executeResult;
                        }
                    default:
                        return CreateExecuteResult(cmd, queryInfo.Parameters, null, cmd.ExecuteNonQuery(), true);
                }

                DbDataReader reader3 = CreateRereader(cmd.ExecuteReader());
                IObjectReader reader4 = factory.Create(reader3, true, this, parentArgs, userArgs, subQueries);
                conManager.UseConnection(reader4.Session);
                var t = typeof(OneTimeEnumerable<>).MakeGenericType(new[]{
                                                                             TypeSystem.GetElementType(queryInfo.ResultType)
                                                                         });
                var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                var objs = new object[] { reader4 };
                var source = (IEnumerable)Activator.CreateInstance(t, flags, null, objs, null);
                if (typeof(IQueryable).IsAssignableFrom(queryInfo.ResultType))
                    source = source.AsQueryable();

                var result = CreateExecuteResult(cmd, queryInfo.Parameters, reader4.Session);
                MetaFunction function = GetFunction(query);
                if ((function != null) && !function.IsComposable)
                {
                    t = typeof(SingleResult<>).MakeGenericType(new[]{
                                                                        TypeSystem.GetElementType(queryInfo.ResultType)
                                                                    });
                    flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                    objs = new object[] { source, result, services.Context };
                    source = (IEnumerable)Activator.CreateInstance(t, flags, null, objs, null);
                }
                result.ReturnValue = source;
                result3 = result;
            }
            finally
            {
                conManager.ReleaseConnection(this);
            }
            return result3;
        }

        DbConnection IProvider.Connection
        {
            get
            {
                CheckDispose();
                CheckInitialized();
                return conManager.Connection;// ((Connection)conManager.Connection).Source;
            }
        }

        internal override ITypeSystemProvider CreateTypeSystemProvider()
        {
            //return new SqlTypeSystem.Sql2005Provider();
            //return new MySqlTypeSystemProvider();
            return new MySqlDataTypeProvider();
        }

        internal override ISqlParameterizer CreateSqlParameterizer(ITypeSystemProvider typeProvider, SqlNodeAnnotations annotations)
        {

            return new MySqlParameterizer(typeProvider, annotations);
        }

        internal override DbFormatter CreateSqlFormatter()
        {
            return new MySqlFormatter(this);
        }

        #region 数据库的操作
        public void CreateDatabase()
        {
            if (DatabaseExists())
            {
                throw SqlClient.Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(dbName);
            }

            //创建数据库文件
            var connection = new MySqlConnection(GetConnectionString(string.Empty));
            connection.Open();

            var transaction = connection.BeginTransaction();
            Execute(connection, transaction, MySqlBuilder.GetCreateDatabaseCommand(dbName, null, null));
            try
            {
                MetaModel model = services.Model;
                foreach (MetaTable table in model.GetTables())
                {
                    string createTableCommand = MySqlBuilder.GetCreateTableCommand(table);

                    if (!string.IsNullOrEmpty(createTableCommand))
                        Execute(connection, transaction, createTableCommand);
                }

                foreach (MetaTable table in model.GetTables())
                    foreach (string commandText in MySqlBuilder.GetCreateForeignKeyCommands(table))
                        if (!string.IsNullOrEmpty(commandText))
                            Execute(connection, transaction, commandText);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                connection.Close();
            }
        }

        public void DeleteDatabase()
        {
            if (!deleted)
            {
                IDbConnection connection = new MySqlConnection(GetConnectionString(string.Empty));
                connection.Open();
                try
                {
                    IDbCommand command = connection.CreateCommand();
                    command.CommandText = "Drop Database " + dbName;
                    command.ExecuteNonQuery();
                }
                finally
                {
                    connection.Close();
                }
                deleted = true;
            }
        }

        public bool DatabaseExists()
        {
            bool exists = false;
            IDbConnection connection = new MySqlConnection(GetConnectionString(string.Empty));
            connection.Open();
            IDbCommand command = connection.CreateCommand();
            command.CommandText = @"show databases";
            IDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (dbName.Equals(reader.GetString(0), StringComparison.CurrentCultureIgnoreCase))
                {
                    exists = true;
                    break;
                }
            }
            connection.Close();
            return exists;
        }

        private void Execute(IDbConnection connection, IDbTransaction transaction, string commandText)
        {
            if (Log != null)
            {
                Log.WriteLine(commandText);
                Log.WriteLine();
            }
            IDbCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = commandText;
            command.ExecuteNonQuery();
        }
        #endregion

        internal override ICompiledSubQuery CompileSubQuery(SqlNode query, Type elementType, ReadOnlyCollection<SqlParameter> parameters)
        {
            query = SqlDuplicator.Copy(query);
            var annotations = new SqlNodeAnnotations();
            QueryInfo[] queries = BuildQuery(ResultShape.Sequence, TypeSystem.GetSequenceType(elementType), query, parameters, annotations);
            QueryInfo queryInfo = queries[0];
            ICompiledSubQuery[] subQueries = this.CompileSubQueries(queryInfo.Query);
            var formatter = new MySqlFormatter(this);
            for (int i = 0; i < subQueries.Length; i++)
            {
                var subQuery = (CompiledSubQuery)subQueries[i];
                subQuery.QueryInfo.CommandText = formatter.Format(subQuery.QueryInfo.Query);
            }
            IObjectReaderFactory readerFactory = this.GetReaderFactory(queryInfo.Query, elementType);
            CheckSqlCompatibility(queries, annotations);
            return new CompiledSubQuery(queryInfo, readerFactory, parameters, subQueries, connectionString, this);
        }

        private new class CompiledSubQuery : ICompiledSubQuery
        {
            // Fields
            private readonly IObjectReaderFactory factory;
            private readonly IList<SqlParameter> parameters;
            private readonly QueryInfo queryInfo;
            private readonly ICompiledSubQuery[] subQueries;
            private readonly string connectionString;
            // Methods
            internal CompiledSubQuery(QueryInfo queryInfo, IObjectReaderFactory factory, IList<SqlParameter> parameters,
                                      ICompiledSubQuery[] subQueries, string connectionString, SqlProvider provider)
            {
                this.queryInfo = queryInfo;
                queryInfo.CommandText = new MySqlFormatter(provider).Format(queryInfo.Query);
                this.factory = factory;
                this.parameters = parameters;
                this.subQueries = subQueries;
                this.connectionString = connectionString;
            }

            public QueryInfo QueryInfo
            {
                get { return queryInfo; }
            }

            public IExecuteResult Execute(IProvider provider, object[] parentArgs, object[] userArgs)
            {
                if (((parentArgs == null) && (this.parameters != null)) && (this.parameters.Count != 0))
                {
                    throw SqlClient.Error.ArgumentNull("arguments");
                }
                var provider2 = new MySqlProvider();//provider as MySqlProvider;//
                var connection = new MySqlConnection(connectionString);
                provider2.Initialize(((MySqlProvider)provider).services, connection);
                provider2.Log = provider.Log;
                if (provider2 == null)
                {
                    throw SqlClient.Error.ArgumentTypeMismatch("provider");
                }
                ((MySqlProvider)provider).subQueryProviders.Add(provider2);
                var list = new List<SqlParameterInfo>(this.queryInfo.Parameters);
                int index = 0;
                int count = this.parameters.Count;
                while (index < count)
                {
                    list.Add(new SqlParameterInfo(this.parameters[index], parentArgs[index]));
                    index++;
                }
                var queryInfo = new QueryInfo(this.queryInfo.Query, this.queryInfo.CommandText, list.AsReadOnly(),
                                                    this.queryInfo.ResultShape, this.queryInfo.ResultType);
                var result = provider2.Execute(null, queryInfo, this.factory, parentArgs, userArgs, this.subQueries, null);
                return result;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                services = null;
                if (conManager != null)
                {
                    conManager.DisposeConnection();
                }
                conManager = null;
                typeProvider = null;
                sqlFactory = null;
                translator = null;
                readerCompiler = null;
                Log = null;
                foreach (var item in subQueryProviders)
                    item.Dispose(disposing);
            }
        }
        internal override void AssignParameters(DbCommand cmd, ReadOnlyCollection<SqlParameterInfo> parms, object[] userArguments, object lastResult)
        {
            if (parms != null)
            {
                foreach (SqlParameterInfo info in parms)
                {
                    DbParameter parameter = cmd.CreateParameter();
                    parameter.ParameterName = info.Parameter.Name;
                    parameter.Direction = info.Parameter.Direction;
                    if ((info.Parameter.Direction != ParameterDirection.Input) &&
                        (info.Parameter.Direction != ParameterDirection.InputOutput))
                    {
                        goto Label_00C4;
                    }
                    object obj2 = info.Value;
                    switch (info.Type)
                    {
                        case SqlParameterType.UserArgument:
                            try
                            {
                                obj2 = info.Accessor.DynamicInvoke(new object[] { userArguments });
                                goto Label_00AA;
                            }
                            catch (TargetInvocationException exception)
                            {
                                throw exception.InnerException;
                            }
                        case SqlParameterType.PreviousResult:
                            break;

                        default:
                            goto Label_00AA;
                    }
                    obj2 = lastResult;
                Label_00AA:
                    if (info.Parameter.SqlType.IsRuntimeOnlyType && (obj2 == null))
                        parameter.Value = DBNull.Value;
                    else
                        typeProvider.InitializeParameter(info.Parameter.SqlType, parameter, obj2);
                    goto Label_00DC;
                Label_00C4:
                    typeProvider.InitializeParameter(info.Parameter.SqlType, parameter, null);
                Label_00DC:
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        internal override Enum GetParameterDbType(DbParameter parameter)
        {
            return ((MySqlParameter)parameter).MySqlDbType;
        }


        internal override PostBindDotNetConverter PostBindDotNetConverter
        {
            get
            {
                if (postBindDotNetConverter == null)
                    postBindDotNetConverter = new MySqlPostBindDotNetConverter(sqlFactory);
                return postBindDotNetConverter;
            }
        }
        internal override SqlIdentifier SqlIdentifier
        {
            get
            {
                return MySqlIdentifier.Instance;
            }
        }

        internal override SqlFactory CreateSqlFactory(ITypeSystemProvider typeProvider, MetaModel metaModel)
        {
            return new MySqlFactory(typeProvider, metaModel);
        }

        protected override DbDataReader CreateRereader(DbDataReader reader)
        {
            var source = base.CreateRereader(reader);
            return new DataReader(source);
        }

        #region IProviderExtend Members

        void IProviderExtend.CreateTable(MetaTable metaTable)
        {
            //var metaTable = services.Model.GetTable(metaTable);
            var command = MySqlBuilder.GetCreateTableCommand(metaTable);
            services.Context.ExecuteCommand(command);
        }

        bool IProviderExtend.TableExists(MetaTable metaTable)
        {
            // var metaTable = services.Model.GetTable(metaTable);
            var sql = @"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = {0} AND table_name = {1}";
            var count = services.Context.ExecuteQuery<int>(sql, dbName, metaTable.TableName.ToLower()).SingleOrDefault();
            return count > 0;
        }

        void IProviderExtend.DeleteTable(MetaTable metaTable)
        {
            // var metaTable = services.Model.GetTable(metaTable);
            var sql = "DROP TABLE {0} CASCADE";
            sql = string.Format(sql, metaTable.TableName);
            services.Context.ExecuteCommand(sql);
        }

        void IProviderExtend.CreateForeignKeys(MetaTable metaTable)
        {
            //  var metaTable = services.Model.GetTable(metaTable);
            var commands = MySqlBuilder.GetCreateForeignKeyCommands(metaTable);
            foreach (var command in commands)
                services.Context.ExecuteCommand(command);
        }

        #endregion
    }
}
