using System;
using System.Reflection;
using LinqToSqlShared.Mapping;

namespace ALinq.Mapping
{
    internal sealed class MappedParameter : MetaParameter
    {
        // Fields
        private readonly ParameterMapping map;
        private readonly ParameterInfo parameterInfo;

        // Methods
        public MappedParameter(ParameterInfo parameterInfo, ParameterMapping map)
        {
            this.parameterInfo = parameterInfo;
            this.map = map;
        }

        // Properties
        public override string DbType
        {
            get
            {
                return map.DbType;
            }
        }

        public override string MappedName
        {
            get
            {
                return this.map.Name;
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