using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ALinq.Mapping;
using ALinq.SqlClient;
using FirebirdSql.Data.FirebirdClient;


namespace ALinq.Firebird
{
    public class FirebirdProvider : SqlProvider, IProvider, IProviderExtend
    {
        private string dbName;
        private string connectionString;
        private bool deleted;

        public FirebirdProvider()
            : base(ProviderMode.Firebird)
        {

        }

        void IProvider.CreateDatabase()
        {
            var connection = ((FbConnection)Connection);
            var builder = new FbConnectionStringBuilder(connection.ConnectionString);

            if (File.Exists(builder.Database))
                throw SqlClient.Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(services.Model.DatabaseName);

            FbConnection.CreateDatabase(connection.ConnectionString);

            connection.Open();
            var transaction = connection.BeginTransaction();

            var sqlBuilder = FirebirdSqlBuilder.CreateInstance(this);
            try
            {
                if (services.Model.GetTables().Count() == 0)
                {
                    throw SqlClient.Error.CreateDatabaseFailedBecauseOfContextWithNoTables(services.Model.DatabaseName);
                }
                var model = services.Model;
                //foreach (var table in model.GetTables())
                //{
                //    string createTableCommand = sqlBuilder.GetCreateTableCommand(table);

                //    if (!string.IsNullOrEmpty(createTableCommand))
                //        Execute(connection, transaction, createTableCommand);
                //}

                //Create Generator
                //foreach (var table in model.GetTables())
                //{
                //    if (table.RowType.DBGeneratedIdentityMember != null)
                //    {
                //        var commandText = "CREATE SEQUENCE  " + FirebirdSqlBuilder.GetSequenceName(table.RowType.DBGeneratedIdentityMember);
                //        Execute(connection, transaction, commandText);
                //    }
                //}

                //foreach (var table in model.GetTables())
                //{
                //    var commandText = sqlBuilder.GetCreatePrimaryKeyCommand(table);
                //    if (!string.IsNullOrEmpty(commandText))
                //        Execute(connection, transaction, commandText);
                //}

                foreach (var table in model.GetTables())
                {
                    var commands = sqlBuilder.GetCreateTableCommands(table);
                    foreach (var command in commands)
                        Execute(connection, transaction, command);
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

        public bool DatabaseExists()
        {
            var builder = new FbConnectionStringBuilder(Connection.ConnectionString);
            if (builder.ServerType == FbServerType.Embedded)
            {
                var fileName = builder.Database;
                return File.Exists(fileName);
            }
            var flag = false;
            try
            {
                conManager.UseConnection(this);
                conManager.Connection.ChangeDatabase(builder.Database);
                conManager.ReleaseConnection(this);
                flag = true;
            }
            catch (Exception)
            {
            }
            finally
            {
                if ((this.conManager.Connection.State == ConnectionState.Closed) &&
                    (string.Compare(this.conManager.Connection.ConnectionString, connectionString,
                                    StringComparison.Ordinal) != 0))
                {
                    //this.conManager.Connection.ConnectionString = connectionString;
                }
            }
            return flag;
        }

        public void DeleteDatabase()
        {
            if (!deleted)
            {
                //File.Delete(dbName);
                FbConnection.ClearAllPools();
                FbConnection.DropDatabase(Connection.ConnectionString);
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
            if (dataServices == null)
                throw SqlClient.Error.ArgumentNull("dataServices");

            IDbConnection connection2;
            if (connection is string)
            {
                var constr = ((string)connection);

                if (constr.EndsWith(".fdb", true, CultureInfo.CurrentCulture))
                {
                    var builder = new FbConnectionStringBuilder()
                    {
                        Database = constr,
                        DataSource = "localhost",
                        ServerType = FbServerType.Default,
                        UserID = "SYSDBA",
                        Password = "masterkey"
                    };
                    connection2 = new FbConnection(builder.ToString());
                }
                else
                    connection2 = new FbConnection(constr);
            }
            else if (connection is IDbConnection)
            {
                connection2 = (IDbConnection)connection;
            }
            else
                throw SqlClient.Error.InvalidConnectionArgument("connection");

            Initialize(dataServices, connection2);
        }

        internal override SqlIdentifier SqlIdentifier
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return FirebirdIdentifier.Instance;
            }
        }

        internal override DbFormatter CreateSqlFormatter()
        {
            return new FirebirdFormatter(this);
        }

        internal override ITypeSystemProvider CreateTypeSystemProvider()
        {
            return new FirebirdDataTypeProvider();
        }

        //internal override void AssignParameters(System.Data.Common.DbCommand cmd, ReadOnlyCollection<SqlParameterInfo> parms, object[] userArguments, object lastResult)
        //{
        //    parms = new ReadOnlyCollection<SqlParameterInfo>(parms.Where(o => o.Parameter.Name != "NULL").ToArray());
        //    base.AssignParameters(cmd, parms, userArguments, lastResult);
        //}

        internal override ISqlParameterizer CreateSqlParameterizer(ITypeSystemProvider typeProvider, SqlNodeAnnotations annotations)
        {
            return new FirebirdParameterizer(typeProvider, annotations);
        }

        private FbPostBindDotNetConverter postBindDotNetConverter;
        internal override PostBindDotNetConverter PostBindDotNetConverter
        {
            get
            {
                if (postBindDotNetConverter == null)
                    postBindDotNetConverter = new FbPostBindDotNetConverter(this.sqlFactory);
                return postBindDotNetConverter;
                //return FbPostBindDotNetConverter.Instance;
            }
        }


        internal override QueryConverter CreateQueryConverter(SqlFactory sql)
        {
            return new FirdbirdQueryConverter(services, typeProvider, translator, sql)
            {
                ConverterStrategy = ConverterStrategy.CanUseJoinOn
            };
        }

        internal override void AssignParameters(System.Data.Common.DbCommand cmd, ReadOnlyCollection<SqlParameterInfo> parms, object[] userArguments, object lastResult)
        {
            if (cmd.CommandText.StartsWith("SELECT"))
            {
                var list = new List<SqlParameterInfo>(parms);
                var regex = new Regex(SqlIdentifier.ParameterPrefix + "[x][0-9]+");
                cmd.CommandText = regex.Replace(cmd.CommandText, new MatchEvaluator(delegate(Match match)
                                                  {
                                                      var pName = match.Value;
                                                      var parameter =
                                                          parms.Where(o => o.Parameter.Name == pName).Single();
                                                      list.Remove(parameter);
                                                      var value = parameter.Value;
                                                      if (value == null)
                                                          return "''";
                                                      if (value is DateTime)
                                                          return "'" + ((DateTime)value).ToShortDateString() + "'";
                                                      else if (value is bool)
                                                      {
                                                          return ((bool)value) ? "1" : "0";
                                                      }
                                                      else if (!(value.GetType().IsValueType))
                                                          return "'" + value + "'";
                                                      else
                                                          return value.ToString();
                                                  }));
                if (list.Count != parms.Count)
                {
                    base.AssignParameters(cmd, new ReadOnlyCollection<SqlParameterInfo>(list), userArguments, lastResult);
                    return;
                }
            }
            base.AssignParameters(cmd, parms, userArguments, lastResult);
        }

        internal override SqlFactory CreateSqlFactory(ITypeSystemProvider typeProvider, MetaModel metaModel)
        {
            return new FirebirdSqlFactory(typeProvider, metaModel);
        }


        #region IProviderExtend Members

        void IProviderExtend.CreateTable(MetaTable metaTable)
        {
            //var metaTable = services.Model.GetTable(metaTable);
            //var sqlBuilder = new FirebirdSqlBuilder(this);
            //var command = sqlBuilder.GetCreateTableCommand(metaTable);
            //services.Context.ExecuteCommand(command);

            var sqlBuilder = new FirebirdSqlBuilder(this);
            var commands = sqlBuilder.GetCreateTableCommands(metaTable);
            foreach (var command in commands)
            {
                Debug.Assert(command != null);
                services.Context.ExecuteCommand(command);
            }
        }

        bool IProviderExtend.TableExists(MetaTable metaTable)
        {
            // var metaTable = services.Model.GetTable(metaTable);
            var sql = @"SELECT COUNT(*) FROM RDB$RELATIONS WHERE UPPER(RDB$RELATION_NAME) = {0}";
            var count = services.Context.ExecuteQuery<int>(sql, metaTable.TableName.ToUpper()).Single();
            return count > 0;
        }

        void IProviderExtend.DeleteTable(MetaTable metaTable)
        {
            //var metaTable = services.Model.GetTable(metaTable);
            //var sql = "DROP TABLE {0}";
            //sql = string.Format(sql, metaTable.TableName);
            //services.Context.ExecuteCommand(sql);


            var commands = new FirebirdSqlBuilder(this).GetDropTableCommands(metaTable);
            foreach (var command in commands)
            {
                Debug.Assert(!string.IsNullOrEmpty(command));
                services.Context.ExecuteCommand(command);
            }
        }

        void IProviderExtend.CreateForeignKeys(MetaTable metaTable)
        {
            //var metaTable = services.Model.GetTable(metaTable);
            var sqlBuilder = new FirebirdSqlBuilder(this);
            var commands = sqlBuilder.GetCreateForeignKeyCommands(metaTable);
            foreach (var command in commands)
                services.Context.ExecuteCommand(command);
        }

        #endregion
    }
}
