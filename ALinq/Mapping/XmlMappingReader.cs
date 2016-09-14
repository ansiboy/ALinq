using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using LinqToSqlShared.Mapping;

namespace ALinq.Mapping
{
    internal class XmlMappingReader
    {
        // Methods
        private static void AssertEmptyElement(XmlReader reader)
        {
            if (!reader.IsEmptyElement)
            {
                string name = reader.Name;
                reader.Read();
                if (reader.NodeType != XmlNodeType.EndElement)
                {
                    throw Error.ExpectedEmptyElement(name, reader.NodeType, reader.Name);
                }
            }
            reader.Skip();
        }

        internal static bool IsInNamespace(XmlReader reader)
        {
            return (reader.LookupNamespace(reader.Prefix) == "http://schemas.microsoft.com/linqtosql/mapping/2007");
        }

        private static string OptionalAttribute(XmlReader reader, string attribute)
        {
            return reader.GetAttribute(attribute);
        }

        private static bool OptionalBoolAttribute(XmlReader reader, string attribute, bool @default)
        {
            string str = OptionalAttribute(reader, attribute);
            if (str == null)
            {
                return @default;
            }
            return bool.Parse(str);
        }

        private static bool? OptionalNullableBoolAttribute(XmlReader reader, string attribute)
        {
            string str = OptionalAttribute(reader, attribute);
            if (str == null)
            {
                return null;
            }
            return new bool?(bool.Parse(str));
        }

