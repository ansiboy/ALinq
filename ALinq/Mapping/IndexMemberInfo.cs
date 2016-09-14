using System;
using System.Globalization;
using System.Reflection;

namespace ALinq.Mapping
{
    #region MyRegion
    //class IndexMemberInfo : MethodInfo
    //{
    //    private MethodInfo source;
    //    private string name;
    //    private Type returnType;

    //    static int count = 0;
    //    public IndexMemberInfo(MethodInfo source, string name)
    //    {
    //        Debug.Assert(source.Name == "get_Item");
    //        this.source = source;
    //        this.name = name;
    //        this.returnType = source.ReturnType;
    //        count = count + 1;
    //    }

    //    public override int MetadataToken
    //    {
    //        get
    //        {
    //            return Int16.MaxValue + count;
    //        }
    //    }

    //    public void SetReturnType(Type value)
    //    {
    //        returnType = value;
    //    }

    //    public override Type ReturnType
    //    {
    //        get
    //        {
    //            return returnType;
    //        }
    //    }

    //    public override object[] GetCustomAttributes(bool inherit)
    //    {
    //        return this.source.GetCustomAttributes(inherit);
    //    }

    //    public override bool IsDefined(Type attributeType, bool inherit)
    //    {
    //        return source.IsDefined(attributeType, inherit);
    //    }

    //    public override ParameterInfo[] GetParameters()
    //    {
    //        return source.GetParameters();
    //    }

    //    public override MethodImplAttributes GetMethodImplementationFlags()
    //    {
    //        return source.GetMethodImplementationFlags();
    //    }

    //    public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder,
    //                                  object[] parameters, CultureInfo culture)
    //    {
    //        return source.Invoke(obj, invokeAttr, binder, parameters, culture);
    //    }

    //    public override MethodInfo GetBaseDefinition()
    //    {
    //        return source.GetBaseDefinition();
    //    }

    //    public override ICustomAttributeProvider ReturnTypeCustomAttributes
    //    {
    //        get { return source.ReturnTypeCustomAttributes; }
    //    }

    //    public override string Name
    //    {
    //        get { return name; }
    //    }

    //    public override Type DeclaringType
    //    {
    //        get { return source.DeclaringType; }
    //    }

    //    public override Type ReflectedType
    //    {
    //        get { return source.ReflectedType; }
    //    }

    //    public override RuntimeMethodHandle MethodHandle
    //    {
    //        get { return source.MethodHandle; }
    //    }

    //    public override MethodAttributes Attributes
    //    {
    //        get { return source.Attributes; }
    //    }

    //    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    //    {
    //        return source.GetCustomAttributes(attributeType, inherit);
    //    }


    //} 
    #endregion

    internal class IndexerMemberInfo : PropertyInfo
    {

        private readonly string _name;

        private Type _propertyType;

        private readonly MethodInfo source;
        private readonly MethodInfo setMethod;
        private readonly MethodInfo getMethod;
        private Type declaringType;

        public IndexerMemberInfo(MethodInfo source, string name, Type propertyType)
        {
            if (source == null)
                throw SqlClient.Error.ArgumentNull("source");

            this.declaringType = source.DeclaringType;
            this._name = name;
            this._propertyType = propertyType;
            this.getMethod = source;
            this.setMethod = source.DeclaringType.GetMethod("set_Item", new[] { typeof(string), typeof(object) });
        }

        public IndexerMemberInfo(Type declaringType, string name, Type propertyType)
        {
            if (declaringType == null)
                throw SqlClient.Error.ArgumentNull("declaringType");

            this.declaringType = declaringType;
            this._name = name;
            this._propertyType = propertyType;
            this.getMethod = declaringType.GetMethod("get_Item", new[] { typeof(string) });
            this.setMethod = declaringType.GetMethod("set_Item", new[] { typeof(string), typeof(object) });
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException("IndexMemberInfo.GetCustomAttributes");
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException("IndexMemberInfo.IsDefined");
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            return this.getMethod.Invoke(obj, new[] { Name });
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            this.setMethod.Invoke(obj, new[] { this.Name, value });
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotImplementedException("IndexMemberInfo.GetAccessors");
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return this.getMethod;
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return this.setMethod;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return new ParameterInfo[] { };
        }

        public override string Name
        {
            get { return _name; }
        }

        public override Type DeclaringType
        {
            get { return declaringType; }
        }

        public override Type ReflectedType
        {
            get { return declaringType; }
        }

        public override Type PropertyType
        {
            get { return _propertyType; }
        }

        public override PropertyAttributes Attributes
        {
            get { return PropertyAttributes.None; }
        }

        public override bool CanRead
        {
            get { return this.getMethod != null; }
        }

        public override bool CanWrite
        {
            get { return this.setMethod != null; }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException("IndexMemberInfo.GetCustomAttributes");
        }

        public void SetPropertyType(Type type)
        {
            this._propertyType = type;
        }

        public override string ToString()
        {
            return Name;
        }
    
    }
}