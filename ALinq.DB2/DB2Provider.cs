using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ALinq.Mapping;
using ALinq.SqlClient;
using IBM.Data.DB2;
using System.Data.Common;

namespace ALinq.DB2
{
    public class DB2Provider : SqlProvider, IProvider, IProviderExtend
    {
        private string connectionString;
        private string dbName;
        private const bool deleted = false;
        private SqlIdentifier sqlIdentifier;

        public DB2Provider()
            : base(ProviderMode.DB2)
        {

        }

        internal override QueryConverter CreateQueryConverter(SqlFactory sql)
        {
            return new DB2QueryConverter(services, typeProvider, translator, sql)
                       {
                           ConverterStrategy = ConverterStrategy.CanUseJoinOn | ConverterStrategy.SkipWithRowNumber
                       };
        }

        internal override DbFormatter CreateSqlFormatter()
        {
            return new DB2Formatter(this);
        }

        protected override DbDataReader CreateRereader(DbDataReader reader)
        {
            return new DataReader((DB2DataReader)reader);
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

        public bool DatabaseExists()
        {
            this.CheckDispose();
            this.CheckInitialized();
            if (deleted)
            {
                return false;
            }
            bool flag = false;
            string constr = this.conManager.Connection.ConnectionString;
            try
            {
                this.conManager.UseConnection(this);
                this.conManager.Connection.ChangeDatabase(this.dbName);
                this.conManager.ReleaseConnection(this);
                flag = true;
            }
            catch (Exception)
            {
            }
            finally
            {
                if ((this.conManager.Connection.State == ConnectionState.Closed) &&
                    (string.Compare(this.conManager.Connection.ConnectionString, constr,
                                    StringComparison.Ordinal) != 0))
                {
                    this.conManager.Connection.ConnectionString = constr;
                }
            }
            return flag;
        }

        void IProvider.Initialize(IDataServices dataServices, object connection)
        {
            if (dataServices == null)
            {
                throw SqlClient.Error.ArgumentNull("dataServices");
            }
            DbConnection conn;
            if (connection is string)
                conn = new DB2Connection((string)connection);
            else
                conn = ((DbConnection)connection);

            connectionString = conn.ConnectionString;
            var builder = new DB2ConnectionStringBuilder(connectionString);
            dbName = builder.Database;
            if (string.IsNullOrEmpty(dbName))
                dbName = dataServices.Model.DatabaseName;
            if (string.IsNullOrEmpty(dbName))
                throw SqlClient.Error.CouldNotDetermineCatalogName();

            Initialize(dataServices, conn);
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

        void IProvider.CreateDatabase()
        {
            if (DatabaseExists())
            {
                throw SqlClient.Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(dbName);
            }

            //创建数据库文件
            var connection = new DB2Connection(GetConnectionString(null));
            connection.Open();

            var sqlBuilder = new DB2Builder(this);

            var transaction = connection.BeginTransaction();
            Execute(connection, transaction, sqlBuilder.GetCreateDatabaseCommand(dbName, null));
            try
            {
                MetaModel model = services.Model;
                foreach (MetaTable table in model.GetTables())
                {
                    string createTableCommand = sqlBuilder.GetCreateTableCommand(table);

                    if (!string.IsNullOrEmpty(createTableCommand))
                        Execute(connection, transaction, createTableCommand);
                }

                foreach (MetaTable table in model.GetTables())
                    foreach (string commandText in sqlBuilder.GetCreateForeignKeyCommands(table))
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



        void IProvider.DeleteDatabase()
        {
            //throw new NotImplementedException();
        }

        private string GetConnectionString(string dataBaseName)
        {
            var builder = new DB2ConnectionStringBuilder(connectionString) { Database = dataBaseName };
            return builder.ConnectionString;
        }

        internal override SqlIdentifier SqlIdentifier
        {
            get
            {
                return DB2Identifier.Instance;
            }
        }

        internal override ITypeSystemProvider CreateTypeSystemProvider()
        {
            return new DB2DataTypeProvider();
        }

        internal override SqlFactory CreateSqlFactory(ITypeSystemProvider typeProvider, MetaModel metaModel)
        {
            return new DB2Factory(typeProvider, metaModel);
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
            var builder = new DB2ConnectionStringBuilder(connectionString);
            var sql = string.Format("SELECT COUNT(*) FROM SYSCAT.TABLES WHERE OWNER = '{0}' AND TABNAME = '{1}'", builder.UserID.ToUpper(), metaTable.TableName.ToUpper());
            var count = services.Context.ExecuteQuery<int>(sql).SingleOrDefault();
            return count > 0;
        }

        void IProviderExtend.DeleteTable(MetaTable metaTable)
        {
            var sql = string.Format("DROP TABLE {0}", metaTable.TableName.ToUpper());
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

        private DB2Builder sqlBuilder;
        DB2Builder SqlBuilder
        {
            get
            {
                if (sqlBuilder == null)
                    sqlBuilder = new DB2Builder(this);
                return sqlBuilder;
            }
        }
        #endregion
    }

}
