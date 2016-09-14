using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using ALinq.Mapping;
using ALinq.SqlClient;
using System.Data.Common;
using Npgsql;
using System.Collections.ObjectModel;

namespace ALinq.PostgreSQL
{
    public class PgsqlProvider : SqlProvider, IProvider, IProviderExtend
    {
        #region CompiledSubQuery
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
                queryInfo.CommandText = new PgsqlFormatter(provider).Format(queryInfo.Query);
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
                var provider2 = new PgsqlProvider();//provider as MySqlProvider;//
                var connection = new NpgsqlConnection(connectionString);
                provider2.Initialize(((PgsqlProvider)provider).services, connection);
                provider2.Log = provider.Log;
                if (provider2 == null)
                {
                    throw SqlClient.Error.ArgumentTypeMismatch("provider");
                }
                ((PgsqlProvider)provider).subQueryProviders.Add(provider2);
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
        #endregion

        private string dbName;
        private string connectionString;
        private bool deleted;
        private readonly List<PgsqlProvider> subQueryProviders;

        public PgsqlProvider()
            : base(ProviderMode.Pgsql)
        {
            subQueryProviders = new List<PgsqlProvider>();
        }

        internal override QueryConverter CreateQueryConverter(SqlFactory sql)
        {
            return new PgsqlQueryConverter(services, typeProvider, translator, sql)
            {
                ConverterStrategy = ConverterStrategy.CanUseJoinOn
            }; ;
        }

        internal override DbFormatter CreateSqlFormatter()
        {
            return new PgsqlFormatter(this);
        }

        internal override ITypeSystemProvider CreateTypeSystemProvider()
        {
            return new PgsqlDataTypeProvider();
        }

        internal override SqlFactory CreateSqlFactory(ITypeSystemProvider typeProvider, MetaModel metaModel)
        {
            return new PostgreSQL.PgsqlFactory(typeProvider, metaModel);
        }

        internal override SqlIdentifier SqlIdentifier
        {
            get
            {
                return PgsqlIdentifier.Instance;
            }
        }

        internal override ICompiledSubQuery CompileSubQuery(SqlNode query, Type elementType, ReadOnlyCollection<SqlParameter> parameters)
        {
            query = SqlDuplicator.Copy(query);
            var annotations = new SqlNodeAnnotations();
            QueryInfo[] queries = BuildQuery(ResultShape.Sequence, TypeSystem.GetSequenceType(elementType), query, parameters, annotations);
            QueryInfo queryInfo = queries[0];
            ICompiledSubQuery[] subQueries = this.CompileSubQueries(queryInfo.Query);
            var formatter = new PgsqlFormatter(this);
            for (int i = 0; i < subQueries.Length; i++)
            {
                var subQuery = (CompiledSubQuery)subQueries[i];
                subQuery.QueryInfo.CommandText = formatter.Format(subQuery.QueryInfo.Query);
            }
            IObjectReaderFactory readerFactory = this.GetReaderFactory(queryInfo.Query, elementType);
            CheckSqlCompatibility(queries, annotations);
            return new CompiledSubQuery(queryInfo, readerFactory, parameters, subQueries, connectionString, this);
        }

