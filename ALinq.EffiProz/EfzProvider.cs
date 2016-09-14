using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EffiProz;
using System.Linq;
using System.Text;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.EffiProz
{
    public class EfzProvider : ALinq.SqlClient.SqlProvider, IProvider
    {
        public EfzProvider()
            : base(ProviderMode.EffiProz)
        {
        }

        internal override QueryConverter CreateQueryConverter(SqlFactory sql)
        {
            return new EfzQueryConverter(services, typeProvider, translator, sql);
        }

        internal override DbFormatter CreateSqlFormatter()
        {
            return new EfzFormatter(this);
        }

        internal override ISqlParameterizer CreateSqlParameterizer(ITypeSystemProvider typeProvider, SqlNodeAnnotations annotations)
        {
            return new EfzParameterizer(typeProvider, annotations);
        }

        internal override ITypeSystemProvider CreateTypeSystemProvider()
        {
            return new EfzDataTypeProvider();
        }

        internal override SqlFactory CreateSqlFactory(ITypeSystemProvider typeProvider, MetaModel metaModel)
        {
            return new EfzSqlFactory(typeProvider, metaModel);
        }

        private SqlIdentifier sqlIdentifier;
        internal override SqlIdentifier SqlIdentifier
        {
            get
            {
                if (sqlIdentifier == null)
                    sqlIdentifier = new EfzIdentifier();
                return sqlIdentifier;
            }
        }

        void IProvider.CreateDatabase()
        {
            var sqlBuilder = new SqlBuilder(SqlIdentifier);
            foreach (MetaTable table in services.Model.GetTables())
            {
                string createTableCommand = sqlBuilder.GetCreateTableCommand(table);
                if (!string.IsNullOrEmpty(createTableCommand))
                {
                    ExecuteCommand(createTableCommand);
                }
            }
            foreach (MetaTable table2 in services.Model.GetTables())
            {
                foreach (string str4 in sqlBuilder.GetCreateForeignKeyCommands(table2))
                {
                    if (!string.IsNullOrEmpty(str4))
                    {
                        ExecuteCommand(str4);
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

        bool IProvider.DatabaseExists()
        {
            return false;
        }
        void IProvider.Initialize(IDataServices dataServices, object connection)
        {
            if (connection is string)
            {
                var builder = new EfzConnectionStringBuilder((string)connection);
                if (builder.ConnectionType == "file")
                {
                    var file = builder.DataSource;

                }
                Initialize(dataServices, new EfzConnection((string)connection));
            }
        }

        internal override void AssignParameters(System.Data.Common.DbCommand cmd, System.Collections.ObjectModel.ReadOnlyCollection<SqlParameterInfo> parms, object[] userArguments, object lastResult)
        {
            base.AssignParameters(cmd, parms, userArguments, lastResult);
        }
    }
}