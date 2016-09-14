using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ALinq.Mapping
{
    internal static class PropertyAccessor
    {
        // Methods
        internal static MetaAccessor Create(Type objectType, PropertyInfo pi, MetaAccessor storageAccessor)
        {
            if (objectType == null)
                throw Error.ArgumentNull("objectType");

            if (pi == null)
                throw Error.ArgumentNull("pi");

            //if (storageAccessor == null)
            //    throw Error.ArgumentNull("storageAccessor");

            Delegate delegate2 = null;
            Delegate delegate3 = null;
            MethodInfo getMethod = pi.GetGetMethod(true);
            var isItemMethod = getMethod.Name == "get_Item";
            Delegate delegate4;
            if (!isItemMethod)
            {
                Type type = typeof(DGet<,>).MakeGenericType(new Type[] { objectType, pi.PropertyType });
                delegate4 = Delegate.CreateDelegate(type, getMethod, true);
            }
            else
            {
                Type type = typeof(DItemGet<>).MakeGenericType(new Type[] { objectType });
                delegate4 = Delegate.CreateDelegate(type, getMethod, true);
            }

            if (delegate4 == null)
            {
                throw Error.CouldNotCreateAccessorToProperty(objectType, pi.PropertyType, pi);
            }
            if (pi.CanWrite)
            {
                if (!objectType.IsValueType)
                {
                    if (!isItemMethod)
                    {
                        var type = typeof(DSet<,>).MakeGenericType(new[] { objectType, pi.PropertyType });
                        delegate2 = Delegate.CreateDelegate(type, pi.GetSetMethod(true), true);
                    }
                    else
                    {
                        var type = typeof(DItemSet<>).MakeGenericType(new[] { objectType });
                        delegate2 = Delegate.CreateDelegate(type, pi.GetSetMethod(true), true);
                    }
                }
                else
                {
                    DynamicMethod method = new DynamicMethod("xset_" + pi.Name, typeof(void), new Type[] { objectType.MakeByRefType(), pi.PropertyType }, true);
                    ILGenerator iLGenerator = method.GetILGenerator();
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    if (!objectType.IsValueType)
                    {
                        iLGenerator.Emit(OpCodes.Ldind_Ref);
                    }
                    iLGenerator.Emit(OpCodes.Ldarg_1);
                    iLGenerator.Emit(OpCodes.Call, pi.GetSetMethod(true));
                    iLGenerator.Emit(OpCodes.Ret);
                    delegate3 = method.CreateDelegate(typeof(DRSet<,>).MakeGenericType(new Type[] { objectType, pi.PropertyType }));
                }
            }
            Type type2 = (storageAccessor != null) ? storageAccessor.Type : pi.PropertyType;
            var t1 = typeof(Accessor<,,>).MakeGenericType(new[] { objectType, pi.PropertyType, type2 });
            var bf = BindingFlags.NonPublic | BindingFlags.Instance;
            object[] args;
            if (!isItemMethod)
                args = new object[] { pi, delegate4, delegate2, delegate3, storageAccessor };
            else
                args = new object[] { pi, delegate4, delegate2, delegate3, storageAccessor, pi.Name };

            return (MetaAccessor)Activator.CreateInstance(t1, bf, null, args, null);
        }

        // Nested Types
        private class Accessor<T, V, V2> : MetaAccessor<T, V> where V2 : V
        {
            // Fields
            private DGet<T, V> dget;
            private DRSet<T, V> drset;
            private DSet<T, V> dset;

            private DItemGet<T> dItemGet;
            private DRItemSet<T> drItemSet;
            private DItemSet<T> dItemSet;

            private PropertyInfo pi;
            private MetaAccessor<T, V2> storage;
            private bool isItem;
            private string key;

            private Accessor(bool isItem, PropertyInfo pi, MetaAccessor<T, V2> storage)
            {
                this.isItem = isItem;
                this.pi = pi;
                this.storage = storage;
            }

            // Methods
            internal Accessor(PropertyInfo pi, DGet<T, V> dget, DSet<T, V> dset, DRSet<T, V> drset, MetaAccessor<T, V2> storage)
                : this(false, pi, storage)
            {
                this.dget = dget;
                this.dset = dset;
                this.drset = drset;
            }

            internal Accessor(PropertyInfo pi, DItemGet<T> dget, DItemSet<T> dset, DRItemSet<T> drset, MetaAccessor<T, V2> storage, string key)
                : this(true, pi, storage)
            {
                this.dItemGet = dget;
                this.dItemSet = dset;
                this.drItemSet = drset;
                this.key = key;
            }

            public override V GetValue(T instance)
            {
                if (isItem)
                {
                    var value = this.dItemGet(instance, key);
                    if (value == null)
                        return default(V);

                    return (V)DBConvert.ChangeType(value, typeof(V));
                }

                return this.dget(instance);
            }

            public override void SetValue(ref T instance, V value)
            {
                if (this.dItemSet != null)
                {
                    this.dItemSet(instance, key, value);
                }
                else if (this.drItemSet != null)
                {
                    this.drItemSet(ref instance, key, value);
                }
                else if (this.dset != null)
                {
                    this.dset(instance, value);
                }
                else if (this.drset != null)
                {
                    this.drset(ref instance, value);
                }
                else
                {
                    if (this.storage == null)
                    {
                        throw Error.UnableToAssignValueToReadonlyProperty(this.pi);
                    }
                    this.storage.SetValue(ref instance, (V2)value);
                }
            }
        }
    }
}