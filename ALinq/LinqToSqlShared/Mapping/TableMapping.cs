namespace LinqToSqlShared.Mapping
{
    #region MyRegion
    //internal class TableMapping : SqlMetalObject
    //{
    //    private const string TypeName = "LinqToSqlShared.Mapping.TableMapping";
    //    internal static System.Type SourceType = GetType(TypeName);

    //    // Methods
    //    internal TableMapping()
    //        : base(CreateSource(TypeName, null))
    //    {
    //    }

    //    public TableMapping(object source)
    //        : base(source)
    //    {
    //    }

    //    // Properties
    //    internal string Member
    //    {
    //        get
    //        {
    //            return (string)GetProperty("Member", false);
    //        }
    //        set
    //        {
    //            SetProperty("Member", value, false);
    //        }
    //    }

    //    public TypeMapping RowType
    //    {
    //        get
    //        {
    //            var result = GetProperty("RowType", false);
    //            if (result == null)
    //                return null;
    //            return new TypeMapping(result);
    //        }
    //        set
    //        {
    //            SetProperty("RowType", value, false);
    //        }
    //    }

    //    public string TableName
    //    {
    //        get
    //        {
    //            return (string)GetProperty("TableName", false);
    //        }
    //        set
    //        {
    //            SetProperty("TableName", value, false);
    //        }
    //    }
    //} 
    #endregion

    internal class TableMapping
    {
        // Fields

        // Methods

        // Properties
        internal string Member { get; set; }

        internal TypeMapping RowType { get; set; }

        internal string TableName { get; set; }
    }


}