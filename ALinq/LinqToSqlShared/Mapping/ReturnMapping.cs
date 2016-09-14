using System.Reflection;

namespace LinqToSqlShared.Mapping
{
    #region MyRegion
    //internal class ReturnMapping : SqlMetalObject
    //{
    //    const string TypeName = "LinqToSqlShared.Mapping.ReturnMapping";
    //    // Fields
    //    private string dbType;

    //    public ReturnMapping():base(CreateSource(TypeName,null,BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance))
    //    {

    //    }

    //    internal ReturnMapping(object source) : base(source)
    //    {
    //    }

    //    // Properties
    //    internal string DbType
    //    {
    //        get
    //        {
    //            return this.dbType;
    //        }
    //        set
    //        {
    //            this.dbType = value;
    //        }
    //    }
    //} 
    #endregion

    internal class ReturnMapping
    {
        // Fields
        private string dbType;

        // Properties
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
    }


}