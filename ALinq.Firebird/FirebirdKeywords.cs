using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ALinq.SqlClient;

namespace ALinq.Firebird
{
    class FirebirdKeywords : Keywords
    {
        private FirebirdKeywords()
        {
            AddRange(new[] { "KEY", "TEST", "VALUE" });

            AddRange(new[] { "ABSOLUTE", "ACTION", "ADD", "AFTER", "ALL", "ALLOCATE", "ALTER", "AND", "ANY",
                             "ARE", "ARRAY", "AS", "ASC", "ASENSITIVE", "ASSERTION", "ASYMMETRIC", "AT", 
                             "ATOMIC", "AUTHORIZATION", "AVG" });
            AddRange(new[] { "BEFORE", "BEGIN", "BETWEEN", "BIGINT", "BINARY", "BIT","BLOB", "BOOLEAN", 
                             "BOTH", "BREADTH", "BY" });//8
            AddRange(new[] { "CALL","CALLED","CASCADED","CASCADE","CASE","CAST","CATALOG","CHAR","CHAR_LENGTH",
                             "CHARACTER","CHARACTER_LENGTH","CHECK","CLOB","CLOSE","COALESCE",
                             "COLLATE","COLLATION","COLUMN","COMMIT","CONDITION","CONNECT","CONNECTION","CONSTRAINT","CONSTRAINTS","CONSTRUCTOR","CONTAINS","CONTINUE","CONVERT","CORRESPONDING", 
                             "COUNT","CREATE","CROSS","CUBE","CURRENT","CURRENT_DATE","CURRENT_DEFAULT_TRANSFORM_GROUP",
                             "CURRENT_PATH","CURRENT_ROLE","CURRENT_TIME","CURRENT_TIMESTAMP","CURRENT_TRANSFORM_GROUP_FOR_TYPE",
                             "CURRENT_USER","CURSOR","CYCLE" });
            base.AddRange(new[] { "DATE","DAY","DEALLOCATE","DEC","DECIMAL","DECLARE","DEFAULT","DEFERRABLE","DELETE","DEPTH","DEREF","DESC","DESCRIBE","DESCRIPTOR",
                                           "DETERMINISTIC","DIAGNOSTICS","DISCONNECT","DISTINCT","DO","DOMAIN","DOUBLE","DROP","DYNAMIC"});
            base.AddRange(new[] { "EACH", "ELEMENT", "ELSE", "ELSEIF", "END", "EQUALS", "ESCAPE", "EXCEPT", "EXCEPTION", "EXEC",
                                           "EXECUTE", "EXISTS", "EXIT",  "EXTERNAL","EXTRACT" });
            base.AddRange(new[] { "FALSE", "FETCH", "FILTER", "FIRST", "FLOAT", "FOR", "FOREIGN", "FOUND", "FREE", "FROM", "FULL", "FUNCTION" });
            base.AddRange(new[] { "GENERAL", "GET", "GLOBAL", "GO", "GOTO", "GRANT", "GROUP", "GROUPING" });
            base.AddRange(new[] { "HANDLER", "HAVING", "HOLD", "HOUR" });
            base.AddRange(new[] { "IDENTITYIF","IMMEDIATE","IN","INDICATOR","INITIALLY","INNER","INOUT","INPUT","INSENSITIVE","INSERT","INT",
                                           "INTEGER","INTERSECT","INTERVAL","INTO","IS","ISOLATION","ITERATEJOIN" });
            base.AddRange(new[] { "LANGUAGE", "LARGE", "LATERAL", "LEADING", "LEAVE", "LEFT", "LEVEL", "LIKE", "LENGTH", "LOCALTIME", 
                                           "LOCALTIMESTAMP", "LOCATOR", "LOOP","LOWER" });//"LOCAL",
            base.AddRange(new[] { "MAP", "MATCH", "MAX", "MEMBER", "MERGE", "METHOD", "MIN", "MINUTE", "MODIFIES", "MODULE", "MONTH", "MULTISET" });
            base.AddRange(new[] { "NAMES", "NATIONAL", "NATURAL", "NCHAR", "NCLOB", "NEW", "NEXT", "NO", "NONE", "NOT", "NULL", "NULLIF", "NUMERIC" });
            base.AddRange(new[] { "OBJECT", "OCTET_LENGTH", "OF", "OLD", "ON", "ONLY", "OPEN", "OPTION", "OR", "ORDER", "ORDINALITY", "OUT", "OUTER", "OUTPUT", "OVER", "OVERLAPS" });
            base.AddRange(new[] { "PAD", "PARAMETER", "PARTIAL", "PARTITION", "PATH", "PRECISION", "PREPARE", "PRESERVE", "PRIMARY", "PRIOR", "PRIVILEGES", "PROCEDURE", "PUBLIC" });
            base.AddRange(new[] { "RANGE", "READ", "READS", "REAL", "RECURSIVE", "REFREFERENCES", "REFERENCING", "RELATIVE", "RELEASE", "REPEAT",
                                           "RESIGNAL", "RESTRICT","RESULT", "RETURN", "RETURNS", "REVOKE", "RIGHT", "ROLE", "ROLLBACK", "ROLLUP", "ROUTINE", "ROW", "ROWS" });
            base.AddRange(new[] { "SAVEPOINT","SCHEMA", "SCOPE", "SCROLL", "SEARCH", "SECOND", "SECTION", "SELECT", "SENSITIVE", "SESSION_USER","SET","SETS",
                                           "SIGNAL", "SIMILAR", "SIZE","SMALLINT", "SOME", "SPACE", "SPECIFIC", "SPECIFICTYPE", "SQL", "SQLCODE","SQLERROR", "SQLEXCEPTION", "SQLSTATE",
                                           "SQLWARNING", "START", "STATE","STATIC", "SUBMULTISET", "SUBSTRING","SUM", "SYMMETRIC",  "SYSTEM", "SYSTEM_USER" });
            base.AddRange(new[] { "TABLE", "TABLESAMPLE", "TEMPORARY", "THEN", "TIME", "TIMES", "TAMP", "TIMEZONE_HOUR", "TIMEZONE_MINUTE", "TO", 
                                           "TRAILING","TRANSACTION","TRANSLATE","TRANSLATION","TREAT","TRIGGER","TRIM","TRUE" });
            base.AddRange(new[] { "UNDER", "UNDO", "UNION", "UNIQUE", "UNKNOWN", "UNNEST", "UNTIL", "UPDATE", "UPPER", "USAGE", "USER", "USING" });
            base.AddRange(new[] { "VALUES", "VARCHAR", "VARYING", "VIEW" });
            base.AddRange(new[] { "WHEN", "WHENEVER", "WHERE", "WHILE", "WINDOW", "WITH", "WITHIN", "WITHOUT", "WORK", "WRITE" });
            base.AddRange(new[] { "YEAR", "ZONE" });
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
                    instance = new FirebirdKeywords();
                return instance;
            }
        }


    }
}
