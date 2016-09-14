using System;
using System.Data.Common;
using ALinq;
using System.Collections.ObjectModel;

namespace ALinq.SqlClient
{
    public partial class SqlProvider
    {
        internal class ExecuteResult : IExecuteResult
        {
            // Fields
            private readonly DbCommand command;
            private readonly int iReturnParameter;
            private bool isDisposed;
            private readonly ReadOnlyCollection<SqlParameterInfo> parameters;
            private readonly IObjectReaderSession session;
            private object value;

            // Methods
            public ExecuteResult(DbCommand command, ReadOnlyCollection<SqlParameterInfo> parameters,
                                 IObjectReaderSession session)
            {
                iReturnParameter = -1;
                this.command = command;
                this.parameters = parameters;
                this.session = session;
            }

            public ExecuteResult(DbCommand command, ReadOnlyCollection<SqlParameterInfo> parameters,
                                 IObjectReaderSession session, object value)
                : this(command, parameters, session, value, false)
            {
            }

            public ExecuteResult(DbCommand command, ReadOnlyCollection<SqlParameterInfo> parameters,
                                 IObjectReaderSession session, object value, bool useReturnValue)
                : this(command, parameters, session)
            {
                this.value = value;
                if (this.command != null && (this.parameters != null && parameters.Count > 0) && useReturnValue)
                {
                    iReturnParameter = GetParameterIndex("@RETURN_VALUE");

                    if (iReturnParameter == -1)
                        iReturnParameter = GetParameterIndex(":RETURN_VALUE");

                    if (iReturnParameter == -1)
                        iReturnParameter = GetParameterIndex("RETURN_VALUE");
                }
            }

            public void Dispose()
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    if (session != null)
                    {
                        session.Dispose();
                    }
                }
            }

            private int GetParameterIndex(string paramName)
            {
                int num2 = 0;
                int count = parameters.Count;
                while (num2 < count)
                {
                    if (
                        string.Compare(parameters[num2].Parameter.Name, paramName,
                                       StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return num2;
                    }
                    num2++;
                }
                return -1;
            }

            public virtual object GetParameterValue(int parameterIndex)
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
                if ((obj2 != null) && (obj2.GetType() != info.Parameter.ClrType))
                {
                    return DBConvert.ChangeType(obj2, info.Parameter.ClrType);
                }
                return obj2;
            }

            internal object GetParameterValue(string paramName)
            {
                int parameterIndex = GetParameterIndex(paramName);
                if (parameterIndex >= 0)
                {
                    return GetParameterValue(parameterIndex);
                }
                return null;
            }

            // Properties
            public object ReturnValue
            {
                get
                {
                    if (iReturnParameter >= 0)
                    {
                        return GetParameterValue(iReturnParameter);
                    }
                    return value;
                }
                set { this.value = value; }
            }
        }

    }
}