        void IProvider.Initialize(IDataServices dataServices, object connection)
        {
            if (dataServices == null)
            {
                throw SqlClient.Error.ArgumentNull("dataServices");
            }
            DbConnection conn;
            if (connection is string)
                conn = new NpgsqlConnection((string)connection);
            else
                conn = ((DbConnection)connection);

            connectionString = conn.ConnectionString;
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            dbName = builder.Database;
            if (string.IsNullOrEmpty(dbName))
                dbName = dataServices.Model.DatabaseName;
            if (string.IsNullOrEmpty(dbName))
                throw SqlClient.Error.CouldNotDetermineCatalogName();
            dbName = dbName.ToLower();
            Initialize(dataServices, conn);
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

        public void CreateDatabase()
        {
            if (DatabaseExists())
            {
                throw SqlClient.Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(dbName);
            }

            var connection = new NpgsqlConnection(GetConnectionString(string.Empty));
            connection.Open();
            try
            {
                Execute(connection, null, PgsqlBuilder.GetCreateDatabaseCommand(dbName, null, null));
            }
            finally
            {
                connection.Close();
            }

            connection.ConnectionString = GetConnectionString(dbName);
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                var builder = new PgsqlBuilder(this);
                MetaModel model = services.Model;
                foreach (MetaTable table in model.GetTables())
                {
                    string createTableCommand = builder.GetCreateTableCommand(table);

                    if (!string.IsNullOrEmpty(createTableCommand))
                        Execute(connection, transaction, createTableCommand);
                }
                foreach (MetaTable metaTable in model.GetTables())
                {
                    var command = builder.GetPrimaryKeyCommand(metaTable);
                    if (!string.IsNullOrEmpty(command))
                        Execute(connection, transaction, command);
                }
                foreach (MetaTable metaTable in model.GetTables())
                {
                    //创建自动编号列
                    var command = SqlBuilder.GetCreateSquenceCommand(metaTable);
                    if (!string.IsNullOrEmpty(command))
                        services.Context.ExecuteCommand(command);
                }
                foreach (MetaTable table in model.GetTables())
                    foreach (string commandText in builder.GetCreateForeignKeyCommands(table))
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

        public bool DatabaseExists()
        {
            IDbConnection connection = new NpgsqlConnection(GetConnectionString(string.Empty));
            connection.Open();
            try
            {
                IDbCommand command = connection.CreateCommand();
                command.CommandText = string.Format("SELECT datname FROM pg_database WHERE LOWER(datname) = '{0}'", dbName);

                var result = command.ExecuteScalar();
                return result != null;
            }
            finally
            {
                connection.Close();
            }
        }

        private string GetConnectionString(string dataBaseName)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString) { Database = dataBaseName };
            return builder.ConnectionString;
        }


        public void DeleteDatabase()
        {
            if (!deleted)
            {
                IDbConnection connection = new Npgsql.NpgsqlConnection(GetConnectionString(string.Empty));
                connection.Open();
                try
                {
                    Execute(connection, null, "DROP DATABASE " + dbName);
                }
                finally
                {
                    connection.Close();
                }
                deleted = true;
            }
        }

        protected override DbDataReader CreateRereader(DbDataReader reader)
        {
            return new DataReader((NpgsqlDataReader)reader);
        }


        #region IProviderExtend Members

        void IProviderExtend.CreateTable(MetaTable metaTable)
        {
            var command = SqlBuilder.GetCreateTableCommand(metaTable);
            services.Context.ExecuteCommand(command);
            //创建主键
            command = SqlBuilder.GetPrimaryKeyCommand(metaTable);
            services.Context.ExecuteCommand(command);
            //创建自动编号列
            command = SqlBuilder.GetCreateSquenceCommand(metaTable);
            if (!string.IsNullOrEmpty(command))
                services.Context.ExecuteCommand(command);
        }

        bool IProviderExtend.TableExists(MetaTable metaTable)
        {
            var sql = @"SELECT tablename FROM pg_tables WHERE schemaname='public' AND LOWER(tablename)={0} LIMIT 1";
            var result = services.Context.ExecuteQuery<string>(sql, metaTable.TableName.ToLower()).SingleOrDefault();
            return result != null;
        }

        void IProviderExtend.DeleteTable(MetaTable metaTable)
        {
            var sql = string.Format("DROP TABLE {0} CASCADE ", metaTable.TableName);
            services.Context.ExecuteCommand(sql);
            sql = SqlBuilder.GetDropSquenceCommand(metaTable);
            if (!string.IsNullOrEmpty(sql))
                services.Context.ExecuteCommand(sql);
        }

        void IProviderExtend.CreateForeignKeys(MetaTable metaTable)
        {
            var commands = sqlBuilder.GetCreateForeignKeyCommands(metaTable);
            foreach (var command in commands)
                services.Context.ExecuteCommand(command);
        }

        private PgsqlBuilder sqlBuilder;
        PgsqlBuilder SqlBuilder
        {
            get
            {
                if (sqlBuilder == null)
                    sqlBuilder = new PgsqlBuilder(this);
                return sqlBuilder;
            }
        }
        #endregion
    }

#if !FREE
    //    class PgsqlLicenseProvider : ALinqLicenseProvider
    //{
    //    protected override string LicenseName
    //    {
    //        get { return "Postgre"; }
    //    }
    //}
#endif
}
