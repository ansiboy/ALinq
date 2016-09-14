using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ALinq;
using ALinq.Mapping;
using System.Globalization;
using System.Text;
using ALinq.SqlClient;

namespace ALinq.Oracle
{
    internal class OracleSqlBuilder
    {
        private readonly SqlIdentifier sqlIdentifier;

        public OracleSqlBuilder(SqlProvider sqlProvider)
        {
            this.sqlIdentifier = sqlProvider.SqlIdentifier;
        }
        // Methods
        internal void BuildFieldDeclarations(MetaTable table, StringBuilder sb)
        {
            int num = 0;
            var memberNameToMappedName = new Dictionary<object, string>();
            foreach (MetaType type in table.RowType.InheritanceTypes)
            {
                num += BuildFieldDeclarations(type, memberNameToMappedName, sb);
            }
            if (num == 0)
            {
                throw SqlClient.Error.CreateDatabaseFailedBecauseOfClassWithNoMembers(table.RowType.Type);
            }
        }

        private int BuildFieldDeclarations(MetaType type, IDictionary<object, string> memberNameToMappedName, StringBuilder sb)
        {
            int num = 0;
            foreach (MetaDataMember member in type.DataMembers)
            {
                string str;
                if ((!member.IsDeclaredBy(type) || member.IsAssociation) || !member.IsPersistent)
                {
                    continue;
                }
                object key = InheritanceRules.DistinguishedMemberName(member.Member);
                if (memberNameToMappedName.TryGetValue(key, out str))
                {
                    if (str != member.MappedName)
                    {
                        goto Label_0075;
                    }
                    continue;
                }
                memberNameToMappedName.Add(key, member.MappedName);
            Label_0075:
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }
                sb.AppendLine();
                sb.Append(string.Format(CultureInfo.InvariantCulture, "  {0} ", new object[] { QuoteIdentifier(member.MappedName) }));
                if (!string.IsNullOrEmpty(member.Expression))
                {
                    sb.Append("AS " + member.Expression);
                }
                else
                {
                    sb.Append(GetDbType(member));
                }
                num++;
            }
            return num;
        }

