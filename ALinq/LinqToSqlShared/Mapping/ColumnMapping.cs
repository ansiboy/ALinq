using System;
using LinqToSqlShared.Mapping;
using ALinq.Mapping;

namespace LinqToSqlShared.Mapping
{
    #region MyRegion
    //internal sealed class ColumnMapping : MemberMapping
    //{
    //    private const string TypeName = "LinqToSqlShared.Mapping.ColumnMapping";
    //    public static System.Type SourceType = GetType(TypeName);

    //    internal ColumnMapping(object source)
    //        : base(source)
    //    {
    //    }

    //    internal ColumnMapping()
    //        : base(CreateSource(TypeName, null))
    //    {
    //    }

    //    public string DbType
    //    {
    //        get { return (string)GetProperty("DbType"); }
    //        set { SetProperty("DbType", value, false); }
    //    }

    //    public string XmlCanBeNull
    //    {
    //        get { return (string)GetProperty("XmlCanBeNull"); }
    //    }

    //    public string Expression
    //    {
    //        get { return (string)GetProperty("Expression"); }
    //        set { SetProperty("Expression", value, false); }
    //    }

    //    public string XmlIsPrimaryKey
    //    {
    //        get { return (string)GetProperty("XmlIsPrimaryKey"); }
    //    }

    //    public string XmlIsDbGenerated
    //    {
    //        get { return (string)GetProperty("XmlIsDbGenerated"); }
    //    }

    //    public string XmlIsVersion
    //    {
    //        get { return (string)GetProperty("XmlIsVersion"); }
    //    }

    //    public string XmlIsDiscriminator
    //    {
    //        get { return (string)GetProperty("XmlIsDiscriminator"); }
    //    }

    //    public string XmlUpdateCheck
    //    {
    //        get { return (string)GetProperty("XmlUpdateCheck"); }
    //    }

    //    public string XmlAutoSync
    //    {
    //        get { return (string)GetProperty("XmlAutoSync"); }
    //    }

    //    public bool IsDbGenerated
    //    {
    //        get { return (bool)GetProperty("IsDbGenerated"); }
    //        set { SetProperty("IsDbGenerated", value, false); }
    //    }

    //    public bool IsDiscriminator
    //    {
    //        get { return (bool)GetProperty("IsDiscriminator"); }
    //        set { SetProperty("IsDiscriminator", value, false); }
    //    }

    //    public bool IsPrimaryKey
    //    {
    //        get { return (bool)GetProperty("IsPrimaryKey", false); }
    //        set { SetProperty("IsPrimaryKey", value, false); }
    //    }

    //    public bool IsVersion
    //    {
    //        get { return (bool)GetProperty("IsVersion", false); }
    //        set { SetProperty("IsVersion", value, false); }
    //    }

    //    public bool? CanBeNull
    //    {
    //        get { return (bool?)GetProperty("CanBeNull"); }
    //        set { SetProperty("CanBeNull", value, false); }
    //    }

    //    public UpdateCheck UpdateCheck
    //    {
    //        get
    //        {
    //            var result = GetProperty("UpdateCheck");
    //            return (UpdateCheck)((int)result);

    //        }
    //        set { SetProperty("UpdateCheck", CreateSourceEnum(value), false); }
    //    }

    //    public AutoSync AutoSync
    //    {
    //        get
    //        {
    //            var result = GetProperty("AutoSync");
    //            return (AutoSync)((int)result);
    //        }
    //        set { SetProperty("AutoSync", CreateSourceEnum(value), false); }
    //    }
    //} 
    #endregion

    internal sealed class ColumnMapping : MemberMapping
    {
        // Fields
        private AutoSync autoSync;
        private bool? canBeNull = null;
        private string dbType;
        private string expression;
        private bool isDBGenerated;
        private bool isDiscriminator;
        private bool isPrimaryKey;
        private bool isVersion;
        private UpdateCheck updateCheck;

        // Methods
        internal ColumnMapping()
        {
        }

        // Properties
        internal ALinq.Mapping.AutoSync AutoSync
        {
            get
            {
                return this.autoSync;
            }
            set
            {
                this.autoSync = value;
            }
        }

        internal bool? CanBeNull
        {
            get
            {
                return this.canBeNull;
            }
            set
            {
                this.canBeNull = value;
            }
        }

        internal string DbType
        {
            get
            {
                return this.dbType;
            }
            set
            {
                this.dbType = value;
            }
        }

        internal string Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                this.expression = value;
            }
        }

        internal bool IsDbGenerated
        {
            get
            {
                return this.isDBGenerated;
            }
            set
            {
                this.isDBGenerated = value;
            }
        }

        internal bool IsDiscriminator
        {
            get
            {
                return this.isDiscriminator;
            }
            set
            {
                this.isDiscriminator = value;
            }
        }

        internal bool IsPrimaryKey
        {
            get
            {
                return this.isPrimaryKey;
            }
            set
            {
                this.isPrimaryKey = value;
            }
        }

        internal bool IsVersion
        {
            get
            {
                return this.isVersion;
            }
            set
            {
                this.isVersion = value;
            }
        }

        internal UpdateCheck UpdateCheck
        {
            get
            {
                return this.updateCheck;
            }
            set
            {
                this.updateCheck = value;
            }
        }

        internal string XmlAutoSync
        {
            get
            {
                if (this.autoSync == AutoSync.Default)
                {
                    return null;
                }
                return this.autoSync.ToString();
            }
            set
            {
                this.autoSync = (value != null) ? ((AutoSync)Enum.Parse(typeof(AutoSync), value)) : AutoSync.Default;
            }
        }

        internal string XmlCanBeNull
        {
            get
            {
                if (this.canBeNull.HasValue && (this.canBeNull != true))
                {
                    return "false";
                }
                return null;
            }
            set
            {
                this.canBeNull = new bool?((value != null) ? bool.Parse(value) : true);
            }
        }

        internal string XmlIsDbGenerated
        {
            get
            {
                if (!this.isDBGenerated)
                {
                    return null;
                }
                return "true";
            }
            set
            {
                this.isDBGenerated = (value != null) ? bool.Parse(value) : false;
            }
        }

        internal string XmlIsDiscriminator
        {
            get
            {
                if (!this.isDiscriminator)
                {
                    return null;
                }
                return "true";
            }
            set
            {
                this.isDiscriminator = (value != null) ? bool.Parse(value) : false;
            }
        }

        internal string XmlIsPrimaryKey
        {
            get
            {
                if (!this.isPrimaryKey)
                {
                    return null;
                }
                return "true";
            }
            set
            {
                this.isPrimaryKey = (value != null) ? bool.Parse(value) : false;
            }
        }

        internal string XmlIsVersion
        {
            get
            {
                if (!this.isVersion)
                {
                    return null;
                }
                return "true";
            }
            set
            {
                this.isVersion = (value != null) ? bool.Parse(value) : false;
            }
        }

        internal string XmlUpdateCheck
        {
            get
            {
                if (this.updateCheck == UpdateCheck.Always)
                {
                    return null;
                }
                return this.updateCheck.ToString();
            }
            set
            {
                this.updateCheck = (value == null) ? UpdateCheck.Always : ((UpdateCheck)Enum.Parse(typeof(UpdateCheck), value));
            }
        }
    }
 

}
