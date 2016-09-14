using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ALinq.Mapping;
using ALinq.SqlClient;


namespace ALinq.Firebird
{
    internal class FirebirdSqlBuilder
    {
        const bool Quote = false;
        public SqlProvider sqlProvider;
        private SqlIdentifier SqlIdentifier;

        public FirebirdSqlBuilder(SqlProvider firebirdProvider)
        {
            this.sqlProvider = firebirdProvider;
            this.SqlIdentifier = firebirdProvider.SqlIdentifier;
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
                throw ALinq.SqlClient.Error.CreateDatabaseFailedBecauseOfClassWithNoMembers(table.RowType.Type);
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
                    if (!(str == member.MappedName))
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
                sb.Append(member.MappedName);
            }
        }

        public string GetCreateDatabaseCommand(string catalog, string dataFilename, string logFilename)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("CREATE DATABASE {0};", catalog);
            if (dataFilename != null)
            {
                builder.AppendFormat(" ON PRIMARY (NAME='{0}', FILENAME='{1}')", Path.GetFileName(dataFilename), dataFilename);
                builder.AppendFormat(" LOG ON (NAME='{0}', FILENAME='{1}')", Path.GetFileName(logFilename), logFilename);
            }
            builder.AppendLine();
            builder.AppendFormat("Use {0};", catalog);
            return builder.ToString();
        }

        private string GetCreatePrimaryKeyCommand(MetaTable table)
        {
            //foreach (var metaType in table.RowType.InheritanceTypes)
            //{
            //    yield return GetCreatePrimaryKeyCommands(metaType);
            //}
            return GetCreatePrimaryKeyCommand(table.RowType);
        }

        private string GetCreatePrimaryKeyCommand(MetaType metaType)
        {
            var command = "ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2})";
            command = string.Format(command, QuoteCompoundIdentifier(metaType.Table.TableName),
                                    QuoteIdentifier("PK_" + FirebirdIdentifier.Instance.UnquoteIdentifier(metaType.Table.TableName)),
                                    string.Join(",", metaType.Table.RowType.IdentityMembers.Select(o => QuoteIdentifier(o.MappedName)).ToArray()));
            return command;
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
                                                       new object[] { QuoteCompoundIdentifier(metaType.Table.TableName), QuoteIdentifier(member.Name) });
                        }
                        var command = "ALTER TABLE {0}" + Environment.NewLine + "  ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3}({4})";

                        string deleteRule = " NO ACTION";
                        var otherMember = association.OtherMember;
                        if (otherMember != null)
                        {
                            if (association.DeleteRule != null)
                                deleteRule = association.DeleteRule;
                            //if (deleteRule != null)
                            //{
                            //    command += Environment.NewLine + "  ON DELETE " + deleteRule;
                            //}
                        }
                        stringBuilder.AppendFormat(command, new object[]
                                                             {
                                                                 QuoteCompoundIdentifier(metaType.Table.TableName),
                                                                 QuoteIdentifier(mappedName),
                                                                 thisKey,
                                                                 QuoteCompoundIdentifier(otherTable),
                                                                 otherKey
                                                             });
                        //ON DELETE NO ACTION
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine("ON DELETE" + deleteRule);
                        stringBuilder.AppendLine("ON UPDATE NO ACTION");
                        yield return stringBuilder.ToString();
                    }
                }
            }

        }

        private string QuoteCompoundIdentifier(string name)
        {
            if (sqlProvider.SqlIdentifier.NeedToQuote(name))
                return sqlProvider.SqlIdentifier.QuoteCompoundIdentifier(name);
            return name;
        }

        string QuoteIdentifier(string name)
        {
            if (sqlProvider.SqlIdentifier.NeedToQuote(name))
                return sqlProvider.SqlIdentifier.QuoteIdentifier(name);
            return name;
        }

        private string GetCreateTableCommand(MetaTable table)
        {
            var builder = new StringBuilder();
            var sb = new StringBuilder();
            BuildFieldDeclarations(table, sb);
            builder.AppendFormat("CREATE TABLE {0}", QuoteCompoundIdentifier(table.TableName));
            builder.Append("(");
            builder.Append(sb.ToString());
            //sb = new StringBuilder();
            //BuildPrimaryKey(table, sb);
            //if (sb.Length > 0)
            //{
            //    string s = string.Format(CultureInfo.InvariantCulture, "PK_{0}",
            //                             new object[] { QuoteCompoundIdentifier(table.TableName) });
            //    builder.Append(", ");
            //    builder.AppendLine();
            //    builder.AppendFormat("  CONSTRAINT {0} PRIMARY KEY {1}", s, sb);
            //}
            builder.AppendLine();
            builder.Append("  );");
            return builder.ToString();
        }

        private string GetDbType(MetaDataMember mm)
        {
            var builder = new StringBuilder();
            var dbType = mm.DbType;

            //bool isUnicode = false;

            if (dbType == null)
            {
                var type = mm.Type;
                var t = sqlProvider.TypeProvider.From(type);
                Debug.Assert(t is FirebirdDataType);
                var fbDbType = (FirebirdDataType)t;

                dbType = fbDbType.ToQueryString();
                //isUnicode = fbDbType.IsUnicodeType;
            }
            builder.Append(dbType);
            bool canBeNull = mm.CanBeNull;
            if (!canBeNull || mm.IsPrimaryKey)
            {
                builder.Append(" NOT NULL");
            }
            return builder.ToString();
        }

        internal static string GetDropDatabaseCommand(string catalog)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("DROP DATABASE {0}", (catalog));
            return builder.ToString();
        }

        internal static bool IsNullable(Type type)
        {
            return (type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()));
        }

        // Nested Types

        public static FirebirdSqlBuilder CreateInstance(SqlProvider provider)
        {
            return new FirebirdSqlBuilder(provider);
        }

        internal static string GetSequenceName(MetaDataMember member)
        {
            return "Seq_" + FirebirdIdentifier.Instance.UnquoteIdentifier(member.DeclaringType.Table.TableName);
        }

        internal IEnumerable<string> GetDropTableCommands(MetaTable metaTable)
        {
            var commands = GetDropForeignKeyCommands(metaTable);
            foreach (var command in commands)
            {
                yield return command;
            }
            var sql = "DROP TABLE {0}";
            sql = string.Format(sql, metaTable.TableName);
            yield return sql;

            sql = GetDropSquenceCommand(metaTable);
            if (!string.IsNullOrEmpty(sql))
                yield return sql;
        }

        private IEnumerable<string> GetDropForeignKeyCommands(MetaTable metaTable)
        {
            var metaType = metaTable.RowType;
            foreach (var member in metaType.DataMembers)
            {
                if (member.IsDeclaredBy(metaType) && member.IsAssociation)
                {
                    MetaAssociation association = member.Association;
                    var stringBuilder = new StringBuilder();
                    var mappedName = member.MappedName;
                    if (mappedName == member.Name)
                    {
                        mappedName = string.Format(CultureInfo.InvariantCulture, "FK_{0}_{1}", SqlIdentifier.UnquoteIdentifier(metaType.Table.TableName),
                                                                                               SqlIdentifier.UnquoteIdentifier(member.Name));
                    }

                    var command = string.Format("SELECT count(*) FROM RDB$RELATION_CONSTRAINTS WHERE UPPER(RDB$CONSTRAINT_NAME) = '{0}'", mappedName.ToUpper());
                    var result = sqlProvider.services.Context.ExecuteQuery<int>(command).Single();
                    if (result == 0)
                        continue;

                    command = "ALTER TABLE {0}" + Environment.NewLine + "  DROP CONSTRAINT {1}";
                    var tableName = association.IsForeignKey ? metaType.Table.TableName : association.OtherType.Table.TableName;
                    yield return stringBuilder.AppendFormat(command, new object[]
                                                    {
                                                        tableName,
                                                        mappedName,
                                                    }).ToString();
                }
            }

        }

        internal IEnumerable<string> GetCreateTableCommands(MetaTable metaTable)
        {
            var command = GetCreateTableCommand(metaTable);
            if (string.IsNullOrEmpty(command) == false)
                yield return command;

            command = GetCreatePrimaryKeyCommand(metaTable);
            if (string.IsNullOrEmpty(command) == false)
                yield return command;

            command = GetCreateSquenceCommand(metaTable);
            if (string.IsNullOrEmpty(command) == false)
                yield return command;
        }

        private string GetCreateSquenceCommand(MetaTable metaTable)
        {
            if (metaTable.RowType.DBGeneratedIdentityMember == null)
                return null;
            var commandText = "CREATE SEQUENCE  " + FirebirdSqlBuilder.GetSequenceName(metaTable.RowType.DBGeneratedIdentityMember);
            return commandText;
        }

        private string GetDropSquenceCommand(MetaTable metaTable)
        {
            if (metaTable.RowType.DBGeneratedIdentityMember == null)
                return null;
            var commandText = "DROP SEQUENCE  " + FirebirdSqlBuilder.GetSequenceName(metaTable.RowType.DBGeneratedIdentityMember);
            return commandText;
        }
    }
}