        private string BuildKey(IEnumerable<MetaDataMember> members)
        {
            var builder = new StringBuilder();
            foreach (MetaDataMember member in members)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }
                builder.Append(QuoteIdentifier(member.MappedName));
            }
            return builder.ToString();
        }

        private void BuildPrimaryKey(MetaTable table, StringBuilder sb)
        {
            foreach (MetaDataMember member in table.RowType.IdentityMembers)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(QuoteIdentifier(member.MappedName));
            }
        }

        private string QuoteIdentifier(string name)
        {
            if (sqlIdentifier.NeedToQuote(name))
                return sqlIdentifier.QuoteIdentifier(name);
            return name;
        }

        private string UnquoteIdentifier(string name)
        {
            return sqlIdentifier.UnquoteIdentifier(name);
        }

        internal string GetPrimaryKeyCommand(MetaTable table)
        {
            var sb = new StringBuilder();
            var builder = new StringBuilder();
            if (table.RowType.IdentityMembers.Count > 0)
            {
                BuildPrimaryKey(table, sb);
                if (sb.Length > 0)
                {
                    builder.AppendFormat("ALTER TABLE {0}", QuoteIdentifier(table.TableName));
                    string s = string.Format(CultureInfo.InvariantCulture, "PK_{0}",
                                             new object[] { UnquoteIdentifier(table.TableName) });
                    builder.AppendLine();
                    builder.AppendFormat("ADD  CONSTRAINT {0} PRIMARY KEY ({1})", (s), sb);
                }
            }
            return builder.ToString();
        }


        public string GetDropSquenceCommand(MetaTable table)
        {
            var builder = new StringBuilder();
            //建立Sequence
            if (table.RowType.IdentityMembers.Count == 1 && table.RowType.IdentityMembers[0].IsDbGenerated)
            {
                var sequenceName = GetSequenceName(table.RowType.IdentityMembers[0]);
                builder.AppendFormat("DROP SEQUENCE {0}", sequenceName);
            }
            return builder.ToString();
        }

        private string GetCreateSquenceCommand(MetaTable table)
        {
            var builder = new StringBuilder();
            //建立Sequence
            if (table.RowType.IdentityMembers.Count == 1 && table.RowType.IdentityMembers[0].IsDbGenerated)
            {
                var sequenceName = GetSequenceName(table.RowType.IdentityMembers[0]);
                builder.AppendFormat("CREATE SEQUENCE {0} START WITH 1 INCREMENT BY 1", sequenceName);
            }
            return builder.ToString();
        }

        public string GetCreateDatabaseCommand(string catalog, string password)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("CREATE USER {0} IDENTIFIED BY {1}", catalog, password);
            return builder.ToString();
        }

        public IEnumerable<string> GetCreateForeignKeyCommands(MetaTable table)
        {
            foreach (var metaType in table.RowType.InheritanceTypes)
            {
                foreach (var command in GetCreateForeignKeyCommands(metaType))
                {
                    yield return command;
                }
            }
        }

        private IEnumerable<string> GetCreateForeignKeyCommands(MetaType metaType)
        {
            foreach (var member in metaType.DataMembers)
            {
                if (member.IsDeclaredBy(metaType) && member.IsAssociation)
                {
                    MetaAssociation association = member.Association;
                    if (association.IsForeignKey)
                    {
                        var stringBuilder = new StringBuilder();
                        var thisKey = BuildKey(association.ThisKey);
                        var otherKey = BuildKey(association.OtherKey);
                        var otherTable = association.OtherType.Table.TableName;
                        var mappedName = member.MappedName;
                        if (mappedName == member.Name)
                        {
                            mappedName = string.Format(CultureInfo.InvariantCulture, "FK_{0}_{1}",
                                                       new object[] { UnquoteIdentifier(metaType.Table.TableName), 
                                                                      UnquoteIdentifier(member.Name) });
                        }
                        string command = "ALTER TABLE {0}" + Environment.NewLine + "  ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3}({4})";
                        var otherMember = association.OtherMember;
                        if (otherMember != null)
                        {
                            string deleteRule = association.DeleteRule;
                            if (deleteRule != null)
                            {
                                command += Environment.NewLine + "  ON DELETE " + deleteRule;
                            }
                        }
                        yield return stringBuilder.AppendFormat(command, new object[]
                                                    {
                                                        QuoteIdentifier(metaType.Table.TableName),
                                                        mappedName,
                                                        thisKey,
                                                        QuoteIdentifier(otherTable),
                                                        otherKey
                                                    }).ToString();
                    }
                }
            }

        }

        private string GetCreateTableCommand(MetaTable table)
        {
            var builder = new StringBuilder();
            var sb = new StringBuilder();
            BuildFieldDeclarations(table, sb);
            builder.AppendFormat("CREATE TABLE {0}", QuoteIdentifier(table.TableName));
            builder.Append("(");
            builder.Append(sb.ToString());
            builder.AppendLine();
            builder.Append("  )");
            return builder.ToString();
        }

        public IEnumerable<string> GetCreateTableCommands(MetaTable table)
        {
            var command = GetCreateTableCommand(table);
            if (string.IsNullOrEmpty(command) == false)
                yield return command;

            command = GetPrimaryKeyCommand(table);
            if (string.IsNullOrEmpty(command) == false)
                yield return command;

            command = GetCreateSquenceCommand(table);
            if (string.IsNullOrEmpty(command) == false)
                yield return command;
        }

        private string GetDbType(MetaDataMember mm)
        {
            string dbType = mm.DbType;
            var builder = new StringBuilder();
            Type type = mm.Type;
            bool canBeNull = mm.CanBeNull;

            if (dbType == null)
            {
                if (type.IsValueType && IsNullable(type))
                {
                    type = type.GetGenericArguments()[0];
                }
                if (mm.IsVersion)
                {
                    builder.Append("Timestamp");
                }
                else
                {
                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.Object:
                            if (type == typeof(Guid))
                            {
                                builder.Append("RAW(16)");
                            }

                            else if (type == typeof(byte[]))
                            {
                                builder.Append("BLOB");
                            }
                            else if (type == typeof(char[]))
                            {
                                builder.Append("VarChar(4000)");
                            }

                            else if (type == typeof(Binary))
                            {
                                //builder.Append("LOB");
                                builder.Append("BLOB");
                            }
                            else if (type == typeof(XDocument) || type == typeof(XElement))
                            {
                                builder.Append("VarChar(4000)");
                            }
                            else
                            {
                                throw SqlClient.Error.CouldNotDetermineSqlType(type);
                            }
                            break;

                        case TypeCode.DBNull:
                            break;

                        case TypeCode.Boolean:
                            builder.Append("NUMBER(1)");
                            break;

                        case TypeCode.Char:
                            builder.Append("CHAR(1)");
                            break;

                        case TypeCode.SByte:
                        case TypeCode.Int16:
                            builder.Append("NUMBER(5)");
                            break;

                        case TypeCode.Byte:
                            builder.Append("NUMBER(3)");
                            break;

                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            builder.Append("NUMBER(10)");
                            break;

                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            builder.Append("NUMBER(20)");
                            break;

                        case TypeCode.UInt64:
                            builder.Append("NUMBER(21)");
                            break;

                        case TypeCode.Single:
                            builder.Append("NUMBER(20,7)");
                            break;

                        case TypeCode.Double:
                            builder.Append("NUMBER(20,7)");
                            break;

                        case TypeCode.Decimal:
                            builder.Append("NUMBER(20,7)");
                            break;

                        case TypeCode.DateTime:
                            builder.Append("Date");
                            break;

                        case TypeCode.String:
                            builder.Append("Clob");
                            break;
                    }
                }
            }
            else
            {
                builder.Append(dbType);
            }

            if (!canBeNull)
                builder.Append(" NOT NULL");

            //if (mm.IsPrimaryKey && mm.IsDbGenerated)
            //{
            //    builder.Append(" PRIMARY KEY");// AUTOINCREMENT
            //}
            return builder.ToString();
        }



        internal bool IsNullable(Type type)
        {
            return (type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()));
        }

        internal string CreateSequence(string typeName)
        {
            return string.Empty;
        }

        internal string GetSequenceName(MetaDataMember member)
        {
            return GetSequenceName(member, sqlIdentifier);
        }

        /// <summary>
        /// 如果为关键字名称，则为它加上双引号，否则返回原值。
        /// </summary>
        //public static string QuoteName(string name)
        //{
        //    if (name.Equals("User", StringComparison.OrdinalIgnoreCase))
        //        return string.Format("\"{0}\"", name);
        //    return name;
        //}

        public static string GetSequenceName(MetaDataMember member, SqlIdentifier sqlIdentifier)
        {
            return "SEQ_" + sqlIdentifier.UnquoteIdentifier(member.DeclaringType.Table.TableName);
        }

        internal IEnumerable<string> GetDropTableCommands(MetaTable metaTable)
        {
            var sql = "DROP TABLE {0} CASCADE CONSTRAINTS";
            sql = string.Format(sql, metaTable.TableName);
            yield return sql;

            sql = this.GetDropSquenceCommand(metaTable);
            if (!string.IsNullOrEmpty(sql))
                yield return sql;
        }
    }
}
