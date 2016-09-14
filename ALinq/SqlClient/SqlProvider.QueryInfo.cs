using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ALinq.SqlClient
{
    public partial class SqlProvider
    {
        internal class QueryInfo
        {
            // Fields

            // Methods
            internal QueryInfo(SqlNode query, string commandText, ReadOnlyCollection<SqlParameterInfo> parameters, SqlProvider.ResultShape resultShape, Type resultType)
            {
                this.Query = query;
                this.CommandText = commandText;
                this.Parameters = parameters;
                this.ResultShape = resultShape;
                this.ResultType = resultType;
            }

            // Properties
            internal string CommandText { get; set; }

            internal ReadOnlyCollection<SqlParameterInfo> Parameters { get; private set; }

            internal SqlNode Query { get; private set; }

            internal SqlProvider.ResultShape ResultShape { get; private set; }

            internal Type ResultType { get; private set; }
        }

 

    }
}
