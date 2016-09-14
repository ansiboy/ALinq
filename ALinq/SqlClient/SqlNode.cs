using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace ALinq.SqlClient
{

    [DebuggerDisplay("{NodeType}|{Text}")]
    internal abstract class SqlNode
    {
        // Methods
        internal SqlNode(SqlNodeType nodeType, Expression sourceExpression)
        {
            this.NodeType = nodeType;
            this.SourceExpression = sourceExpression;
        }

        internal void ClearSourceExpression()
        {
            this.SourceExpression = null;
        }

        // Properties
        internal SqlNodeType NodeType { get; private set; }

        internal Expression SourceExpression { get; private set; }

        public virtual string Text
        {
            get
            {
                return string.Empty;
            }
        }
    }

 

}