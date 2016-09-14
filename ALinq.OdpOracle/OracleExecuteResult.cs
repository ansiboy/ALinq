using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using ALinq.SqlClient;
using Oracle.DataAccess.Types;

namespace ALinq.Oracle.Odp
{
    class OracleExecuteResult : ALinq.SqlClient.SqlProvider.ExecuteResult
    {
        private readonly ReadOnlyCollection<SqlParameterInfo> parameters;
        private readonly IObjectReaderSession session;
        private readonly DbCommand command;

        public OracleExecuteResult(DbCommand command, ReadOnlyCollection<SqlParameterInfo> parameters, IObjectReaderSession session)
            : base(command, parameters, session)
        {
            this.command = command;
            this.parameters = parameters;
            this.session = session;
        }

        public OracleExecuteResult(DbCommand command, ReadOnlyCollection<SqlParameterInfo> parameters, IObjectReaderSession session, object value)
            : base(command, parameters, session, value)
        {
            this.command = command;
            this.parameters = parameters;
            this.session = session;
        }

        public OracleExecuteResult(DbCommand command, ReadOnlyCollection<SqlParameterInfo> parameters, IObjectReaderSession session, object value, bool useReturnValue)
            : base(command, parameters, session, value, useReturnValue)
        {
            this.command = command;
            this.parameters = parameters;
            this.session = session;
        }

        public override object GetParameterValue(int parameterIndex)
        {
            if (((parameters == null) || (parameterIndex < 0)) || (parameterIndex > parameters.Count))
            {
                throw Error.ArgumentOutOfRange("parameterIndex");
            }
            if ((session != null) && !session.IsBuffered)
            {
                session.Buffer();
            }
            SqlParameterInfo info = parameters[parameterIndex];
            object obj2 = command.Parameters[parameterIndex].Value;
            if (obj2 == DBNull.Value)
            {
                obj2 = null;
            }
            if (obj2 != null)
            {
                var objType = obj2.GetType();
                var ns = objType.Namespace;
                if (ns == "Oracle.DataAccess.Types")
                {
                    if (((INullable)obj2).IsNull)
                        return null;

                    var propertyInfo = objType.GetProperty("Value");
                    if (propertyInfo != null)
                    {
                        obj2 = propertyInfo.GetValue(obj2, null);
                        if (obj2 != null)
                            objType = obj2.GetType();
                    }
                }

                if (objType != info.Parameter.ClrType)
                {
                    return DBConvert.ChangeType(obj2, info.Parameter.ClrType);
                }
            }
            return obj2;
        }
    }
}
