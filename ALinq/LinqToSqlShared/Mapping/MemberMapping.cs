namespace LinqToSqlShared.Mapping
{
    #region MyRegion
    //internal abstract class MemberMapping : SqlMetalObject
    //{
    //    private const string TypeName = "LinqToSqlShared.Mapping.MemberMapping";

    //    // Methods
    //    internal MemberMapping()
    //        : base(CreateSource(TypeName, null))
    //    {
    //    }

    //    protected MemberMapping(object source)
    //        : base(source)
    //    {

    //    }

    //    // Properties
    //    internal string DbName
    //    {
    //        get
    //        {
    //            return (string)GetProperty("DbName", false);
    //        }
    //        set
    //        {
    //            SetProperty("DbName", value, false);
    //        }
    //    }

    //    public string MemberName
    //    {
    //        get
    //        {
    //            return (string)GetProperty("MemberName", false);
    //        }
    //        set
    //        {
    //            SetProperty("MemberName", value, false);

    //        }
    //    }

    //    public string StorageMemberName
    //    {
    //        get
    //        {
    //            return (string)GetProperty("StorageMemberName", false);
    //        }
    //        set
    //        {
    //            SetProperty("StorageMemberName", value, false);
    //        }
    //    }
    //} 
    #endregion

    internal abstract class MemberMapping
    {
        // Fields
        private string member;
        private string name;
        private string storageMember;

        // Methods
        internal MemberMapping()
        {
        }

        // Properties
        internal string DbName
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        internal string MemberName
        {
            get
            {
                return this.member;
            }
            set
            {
                this.member = value;
            }
        }

        internal string StorageMemberName
        {
            get
            {
                return this.storageMember;
            }
            set
            {
                this.storageMember = value;
            }
        }
    }
}