using System.Collections;
using System.Linq.Expressions;
using ALinq.Mapping;

namespace ALinq
{
    internal interface IDataServices
    {
        // Methods
        object GetCachedObject(Expression query);
        IDeferredSourceFactory GetDeferredSourceFactory(MetaDataMember member);
        object InsertLookupCachedObject(MetaType type, object instance);
        bool IsCachedObject(MetaType type, object instance);
        void OnEntityMaterialized(MetaType type, object instance);

        // Properties
        DataContext Context { get; }
        MetaModel Model { get; }

    }

    internal interface IDeferredSourceFactory
    {
        // Methods
        IEnumerable CreateDeferredSource(object instance);
        IEnumerable CreateDeferredSource(object[] keyValues);
    }

 

}
