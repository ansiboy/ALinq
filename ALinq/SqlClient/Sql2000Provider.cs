using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using ALinq;
using ALinq.Mapping;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ALinq.SqlClient
{
    /// <summary>
    /// Contains functionality to connect to and communicate with a SQL Server 2000.
    /// </summary>
    public sealed class Sql2000Provider : SqlProvider, IProvider
    {
        private string dbName;
        private bool deleted;

        /// <summary>
        /// Initializes a new instance of the ALinq.SqlClient.Sql2000Provider class.
        /// </summary>
        public Sql2000Provider()
            : base(ProviderMode.Sql2000)
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
            this.services = dataServices;
            DbTransaction transaction = null;
            var fileOrServerOrConnectionString = connection as string;
            if (fileOrServerOrConnectionString != null)
            {
                string connectionString = GetConnectionString(fileOrServerOrConnectionString);
                dbName = GetDatabaseName(connectionString);
                if (this.dbName.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
                {
                    this.Mode = ProviderMode.SqlCE;
                }
                if (this.Mode == ProviderMode.SqlCE)
                {
                    DbProviderFactory provider = GetProvider("System.Data.SqlServerCe.3.5");
                    if (provider == null)
                    {
                        throw Error.ProviderNotInstalled(this.dbName, "System.Data.SqlServerCe.3.5");
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
                    this.Mode = ProviderMode.SqlCE;
                }
                this.dbName = this.GetDatabaseName(connection2.ConnectionString);
            }
            using (DbCommand command = connection2.CreateCommand())
            {
                this.CommandTimeout = command.CommandTimeout;
            }
            int maxUsers = 1;
            if (connection2.ConnectionString.Contains("MultipleActiveResultSets"))
            {
                var builder = new DbConnectionStringBuilder
                                  {
                                      ConnectionString = connection2.ConnectionString
                                  };
                if (string.Compare((string)builder["MultipleActiveResultSets"], "true",
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
            var builder = new DbConnectionStringBuilder();
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
                builder.Add("Database", this.services.Model.DatabaseName);
                builder.Add("Integrated Security", "SSPI");
            }
            return builder.ToString();
        }

        private string GetDatabaseName(string constr)
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = constr };
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
            return this.services.Model.DatabaseName;
        }


        private static DbProviderFactory GetProvider(string providerName)
        {
            if (
                DbProviderFactories.GetFactoryClasses().Rows.OfType<DataRow>()
                .Select(r => (string)r["InvariantName"])
                .Contains(providerName, StringComparer.OrdinalIgnoreCase))
            {
                return DbProviderFactories.GetFactory(providerName);
            }
            return null;
        }

        bool IProvider.DatabaseExists()
        {
            this.CheckDispose();
            this.CheckInitialized();
            if (this.deleted)
            {
                return false;
            }
            bool flag = false;
            if (this.Mode == ProviderMode.SqlCE)
            {
                return File.Exists(this.dbName);
            }
            string connectionString = this.conManager.Connection.ConnectionString;
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
                    (string.Compare(this.conManager.Connection.ConnectionString, connectionString,
                                    StringComparison.Ordinal) != 0))
                {
                    this.conManager.Connection.ConnectionString = connectionString;
                }
            }
            return flag;
        }

        internal override QueryConverter CreateQueryConverter(SqlFactory sql)
        {
            return new SqlQueryConverter(services, typeProvider, translator, sql)
                       {
                           ConverterStrategy = ConverterStrategy.CanUseJoinOn | ConverterStrategy.CanUseRowStatus |
                                               ConverterStrategy.CanUseScopeIdentity
                       };
        }

        internal override DbFormatter CreateSqlFormatter()
        {
            return new MsSqlFormatter(this);
        }

        void IProvider.CreateDatabase()
        {
            var SqlBuilder = new SqlBuilder(SqlIdentifier);
            object obj3;
            this.CheckDispose();
            this.CheckInitialized();
            string dbName = null;
            string str2 = null;
            var builder = new DbConnectionStringBuilder();
            builder.ConnectionString = this.conManager.Connection.ConnectionString;
            if (this.conManager.Connection.State != ConnectionState.Closed)
            {
                object obj4;
                if ((this.Mode == ProviderMode.SqlCE) && File.Exists(this.dbName))
                {
                    throw Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(this.dbName);
                }
                if (builder.TryGetValue("Initial Catalog", out obj4))
                {
                    dbName = obj4.ToString();
                }
                if (builder.TryGetValue("Database", out obj4))
                {
                    dbName = obj4.ToString();
                }
                if (builder.TryGetValue("AttachDBFileName", out obj4))
                {
                    str2 = obj4.ToString();
                }
                goto Label_01D2;
            }
            if (this.Mode == ProviderMode.SqlCE)
            {
                if (!File.Exists(this.dbName))
                {
                    Type type =
                        this.conManager.Connection.GetType().Module.GetType("System.Data.SqlServerCe.SqlCeEngine");
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
                dbName = obj3.ToString();
                builder.Remove("Initial Catalog");
            }
            if (builder.TryGetValue("Database", out obj3))
            {
                dbName = obj3.ToString();
                builder.Remove("Database");
            }
            if (builder.TryGetValue("AttachDBFileName", out obj3))
            {
                str2 = obj3.ToString();
                builder.Remove("AttachDBFileName");
            }
        Label_0153:
            this.conManager.Connection.ConnectionString = builder.ToString();
        Label_01D2:
            if (string.IsNullOrEmpty(dbName))
            {
                if (string.IsNullOrEmpty(str2))
                {
                    if (string.IsNullOrEmpty(this.dbName))
                    {
                        throw Error.CouldNotDetermineCatalogName();
                    }
                    dbName = this.dbName;
                }
                else
                {
                    dbName = Path.GetFullPath(str2);
                }
            }
            this.conManager.UseConnection(this);
            this.conManager.AutoClose = false;
            try
            {
                if (this.services.Model.GetTables().FirstOrDefault<MetaTable>() == null)
                {
                    throw Error.CreateDatabaseFailedBecauseOfContextWithNoTables(this.services.Model.DatabaseName);
                }
                this.deleted = false;
                if (this.Mode == ProviderMode.SqlCE)
                {
                    foreach (MetaTable table in this.services.Model.GetTables())
                    {
                        string createTableCommand = SqlBuilder.GetCreateTableCommand(table);
                        if (!string.IsNullOrEmpty(createTableCommand))
                        {
                            this.ExecuteCommand(createTableCommand);
                        }
                    }
                    foreach (MetaTable table2 in this.services.Model.GetTables())
                    {
                        foreach (string str4 in SqlBuilder.GetCreateForeignKeyCommands(table2))
                        {
                            if (!string.IsNullOrEmpty(str4))
                            {
                                this.ExecuteCommand(str4);
                            }
                        }
                    }
                }
                else
                {
                    string command = SqlBuilder.GetCreateDatabaseCommand(dbName, str2,
                                                                         Path.ChangeExtension(str2, ".ldf"));
                    this.ExecuteCommand(command);
                    this.conManager.Connection.ChangeDatabase(dbName);
                    if (this.Mode == ProviderMode.Sql2005)
                    {
                        HashSet<string> set = new HashSet<string>();
                        foreach (MetaTable table3 in this.services.Model.GetTables())
                        {
                            string createSchemaForTableCommand = SqlBuilder.GetCreateSchemaForTableCommand(table3);
                            if (!string.IsNullOrEmpty(createSchemaForTableCommand))
                            {
                                set.Add(createSchemaForTableCommand);
                            }
                        }
                        foreach (string str7 in set)
                        {
                            this.ExecuteCommand(str7);
                        }
                    }
                    StringBuilder builder2 = new StringBuilder();
                    foreach (MetaTable table4 in this.services.Model.GetTables())
                    {
                        string str8 = SqlBuilder.GetCreateTableCommand(table4);
                        if (!string.IsNullOrEmpty(str8))
                        {
                            builder2.AppendLine(str8);
                        }
                    }
                    foreach (MetaTable table5 in this.services.Model.GetTables())
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
                        this.ExecuteCommand(builder2.ToString());
                    }
                }
            }
            finally
            {
                this.conManager.ReleaseConnection(this);
                if (this.conManager.Connection is SqlConnection)
                {
                    SqlConnection.ClearAllPools();
                }
            }
        }

        void IProvider.DeleteDatabase()
        {
            var SqlBuilder = new SqlBuilder(SqlIdentifier);

            this.CheckDispose();
            this.CheckInitialized();
            if (!this.deleted)
            {
                if (this.Mode == ProviderMode.SqlCE)
                {
                    ((IProvider)this).ClearConnection();
                    File.Delete(this.dbName);
                    this.deleted = true;
                }
                else
                {
                    string connectionString = this.conManager.Connection.ConnectionString;
                    IDbConnection connection = this.conManager.UseConnection(this);
                    try
                    {
                        connection.ChangeDatabase("MASTER");
                        if (connection is SqlConnection)
                        {
                            SqlConnection.ClearAllPools();
                        }
                        if (this.Log != null)
                        {
                            this.Log.WriteLine(Strings.LogAttemptingToDeleteDatabase(this.dbName));
                        }
                        this.ExecuteCommand(SqlBuilder.GetDropDatabaseCommand(this.dbName));
                        this.deleted = true;
                    }
                    finally
                    {
                        this.conManager.ReleaseConnection(this);
                        if ((this.conManager.Connection.State == ConnectionState.Closed) &&
                            (string.Compare(this.conManager.Connection.ConnectionString, connectionString,
                                            StringComparison.Ordinal) != 0))
                        {
                            this.conManager.Connection.ConnectionString = connectionString;
                        }
                    }
                }
            }
        }

        private void ExecuteCommand(string command)
        {
            if (this.Log != null)
            {
                this.Log.WriteLine(command);
                this.Log.WriteLine();
            }
            IDbCommand command2 = this.conManager.Connection.CreateCommand();
            command2.CommandTimeout = this.CommandTimeout;
            command2.Transaction = this.conManager.Transaction;
            command2.CommandText = command;
            command2.ExecuteNonQuery();
        }

        internal override ITypeSystemProvider CreateTypeSystemProvider()
        {
            return new SqlTypeSystem.Sql2000Provider();
        }
    }
}