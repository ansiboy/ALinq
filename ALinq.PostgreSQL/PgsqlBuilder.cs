using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.PostgreSQL
{
    partial class PgsqlBuilder
    {
        public static string GetCreateDatabaseCommand(string catalog, string dataFilename, string logFilename)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("CREATE DATABASE {0}", catalog);
            return builder.ToString();
        }
    }

    internal partial class PgsqlBuilder
    {
        //PgsqlIdentifier
        private readonly SqlIdentifier sqlIdentifier;
        private PgsqlDataTypeProvider dbTypeProvider;

        public PgsqlBuilder(SqlProvider sqlProvider)
        {
            this.sqlIdentifier = sqlProvider.SqlIdentifier;
            dbTypeProvider = new PgsqlDataTypeProvider();
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

        public string GetPrimaryKeyCommand(MetaTable table)
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

        public string GetCreateSquenceCommand(MetaTable table)
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

        public string GetCreateTableCommand(MetaTable table)
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

        private string GetDbType(MetaDataMember mm)
        {
            string dbType = mm.DbType;
            if (dbType != null)
            {
                return dbType;
            }
            var builder = new StringBuilder();
            Type type = mm.Type;
            bool canBeNull = mm.CanBeNull;
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
                var dataType = dbTypeProvider.From(type);
                var str = dataType.ToQueryString();
                builder.Append(str);
            }
            if (!canBeNull)
                builder.Append(" NOT NULL");

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

        public static string GetSequenceName(MetaDataMember member, SqlIdentifier sqlIdentifier)
        {
            return "Seq_" + sqlIdentifier.UnquoteIdentifier(member.DeclaringType.Table.TableName);
        }
    }
}
