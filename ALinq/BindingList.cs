using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ALinq.Mapping;
using System.Xml.Linq;

namespace ALinq
{
    internal static class BindingList
    {
        // Methods
        internal static IBindingList Create<T>(DataContext context, IEnumerable<T> sequence)
        {
            List<T> list = sequence.ToList();
            MetaTable table = context.Services.Model.GetTable(typeof(T));
            if (table != null)
            {
                ITable table2 = context.GetTable(table.RowType.Type);
                return (IBindingList)Activator.CreateInstance(typeof(DataBindingList<>).MakeGenericType(new[] { table.RowType.Type }), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new object[] { list, table2 }, null);
            }
            return new SortableBindingList<T>(list);

        }
    }

    internal class DataBindingList<TEntity> : SortableBindingList<TEntity> where TEntity : class
    {
        // Fields
        private bool addingNewInstance;
        private TEntity addNewInstance;
        private TEntity cancelNewInstance;
        private readonly Table<TEntity> data;

        // Methods
        internal DataBindingList(IList<TEntity> sequence, Table<TEntity> data)
            : base(sequence ?? new List<TEntity>())
        {
            if (sequence == null)
            {
                throw Error.ArgumentNull("sequence");
            }
            if (data == null)
            {
                throw Error.ArgumentNull("data");
            }
            this.data = data;
        }

        protected override object AddNewCore()
        {
            addingNewInstance = true;
            addNewInstance = (TEntity)base.AddNewCore();
            return addNewInstance;
        }

        public override void CancelNew(int itemIndex)
        {
            if (((itemIndex >= 0) && (itemIndex < Count)) && (base[itemIndex] == addNewInstance))
            {
                cancelNewInstance = addNewInstance;
                addNewInstance = default(TEntity);
                addingNewInstance = false;
            }
            base.CancelNew(itemIndex);
        }

        protected override void ClearItems()
        {
            data.DeleteAllOnSubmit(data.ToList());
            base.ClearItems();
        }

        public override void EndNew(int itemIndex)
        {
            if (((itemIndex >= 0) && (itemIndex < Count)) && (base[itemIndex] == addNewInstance))
            {
                data.InsertOnSubmit(addNewInstance);
                addNewInstance = default(TEntity);
                addingNewInstance = false;
            }
            base.EndNew(itemIndex);
        }

        protected override void InsertItem(int index, TEntity item)
        {
            base.InsertItem(index, item);
            if ((!addingNewInstance && (index >= 0)) && (index <= Count))
            {
                data.InsertOnSubmit(item);
            }
        }

        protected override void RemoveItem(int index)
        {
            if (((index >= 0) && (index < Count)) && (base[index] == cancelNewInstance))
            {
                cancelNewInstance = default(TEntity);
            }
            else
            {
                data.DeleteOnSubmit(base[index]);
            }
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TEntity item)
        {
            TEntity entity = base[index];
            base.SetItem(index, item);
            if ((index >= 0) && (index < Count))
            {
                if (entity == addNewInstance)
                {
                    addNewInstance = default(TEntity);
                    addingNewInstance = false;
                }
                else
                {
                    data.DeleteOnSubmit(entity);
                }
                data.InsertOnSubmit(item);
            }
        }
    }

    internal class SortableBindingList<T> : BindingList<T>
    {
        // Fields
        private bool isSorted;
        private ListSortDirection sortDirection;
        private PropertyDescriptor sortProperty;

        // Methods
        internal SortableBindingList(IList<T> list)
            : base(list)
        {
        }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            if (PropertyComparer<T>.IsAllowable(prop.PropertyType))
            {
                ((List<T>)Items).Sort(new PropertyComparer<T>(prop, direction));
                sortDirection = direction;
                sortProperty = prop;
                isSorted = true;
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }

 


        protected override void RemoveSortCore()
        {
            isSorted = false;
            sortProperty = null;
        }

        // Properties
        protected override bool IsSortedCore
        {
            get
            {
                return isSorted;
            }
        }

        protected override ListSortDirection SortDirectionCore
        {
            get
            {
                return sortDirection;
            }
        }

        protected override PropertyDescriptor SortPropertyCore
        {
            get
            {
                return sortProperty;
            }
        }

        protected override bool SupportsSortingCore
        {
            get
            {
                return true;
            }
        }

        // Nested Types
        internal class PropertyComparer<T> : Comparer<T>
        {
            // Fields
            private readonly IComparer comparer;
            private readonly ListSortDirection direction;
            private readonly PropertyDescriptor prop;
            private readonly bool useToString;

            // Methods
            internal PropertyComparer(PropertyDescriptor prop, ListSortDirection direction)
            {
                if (prop.ComponentType != typeof(T))
                {
                    throw new MissingMemberException(typeof(T).Name, prop.Name);
                }
                this.prop = prop;
                this.direction = direction;
                if (SortableBindingList<T>.PropertyComparer<T>.OkWithIComparable(prop.PropertyType))
                {
                    PropertyInfo property = typeof(Comparer<>).MakeGenericType(new[] { prop.PropertyType }).GetProperty("Default");
                    comparer = (IComparer)property.GetValue(null, null);
                    useToString = false;
                }
                else if (SortableBindingList<T>.PropertyComparer<T>.OkWithToString(prop.PropertyType))
                {
                    comparer = StringComparer.CurrentCultureIgnoreCase;
                    useToString = true;
                }
            }

            public override int Compare(T x, T y)
            {
                object obj2 = prop.GetValue(x);
                object obj3 = prop.GetValue(y);
                if (useToString)
                {
                    obj2 = (obj2 != null) ? obj2.ToString() : null;
                    obj3 = (obj3 != null) ? obj3.ToString() : null;
                }
                if (direction == ListSortDirection.Ascending)
                {
                    return comparer.Compare(obj2, obj3);
                }
                return comparer.Compare(obj3, obj2);
            }

            public static bool IsAllowable(Type t)
            {
                if (!SortableBindingList<T>.PropertyComparer<T>.OkWithToString(t))
                {
                    return SortableBindingList<T>.PropertyComparer<T>.OkWithIComparable(t);
                }
                return true;
            }

            protected static bool OkWithIComparable(Type t)
            {
                return ((t.GetInterface("IComparable") != null) || (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Nullable<>))));
            }

            protected static bool OkWithToString(Type t)
            {
                if (!t.Equals(typeof(XNode)))
                {
                    return t.IsSubclassOf(typeof(XNode));
                }
               return true;
            }
        }

 

    }

 


}
