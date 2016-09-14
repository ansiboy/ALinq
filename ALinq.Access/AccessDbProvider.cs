using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using ALinq.Mapping;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using dao;
using ALinq.SqlClient;

namespace ALinq.Access
{
    public class AccessDbProvider : SqlProvider, IProvider, IProviderExtend
    {
        //private IDataServices services;
        private string dbName;
        private bool deleted;

        public AccessDbProvider()
            : base(ProviderMode.Access)
        {

        }

        void IProvider.CreateDatabase()
        {
            if (File.Exists(dbName))
            {
                throw SqlClient.Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(dbName);
            }

            //创建数据库文件
            var dbEngineClass = new DBEngineClass();
            var dataBase = dbEngineClass.CreateDatabase(dbName, LanguageConstants.dbLangGeneral, DatabaseTypeEnum.dbEncrypt);
            dataBase.Close();

            var connection = new OleDbConnection(GetConnectionString(dbName));
            connection.Open();
            var transaction = connection.BeginTransaction();
            var AccessDbSqlBuilder = new AccessDbSqlBuilder(this);
            try
            {
                if (services.Model.GetTables().FirstOrDefault() == null)
                {
                    throw SqlClient.Error.CreateDatabaseFailedBecauseOfContextWithNoTables(services.Model.DatabaseName);
                }
                var model = services.Model;
                foreach (var table in model.GetTables())
                {
                    //string createTableCommand = AccessDbSqlBuilder.GetCreateTableCommand(table);
                    //if (!string.IsNullOrEmpty(createTableCommand))
                    //    Execute(connection, transaction, createTableCommand);
                    var commands = AccessDbSqlBuilder.GetCreateTableCommands(table);
                    foreach (var command in commands)
                    {
                        Debug.Assert(command != null);
                        Execute(connection, transaction, command);
                    }
                }

                foreach (MetaTable table in model.GetTables())
                    foreach (string commandText in AccessDbSqlBuilder.GetCreateForeignKeyCommands(table))
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

        public bool DatabaseExists()
        {
            return File.Exists(dbName);
        }

        public void DeleteDatabase()
        {
            if (!deleted)
            {
                File.Delete(dbName);
                deleted = true;
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

        void IProvider.Initialize(IDataServices dataServices, object connection)
        {
            IDbConnection connection2;
            if (dataServices == null)
            {
                throw SqlClient.Error.ArgumentNull("dataServices");
            }
            var fileOrServerOrConnectionString = connection as string;
            if (fileOrServerOrConnectionString != null)
            {
                string connectionString = GetConnectionString(fileOrServerOrConnectionString);
                connection2 = new OleDbConnection(connectionString);
            }
            else
            {
                connection2 = connection as DbConnection;
                if (connection2 == null)
                    throw SqlClient.Error.InvalidConnectionArgument("connection");
            }
            dbName = GetDatabaseName(connection2.ConnectionString);
            Debug.Assert(connection2 is OleDbConnection);

            Initialize(dataServices, connection2);

        }

        protected override DbDataReader CreateRereader(DbDataReader reader)
        {
            return new AccessDbReader(reader);
        }

        private static string GetDatabaseName(string connectionString)
        {
            var builder = new OleDbConnectionStringBuilder(connectionString);
            return builder.DataSource;
        }

        private static string GetConnectionString(string fileOrServerOrConnectionString)
        {
            if (fileOrServerOrConnectionString.IndexOf('=') >= 0)
            {
                return fileOrServerOrConnectionString;
            }
            var builder = new OleDbConnectionStringBuilder();
            if (!fileOrServerOrConnectionString.StartsWith("Provider", StringComparison.OrdinalIgnoreCase) ||
                !fileOrServerOrConnectionString.EndsWith(".mdb", StringComparison.OrdinalIgnoreCase))
            {
                builder.DataSource = fileOrServerOrConnectionString;
                builder.Provider = "Microsoft.Jet.OLEDB.4.0";
                return builder.ConnectionString;
            }
            if (!fileOrServerOrConnectionString.StartsWith("Provider", StringComparison.OrdinalIgnoreCase) ||
                !fileOrServerOrConnectionString.EndsWith(".accdb", StringComparison.OrdinalIgnoreCase))
            {
                builder.DataSource = fileOrServerOrConnectionString;
                builder.Provider = "Microsoft.ACE.OLEDB.12.0";
                return builder.ConnectionString;
            }
            return fileOrServerOrConnectionString;
        }


        //public new DbTransaction Transaction
        //{
        //    get
        //    {
        //        CheckDispose();
        //        CheckInitialized();
        //        if (conManager.Transaction == null)
        //            return null;
        //        Debug.Assert(conManager.Transaction is AccessDbTransaction);
        //        return ((AccessDbTransaction)conManager.Transaction).Source;
        //    }
        //    set
        //    {
        //        CheckDispose();
        //        CheckInitialized();
        //        if (value is OleDbTransaction)
        //            conManager.Transaction = new AccessDbTransaction((OleDbTransaction)value);
        //        else
        //            conManager.Transaction = value;
        //    }
        //}

        //public new DbConnection Connection
        //{
        //    get
        //    {
        //        CheckDispose();
        //        CheckInitialized();
        //        return conManager.Connection;
        //        //Debug.Assert(conManager.Connection is AccessDbConnection);
        //        //return ((AccessDbConnection)conManager.Connection).Source;
        //    }
        //}


        internal override ISqlParameterizer CreateSqlParameterizer(ITypeSystemProvider typeProvider1, SqlNodeAnnotations annotations)
        {
            return new AccessParameterizer(typeProvider1, annotations);
        }

        internal override ITypeSystemProvider CreateTypeSystemProvider()
        {
            return new AccessDataTypeProvider();
            //return new AccessTypeSystemProvider();
        }

        internal override QueryConverter CreateQueryConverter(SqlFactory sql)
        {
            return new AccessDbQueryConverter(services, typeProvider, translator, sql)
            {
                ConverterStrategy = ConverterStrategy.CanUseJoinOn
            };
        }

        internal override DbFormatter CreateSqlFormatter()
        {
            return new AccessDbFormatter(this);
        }

        private AccessPostBindDotNetConverter postBindDotNetConverter;
        internal override PostBindDotNetConverter PostBindDotNetConverter
        {
            get
            {
                return postBindDotNetConverter ??
                       (postBindDotNetConverter = new AccessPostBindDotNetConverter(this.sqlFactory));
            }
        }

        internal override SqlFactory CreateSqlFactory(ITypeSystemProvider typeProvider1, MetaModel metaModel)
        {
            return new AccessSqlFactory(typeProvider1, metaModel);
        }

        #region IProviderExtend Members

        void IProviderExtend.CreateTable(MetaTable metaTable)
        {
            //var metaTable = services.Model.GetTable(metaTable);
            var sqlBuilder = new AccessDbSqlBuilder(this);
            var commands = sqlBuilder.GetCreateTableCommands(metaTable);
            //services.Context.ExecuteCommand(command);
            foreach (var command in commands)
            {
                Debug.Assert(command != null);
                services.Context.ExecuteCommand(command);
            }
        }

        bool IProviderExtend.TableExists(MetaTable metaTable)
        {
            const string sql = @"SELECT COUNT(*) FROM MSYSObjects WHERE Type = {0} AND [Name]= {1}";
            var count = services.Context.ExecuteQuery<int>(sql, 1, metaTable.TableName).Single();
            return count > 0;
        }

        void IProviderExtend.DeleteTable(MetaTable metaTable)
        {
            var commands = new AccessDbSqlBuilder(this).GetDropTableCommands(metaTable);
            foreach (var command in commands)
            {
                Debug.Assert(!string.IsNullOrEmpty(command));
                services.Context.ExecuteCommand(command);
            }
        }

        void IProviderExtend.CreateForeignKeys(MetaTable metaTable)
        {
            var sqlBuilder = new AccessDbSqlBuilder(this);
            var commands = sqlBuilder.GetCreateForeignKeyCommands(metaTable);
            foreach (var command in commands)
                services.Context.ExecuteCommand(command);
        }

        #endregion
    }
}