        private static AssociationMapping ReadAssociationMapping(XmlReader reader)
        {
            if (!IsInNamespace(reader) || (reader.LocalName != "Association"))
            {
                throw Error.UnexpectedElement("Association", string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName }));
            }
            ValidateAttributes(reader, new string[] { "Name", "IsForeignKey", "IsUnique", "Member", "OtherKey", "Storage", "ThisKey", "DeleteRule", "DeleteOnNull" });
            AssociationMapping mapping = new AssociationMapping();
            mapping.DbName = OptionalAttribute(reader, "Name");
            mapping.IsForeignKey = OptionalBoolAttribute(reader, "IsForeignKey", false);
            mapping.IsUnique = OptionalBoolAttribute(reader, "IsUnique", false);
            mapping.MemberName = RequiredAttribute(reader, "Member");
            mapping.OtherKey = OptionalAttribute(reader, "OtherKey");
            mapping.StorageMemberName = OptionalAttribute(reader, "Storage");
            mapping.ThisKey = OptionalAttribute(reader, "ThisKey");
            mapping.DeleteRule = OptionalAttribute(reader, "DeleteRule");
            mapping.DeleteOnNull = OptionalBoolAttribute(reader, "DeleteOnNull", false);
            AssertEmptyElement(reader);
            return mapping;
        }

        private static ColumnMapping ReadColumnMapping(XmlReader reader)
        {
            if (!IsInNamespace(reader) || (reader.LocalName != "Column"))
            {
                throw Error.UnexpectedElement("Column", string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName }));
            }
            ValidateAttributes(reader, new string[] { "Name", "DbType", "IsDbGenerated", "IsDiscriminator", "IsPrimaryKey", "IsVersion", "Member", "Storage", "Expression", "CanBeNull", "UpdateCheck", "AutoSync" });
            ColumnMapping mapping = new ColumnMapping();
            mapping.DbName = OptionalAttribute(reader, "Name");
            mapping.DbType = OptionalAttribute(reader, "DbType");
            mapping.IsDbGenerated = OptionalBoolAttribute(reader, "IsDbGenerated", false);
            mapping.IsDiscriminator = OptionalBoolAttribute(reader, "IsDiscriminator", false);
            mapping.IsPrimaryKey = OptionalBoolAttribute(reader, "IsPrimaryKey", false);
            mapping.IsVersion = OptionalBoolAttribute(reader, "IsVersion", false);
            mapping.MemberName = RequiredAttribute(reader, "Member");
            mapping.StorageMemberName = OptionalAttribute(reader, "Storage");
            mapping.Expression = OptionalAttribute(reader, "Expression");
            mapping.CanBeNull = OptionalNullableBoolAttribute(reader, "CanBeNull");
            string str = OptionalAttribute(reader, "UpdateCheck");
            mapping.UpdateCheck = (str == null) ? UpdateCheck.Always : ((UpdateCheck)Enum.Parse(typeof(UpdateCheck), str));
            string str2 = OptionalAttribute(reader, "AutoSync");
            mapping.AutoSync = (str2 == null) ? AutoSync.Default : ((AutoSync)Enum.Parse(typeof(AutoSync), str2));
            AssertEmptyElement(reader);
            return mapping;
        }

        internal static DatabaseMapping ReadDatabaseMapping(XmlReader reader)
        {
            if (!IsInNamespace(reader) || (reader.LocalName != "Database"))
            {
                return null;
            }
            ValidateAttributes(reader, new string[] { "Name", "Provider" });
            DatabaseMapping mapping = new DatabaseMapping();
            mapping.DatabaseName = RequiredAttribute(reader, "Name");
            mapping.Provider = OptionalAttribute(reader, "Provider");
            if (reader.IsEmptyElement)
            {
                reader.Skip();
                return mapping;
            }
            reader.ReadStartElement();
            reader.MoveToContent();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                if ((reader.NodeType == XmlNodeType.Whitespace) || !IsInNamespace(reader))
                {
                    reader.Skip();
                    continue;
                }
                string localName = reader.LocalName;
                if (localName == null)
                {
                    goto Label_00E8;
                }
                if (!(localName == "Table"))
                {
                    if (localName == "Function")
                    {
                        goto Label_00D5;
                    }
                    goto Label_00E8;
                }
                mapping.Tables.Add(ReadTableMapping(reader));
                goto Label_0133;
            Label_00D5:
                mapping.Functions.Add(ReadFunctionMapping(reader));
                goto Label_0133;
            Label_00E8: ;
                throw Error.UnrecognizedElement(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName }));
            Label_0133:
                reader.MoveToContent();
            }
            if (reader.LocalName != "Database")
            {
                throw Error.UnexpectedElement("Database", string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName }));
            }
            reader.ReadEndElement();
            return mapping;
        }

        private static TypeMapping ReadElementTypeMapping(TypeMapping baseType, XmlReader reader)
        {
            if (!IsInNamespace(reader) || (reader.LocalName != "ElementType"))
            {
                throw Error.UnexpectedElement("Type", string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName }));
            }
            return ReadTypeMappingImpl(baseType, reader);
        }

        internal static FunctionMapping ReadFunctionMapping(XmlReader reader)
        {
            if (!IsInNamespace(reader) || (reader.LocalName != "Function"))
            {
                throw Error.UnexpectedElement("Function", string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName }));
            }
            ValidateAttributes(reader, new string[] { "Name", "Method", "IsComposable" });
            FunctionMapping mapping = new FunctionMapping();
            mapping.MethodName = RequiredAttribute(reader, "Method");
            mapping.Name = OptionalAttribute(reader, "Name");
            mapping.IsComposable = OptionalBoolAttribute(reader, "IsComposable", false);
            if (reader.IsEmptyElement)
            {
                reader.Skip();
                return mapping;
            }
            reader.ReadStartElement();
            reader.MoveToContent();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                if ((reader.NodeType == XmlNodeType.Whitespace) || !IsInNamespace(reader))
                {
                    reader.Skip();
                    continue;
                }
                string localName = reader.LocalName;
                if (localName == null)
                {
                    goto Label_016C;
                }
                if (!(localName == "Parameter"))
                {
                    if (localName == "ElementType")
                    {
                        goto Label_014A;
                    }
                    if (localName == "Return")
                    {
                        goto Label_015E;
                    }
                    goto Label_016C;
                }
                mapping.Parameters.Add(ReadParameterMapping(reader));
                goto Label_01BC;
            Label_014A:
                mapping.Types.Add(ReadElementTypeMapping(null, reader));
                goto Label_01BC;
            Label_015E:
                mapping.FunReturn = ReadReturnMapping(reader);
                goto Label_01BC;
            Label_016C: ;
                throw Error.UnrecognizedElement(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName }));
            Label_01BC:
                reader.MoveToContent();
            }
            reader.ReadEndElement();
            return mapping;
        }

        private static ParameterMapping ReadParameterMapping(XmlReader reader)
        {
            if (!IsInNamespace(reader) || (reader.LocalName != "Parameter"))
            {
                throw Error.UnexpectedElement("Parameter", string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName }));
            }
            ValidateAttributes(reader, new string[] { "Name", "DbType", "Parameter", "Direction" });
            ParameterMapping mapping = new ParameterMapping();
            mapping.Name = RequiredAttribute(reader, "Name");
            mapping.ParameterName = RequiredAttribute(reader, "Parameter");
            mapping.DbType = OptionalAttribute(reader, "DbType");
            mapping.XmlDirection = OptionalAttribute(reader, "Direction");
            AssertEmptyElement(reader);
            return mapping;
        }

        private static LinqToSqlShared.Mapping.ReturnMapping ReadReturnMapping(XmlReader reader)
        {
            if (!IsInNamespace(reader) || (reader.LocalName != "Return"))
            {
                throw Error.UnexpectedElement("Return", string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName }));
            }
            ValidateAttributes(reader, new[] { "DbType" });
            var mapping = new LinqToSqlShared.Mapping.ReturnMapping();
            mapping.DbType = OptionalAttribute(reader, "DbType");
            AssertEmptyElement(reader);
            return mapping;
        }

        private static TableMapping ReadTableMapping(XmlReader reader)
        {
            if (!IsInNamespace(reader) || (reader.LocalName != "Table"))
            {
                throw Error.UnexpectedElement("Table", string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName }));
            }
            ValidateAttributes(reader, new string[] { "Name", "Member" });
            TableMapping mapping = new TableMapping();
            mapping.TableName = OptionalAttribute(reader, "Name");
            mapping.Member = OptionalAttribute(reader, "Member");
            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement();
                reader.MoveToContent();
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    if ((reader.NodeType == XmlNodeType.Whitespace) || !IsInNamespace(reader))
                    {
                        reader.Skip();
                    }
                    else
                    {
                        string str;
                        if ((((str = reader.LocalName) == null) || (str != "Type")) || (mapping.RowType != null))
                        {
                            throw Error.UnrecognizedElement(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName }));
                        }
                        mapping.RowType = ReadTypeMapping(null, reader);
                        reader.MoveToContent();
                    }
                }
                if (reader.LocalName != "Table")
                {
                    throw Error.UnexpectedElement("Table", string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName }));
                }
                reader.ReadEndElement();
                return mapping;
            }
            reader.Skip();
            return mapping;
        }

        private static TypeMapping ReadTypeMapping(TypeMapping baseType, XmlReader reader)
        {
            if (!IsInNamespace(reader) || (reader.LocalName != "Type"))
            {
                throw Error.UnexpectedElement("Type", string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName }));
            }
            return ReadTypeMappingImpl(baseType, reader);
        }

        private static TypeMapping ReadTypeMappingImpl(TypeMapping baseType, XmlReader reader)
        {
            ValidateAttributes(reader, new string[] { "Name", "InheritanceCode", "IsInheritanceDefault" });
            TypeMapping mapping = new TypeMapping();
            mapping.BaseType = baseType;
            mapping.Name = RequiredAttribute(reader, "Name");
            mapping.InheritanceCode = OptionalAttribute(reader, "InheritanceCode");
            mapping.IsInheritanceDefault = OptionalBoolAttribute(reader, "IsInheritanceDefault", false);
            if (reader.IsEmptyElement)
            {
                reader.Skip();
                return mapping;
            }
            reader.ReadStartElement();
            reader.MoveToContent();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                if ((reader.NodeType == XmlNodeType.Whitespace) || !IsInNamespace(reader))
                {
                    reader.Skip();
                    continue;
                }
                string localName = reader.LocalName;
                if (localName == null)
                {
                    goto Label_010E;
                }
                if (!(localName == "Type"))
                {
                    if (localName == "Association")
                    {
                        goto Label_00E8;
                    }
                    if (localName == "Column")
                    {
                        goto Label_00FB;
                    }
                    goto Label_010E;
                }
                mapping.DerivedTypes.Add(ReadTypeMapping(mapping, reader));
                goto Label_0159;
            Label_00E8:
                mapping.Members.Add(ReadAssociationMapping(reader));
                goto Label_0159;
            Label_00FB:
                mapping.Members.Add(ReadColumnMapping(reader));
                goto Label_0159;
            Label_010E: ;
                throw Error.UnrecognizedElement(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : "/", reader.LocalName }));
            Label_0159:
                reader.MoveToContent();
            }
            reader.ReadEndElement();
            return mapping;
        }

        private static string RequiredAttribute(XmlReader reader, string attribute)
        {
            string str = OptionalAttribute(reader, attribute);
            if (str == null)
            {
                throw Error.CouldNotFindRequiredAttribute(attribute, reader.ReadOuterXml());
            }
            return str;
        }

        internal static void ValidateAttributes(XmlReader reader, string[] validAttributes)
        {
            if (reader.HasAttributes)
            {
                List<string> list = new List<string>(validAttributes);
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    if ((IsInNamespace(reader) && (reader.LocalName != "xmlns")) && !list.Contains(reader.LocalName))
                    {
                        throw Error.UnrecognizedAttribute(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { reader.Prefix, string.IsNullOrEmpty(reader.Prefix) ? "" : ":", reader.LocalName }));
                    }
                }
                reader.MoveToElement();
            }
        }
    }
}
