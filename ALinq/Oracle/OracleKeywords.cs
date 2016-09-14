using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ALinq.SqlClient;

namespace ALinq.Oracle
{
    class OracleKeywords : Keywords
    {
        public OracleKeywords()
        {
            base.AddRange(new[] { "KEY", "VALUE", "TEST" });

            base.AddRange(new[] { "ACCESS", "ADD", "ALL", "ALTER", "AND", "ANY", "AS", "ASC", "AUDIT" });
            base.AddRange(new[] { "BETWEEN", "BY" });
            base.AddRange(new[] { "CHAR", "CHECK", "CLUSTER", "COLUMN", "COMMENT", "COMPRESS", "CONNECT", 
                                           "CREATE", "CURRENT"});
            base.AddRange(new[] { "DATE", "DECIMAL", "DEFAULT", "DELETE", "DESC", "DISTINCT", "DROP" });
            base.AddRange(new[] { "ELSE", "EXCLUSIVE", "EXISTS" });
            base.AddRange(new[] { "FILE", "FLOAT", "FOR", "FROM" });
            base.AddRange(new[] { "GRANT", "GROUP" });
            base.AddRange(new[] { "HAVING" });
            base.AddRange(new[] { "IDENTIFIED", "IMMEDIATE", "IN", "INCREMENT", "INDEX", "INTEGER","INITIAL",
                                  "INSERT", "INTERSECT", "INTO", "IS"});
            base.AddRange(new[] { "LEVEL", "LIKE", "LOCK", "LONG" });
            base.AddRange(new[] { "MAXEXTENTS", "MINUS", "MLSLABEL", "MODE", "MODIFY" });
            base.AddRange(new[] { "NCHAR", "NOAUDIT", "NOCOMPRESS", "NOT", "NOWAIT", "NULL", "NUMBER", "NVARCHAR", "NVARCHAR2" });
            base.AddRange(new[] { "OF", "OFFLINE", "ON", "ONLINE", "OPTION", "OR", "ORDER" });
            base.AddRange(new[] { "PCTREE", "PRIOR", "PRIVLEGES", "PUBLIC" });
            base.AddRange(new[] { "RAW", "RENAME", "RESOURCE", "REVOKE", "ROW", "ROWID", "ROWNUM", "ROWS" });
            base.AddRange(new[] { "SELECT", "SESSION", "SET", "SHARE", "SIZE", "SMALLINT", "START", "SUCCESSFUL", 
                                           "SYNONYM", "SYSDATE" });
            base.AddRange(new[] { "TABLE", "THEN", "TO", "TRIGGER" });
            base.AddRange(new[] { "UID", "UNION", "UNIQUE", "UPDATE", "USER" });
            base.AddRange(new[] { "VALIDATE", "VALUES", "VARCHAR", "VARCHAR2", "VIEW" });
            base.AddRange(new[] { "WHENEVER", "WHERE", "WITH" });
            base.AddRange(new[] { "XMLTYPE" });
        }

        internal new static bool Contains(string item)
        {
            return Instance.Contains(item);
        }

        private static Keywords instance;

        static Keywords Instance
        {
            get
            {
                if (instance == null)
                    instance = new OracleKeywords();
                return instance;
            }
        }
    }
}
