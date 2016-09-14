using System;
using System.Collections.Generic;

namespace ALinq
{
    internal class EntitySetBindingList<TEntity> : SortableBindingList<TEntity> where TEntity : class
    {
        // Fields
        private bool addingNewInstance;
        private TEntity addNewInstance;
        private TEntity cancelNewInstance;
        private EntitySet<TEntity> data;

        // Methods
        internal EntitySetBindingList(IList<TEntity> sequence, EntitySet<TEntity> data)
            : base(sequence)
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
            this.ThrowEntitySetErrorsIfTypeInappropriate();
            this.addingNewInstance = true;
            this.addNewInstance = (TEntity)base.AddNewCore();
            return this.addNewInstance;
        }

        public override void CancelNew(int itemIndex)
        {
            if (((itemIndex >= 0) && (itemIndex < base.Count)) && (base[itemIndex] == this.addNewInstance))
            {
                this.cancelNewInstance = this.addNewInstance;
                this.addNewInstance = default(TEntity);
                this.addingNewInstance = false;
            }
            base.CancelNew(itemIndex);
        }

        protected override void ClearItems()
        {
            this.data.Clear();
            base.ClearItems();
        }

        public override void EndNew(int itemIndex)
        {
            if (((itemIndex >= 0) && (itemIndex < base.Count)) && (base[itemIndex] == this.addNewInstance))
            {
                this.data.Add(this.addNewInstance);
                this.addNewInstance = default(TEntity);
                this.addingNewInstance = false;
            }
            base.EndNew(itemIndex);
        }

        protected override void InsertItem(int index, TEntity item)
        {
            base.InsertItem(index, item);
            if ((!this.addingNewInstance && (index >= 0)) && (index <= base.Count))
            {
                this.data.Insert(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            if (((index >= 0) && (index < base.Count)) && (base[index] == this.cancelNewInstance))
            {
                this.cancelNewInstance = default(TEntity);
            }
            else
            {
                this.data.Remove(base[index]);
            }
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TEntity item)
        {
            TEntity entity = base[index];
            base.SetItem(index, item);
            if ((index >= 0) && (index < base.Count))
            {
                if (entity == this.addNewInstance)
                {
                    this.addNewInstance = default(TEntity);
                    this.addingNewInstance = false;
                }
                else
                {
                    this.data.Remove(entity);
                }
                this.data.Insert(index, item);
            }
        }

        private void ThrowEntitySetErrorsIfTypeInappropriate()
        {
            Type type = typeof(TEntity);
            if (type.IsAbstract)
            {
                throw Error.EntitySetDataBindingWithAbstractBaseClass(type.Name);
            }
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                throw Error.EntitySetDataBindingWithNonPublicDefaultConstructor(type.Name);
            }
        }
    }
}