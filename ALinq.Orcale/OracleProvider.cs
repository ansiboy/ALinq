using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using ALinq;
using ALinq.Mapping;
using System.Data.OracleClient;
using ALinq.SqlClient;

namespace ALinq.Oracle
{
    public class OracleProvider : SqlProvider, IProvider, IProviderExtend
    {
        private string dbName;
        //private DbConnection conn;
        private bool deleted;
        private string passowrd;
        private OracleSqlBuilder OracleSqlBuilder;

        public OracleProvider()
            : base(ProviderMode.Oracle)
        {

        }

        #region CreateDatabase

        public void CreateDatabase()
        {
            if (DatabaseExists())
            {
                throw SqlClient.Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(dbName);
            }
            if (string.IsNullOrEmpty(dbName))
                throw Error.ArgumentNull("Database Name");

            //创建数据库文件
            var conn = conManager.UseConnection(this);
            var transaction = conn.BeginTransaction();
            OracleSqlBuilder = new OracleSqlBuilder(this);
            try
            {
                Execute(conn, transaction, OracleSqlBuilder.GetCreateDatabaseCommand(dbName, passowrd));
                var commandText = "GRANT DBA TO " + dbName;
                Execute(conn, transaction, commandText);
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                conManager.ReleaseConnection(this);
            }

            DbConnectionStringBuilder builder = new OracleConnectionStringBuilder()
                                                    {
                                                        DataSource = conn.DataSource,
                                                        UserID = dbName,
                                                        Password = passowrd,
                                                    };
            conn = new OracleConnection(builder.ToString());
            conn.Open();
            transaction = conn.BeginTransaction();
            try
            {
                MetaModel model = services.Model;
                foreach (MetaTable table in model.GetTables())
                {
                    //string createTableCommand = OracleSqlBuilder.GetCreateTableCommand(table);
                    //if (!string.IsNullOrEmpty(createTableCommand))
                    //    Execute(conn, transaction, createTableCommand);

                    //string createPrimaryKey = OracleSqlBuilder.GetPrimaryKeyCommand(table);
                    //if (!string.IsNullOrEmpty(createPrimaryKey))
                    //    Execute(conn, transaction, createPrimaryKey);
                    var commands = OracleSqlBuilder.GetCreateTableCommands(table);
                    foreach (var command in commands)
                        Execute(conn, transaction, command);
                }

                //创建外建
                foreach (MetaTable table in model.GetTables())
                    foreach (string commandText in OracleSqlBuilder.GetCreateForeignKeyCommands(table))
                        if (!string.IsNullOrEmpty(commandText))
                            Execute(conn, transaction, commandText);
                ////创建自动编号列
                //foreach (MetaTable table in model.GetTables())
                //{
                //    var create = OracleSqlBuilder.GetCreateSquenceCommand(table);
                //    if (!string.IsNullOrEmpty(create))
                //        Execute(conn, transaction, create);
                //}
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                conManager.ReleaseConnection(this);
            }
        }

        public bool DatabaseExists()
        {

            bool exists = false;
            IDbConnection connection = conManager.UseConnection(this);
            IDbCommand command = connection.CreateCommand();
            command.CommandText = string.Format(@"SELECT USERNAME  FROM ALL_USERS WHERE USERNAME  = '{0}'", dbName.ToUpper());
            IDataReader reader = command.ExecuteReader();
            if (reader.Read())
                exists = true;
            conManager.ReleaseConnection(this);
            return exists;
        }

