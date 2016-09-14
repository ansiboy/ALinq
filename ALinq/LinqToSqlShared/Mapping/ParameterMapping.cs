using System;

namespace LinqToSqlShared.Mapping
{
    #region MyRegion
    //internal class ParameterMapping : SqlMetalObject
    //{
    //    private const string TypeName = "LinqToSqlShared.Mapping.ParameterMapping";
    //    // Fields
    //    private string dbType;
    //    private MappingParameterDirection direction;
    //    private string name;
    //    private string parameterName;

    //    // Properties
    //    public ParameterMapping()
    //        : base(CreateSource(TypeName, null))
    //    {
    //    }

    //    internal string DbType
    //    {
    //        get
    //        {
    //            return (string)GetProperty("DbType");
    //        }
    //        set
    //        {
    //            SetProperty("DbType", value, false);
    //        }
    //    }

    //    public MappingParameterDirection Direction
    //    {
    //        get
    //        {
    //            var result = GetProperty("Direction");
    //            return (MappingParameterDirection)((int)result);
    //        }
    //        set
    //        {
    //            SetProperty("Direction", CreateSourceEnum(value), false);
    //        }
    //    }

    //    internal string Name
    //    {
    //        get
    //        {
    //            return this.name;
    //        }
    //        set
    //        {
    //            this.name = value;
    //        }
    //    }

    //    internal string ParameterName
    //    {
    //        get
    //        {
    //            return this.parameterName;
    //        }
    //        set
    //        {
    //            this.parameterName = value;
    //        }
    //    }

    //    public string XmlDirection
    //    {
    //        get
    //        {
    //            if (this.direction != MappingParameterDirection.In)
    //            {
    //                return this.direction.ToString();
    //            }
    //            return null;
    //        }
    //        set
    //        {
    //            this.direction = (value == null) ? MappingParameterDirection.In : ((MappingParameterDirection)Enum.Parse(typeof(MappingParameterDirection), value, true));
    //        }
    //    }
    //} 
    #endregion

    internal class ParameterMapping
    {
        // Fields
        private string dbType;
        private MappingParameterDirection direction;
        private string name;
        private string parameterName;

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

        public MappingParameterDirection Direction
        {
            get
            {
                return this.direction;
            }
            set
            {
                this.direction = value;
            }
        }

        internal string Name
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

        internal string ParameterName
        {
            get
            {
                return this.parameterName;
            }
            set
            {
                this.parameterName = value;
            }
        }

        public string XmlDirection
        {
            get
            {
                if (this.direction != MappingParameterDirection.In)
                {
                    return this.direction.ToString();
                }
                return null;
            }
            set
            {
                this.direction = (value == null) ? MappingParameterDirection.In : ((MappingParameterDirection)Enum.Parse(typeof(MappingParameterDirection), value, true));
            }
        }
    }

 

}