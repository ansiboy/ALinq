using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.DynamicData.ModelProviders;
using ALinq.Mapping;

namespace ALinq.Web.DynamicData
{
    internal sealed class ALinqAssociationProvider : AssociationProvider
    {
        // Methods
        public ALinqAssociationProvider(ALinqColumnProvider column)
        {
            Func<ALinqTableProvider, bool> predicate = null;
            this.FromColumn = column;
            MetaAssociation association = column.Member.Association;
            ALinqTableProvider table = (ALinqTableProvider)column.Table;
            ALinqDataModelProvider dataModel = (ALinqDataModelProvider)table.DataModel;
            if (association.OtherMember != null)
            {
                this.ToColumn = dataModel.EntityMemberLookup[(PropertyInfo)association.OtherMember.Member];
            }
            else
            {
                if (predicate == null)
                {
                    predicate = delegate(ALinqTableProvider tp)
                    {
                        return tp.EntityType == association.OtherType.Type;
                    };
                }
                this.ToTable = ((ALinqDataModelProvider)column.Table.DataModel).DLinqTables.Single<ALinqTableProvider>(predicate);
            }
            if (association.IsMany)
            {
                this.Direction = AssociationDirection.OneToMany;
            }
            else if ((association.OtherMember == null) || association.OtherMember.Association.IsMany)
            {
                this.Direction = AssociationDirection.ManyToOne;
            }
            else
            {
                this.Direction = AssociationDirection.OneToOne;
            }
            List<string> list = new List<string>();
            using (IEnumerator<MetaDataMember> enumerator = column.Member.Association.ThisKey.GetEnumerator())
            {
                //c__DisplayClass3 class3;
                Func<ColumnProvider, bool> func = null;
                //c__DisplayClass3 __locals4 = class3;
                while (enumerator.MoveNext())
                {
                    MetaDataMember thisKeyMetaDataMember = enumerator.Current;
                    if (func == null)
                    {
                        func = delegate(ColumnProvider member)
                        {
                            return member.Name.Equals(thisKeyMetaDataMember.Name);
                        };
                    }
                    ALinqColumnProvider provider3 = (ALinqColumnProvider)column.Table.Columns.First(func);
                    list.Add(provider3.Name);
                    if (provider3.IsPrimaryKey)
                    {
                        this.IsPrimaryKeyInThisTable = true;
                    }
                    if (association.IsForeignKey)
                    {
                        provider3.IsForeignKeyComponent = true;
                    }
                }
            }
            this.ForeignKeyNames = new ReadOnlyCollection<string>(list);
        }

        public override string GetSortExpression(ColumnProvider sortColumn)
        {
            return this.GetSortExpression(sortColumn, "{0}.{1}");
        }

        internal string GetSortExpression(ColumnProvider sortColumn, string format)
        {
            if ((this.Direction == AssociationDirection.OneToMany) || (this.Direction == AssociationDirection.ManyToMany))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, DynamicDataResources.AssociationProvider_DirectionDoesNotSupportSorting, new object[] { this.Direction }));
            }
            if (sortColumn == null)
            {
                throw new ArgumentNullException("sortColumn");
            }
            if (!this.ToTable.Columns.Contains(sortColumn))
            {
                throw new ArgumentException(DynamicDataResources.AssociationProvider_SortColumnDoesNotBelongToEndTable, "sortColumn");
            }
            if (sortColumn.IsSortable)
            {
                return string.Format(CultureInfo.InvariantCulture, format, new object[] { this.FromColumn.Name, sortColumn.Name });
            }
            return null;
        }



    }


}
