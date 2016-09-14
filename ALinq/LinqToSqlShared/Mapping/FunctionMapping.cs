using System.Collections;
using System.Collections.Generic;

namespace LinqToSqlShared.Mapping
{
    #region MyRegion
    //internal class FunctionMapping : SqlMetalObject
    //{
    //    private const string TypeName = "LinqToSqlShared.Mapping";

    //    // Methods
    //    internal FunctionMapping()
    //        : base(CreateSource(TypeName, null))
    //    {
    //    }

    //    // Properties
    //    public ReturnMapping FunReturn
    //    {
    //        get
    //        {
    //            var result = GetProperty("FunReturn", false);
    //            return new ReturnMapping(result);
    //        }
    //        set
    //        {
    //            SetProperty("FunReturn", value.Source, false);
    //        }
    //    }

    //    public bool IsComposable
    //    {
    //        get
    //        {
    //            return (bool)GetProperty("IsComposable");
    //        }
    //        set
    //        {
    //            SetProperty("IsComposable", value, false);
    //        }
    //    }

    //    public string MethodName
    //    {
    //        get
    //        {
    //            return (string)GetProperty("MethodName", false);
    //        }
    //        set
    //        {
    //            SetProperty("MethodName", value, false);
    //        }
    //    }

    //    public string Name
    //    {
    //        get
    //        {
    //            return (string)GetProperty("Name");
    //        }
    //        set
    //        {
    //            SetProperty("Name", value, false);
    //        }
    //    }

    //    private ReflectObjectList<ParameterMapping> parameters;

    //    public IList<ParameterMapping> Parameters
    //    {
    //        get
    //        {
    //            if (parameters == null)
    //            {
    //                var result = GetProperty("Parameters", false);
    //                parameters = new ReflectObjectList<ParameterMapping>((IList)result);
    //            }
    //            return this.parameters;
    //        }
    //    }

    //    private ReflectObjectList<TypeMapping> types;

    //    public FunctionMapping(object source)
    //        : base(source)
    //    {

    //    }

    //    public IList<TypeMapping> Types
    //    {
    //        get
    //        {
    //            if (types == null)
    //            {
    //                var result = GetProperty("Types");
    //                types = new ReflectObjectList<TypeMapping>((IList)result);
    //            }
    //            return types;
    //        }
    //    }

    //    internal string XmlIsComposable
    //    {
    //        get
    //        {
    //            return (string)GetProperty("XmlIsComposable");
    //        }
    //        set
    //        {
    //            SetProperty("XmlIsComposable", value, false);
    //        }
    //    }
    //} 
    #endregion

    internal class FunctionMapping
    {
        // Fields
        private ReturnMapping funReturn;
        private bool isComposable;
        private string methodName;
        private string name;
        private List<ParameterMapping> parameters = new List<ParameterMapping>();
        private List<TypeMapping> types = new List<TypeMapping>();

        // Methods
        internal FunctionMapping()
        {
        }

        // Properties
        internal ReturnMapping FunReturn
        {
            get
            {
                return this.funReturn;
            }
            set
            {
                this.funReturn = value;
            }
        }

        internal bool IsComposable
        {
            get
            {
                return this.isComposable;
            }
            set
            {
                this.isComposable = value;
            }
        }

        internal string MethodName
        {
            get
            {
                return this.methodName;
            }
            set
            {
                this.methodName = value;
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

        internal List<ParameterMapping> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        internal List<TypeMapping> Types
        {
            get
            {
                return this.types;
            }
        }

        internal string XmlIsComposable
        {
            get
            {
                if (!this.isComposable)
                {
                    return null;
                }
                return "true";
            }
            set
            {
                this.isComposable = (value != null) ? bool.Parse(value) : false;
            }
        }
    }
 

}