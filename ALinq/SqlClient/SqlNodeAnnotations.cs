using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ALinq.SqlClient
{
    internal class SqlNodeAnnotations
    {
        // Fields
        private Dictionary<SqlNode, List<SqlNodeAnnotation>> annotationMap = new Dictionary<SqlNode, List<SqlNodeAnnotation>>();
        private Dictionary<Type, string> uniqueTypes = new Dictionary<Type, string>();

        // Methods
        internal void Add(SqlNode node, SqlNodeAnnotation annotation)
        {
            List<SqlNodeAnnotation> list = null;
            if (!this.annotationMap.TryGetValue(node, out list))
            {
                list = new List<SqlNodeAnnotation>();
                this.annotationMap[node] = list;
            }
            this.uniqueTypes[annotation.GetType()] = string.Empty;
            list.Add(annotation);
        }

        internal List<SqlNodeAnnotation> Get(SqlNode node)
        {
            List<SqlNodeAnnotation> list = null;
            this.annotationMap.TryGetValue(node, out list);
            return list;
        }

        internal bool HasAnnotationType(Type type)
        {
            return this.uniqueTypes.ContainsKey(type);
        }

        internal bool NodeIsAnnotated(SqlNode node)
        {
            if (node == null)
            {
                return false;
            }
            return this.annotationMap.ContainsKey(node);
        }
    }


}