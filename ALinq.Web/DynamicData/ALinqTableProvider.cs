using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Web.DynamicData.ModelProviders;
using ALinq.Mapping;
using System.Collections.Generic;

namespace ALinq.Web.DynamicData
{
    internal sealed class ALinqTableProvider : TableProvider
    {
        // Fields
        private readonly ReadOnlyCollection<ColumnProvider> _roMembers;


        // Methods
        public ALinqTableProvider(ALinqDataModelProvider dataModel, MetaTable table, PropertyInfo contextProperty)
            : base(dataModel)
        {
            this.ContextProperty = contextProperty;
            this.Name = this.ContextProperty.Name;
            this.EntityType = table.RowType.Type;
            List<ColumnProvider> list = new List<ColumnProvider>();
            foreach (MetaDataMember member in table.RowType.DataMembers)
            {
                ALinqColumnProvider item = new ALinqColumnProvider(this, member);
                list.Add(item);
                dataModel.EntityMemberLookup[(PropertyInfo)member.Member] = item;
            }
            this._roMembers = new ReadOnlyCollection<ColumnProvider>(list);
        }

        public override IQueryable GetQuery(object context)
        {
            return (IQueryable)this.ContextProperty.GetValue(context, null);
        }

        internal void Initialize()
        {
            foreach (ColumnProvider provider in this.Columns)
            {
                ((ALinqColumnProvider)provider).Initialize();
            }
        }

        // Properties
        public override ReadOnlyCollection<ColumnProvider> Columns
        {
            get
            {
                return this._roMembers;
            }
        }

        private PropertyInfo ContextProperty
        {
            get;
            set;
        }
    }


}
