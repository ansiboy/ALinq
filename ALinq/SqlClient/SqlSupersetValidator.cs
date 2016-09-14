using System.Collections.Generic;
using ALinq.SqlClient;

namespace ALinq.SqlClient
{
    internal class SqlSupersetValidator
    {
        // Fields
        private readonly List<SqlVisitor> validators = new List<SqlVisitor>();

        // Methods
        internal void AddValidator(SqlVisitor validator)
        {
            validators.Add(validator);
        }

        internal void Validate(SqlNode node)
        {
            foreach (SqlVisitor visitor in validators)
            {
                visitor.Visit(node);
            }
        }
    }
}