using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using ALinq;

namespace ALinq.SqlClient
{
    internal class SqlSelect : SqlStatement
    {

        // Fields
        private SqlExpression having;
        private SqlRow row;
        private SqlExpression selection;
        private SqlExpression where;

        // Methods
        internal SqlSelect(SqlExpression selection, SqlSource from, Expression sourceExpression)
            : base(SqlNodeType.Select, sourceExpression)
        {
            this.Row = new SqlRow(sourceExpression);
            this.Selection = selection;
            this.From = from;
            this.GroupBy = new List<SqlExpression>();
            this.OrderBy = new List<SqlOrderExpression>();
            this.OrderingType = SqlOrderingType.Default;
        }

        // Properties
        internal bool DoNotOutput { get; set; }

        internal SqlSource From { get; set; }

        internal List<SqlExpression> GroupBy { get; private set; }

        internal SqlExpression Having
        {
            get
            {
                return this.having;
            }
            set
            {
                if ((value != null) && (TypeSystem.GetNonNullableType(value.ClrType) != typeof(bool)))
                {
                    throw Error.ArgumentWrongType("value", "bool", value.ClrType);
                }
                this.having = value;
            }
        }

        internal bool IsDistinct { get; set; }

        internal bool IsPercent { get; set; }

        internal List<SqlOrderExpression> OrderBy { get; private set; }

        internal SqlOrderingType OrderingType { get; set; }

        internal SqlRow Row
        {
            [DebuggerStepThrough]
            get
            {
                return this.row;
            }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }
                this.row = value;
            }
        }

        internal SqlExpression Selection
        {
            [DebuggerStepThrough]
            get
            {
                return this.selection;
            }
            set
            {
                //if (value == null)
                //{
                //    throw Error.ArgumentNull("value");
                //}
                this.selection = value;
            }
        }

        internal SqlExpression Top
        {
            [DebuggerStepThrough]
            get;
            [DebuggerStepThrough]
            set;
        }

        internal SqlExpression Where
        {
            [DebuggerStepThrough]
            get
            {
                return this.where;
            }
            set
            {
                if ((value != null) && (TypeSystem.GetNonNullableType(value.ClrType) != typeof(bool)))
                {
                    throw Error.ArgumentWrongType("value", "bool", value.ClrType);
                }
                this.where = value;
            }
        }

        public override string Text
        {
            get
            {
                string str = string.Empty;
                if (From != null)
                    str = str + " From " + From.Text;

                if (Where != null)
                    str = str + " Where" + Where.Text;

                if (Selection != null)
                    str = str + " Select" + Selection.Text;


                return str.TrimStart();
            }
        }

    }

    
}