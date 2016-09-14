using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ALinq.Mapping;
using System.ComponentModel;

namespace ALinq.SqlClient
{
    /// <summary>
    /// Contains functionality to connect to and communicate with a SQL Server 2005.
    /// </summary>
    public sealed class Sql2005Provider : SqlProvider, IProvider
    {
        private string dbName;
        private bool deleted;

        /// <summary>
        /// Initializes a new instance of the ALinq.SqlClient.Sql2005Provider class.
        /// </summary>
        public Sql2005Provider()
            : base(ProviderMode.Sql2005)
        {

        }

        void IProvider.Initialize(IDataServices dataServices, object connection)
        {
            DbConnection connection2;
            Type type;
            if (dataServices == null)
            {
                throw Error.ArgumentNull("dataServices");
            }
            services = dataServices;
            DbTransaction transaction = null;
            string fileOrServerOrConnectionString = connection as string;
            if (fileOrServerOrConnectionString != null)
            {
                string connectionString = GetConnectionString(fileOrServerOrConnectionString);
                dbName = GetDatabaseName(connectionString);
                if (dbName.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
                {
                    Mode = ProviderMode.SqlCE;
                }
                if (Mode == ProviderMode.SqlCE)
                {
                    DbProviderFactory provider = GetProvider("System.Data.SqlServerCe.3.5");
                    if (provider == null)
                    {
                        throw Error.ProviderNotInstalled(dbName, "System.Data.SqlServerCe.3.5");
                    }
                    connection2 = provider.CreateConnection();
                }
                else
                {
                    connection2 = new SqlConnection();
                }
                connection2.ConnectionString = connectionString;
            }
            else
            {
                transaction = connection as SqlTransaction;
                if ((transaction == null) &&
                    (connection.GetType().FullName == "System.Data.SqlServerCe.SqlCeTransaction"))
                {
                    transaction = connection as DbTransaction;
                }
                if (transaction != null)
                {
                    connection = transaction.Connection;
                }
                connection2 = connection as DbConnection;
                if (connection2 == null)
                {
                    throw Error.InvalidConnectionArgument("connection");
                }
                if (connection2.GetType().FullName == "System.Data.SqlServerCe.SqlCeConnection")
                {
                    Mode = ProviderMode.SqlCE;
                }
                dbName = GetDatabaseName(connection2.ConnectionString);
            }
            using (DbCommand command = connection2.CreateCommand())
            {
                CommandTimeout = command.CommandTimeout;
            }
            int maxUsers = 1;
            if (connection2.ConnectionString.Contains("MultipleActiveResultSets"))
            {
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
                builder.ConnectionString = connection2.ConnectionString;
                if (
                    string.Compare((string)builder["MultipleActiveResultSets"], "true",
                                   StringComparison.OrdinalIgnoreCase) == 0)
                {
                    maxUsers = 10;
                }
            }
            conManager = new SqlConnectionManager(this, connection2, maxUsers);
            if (transaction != null)
            {
                conManager.Transaction = transaction;
            }
            if (Mode == ProviderMode.SqlCE)
            {
                type = connection2.GetType().Module.GetType("System.Data.SqlServerCe.SqlCeDataReader");
            }
            else if (connection2 is SqlConnection)
            {
                type = typeof(SqlDataReader);
            }
            else
            {
                type = typeof(DbDataReader);
            }
            readerCompiler = new ObjectReaderCompiler(type, services);
            //InvokeIProviderMethod("Initialize", new[] { SourceService(conManager.Connection), connection });
        }

        private string GetConnectionString(string fileOrServerOrConnectionString)
        {
            if (fileOrServerOrConnectionString.IndexOf('=') >= 0)
            {
                return fileOrServerOrConnectionString;
            }
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            if (fileOrServerOrConnectionString.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add("AttachDBFileName", fileOrServerOrConnectionString);
                builder.Add("Server", @"localhost\sqlexpress");
                builder.Add("Integrated Security", "SSPI");
                builder.Add("User Instance", "true");
                builder.Add("MultipleActiveResultSets", "true");
            }
            else if (fileOrServerOrConnectionString.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add("Data Source", fileOrServerOrConnectionString);
            }
            else
            {
                builder.Add("Server", fileOrServerOrConnectionString);
                builder.Add("Database", services.Model.DatabaseName);
                builder.Add("Integrated Security", "SSPI");
            }
            return builder.ToString();
        }

        private string GetDatabaseName(string constr)
        {
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            builder.ConnectionString = constr;
            if (builder.ContainsKey("Initial Catalog"))
            {
                return (string)builder["Initial Catalog"];
            }
            if (builder.ContainsKey("Database"))
            {
                return (string)builder["Database"];
            }
            if (builder.ContainsKey("AttachDBFileName"))
            {
                return (string)builder["AttachDBFileName"];
            }
            if (builder.ContainsKey("Data Source") &&
                ((string)builder["Data Source"]).EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
            {
                return (string)builder["Data Source"];
            }
            return services.Model.DatabaseName;
        }


        private static DbProviderFactory GetProvider(string providerName)
        {
            if (
                DbProviderFactories.GetFactoryClasses().Rows.OfType<DataRow>().Select(
                    delegate(DataRow r) { return (string)r["InvariantName"]; }).Contains(providerName,
                                                                                          StringComparer.
                                                                                              OrdinalIgnoreCase))
            {
                return DbProviderFactories.GetFactory(providerName);
            }
            return null;
        }

        bool IProvider.DatabaseExists()
        {
            CheckDispose();
            CheckInitialized();
            if (deleted)
            {
                return false;
            }
            bool flag = false;
            if (Mode == ProviderMode.SqlCE)
            {
                return File.Exists(dbName);
            }
            string connectionString = conManager.Connection.ConnectionString;
            try
            {
                conManager.UseConnection(this);
                conManager.Connection.ChangeDatabase(dbName);
                conManager.ReleaseConnection(this);
                flag = true;
            }
            catch (Exception exc)
            {

            }
            finally
            {
                if ((conManager.Connection.State == ConnectionState.Closed) &&
                    (string.Compare(conManager.Connection.ConnectionString, connectionString,
                                    StringComparison.Ordinal) != 0))
                {
                    conManager.Connection.ConnectionString = connectionString;
                }
            }
            return flag;
        }

        internal override QueryConverter CreateQueryConverter(SqlFactory sql)
        {
            return new SqlQueryConverter(services, typeProvider, translator, sql);
        }

        internal override DbFormatter CreateSqlFormatter()
        {
            return new MsSqlFormatter(this);
        }

        void IProvider.CreateDatabase()
        {
            var SqlBuilder = new SqlBuilder(SqlIdentifier);
            object obj3;
            CheckDispose();
            CheckInitialized();
            string databaseName = null;
            string str2 = null;
            var builder = new DbConnectionStringBuilder();
            builder.ConnectionString = conManager.Connection.ConnectionString;
            if (conManager.Connection.State != ConnectionState.Closed)
            {
                object obj4;
                if ((Mode == ProviderMode.SqlCE) && File.Exists(this.dbName))
                {
                    throw Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(this.dbName);
                }
                if (builder.TryGetValue("Initial Catalog", out obj4))
                {
                    databaseName = obj4.ToString();
                }
                if (builder.TryGetValue("Database", out obj4))
                {
                    databaseName = obj4.ToString();
                }
                if (builder.TryGetValue("AttachDBFileName", out obj4))
                {
                    str2 = obj4.ToString();
                }
                goto Label_01D2;
            }
            if (Mode == ProviderMode.SqlCE)
            {
                if (!File.Exists(this.dbName))
                {
                    Type type =
                        conManager.Connection.GetType().Module.GetType("System.Data.SqlServerCe.SqlCeEngine");
                    object target = Activator.CreateInstance(type, new object[] { builder.ToString() });
                    try
                    {
                        type.InvokeMember("CreateDatabase",
                                          BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null,
                                          target, new object[0], CultureInfo.InvariantCulture);
                        goto Label_0153;
                    }
                    catch (TargetInvocationException exception)
                    {
                        throw exception.InnerException;
                    }
                    finally
                    {
                        IDisposable disposable = target as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                }
                throw Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(this.dbName);
            }
            if (builder.TryGetValue("Initial Catalog", out obj3))
            {
                databaseName = obj3.ToString();
                builder.Remove("Initial Catalog");
            }
            if (builder.TryGetValue("Database", out obj3))
            {
                databaseName = obj3.ToString();
                builder.Remove("Database");
            }
            if (builder.TryGetValue("AttachDBFileName", out obj3))
            {
                str2 = obj3.ToString();
                builder.Remove("AttachDBFileName");
            }
        Label_0153:
            conManager.Connection.ConnectionString = builder.ToString();
        Label_01D2:
            if (string.IsNullOrEmpty(databaseName))
            {
                if (string.IsNullOrEmpty(str2))
                {
                    if (string.IsNullOrEmpty(this.dbName))
                    {
                        throw Error.CouldNotDetermineCatalogName();
                    }
                    databaseName = this.dbName;
                }
                else
                {
                    databaseName = Path.GetFullPath(str2);
                }
            }
            conManager.UseConnection(this);
            conManager.AutoClose = false;
            try
            {
                if (services.Model.GetTables().FirstOrDefault() == null)
                {
                    throw Error.CreateDatabaseFailedBecauseOfContextWithNoTables(services.Model.DatabaseName);
                }
                deleted = false;
                if (Mode == ProviderMode.SqlCE)
                {
                    foreach (MetaTable table in services.Model.GetTables())
                    {
                        string createTableCommand = SqlBuilder.GetCreateTableCommand(table);
                        if (!string.IsNullOrEmpty(createTableCommand))
                        {
                            ExecuteCommand(createTableCommand);
                        }
                    }
                    foreach (MetaTable table2 in services.Model.GetTables())
                    {
                        foreach (string str4 in SqlBuilder.GetCreateForeignKeyCommands(table2))
                        {
                            if (!string.IsNullOrEmpty(str4))
                            {
                                ExecuteCommand(str4);
                            }
                        }
                    }
                }
                else
                {
                    string command = SqlBuilder.GetCreateDatabaseCommand(databaseName, str2,
                                                                         Path.ChangeExtension(str2, ".ldf"));
                    ExecuteCommand(command);
                    conManager.Connection.ChangeDatabase(databaseName);
                    if (Mode == ProviderMode.Sql2005)
                    {
                        var set = new HashSet<string>();
                        foreach (MetaTable table3 in services.Model.GetTables())
                        {
                            string createSchemaForTableCommand = SqlBuilder.GetCreateSchemaForTableCommand(table3);
                            if (!string.IsNullOrEmpty(createSchemaForTableCommand))
                            {
                                set.Add(createSchemaForTableCommand);
                            }
                        }
                        foreach (string str7 in set)
                        {
                            ExecuteCommand(str7);
                        }
                    }
                    var builder2 = new StringBuilder();
                    foreach (MetaTable table4 in services.Model.GetTables())
                    {
                        string str8 = SqlBuilder.GetCreateTableCommand(table4);
                        if (!string.IsNullOrEmpty(str8))
                        {
                            builder2.AppendLine(str8);
                        }
                    }
                    foreach (MetaTable table5 in services.Model.GetTables())
                    {
                        foreach (string str9 in SqlBuilder.GetCreateForeignKeyCommands(table5))
                        {
                            if (!string.IsNullOrEmpty(str9))
                            {
                                builder2.AppendLine(str9);
                            }
                        }
                    }
                    if (builder2.Length > 0)
                    {
                        builder2.Insert(0, "SET ARITHABORT ON" + Environment.NewLine);
                        ExecuteCommand(builder2.ToString());
                    }
                }
            }
            finally
            {
                conManager.ReleaseConnection(this);
                if (conManager.Connection is SqlConnection)
                {
                    SqlConnection.ClearAllPools();
                }
            }
        }

        void IProvider.DeleteDatabase()
        {
            var SqlBuilder = new SqlBuilder(SqlIdentifier);
            CheckDispose();
            CheckInitialized();
            if (!deleted)
            {
                if (Mode == ProviderMode.SqlCE)
                {
                    ((IProvider)this).ClearConnection();
                    File.Delete(dbName);
                    deleted = true;
                }
                else
                {
                    string connectionString = conManager.Connection.ConnectionString;
                    IDbConnection connection = conManager.UseConnection(this);
                    try
                    {
                        connection.ChangeDatabase("MASTER");
                        if (connection is SqlConnection)
                        {
                            SqlConnection.ClearAllPools();
                        }
                        if (Log != null)
                        {
                            Log.WriteLine(Strings.LogAttemptingToDeleteDatabase(dbName));
                        }
                        ExecuteCommand(SqlBuilder.GetDropDatabaseCommand(dbName));
                        deleted = true;
                    }
                    finally
                    {
                        conManager.ReleaseConnection(this);
                        if ((conManager.Connection.State == ConnectionState.Closed) &&
                            (string.Compare(conManager.Connection.ConnectionString, connectionString,
                                            StringComparison.Ordinal) != 0))
                        {
                            conManager.Connection.ConnectionString = connectionString;
                        }
                    }
                }
            }
        }

        private void ExecuteCommand(string command)
        {
            if (Log != null)
            {
                Log.WriteLine(command);
                Log.WriteLine();
            }
            IDbCommand command2 = conManager.Connection.CreateCommand();
            command2.CommandTimeout = CommandTimeout;
            command2.Transaction = conManager.Transaction;
            command2.CommandText = command;
            command2.ExecuteNonQuery();
        }

        internal override ITypeSystemProvider CreateTypeSystemProvider()
        {
            return new SqlTypeSystem.Sql2005Provider();
        }
    }
}