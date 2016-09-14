using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ALinq.Mapping;
using System.Globalization;
using System.IO;
using System.Text;
using ALinq.SqlClient;

namespace ALinq.MySQL
{
    internal static class MySqlBuilder
    {
        // Methods
        internal static void BuildFieldDeclarations(MetaTable table, StringBuilder sb)
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

        private static int BuildFieldDeclarations(MetaType type, IDictionary<object, string> memberNameToMappedName, StringBuilder sb)
        {
            int num = 0;
            foreach (var member in type.DataMembers)
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
                sb.Append(string.Format(CultureInfo.InvariantCulture, "  {0} ", 
                                        new object[] { MySqlIdentifier.QuoteCompoundIdentifier(member.MappedName) }));
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

        private static string BuildKey(IEnumerable<MetaDataMember> members)
        {
            var builder = new StringBuilder();
            foreach (MetaDataMember member in members)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }
                builder.Append(MySqlIdentifier.QuoteIdentifier(member.MappedName));
            }
            return builder.ToString();
        }

        private static void BuildPrimaryKey(MetaTable table, StringBuilder sb)
        {
            foreach (MetaDataMember member in table.RowType.IdentityMembers)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(MySqlIdentifier.QuoteIdentifier(member.MappedName));
            }
        }

        public static string GetCreateDatabaseCommand(string catalog, string dataFilename, string logFilename)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("CREATE DATABASE {0};", MySqlIdentifier.QuoteIdentifier(catalog));
            //if (dataFilename != null)
            //{
            //    builder.AppendFormat(" ON PRIMARY (NAME='{0}', FILENAME='{1}')", Path.GetFileName(dataFilename), dataFilename);
            //    builder.AppendFormat(" LOG ON (NAME='{0}', FILENAME='{1}')", Path.GetFileName(logFilename), logFilename);
            //}
            builder.AppendLine();
            builder.AppendFormat("Use {0};", catalog);
            return builder.ToString();
        }

        public static IEnumerable<string> GetCreateForeignKeyCommands(MetaTable table)
        {
            foreach (var metaType in table.RowType.InheritanceTypes)
            {
                foreach (var command in GetCreateForeignKeyCommands(metaType))
                {
                    yield return command;
                }
            }
        }

        private static IEnumerable<string> GetCreateForeignKeyCommands(MetaType metaType)
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
                                                       new object[] { MySqlIdentifier.QuoteIdentifier(metaType.Table.TableName), MySqlIdentifier.QuoteIdentifier(member.Name) });
                        }
                        var command = "ALTER TABLE {0}" + Environment.NewLine + "  ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3}({4})";
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
                                                        metaType.Table.TableName,
                                                        mappedName,
                                                        thisKey,
                                                        otherTable,
                                                        otherKey
                                                    }).ToString();
                    }
                }
            }

        }

        public static string GetCreateTableCommand(MetaTable table)
        {
            var builder = new StringBuilder();
            var sb = new StringBuilder();
            BuildFieldDeclarations(table, sb);
            builder.AppendFormat("CREATE TABLE {0}", MySqlIdentifier.QuoteIdentifier(table.TableName));
            builder.Append("(");
            builder.Append(sb.ToString());
            sb = new StringBuilder();
            BuildPrimaryKey(table, sb);
            if (sb.Length > 0)
            {
                string s = string.Format(CultureInfo.InvariantCulture, "PK_{0}",
                                         new object[] { table.TableName });
                builder.Append(", ");
                builder.AppendLine();
                builder.AppendFormat("  CONSTRAINT {0} PRIMARY KEY ({1})", s, sb);
            }
            builder.AppendLine();
            builder.Append("  )");
            return builder.ToString();
        }

        private static string GetDbType(MetaDataMember mm)
        {
            string dbType = mm.DbType;
            if (dbType != null)
            {
                if (dbType.StartsWith("N", StringComparison.CurrentCultureIgnoreCase))
                    return dbType.Substring(1);
                if (dbType.Equals("Image", StringComparison.CurrentCultureIgnoreCase))
                    return "BLOB";
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
            else if (mm.IsPrimaryKey && mm.IsDbGenerated)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Object:
                        if (type != typeof(Guid))
                        {
                            throw SqlClient.Error.CouldNotDetermineDbGeneratedSqlType(type);
                        }
                        builder.Append("TINYBLOB");
                        goto Label_02AD;

                    case TypeCode.DBNull:
                    case TypeCode.Boolean:
                    case TypeCode.Char:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        goto Label_02AD;

                    case TypeCode.SByte:
                    case TypeCode.Int16:
                        builder.Append("SmallInt");
                        goto Label_02AD;

                    case TypeCode.Byte:
                        builder.Append("TinyInt");
                        goto Label_02AD;

                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                        builder.Append("Int");
                        goto Label_02AD;

                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                        builder.Append("BigInt");
                        goto Label_02AD;

                    case TypeCode.UInt64:
                    case TypeCode.Decimal:
                        builder.Append("Real");
                        goto Label_02AD;
                }
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Object:
                        if (type == typeof(Guid))
                        {
                            builder.Append("TINYBLOB");
                        }

                        else if (type == typeof(byte[]))
                        {
                            builder.Append("LONGBLOB");
                        }
                        else if (type == typeof(char[]))
                        {
                            builder.Append("VarChar(4000)");
                        }
                        else if (type == typeof(XDocument) || type == typeof(XElement))
                        {
                            builder.Append("VarChar(4000)");
                        }
                        else if (type == typeof(ALinq.Binary))
                        {
                            builder.Append("LONGBLOB");
                        }
                        else
                        {
                            throw SqlClient.Error.CouldNotDetermineSqlType(type);
                        }
                        goto Label_02AD;

                    case TypeCode.DBNull:
                        goto Label_02AD;

                    case TypeCode.Boolean:
                        builder.Append("BIT");
                        goto Label_02AD;

                    case TypeCode.Char:
                        builder.Append("CHAR(1)");
                        goto Label_02AD;

                    case TypeCode.SByte:
                    case TypeCode.Int16:
                        builder.Append("SmallInt");
                        goto Label_02AD;

                    case TypeCode.Byte:
                        builder.Append("TinyInt");
                        goto Label_02AD;

                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                        builder.Append("Int");
                        goto Label_02AD;

                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                        builder.Append("BigInt");
                        goto Label_02AD;

                    case TypeCode.UInt64:
                        builder.Append("Decimal(20)");
                        goto Label_02AD;

                    case TypeCode.Single:
                        builder.Append("Real");
                        goto Label_02AD;

                    case TypeCode.Double:
                        builder.Append("Float");
                        goto Label_02AD;

                    case TypeCode.Decimal:
                        builder.Append("Real");
                        goto Label_02AD;

                    case TypeCode.DateTime:
                        builder.Append("DateTime");
                        goto Label_02AD;

                    case TypeCode.String:
                        builder.Append("TEXT");
                        goto Label_02AD;
                }
            }
        Label_02AD:
            if (!canBeNull)
            {
                builder.Append(" NOT NULL");
            }
            if (mm.IsPrimaryKey && mm.IsDbGenerated)
            {
                if (type == typeof(Guid))
                {
                    builder.Append(" DEFAULT NEWID()");
                }
                else
                {
                    builder.Append(" AUTO_INCREMENT");
                }
            }
            return builder.ToString();
        }

        internal static string GetDropDatabaseCommand(string catalog)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("DROP DATABASE {0}", MySqlIdentifier.QuoteIdentifier(catalog));
            return builder.ToString();
        }

        internal static bool IsNullable(Type type)
        {
            return (type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()));
        }

        // Nested Types

    }
}
