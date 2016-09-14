using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using ALinq.SqlClient;

namespace ALinq.Access
{
    internal class DbCommandBuilder : AccessDbFormatter.Visitor
    {
        private DbCommand command;
        private DbParameter parameter;
        private readonly ITypeSystemProvider typeProvider;
        //private readonly IList<DbCommand> commands;
        private readonly DbConnection connection;
        private ReadOnlyCollection<SqlParameterInfo> parameterInfos;
        private DbTransaction transaction;

        public DbCommandBuilder(DbConnection connection, DbTransaction transaction)
        {
            this.connection = connection;
            this.transaction = transaction;
        }

        public DbCommand CreateCommand(SqlNode sqlNode, ReadOnlyCollection<SqlParameterInfo> parameterInfos)
        {
            this.parameterInfos = parameterInfos;
            command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = Format(sqlNode);
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            return command;
        }

        internal override SqlExpression VisitParameter(SqlParameter p)
        {
            if (!command.Parameters.Contains(p.Name))
            {
                parameter = command.CreateParameter();
                parameter.ParameterName = p.Name;
                command.Parameters.Add(parameter);
                parameter.Value = (from parameterInfo in parameterInfos
                                   where parameterInfo.Parameter.Name == parameter.ParameterName
                                   select parameterInfo.Value).FirstOrDefault() ?? DBNull.Value;
                if (parameter.Value != null && parameter.Value.GetType() == typeof(Binary))
                {
                    parameter.DbType = DbType.Binary;
                    parameter.Value = ((Binary) parameter.Value).ToArray();
                }
                if (parameter.DbType == DbType.DateTime)
                    parameter.DbType = DbType.String;
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
