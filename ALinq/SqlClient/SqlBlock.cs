using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlBlock : SqlStatement
    {
       
        // Fields

        // Methods
        internal SqlBlock(Expression sourceExpression)
            : base(SqlNodeType.Block, sourceExpression)
        {
            this.Statements = new List<SqlStatement>();
        }

        // Properties
        internal List<SqlStatement> Statements { get; private set; }
    }
}