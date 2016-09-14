using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.DynamicData.ModelProviders;
using System.Collections.ObjectModel;
using System.Reflection;
using ALinq.Mapping;

namespace ALinq.Web.DynamicData
{
    public sealed class ALinqDataModelProvider : DataModelProvider
    {
        // Fields
        private readonly Dictionary<PropertyInfo, ALinqColumnProvider> _entityMemberLookup = new Dictionary<PropertyInfo, ALinqColumnProvider>();
        private readonly ReadOnlyCollection<TableProvider> _roEntities;

        // Methods
        public ALinqDataModelProvider(Func<object> contextFactory)
            : this(null, contextFactory)
        {
        }

        public ALinqDataModelProvider(object contextInstance, Func<object> contextFactory)
        {
            this.ContextFactory = contextFactory;
            DataContext context = ((DataContext)contextInstance) ?? ((DataContext)this.CreateContext());
            this.ContextType = context.GetType();
            Dictionary<Type, PropertyInfo> dictionary = new Dictionary<Type, PropertyInfo>();
            foreach (PropertyInfo info in this.ContextType.GetProperties())
            {
                if (info.PropertyType.IsGenericType && (info.PropertyType.GetGenericTypeDefinition() == typeof(Table<>)))
                {
                    Type type2 = info.PropertyType.GetGenericArguments()[0];
                    dictionary[type2] = info;
                }
            }
            List<TableProvider> source = new List<TableProvider>();
            foreach (MetaTable table in context.Mapping.GetTables())
            {
                PropertyInfo contextProperty = dictionary[table.RowType.Type];
                source.Add(new ALinqTableProvider(this, table, contextProperty));
            }
            this.DLinqTables = new List<ALinqTableProvider>(source.Cast<ALinqTableProvider>());
            foreach (TableProvider provider in source)
            {
                ((ALinqTableProvider)provider).Initialize();
            }
            this._roEntities = new ReadOnlyCollection<TableProvider>(source);
        }

        public override object CreateContext()
        {
            return this.ContextFactory();
        }

        // Properties
        private Func<object> ContextFactory
        {
            get;
            set;
        }

        internal List<ALinqTableProvider> DLinqTables
        {
            get;
            private set;
        }

        internal Dictionary<PropertyInfo, ALinqColumnProvider> EntityMemberLookup
        {
            get
            {
                return this._entityMemberLookup;
            }
        }

        public override ReadOnlyCollection<TableProvider> Tables
        {
            get
            {
                return this._roEntities;
            }
        }
    }


}