        public void DeleteDatabase()
        {
            if (!deleted)
            {
                var conn = conManager.UseConnection(this);
                try
                {
                    var commandText = string.Format("DROP USER {0} CASCADE", dbName);
                    var transaction = conn.BeginTransaction();
                    Execute(conn, transaction, commandText);
                    deleted = true;
                }
                finally
                {
                    conManager.ReleaseConnection(this);
                }
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

        //public new DbConnection Connection
        //{
        //    get
        //    {
        //        CheckDispose();
        //        CheckInitialized();
        //        var conn = conManager.Connection;
        //        if (conn is Connection)
        //            return ((Connection)conn).Source;
        //        return conn;
        //    }
        //}
        #endregion

        void IProvider.Initialize(IDataServices dataServices, object connection)
        {
            if (dataServices == null)
            {
                throw SqlClient.Error.ArgumentNull("dataServices");
            }

            DbConnection conn;
            string constr;
            if (connection is string)
            {
                conn = new OracleConnection((string)connection);
                constr = (string)connection;
            }
            //else if (connection is Connection)
            //{
            //    conn = (Connection)connection;
            //    constr = ((Connection)connection).ConnectionString;
            //}
            else
            {
                conn = (OracleConnection)connection;
                constr = conn.ConnectionString;
            }

            dbName = dataServices.Model.DatabaseName;

            object obj;
            var builder = new DbConnectionStringBuilder { ConnectionString = constr };
            passowrd = !builder.TryGetValue("password", out obj) ? dbName : obj.ToString();

            Initialize(dataServices, conn);
        }

        //private new void Initialize(IDataServices dataServices, IDbConnection connection)
        //{
        //    services = dataServices;
        //    conManager = new SqlConnectionManager(this, (DbConnection)connection, 100);
        //    var type = typeof(ALinq.Oracle.DataReader);
        //    readerCompiler = new ObjectReaderCompiler(type, services);
        //    InitializeProviderMode();
        //}

        internal override ICompiledSubQuery CompileSubQuery(SqlNode query, Type elementType, ReadOnlyCollection<SqlParameter> parameters)
        {
            query = SqlDuplicator.Copy(query);
            var annotations = new SqlNodeAnnotations();
            var queries = BuildQuery(ResultShape.Sequence, TypeSystem.GetSequenceType(elementType), query, parameters, annotations);
            var queryInfo = queries[0];
            //Set CommandText
            queryInfo.CommandText = new OracleFormatter(this).Format(queryInfo.Query);
            ICompiledSubQuery[] subQueries = CompileSubQueries(queryInfo.Query);
            IObjectReaderFactory readerFactory = GetReaderFactory(queryInfo.Query, elementType);
            CheckSqlCompatibility(queries, annotations);

            return new CompiledSubQuery(queryInfo, readerFactory, parameters, subQueries);
        }

        internal override void AssignParameters(DbCommand cmd, ReadOnlyCollection<SqlParameterInfo> parms, object[] userArguments, object lastResult)
        {
            base.AssignParameters(cmd, parms, userArguments, lastResult);
            foreach (DbParameter parameter in cmd.Parameters)
            {
                if (parameter.Value.GetType() == typeof(Guid))
                {
                    parameter.DbType = DbType.Binary;
                    parameter.Value = ((Guid)parameter.Value).ToByteArray();
                }
                if (parameter.Value.GetType() == typeof(Binary))
                {
                    parameter.DbType = DbType.Binary;
                    parameter.Value = ((Binary)parameter.Value).ToArray();
                }
            }
        }

        internal override Enum GetParameterDbType(DbParameter parameter)
        {
            return ((OracleParameter)parameter).OracleType;
        }

        internal override SqlIdentifier SqlIdentifier
        {
            get
            {
                return OracleIdentifier.Instance;
            }
        }

        internal override ITypeSystemProvider CreateTypeSystemProvider()
        {
            //return new OracleTypeSystemProvider();
            return new OracleDataTypeProvider();
        }

        internal override ISqlParameterizer CreateSqlParameterizer(ITypeSystemProvider typeProvider, SqlNodeAnnotations annotations)
        {
            return new OracleParameterizer(typeProvider, annotations);
        }

        internal override DbFormatter CreateSqlFormatter()
        {
            return new OracleFormatter(this);
        }

        private PostBindDotNetConverter postBindDotNetConverter;
        internal override PostBindDotNetConverter PostBindDotNetConverter
        {
            get
            {
                if (postBindDotNetConverter == null)
                    postBindDotNetConverter = new OraclePostBindDotNetConverter(this.sqlFactory);
                return postBindDotNetConverter;
            }
        }

        internal override QueryConverter CreateQueryConverter(SqlFactory sql)
        {
            return new OracleQueryConverter(services, typeProvider, translator, sql)
            {
                ConverterStrategy = ConverterStrategy.CanUseJoinOn | ConverterStrategy.SkipWithRowNumber
            };
        }

        internal override SqlFactory CreateSqlFactory(ITypeSystemProvider typeProvider, MetaModel metaModel)
        {
            return new OracleFactory(typeProvider, metaModel);
        }

        protected override DbDataReader CreateRereader(DbDataReader reader)
        {
            return new DataReader((OracleDataReader)reader);
        }

        #region IProviderExtend Members

        void IProviderExtend.CreateTable(MetaTable metaTable)
        {
            var sqlBuilder = new OracleSqlBuilder(this);
            var commands = sqlBuilder.GetCreateTableCommands(metaTable);
            foreach (var command in commands)
                services.Context.ExecuteCommand(command);
        }

        bool IProviderExtend.TableExists(MetaTable metaTable)
        {
            //var metaTable = services.Model.GetTable(metaTable);
            var sql = @"SELECT COUNT(*) FROM dba_tables where UPPER(table_name) = {0}";
            var count = services.Context.ExecuteQuery<decimal>(sql, metaTable.TableName.ToUpper()).SingleOrDefault();
            return count > 0;
        }

        void IProviderExtend.DeleteTable(MetaTable metaTable)
        {
            var OracleSqlBuilder = new OracleSqlBuilder(this);
            var commands = OracleSqlBuilder.GetDropTableCommands(metaTable);
            foreach (var command in commands)
                services.Context.ExecuteCommand(command);
        }

        void IProviderExtend.CreateForeignKeys(MetaTable metaTable)
        {
            //var metaTable = services.Model.GetTable(metaTable);
            var sqlBuilder = new OracleSqlBuilder(this);
            var commands = sqlBuilder.GetCreateForeignKeyCommands(metaTable);
            foreach (var command in commands)
                services.Context.ExecuteCommand(command);
        }

        #endregion
    }
}
