using System;
using System.Globalization;
using System.Reflection;

namespace ALinq.Mapping
{
    sealed partial class DynamicMappingSource : MappingSource
    {
        private readonly MappingSource source;
        //public static Func<MetaDataMember, bool> ItemMemberPredicate = o => o.Name != "Item";
        private AttributeMappingSource extendMappingSource;

        internal DynamicMappingSource(MappingSource source)
        {
            this.source = source;
        }

        protected override MetaModel CreateModel(Type dataContextType)
        {
            var sourceModel = source.GetModel(dataContextType);
            return new DynamicModel(sourceModel, this);
        }

   
    }
}