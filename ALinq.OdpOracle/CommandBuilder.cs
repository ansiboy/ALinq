using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using ALinq.SqlClient;

namespace ALinq.Oracle.Odp
{
    internal class DbCommandBuilder : OracleFormatter.Visitor
    {
        private DbCommand command;
        private DbParameter parameter;
        //private readonly IList<DbCommand> commands;
        private readonly DbConnection connection;
        private ReadOnlyCollection<SqlParameterInfo> parameterInfos;
        private readonly DbTransaction transaction;

        public DbCommandBuilder(DbConnection connection, DbTransaction transaction)
        {
            this.connection = connection;
            this.transaction = transaction;
        }

        public DbCommand CreateCommand(SqlNode sqlNode, ReadOnlyCollection<SqlParameterInfo> parameters)
        {
            parameterInfos = parameters;
            command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = Format(sqlNode);

            //if (connection.State == ConnectionState.Closed)
            //    connection.Open();
            return command;
        }

        internal override SqlExpression VisitParameter(SqlParameter p)
        {
            if (!command.Parameters.Contains(p.Name))
            {
                Debug.Assert(p.Name.StartsWith("@"));
                parameter = command.CreateParameter();
                command.Parameters.Add(parameter);
                parameter.Value = (from parameterInfo in parameterInfos
                                   where parameterInfo.Parameter.Name == p.Name
                                   select parameterInfo.Value).FirstOrDefault() ?? DBNull.Value;

                if (parameter.Value != null)
                {
                    if (parameter.Value.GetType() == typeof(Binary))
                    {
                        parameter.DbType = DbType.Binary;
                        parameter.Value = ((Binary)parameter.Value).ToArray();
                    }
                    else if (parameter.Value.GetType() == typeof(Guid))
                    {
                        parameter.DbType = DbType.Binary;
                        parameter.Value = ((Guid)parameter.Value).ToByteArray();
                    }
                }
                parameter.ParameterName = ":" + p.Name.Substring(1);
            }
            return base.VisitParameter(p);
        }

        internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
        {
            if (uo.NodeType == SqlNodeType.Convert)
            {
                return VisitExpression(uo.Operand);
            }
            return base.VisitUnaryOperator(uo);
        }

    }
}
