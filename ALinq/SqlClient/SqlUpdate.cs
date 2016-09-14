using System.Collections.Generic;
using System.Linq.Expressions;

namespace ALinq.SqlClient
{
    internal class SqlUpdate : SqlStatement
    {
        // Fields
        private readonly List<SqlAssign> assignments;
        private SqlSelect select;

        // Methods
        internal SqlUpdate(SqlSelect select, IEnumerable<SqlAssign> assignments, Expression sourceExpression)
            : base(SqlNodeType.Update, sourceExpression)
        {
            this.select = select;
            this.assignments = new List<SqlAssign>(assignments);
        }

        // Properties
        internal List<SqlAssign> Assignments
        {
            get
            {
                return this.assignments;
            }
        }

        internal SqlSelect Select
        {
            get
            {
                return select;
            }
            set
            {
                //if (value == null)
                //{
                //    throw Error.ArgumentNull("value");
                //}
                select = value;
            }
        }

        /// <summary>
        /// HACK:用来实现匿名类的插入，如：db.Employees.Insert(o=> new { }) 。 
        /// </summary>
        internal bool IsInsert { get; set; }
    }
}