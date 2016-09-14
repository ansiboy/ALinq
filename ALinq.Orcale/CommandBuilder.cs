using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using ALinq;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ALinq.SqlClient;

namespace ALinq.Oracle
{
    //internal class DbCommandBuilder : OracleFormatter.Visitor
    //{
    //    private DbCommand command;
    //    private DbParameter parameter;
    //    //private readonly TypeSystemProvider typeProvider;
    //    //private readonly IList<DbCommand> commands;
    //    private readonly DbConnection connection;
    //    private ReadOnlyCollection<SqlParameterInfo> parameterInfos;
    //    private DbTransaction transaction;

    //    public DbCommandBuilder(DbConnection connection, DbTransaction transaction)
    //    {
    //        this.connection = connection;
    //        this.transaction = transaction;
    //    }

    //    public DbCommand CreateCommand(SqlNode sqlNode, ReadOnlyCollection<SqlParameterInfo> parameterInfos)
    //    {
    //        this.parameterInfos = parameterInfos;
    //        command = connection.CreateCommand();
    //        command.Transaction = transaction;
    //        command.CommandText = Format(sqlNode);
    //        //foreach (var parameterInfo in parameterInfos)
    //        //{
    //        //    Debug.Assert(parameterInfo.Parameter.Name.StartsWith("@"));
    //        //    var p = command.CreateParameter();
    //        //    p.Value = parameterInfo.Value ?? DBNull.Value;
    //        //    p.ParameterName = ":" + parameterInfo.Parameter.Name.Substring(1);
    //        //    command.Parameters.Add(p);
    //        //    if(p.Value != null)
    //        //    {
    //        //        if (p.Value.GetType() == typeof (Binary))
    //        //        {
    //        //            p.DbType = DbType.Binary;
    //        //            p.Value = ((Binary) p.Value).ToArray();
    //        //        }
    //        //        else if(p.Value.GetType() == typeof(Guid))
    //        //        {
    //        //            p.DbType = DbType.Binary;
    //        //            p.Value = ((Guid)p.Value).ToByteArray();
    //        //        }
    //        //    }
    //        //}
    //        if (connection.State == ConnectionState.Closed)
    //            connection.Open();
    //        return command;
    //    }

    //    internal override SqlExpression VisitParameter(SqlParameter p)
    //    {
    //        if (!command.Parameters.Contains(p.Name))
    //        {
    //            Debug.Assert(p.Name.StartsWith("@"));
    //            parameter = command.CreateParameter();
    //            command.Parameters.Add(parameter);
    //            parameter.Value = (from parameterInfo in parameterInfos
    //                               where parameterInfo.Parameter.Name == p.Name
    //                               select parameterInfo.Value).FirstOrDefault() ?? DBNull.Value;
    //            if (parameter.Value != null)
    //            {
    //                if (parameter.Value.GetType() == typeof(Binary))
    //                {
    //                    parameter.DbType = DbType.Binary;
    //                    parameter.Value = ((Binary)parameter.Value).ToArray();
    //                }
    //                else if (parameter.Value.GetType() == typeof(Guid))
    //                {
    //                    parameter.DbType = DbType.Binary;
    //                    parameter.Value = ((Guid)parameter.Value).ToByteArray();
    //                }
    //            }
    //            parameter.ParameterName = ":" + p.Name.Substring(1);
    //        }
    //        return base.VisitParameter(p);
    //    }

    //    internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
    //    {
    //        if (uo.NodeType == SqlNodeType.Convert)
    //        {
    //            return VisitExpression(uo.Operand);
    //        }
    //        return base.VisitUnaryOperator(uo);
    //    }

    //}
}
