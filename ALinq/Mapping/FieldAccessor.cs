using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ALinq.Mapping
{
    internal static class FieldAccessor
    {
        // Methods
        internal static MetaAccessor Create(Type objectType, FieldInfo fi)
        {
            if (!fi.ReflectedType.IsAssignableFrom(objectType))
            {
                throw Error.InvalidFieldInfo(objectType, fi.FieldType, fi);
            }
            Delegate delegate2 = null;
            Delegate delegate3 = null;
            if (!objectType.IsGenericType)
            {
                var method = new DynamicMethod("xget_" + fi.Name, fi.FieldType, new[] { objectType }, true);
                ILGenerator iLGenerator = method.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldfld, fi);
                iLGenerator.Emit(OpCodes.Ret);
                delegate2 = method.CreateDelegate(typeof(DGet<,>).MakeGenericType(new[] { objectType, fi.FieldType }));
                var method2 = new DynamicMethod("xset_" + fi.Name, typeof(void), new[] { objectType.MakeByRefType(), fi.FieldType }, true);
                iLGenerator = method2.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                if (!objectType.IsValueType)
                {
                    iLGenerator.Emit(OpCodes.Ldind_Ref);
                }
                iLGenerator.Emit(OpCodes.Ldarg_1);
                iLGenerator.Emit(OpCodes.Stfld, fi);
                iLGenerator.Emit(OpCodes.Ret);
                delegate3 = method2.CreateDelegate(typeof(DRSet<,>).MakeGenericType(new[] { objectType, fi.FieldType }));
            }
            return (MetaAccessor)Activator.CreateInstance(typeof(Accessor<,>).MakeGenericType(new[] { objectType, fi.FieldType }), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { fi, delegate2, delegate3 }, null);
        }
 
        // Nested Types
        private class Accessor<T, V> : MetaAccessor<T, V>
        {
            // Fields
            private readonly DGet<T, V> dget;
            private readonly DRSet<T, V> drset;
            private readonly FieldInfo fi;

            // Methods
            internal Accessor(FieldInfo fi, DGet<T, V> dget, DRSet<T, V> drset)
            {
                this.fi = fi;
                this.dget = dget;
                this.drset = drset;
            }

            public override V GetValue(T instance)
            {
                if (dget != null)
                {
                    return dget(instance);
                }
                return (V)fi.GetValue(instance);
            }

            public override void SetValue(ref T instance, V value)
            {
                if (drset != null)
                {
                    drset(ref instance, value);
                }
                else
                {
                    fi.SetValue(instance, value);
                }
            }
        }
    }
}