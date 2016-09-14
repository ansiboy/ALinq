using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using ALinq;
using ALinq.Mapping;
using ALinq.SqlClient;
using Oracle.DataAccess.Client;

namespace ALinq.Oracle.Odp
{
    public class OracleProvider : SqlProvider, IProvider, IProviderExtend
    {
        private string dbName;
        //private DbConnection conn;
        private bool deleted;
        private string passowrd;

        public OracleProvider()
            : base(ProviderMode.OdpOracle)
        {

        }

        #region
        public void CreateDatabase()
        {
            if (DatabaseExists())
            {
                throw SqlClient.Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(dbName);
            }
            if (string.IsNullOrEmpty(dbName))
                throw Error.ArgumentNull("Database Name");

            var conn = conManager.UseConnection(this);



            var transaction = conn.BeginTransaction();
            var OracleSqlBuilder = new OracleSqlBuilder(this);
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

            var str = string.Format("Data Source={0};User ID={1};Password={2}", conn.DataSource, dbName, passowrd);
            var oracleConnection = new OracleConnection(str);
            oracleConnection.Open();
            var oracleTransaction = oracleConnection.BeginTransaction();
            //var OracleSqlBuilder = new OracleSqlBuilder(this);
            try
            {
                MetaModel model = services.Model;
                foreach (MetaTable table in model.GetTables())
                {
                    var commands = OracleSqlBuilder.GetCreateTableCommands(table);
                    foreach (var command in commands)
                        Execute(oracleConnection, oracleTransaction, command);
                }

                foreach (MetaTable table in model.GetTables())
                    foreach (string commandText in OracleSqlBuilder.GetCreateForeignKeyCommands(table))
                        if (!string.IsNullOrEmpty(commandText))
                            Execute(oracleConnection, oracleTransaction, commandText);

                oracleTransaction.Commit();
            }
            catch
            {
                oracleTransaction.Rollback();
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
                //var builder = new OracleConnectionStringBuilder(connectionString) { UserID = "System" };
                //var connection = new OracleConnection(builder.ToString());
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
            //else if (connection is OdpConnection)
            //{
            //    conn = ((OdpConnection)connection);
            //    constr = conn.ConnectionString;
            //}
            else
            {
                conn = (OracleConnection)connection;
                constr = ((OracleConnection)connection).ConnectionString;
            }

            object obj;
            var builder = new DbConnectionStringBuilder { ConnectionString = constr };
            passowrd = !builder.TryGetValue("password", out obj) ? dbName : obj.ToString();

            dbName = dataServices.Model.DatabaseName;
            if (string.IsNullOrEmpty(dbName))
                throw SqlClient.Error.CouldNotDetermineCatalogName();

            Initialize(dataServices, conn);
        }

        internal override Enum GetParameterDbType(DbParameter parameter)
        {
            //return ((OdpParameter)parameter).OracleDbType;
            return ((OracleParameter)parameter).OracleDbType;
        }

        protected override DbDataReader CreateRereader(DbDataReader reader)
        {
            return new OdpDataReader((OracleDataReader)reader);
        }

        internal override void AssignParameters(DbCommand cmd, ReadOnlyCollection<SqlParameterInfo> parms, object[] userArguments, object lastResult)
        {
            if (cmd.CommandText.StartsWith("begin") && cmd.CommandText.EndsWith("end;"))//(cmd.CommandType == CommandType.StoredProcedure)
            {
                base.AssignParameters(cmd, parms, userArguments, lastResult);
                //cmd.Parameters.Add(new OdpParameter("mycs", OracleDbType.RefCursor) { Direction = ParameterDirection.Output });
                return;
            }

            if (parms != null)
            {
                var list = new List<string>();
                var match = Regex.Match(cmd.CommandText, "[:][a-z][0-9]+");
                while (match.Success)
                {
                    list.Add(match.Value);
                    match = match.NextMatch();
                }
                var parameters = new List<SqlParameterInfo>();
                foreach (var parameterName in list)
                {
                    var info = parms.Where(o => o.Parameter.Name == parameterName).Single();
                    info.Parameter.Name = ":" + info.Parameter.Name.Substring(1);
                    parameters.Add(info);
                }
                base.AssignParameters(cmd, parameters.AsReadOnly(), userArguments, lastResult);
            }
        }

        internal override ISqlParameterizer CreateSqlParameterizer(ITypeSystemProvider typeProvider, SqlNodeAnnotations annotations)
        {
            return new OracleParameterizer(typeProvider, annotations);
        }

        internal override ITypeSystemProvider CreateTypeSystemProvider()
        {
            //return new OracleTypeSystemProvider();
            return new OracleDataTypeProvider();
        }

        internal override IExecuteResult CreateExecuteResult(DbCommand cmd, ReadOnlyCollection<SqlParameterInfo> parameters, IObjectReaderSession session, object value)
        {
            return new OracleExecuteResult(cmd, parameters, session, value);
        }

        internal override IExecuteResult CreateExecuteResult(DbCommand cmd, ReadOnlyCollection<SqlParameterInfo> parameters, IObjectReaderSession session,
                                                             object value, bool useReturnValue)
        {
            return new OracleExecuteResult(cmd, parameters, session, value, useReturnValue);
        }

        internal override SqlProvider.ExecuteResult CreateExecuteResult(DbCommand command, ReadOnlyCollection<SqlParameterInfo> parameters, IObjectReaderSession session)
        {
            return new OracleExecuteResult(command, parameters, session);
        }

        internal override SqlIdentifier SqlIdentifier
        {
            get
            {
                return OracleIdentifier.Instance;
            }
        }

        internal override QueryConverter CreateQueryConverter(SqlFactory sql)
        {
            return new OracleQueryConverter(services, typeProvider, translator, sql)
            {
                ConverterStrategy = ConverterStrategy.CanUseJoinOn | ConverterStrategy.SkipWithRowNumber
            };
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
                    postBindDotNetConverter = new OraclePostBindDotNetConverter(sqlFactory);
                return postBindDotNetConverter;
            }
        }

        internal override SqlFactory CreateSqlFactory(ITypeSystemProvider typeProvider, MetaModel metaModel)
        {
            return new OracleFactory(typeProvider, metaModel);
        }

        #region IProviderExtend Members

        void IProviderExtend.CreateTable(MetaTable metaTable)
        {
            //var metaTable = services.Model.GetTable(metaTable);
            var sqlBuilder = new OracleSqlBuilder(this);
            var commands = sqlBuilder.GetCreateTableCommands(metaTable);
            foreach (var command in commands)
                services.Context.ExecuteCommand(command);
        }

        bool IProviderExtend.TableExists(MetaTable metaTable)
        {
            //var metaTable = services.Model.GetTable(metaTable);
            var sql = @"SELECT Count(*) FROM dba_tables where UPPER(table_name) = {0}";
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
