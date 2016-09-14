using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal class SqlRow : SqlNode
    {

        // Fields

        // Methods
        internal SqlRow(Expression sourceExpression)
            : base(SqlNodeType.Row, sourceExpression)
        {
            this.Columns = new List<SqlColumn>();
        }

        internal SqlColumn Find(string name)
        {
            foreach (SqlColumn column in this.Columns)
            {
                if (name == column.Name)
                {
                    return column;
                }
            }
            return null;
        }

        // Properties
        internal List<SqlColumn> Columns { get; private set; }

        public override string Text
        {
            get
            {
                return string.Format("({0})", string.Join(",", Columns.Select(o => o.Name).ToArray()));
            }
        }
    }
}