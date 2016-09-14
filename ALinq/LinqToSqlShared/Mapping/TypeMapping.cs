using System.Collections;
using System.Collections.Generic;

namespace LinqToSqlShared.Mapping
{
    #region MyRegion
    //internal class TypeMapping : SqlMetalObject
    //{
    //    private const string TypeName = "LinqToSqlShared.Mapping.TypeMapping";
    //    internal static System.Type SourceType = GetType(TypeName);

    //    // Methods
    //    internal TypeMapping()
    //        : base(CreateSource(TypeName, null))
    //    {
    //    }

    //    internal TypeMapping(object source)
    //        : base(source)
    //    {
    //    }

    //    // Properties
    //    public TypeMapping BaseType
    //    {
    //        get
    //        {
    //            return (TypeMapping)CreateInstance(GetProperty("BaseType"));
    //        }
    //        set
    //        {
    //            SetProperty("BaseType", value, false);
    //        }
    //    }

    //    private ReflectObjectList<TypeMapping> derivedTypes;
    //    internal IList<TypeMapping> DerivedTypes
    //    {
    //        get
    //        {
    //            if (derivedTypes == null)
    //            {
    //                var result = GetProperty("DerivedTypes", false);
    //                derivedTypes = new ReflectObjectList<TypeMapping>((IList)result);
    //            }
    //            return this.derivedTypes;
    //        }
    //    }

    //    internal string InheritanceCode
    //    {
    //        get
    //        {
    //            return (string)GetProperty("InheritanceCode");
    //        }
    //        set
    //        {
    //            SetProperty("InheritanceCode", value, false);
    //        }
    //    }

    //    internal bool IsInheritanceDefault
    //    {
    //        get
    //        {
    //            return (bool)GetProperty("IsInheritanceDefault");
    //        }
    //        set
    //        {
    //            SetProperty("IsInheritanceDefault", value, false);
    //        }
    //    }

    //    private ReflectObjectList<MemberMapping> members;
    //    public IList<MemberMapping> Members
    //    {
    //        get
    //        {
    //            if (members == null)
    //            {
    //                var result = GetProperty("Members");
    //                members = new ReflectObjectList<MemberMapping>((IList)result);
    //            }
    //            return members;
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

    //    internal string XmlIsInheritanceDefault
    //    {
    //        get
    //        {
    //            return (string)GetProperty("XmlIsInheritanceDefault");
    //        }
    //        set
    //        {
    //            SetProperty("XmlIsInheritanceDefault", value, false);
    //        }
    //    }
    //} 
    #endregion

    internal class TypeMapping
    {
        // Fields
        private TypeMapping baseType;
        private List<TypeMapping> derivedTypes = new List<TypeMapping>();
        private string inheritanceCode;
        private bool isInheritanceDefault;
        private List<MemberMapping> members = new List<MemberMapping>();
        private string name;

        // Methods
        internal TypeMapping()
        {
        }

        // Properties
        internal TypeMapping BaseType
        {
            get
            {
                return this.baseType;
            }
            set
            {
                this.baseType = value;
            }
        }

        internal List<TypeMapping> DerivedTypes
        {
            get
            {
                return this.derivedTypes;
            }
        }

        internal string InheritanceCode
        {
            get
            {
                return this.inheritanceCode;
            }
            set
            {
                this.inheritanceCode = value;
            }
        }

        internal bool IsInheritanceDefault
        {
            get
            {
                return this.isInheritanceDefault;
            }
            set
            {
                this.isInheritanceDefault = value;
            }
        }

        internal List<MemberMapping> Members
        {
            get
            {
                return this.members;
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

        internal string XmlIsInheritanceDefault
        {
            get
            {
                if (!this.isInheritanceDefault)
                {
                    return null;
                }
                return "true";
            }
            set
            {
                this.isInheritanceDefault = (value != null) ? bool.Parse(value) : false;
            }
        }
    }
}