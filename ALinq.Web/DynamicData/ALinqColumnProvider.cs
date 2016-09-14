using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.DynamicData.ModelProviders;
using System.Xml.Linq;
using ALinq.Mapping;

namespace ALinq.Web.DynamicData
{
    internal sealed class ALinqColumnProvider : ColumnProvider
    {
        // Fields
        private AssociationProvider _association;
        private readonly bool _isAssociation;
        //[CompilerGenerated]
        //private MetaDataMember<Member> k__BackingField;
        private static readonly Regex s_varCharRegEx = new Regex(@"N?(?:Var)?Char\(([0-9]+)\)", RegexOptions.IgnoreCase);

        // Methods
        public ALinqColumnProvider(ALinqTableProvider table, MetaDataMember member)
            : base(table)
        {
            this.Member = member;
            this.Name = member.Name;
            this.ColumnType = GetMemberType(member);
            this.IsPrimaryKey = member.IsPrimaryKey;
            this.IsGenerated = member.IsDbGenerated;
            this._isAssociation = member.IsAssociation;
            this.IsCustomProperty = !member.IsAssociation && (this.Member.DbType == null);
            this.Nullable = this.Member.IsAssociation ? this.Member.Association.IsNullable : this.Member.CanBeNull;
            this.MaxLength = ProcessMaxLength(this.ColumnType, this.Member.DbType);
            this.IsSortable = ProcessIsSortable(this.ColumnType, this.Member.DbType);
        }

        private static Type GetMemberType(MetaDataMember member)
        {
            Type type = member.Type;
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(EntitySet<>)))
            {
                return type.GetGenericArguments()[0];
            }
            return type;
        }

        internal void Initialize()
        {
            if (this._isAssociation && (this._association == null))
            {
                this._association = new ALinqAssociationProvider(this);
            }
        }

        internal static bool ProcessIsSortable(Type memberType, string dbType)
        {
            if (dbType == null)
            {
                return false;
            }
            if ((memberType == typeof(string)) && (dbType.StartsWith("Text", StringComparison.OrdinalIgnoreCase) || dbType.StartsWith("NText", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            if ((memberType == typeof(Binary)) && dbType.StartsWith("Image", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (memberType == typeof(XElement))
            {
                return false;
            }
            return true;
        }

        internal static int ProcessMaxLength(Type memberType, string dbType)
        {
            if ((dbType != null) && ((memberType == typeof(string)) || (Misc.RemoveNullableFromType(memberType) == typeof(char))))
            {
                if (dbType.StartsWith("NText", StringComparison.OrdinalIgnoreCase))
                {
                    return 0x3fffffff;
                }
                if (dbType.StartsWith("Text", StringComparison.OrdinalIgnoreCase))
                {
                    return 0x7fffffff;
                }
                if (dbType.StartsWith("NVarChar(MAX)", StringComparison.OrdinalIgnoreCase))
                {
                    return 0x3ffffffd;
                }
                if (dbType.StartsWith("VarChar(MAX)", StringComparison.OrdinalIgnoreCase))
                {
                    return 0x7ffffffd;
                }
                Match match = s_varCharRegEx.Match(dbType);
                if (match.Success)
                {
                    return int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                }
            }
            return 0;
        }

        // Properties
        public override AssociationProvider Association
        {
            get
            {
                this.Initialize();
                return this._association;
            }
        }

        public override PropertyInfo EntityTypeProperty
        {
            get
            {
                return (PropertyInfo)this.Member.Member;
            }
        }

        internal new bool IsForeignKeyComponent
        {
            set
            {
                base.IsForeignKeyComponent = value;
            }
        }

        internal MetaDataMember Member
        {
            get;
            set;
        }
    }


}
