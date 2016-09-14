using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using ALinq;
using ALinq.Mapping;
using System.Data.SQLite;
using System.IO;
using ALinq.SqlClient;

namespace ALinq.SQLite
{
    public class SQLiteProvider : SqlProvider, IProvider, IProviderExtend
    {
        private string dbName;
        //private IDataServices services;
        private bool deleted;

        public SQLiteProvider()
            : base(ProviderMode.SQLite)
        {

        }

        #region IProvider Members

        void IProvider.CreateDatabase()
        {
            if (File.Exists(dbName))
            {
                throw SqlClient.Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(dbName);
            }

            //创数据库文件
            SQLiteConnection.CreateFile(dbName);

            //var connection = new SQLiteConnection(GetConnectionString(dbName));
            //services.Context.Connection.Open();
            //var transaction = services.Context.Connection.BeginTransaction();
            //services.Context.Transaction = transaction;
            try
            {
                var SQLiteSqlBuilder = new SQLiteSqlBuilder(this);
                MetaModel model = services.Model;
                foreach (MetaTable table in model.GetTables())
                {
                    //string createTableCommand = SQLiteSqlBuilder.GetCreateTableCommand(table);

                    //if (!string.IsNullOrEmpty(createTableCommand))
                    //    Execute(connection, transaction, createTableCommand);
                    var commands = SQLiteSqlBuilder.GetCreateTableCommands(table);
                    foreach (var command in commands)
                    {
                        //Execute(connection, transaction, command);
                        services.Context.ExecuteCommand(command);
                    }
                }

                //-----------------SQLite不支持外键--------------------/
                /*
                foreach (MetaTable table in model.GetTables())
                    foreach (string commandText in SQLiteSqlBuilder.GetCreateForeignKeyCommands(table))
                        if (!string.IsNullOrEmpty(commandText))
                            Execute(connection, transaction, commandText);
                */
                //transaction.Commit();
            }
            catch
            {
                //transaction.Rollback();
                throw;
            }
            finally
            {
                //Connection.Close();
            }
        }

        bool IProvider.DatabaseExists()
        {
            return File.Exists(dbName);
        }

        void IProvider.DeleteDatabase()
        {
            if (!deleted)
            {
                ((IProvider)this).ClearConnection();
                File.Delete(dbName);
                deleted = true;
            }
        }

        void IProvider.Initialize(IDataServices dataServices, object connection)
        {
            //Debug.Assert(connection is SQLiteConnection);
            if (dataServices == null)
                throw SqlClient.Error.ArgumentNull("dataServices");

            var connection2 = connection as SQLiteConnection;
            if (connection2 == null)
            {
                var fileOrConnectionString = connection as string;
                if (fileOrConnectionString == null)
                    throw SqlClient.Error.InvalidConnectionArgument("connection");
                string connectionString = GetConnectionString(fileOrConnectionString);
                connection2 = new SQLiteConnection(connectionString);
            }
            services = dataServices;
            dbName = GetDatabaseName(connection2.ConnectionString);
            Initialize(dataServices, connection2);
        }

        #endregion

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

        private static string GetDatabaseName(string connectionString)
        {
            var builder = new SQLiteConnectionStringBuilder(connectionString);
            return builder.DataSource;
        }

        private static string GetConnectionString(string fileOrConnectionString)
        {
            if (fileOrConnectionString.IndexOf('=') >= 0)
            {
                return fileOrConnectionString;
            }
            var builder = new SQLiteConnectionStringBuilder { DataSource = fileOrConnectionString };
            return builder.ConnectionString;
        }

        //public new DbConnection Connection
        //{
        //    get
        //    {
        //        this.CheckDispose();
        //        this.CheckInitialized();
        //        return ((Connection)this.conManager.Connection).Source;
        //    }
        //}

        internal override QueryConverter CreateQueryConverter(SqlFactory sql)
        {
            return new SQLiteQueryConverter(services, typeProvider, translator, sql)
                       {
                           ConverterStrategy = ConverterStrategy.CanOutputFromInsert | ConverterStrategy.CanUseJoinOn |
                                               ConverterStrategy.CanUseOuterApply & ConverterStrategy.CanUseRowStatus
                       };
        }

        internal override DbFormatter CreateSqlFormatter()
        {
            return new SQLiteFormatter(this);
        }

        internal override SqlFactory CreateSqlFactory(ITypeSystemProvider typeProvider, MetaModel metaModel)
        {
            return new SQLiteSqlFactory(typeProvider, metaModel);
        }

        private PostBindDotNetConverter postBindDotNetConverter;
        internal override PostBindDotNetConverter PostBindDotNetConverter
        {
            get
            {
                if (postBindDotNetConverter == null)
                    postBindDotNetConverter = new SQLitePostBindDotNetConverter(this.sqlFactory);
                return postBindDotNetConverter;
            }
        }

        internal override ITypeSystemProvider CreateTypeSystemProvider()
        {
            //return new SqlTypeSystem.Sql2000Provider();
            return new SQLiteDataTypeProvider();
        }

        protected override DbDataReader CreateRereader(DbDataReader reader)
        {
            return new DataReader(reader);
        }

        #region IProviderExtend Members

        void IProviderExtend.CreateTable(MetaTable metaTable)
        {
            var sqlBuilder = new SQLiteSqlBuilder(this);
            var commands = sqlBuilder.GetCreateTableCommands(metaTable);
            foreach (var command in commands)
            {
                services.Context.ExecuteCommand(command);
            }
        }

        bool IProviderExtend.TableExists(MetaTable metaTable)
        {
            //var metaTable = services.Model.GetTable(metaTable);
            var sql = @"SELECT COUNT(*) FROM sqlite_master WHERE Name= {0}";
            var count = services.Context.ExecuteQuery<int>(sql, metaTable.TableName).SingleOrDefault();
            return count > 0;
        }

        void IProviderExtend.DeleteTable(MetaTable metaTable)
        {
            //var metaTable = services.Model.GetTable(metaTable);
            var sql = "DROP TABLE {0}";
            sql = string.Format(sql, metaTable.TableName);
            services.Context.ExecuteCommand(sql);
        }

        void IProviderExtend.CreateForeignKeys(MetaTable metaTable)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
