using System;
using System.Reflection;

namespace ALinq.Mapping
{
    internal sealed class AttributedMetaParameter : MetaParameter
    {
        // Fields
        private readonly ParameterAttribute paramAttrib;
        private readonly ParameterInfo parameterInfo;

        // Methods
        public AttributedMetaParameter(ParameterInfo parameterInfo)
        {
            this.parameterInfo = parameterInfo;
            this.paramAttrib = Attribute.GetCustomAttribute(parameterInfo, typeof(ParameterAttribute), false) as ParameterAttribute;
        }

        // Properties
        public override string DbType
        {
            get
            {
                if ((this.paramAttrib != null) && (this.paramAttrib.DbType != null))
                {
                    return this.paramAttrib.DbType;
                }
                return null;
            }
        }

        public override string MappedName
        {
            get
            {
                if ((this.paramAttrib != null) && (this.paramAttrib.Name != null))
                {
                    return this.paramAttrib.Name;
                }
                return this.parameterInfo.Name;
            }
        }

        public override string Name
        {
            get
            {
                return this.parameterInfo.Name;
            }
        }

        public override ParameterInfo Parameter
        {
            get
            {
                return this.parameterInfo;
            }
        }

        public override Type ParameterType
        {
            get
            {
                return this.parameterInfo.ParameterType;
            }
        }
    }
}