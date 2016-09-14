using System;
using System.Reflection;
using LinqToSqlShared.Mapping;

namespace ALinq.Mapping
{
    internal sealed class MappedReturnParameter : MetaParameter
    {
        // Fields
        private ReturnMapping map;
        private ParameterInfo parameterInfo;

        // Methods
        public MappedReturnParameter(ParameterInfo parameterInfo, ReturnMapping map)
        {
            this.parameterInfo = parameterInfo;
            this.map = map;
        }

        // Properties
        public override string DbType
        {
            get
            {
                return this.map.DbType;
            }
        }

        public override string MappedName
        {
            get
            {
                return null;
            }
        }

        public override string Name
        {
            get
            {
                return null;
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