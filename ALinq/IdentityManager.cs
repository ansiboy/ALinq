using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq
{
    internal abstract class IdentityManager
    {
        // Methods

        internal static IdentityManager CreateIdentityManager(bool asReadOnly)
        {
            if (asReadOnly)
            {
                return new ReadOnlyIdentityManager();
            }
            return new StandardIdentityManager();
        }

        internal abstract object Find(MetaType type, object[] keyValues);
        internal abstract object FindLike(MetaType type, object instance);
        internal abstract object InsertLookup(MetaType type, object instance);
        internal abstract bool RemoveLike(MetaType type, object instance);

        // Nested Types
        private class ReadOnlyIdentityManager : IdentityManager
        {
            // Methods

            internal override object Find(MetaType type, object[] keyValues)
            {
                return null;
            }

            internal override object FindLike(MetaType type, object instance)
            {
                return null;
            }

            internal override object InsertLookup(MetaType type, object instance)
            {
                return instance;
            }

            internal override bool RemoveLike(MetaType type, object instance)
            {
                return false;
            }
        }

        private class StandardIdentityManager : IdentityManager
        {
            // Fields
            private readonly Dictionary<MetaType, IdentityCache> caches;
            private IdentityCache currentCache;
            private MetaType currentType;

            public StandardIdentityManager()
            {
                caches = new Dictionary<MetaType, IdentityCache>();
            }

            // Methods

            internal override object Find(MetaType type, object[] keyValues)
            {
                SetCurrent(type);
                return currentCache.Find(keyValues);
            }

            internal override object FindLike(MetaType type, object instance)
            {
                SetCurrent(type);
                return currentCache.FindLike(instance);
            }

            private static KeyManager GetKeyManager(MetaType type)
            {
                int count = type.IdentityMembers.Count;
                MetaDataMember member = type.IdentityMembers[0];
                var manager = (KeyManager)Activator.CreateInstance(typeof(SingleKeyManager<,>).MakeGenericType(new[] { type.Type, member.Type }), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new object[] { member.StorageAccessor, 0 }, null);
                for (int i = 1; i < count; i++)
                {
                    member = type.IdentityMembers[i];
                    manager = (KeyManager)Activator.CreateInstance(typeof(MultiKeyManager<,,>).MakeGenericType(new[] { type.Type, member.Type, manager.KeyType }), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new object[] { member.StorageAccessor, i, manager }, null);
                }
                return manager;
            }

            internal override object InsertLookup(MetaType type, object instance)
            {
                SetCurrent(type);
                return currentCache.InsertLookup(instance);
            }

            internal override bool RemoveLike(MetaType type, object instance)
            {
                SetCurrent(type);
                return currentCache.RemoveLike(instance);
            }

            private void SetCurrent(MetaType type)
            {
                type = type.InheritanceRoot;
                if (currentType != type)
                {
                    if (!caches.TryGetValue(type, out currentCache))
                    {
                        KeyManager keyManager = GetKeyManager(type);
                        currentCache = (IdentityCache)Activator.CreateInstance(typeof(IdentityCache<,>).MakeGenericType(new[] { type.Type, keyManager.KeyType }), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new object[] { keyManager }, null);
                        caches.Add(type, currentCache);
                    }
                    currentType = type;
                }
            }

            // Nested Types
            internal abstract class IdentityCache
            {
                // Methods

                internal abstract object Find(object[] keyValues);
                internal abstract object FindLike(object instance);
                internal abstract object InsertLookup(object instance);
                internal abstract bool RemoveLike(object instance);
            }

            internal class IdentityCache<T, K> : IdentityCache
            {
                // Fields
                private int[] buckets;
                private readonly IEqualityComparer<K> comparer;
                private int count;
                private int freeList;
                private readonly KeyManager<T, K> keyManager;
                //private Slot<T, K>[] slots;
                private Slot[] slots;

                // Methods
                public IdentityCache(KeyManager<T, K> keyManager)
                {
                    this.keyManager = keyManager;
                    comparer = keyManager.Comparer;
                    buckets = new int[7];
                    //slots = new Slot<T, K>[7];
                    slots = new Slot[7];
                    freeList = -1;
                }

                internal override object Find(object[] keyValues)
                {
                    K local;
                    if (keyManager.TryCreateKeyFromValues(keyValues, out local))
                    {
                        T local2 = default(T);
                        if (Find(local, ref local2, false))
                        {
                            return local2;
                        }
                    }
                    return null;
                }

                private bool Find(K key, ref T value, bool add)
                {
                    int num = comparer.GetHashCode(key) & 0x7fffffff;
                    for (int i = buckets[num % buckets.Length] - 1; i >= 0; i = slots[i].next)
                    {
                        if ((slots[i].hashCode == num) && comparer.Equals(slots[i].key, key))
                        {
                            value = slots[i].value;
                            return true;
                        }
                    }
                    if (add)
                    {
                        int tmpFreeList;
                        if (this.freeList >= 0)
                        {
                            tmpFreeList = this.freeList;
                            this.freeList = this.slots[tmpFreeList].next;
                        }
                        else
                        {
                            if (this.count == this.slots.Length)
                            {
                                this.Resize();
                            }
                            tmpFreeList = this.count;
                            this.count++;
                        }
                        int index = num % this.buckets.Length;
                        this.slots[tmpFreeList].hashCode = num;
                        this.slots[tmpFreeList].key = key;
                        this.slots[tmpFreeList].value = value;
                        this.slots[tmpFreeList].next = this.buckets[index] - 1;
                        this.buckets[index] = tmpFreeList + 1;
                    }
                    return false;
                }

                internal override object FindLike(object instance)
                {
                    T local = (T)instance;
                    K key = this.keyManager.CreateKeyFromInstance(local);
                    if (this.Find(key, ref local, false))
                    {
                        return local;
                    }
                    return null;
                }

                internal override object InsertLookup(object instance)
                {
                    T local = (T)instance;
                    K key = keyManager.CreateKeyFromInstance(local);
                    Find(key, ref local, true);
                    return local;
                }

                internal override bool RemoveLike(object instance)
                {
                    T local = (T)instance;
                    K local2 = keyManager.CreateKeyFromInstance(local);
                    int num = comparer.GetHashCode(local2) & 0x7fffffff;
                    int index = num % this.buckets.Length;
                    int num3 = -1;
                    for (int i = this.buckets[index] - 1; i >= 0; i = this.slots[i].next)
                    {
                        if ((this.slots[i].hashCode == num) && this.comparer.Equals(this.slots[i].key, local2))
                        {
                            if (num3 < 0)
                            {
                                this.buckets[index] = this.slots[i].next + 1;
                            }
                            else
                            {
                                this.slots[num3].next = this.slots[i].next;
                            }
                            this.slots[i].hashCode = -1;
                            this.slots[i].value = default(T);
                            this.slots[i].next = this.freeList;
                            this.freeList = i;
                            return true;
                        }
                        num3 = i;
                    }
                    return false;
                }

                private void Resize()
                {
                    int num = (this.count * 2) + 1;
                    var numArray = new int[num];
                    //Slot<T, K>[] destinationArray = new Slot<T, K>[num];
                    var destinationArray = new Slot[num];
                    Array.Copy(this.slots, 0, destinationArray, 0, this.count);
                    for (int i = 0; i < this.count; i++)
                    {
                        int index = destinationArray[i].hashCode % num;
                        destinationArray[i].next = numArray[index] - 1;
                        numArray[index] = i + 1;
                    }
                    buckets = numArray;
                    slots = destinationArray;
                }

                // Nested Types
                [StructLayout(LayoutKind.Sequential)]
                internal struct Slot
                {
                    internal int hashCode;
                    internal K key;
                    internal T value;
                    internal int next;
                }
            }

            internal abstract class KeyManager
            {
                // Methods
                protected KeyManager()
                {
                }

                // Properties
                internal abstract Type KeyType { get; }
            }

            internal abstract class KeyManager<T, K> : IdentityManager.StandardIdentityManager.KeyManager
            {
                // Methods
                protected KeyManager()
                {
                }

                internal abstract K CreateKeyFromInstance(T instance);
                internal abstract bool TryCreateKeyFromValues(object[] values, out K k);

                // Properties
                internal abstract IEqualityComparer<K> Comparer { get; }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct MultiKey<T1, T2>
            {
                private readonly T1 value1;
                private readonly T2 value2;
                internal MultiKey(T1 value1, T2 value2)
                {
                    this.value1 = value1;
                    this.value2 = value2;
                }
                // Nested Types
                internal class Comparer : IEqualityComparer<MultiKey<T1, T2>>, IEqualityComparer
                {
                    // Fields
                    private readonly IEqualityComparer<T1> comparer1;
                    private readonly IEqualityComparer<T2> comparer2;

                    // Methods
                    internal Comparer(IEqualityComparer<T1> comparer1, IEqualityComparer<T2> comparer2)
                    {
                        this.comparer1 = comparer1;
                        this.comparer2 = comparer2;
                    }

                    public bool Equals(MultiKey<T1, T2> x, MultiKey<T1, T2> y)
                    {
                        return (this.comparer1.Equals(x.value1, y.value1) && this.comparer2.Equals(x.value2, y.value2));
                    }

                    public int GetHashCode(MultiKey<T1, T2> x)
                    {
                        return (this.comparer1.GetHashCode(x.value1) ^ comparer2.GetHashCode(x.value2));
                    }

                    bool System.Collections.IEqualityComparer.Equals(object x, object y)
                    {
                        return this.Equals((MultiKey<T1, T2>)x, (MultiKey<T1, T2>)y);
                    }

                    int IEqualityComparer.GetHashCode(object x)
                    {
                        return this.GetHashCode((MultiKey<T1, T2>)x);
                    }
                }
            }

            internal class MultiKeyManager<T, V1, V2> : KeyManager<T, MultiKey<V1, V2>>
            {
                // Fields
                private readonly MetaAccessor<T, V1> accessor;
                private IEqualityComparer<MultiKey<V1, V2>> comparer;
                private readonly KeyManager<T, V2> next;
                private readonly int offset;

                // Methods
                internal MultiKeyManager(MetaAccessor<T, V1> accessor, int offset, IdentityManager.StandardIdentityManager.KeyManager<T, V2> next)
                {
                    this.accessor = accessor;
                    this.next = next;
                    this.offset = offset;
                }

                internal override MultiKey<V1, V2> CreateKeyFromInstance(T instance)
                {
                    return new MultiKey<V1, V2>(this.accessor.GetValue(instance), this.next.CreateKeyFromInstance(instance));
                }

                internal override bool TryCreateKeyFromValues(object[] values, out MultiKey<V1, V2> k)
                {
                    V2 local;
                    object obj2 = values[this.offset];
                    if ((obj2 == null) && typeof(V1).IsValueType)
                    {
                        k = new MultiKey<V1, V2>();
                        return false;
                    }
                    if (!this.next.TryCreateKeyFromValues(values, out local))
                    {
                        k = new MultiKey<V1, V2>();
                        return false;
                    }
                    k = new MultiKey<V1, V2>((V1)obj2, local);
                    return true;
                }

                // Properties
                internal override IEqualityComparer<MultiKey<V1, V2>> Comparer
                {
                    get
                    {
                        if (this.comparer == null)
                        {
                            this.comparer = new MultiKey<V1, V2>.Comparer(EqualityComparer<V1>.Default, this.next.Comparer);
                        }
                        return this.comparer;
                    }
                }

                internal override Type KeyType
                {
                    get
                    {
                        return typeof(MultiKey<V1, V2>);
                    }
                }
            }

            internal class SingleKeyManager<T, V> : KeyManager<T, V>
            {
                // Fields
                private readonly MetaAccessor<T, V> accessor;
                private IEqualityComparer<V> comparer;
                private readonly bool isKeyNullAssignable;
                private readonly int offset;

                // Methods
                internal SingleKeyManager(MetaAccessor<T, V> accessor, int offset)
                {
                    this.accessor = accessor;
                    this.offset = offset;
                    this.isKeyNullAssignable = TypeSystem.IsNullAssignable(typeof(V));
                }

                internal override V CreateKeyFromInstance(T instance)
                {
                    return this.accessor.GetValue(instance);
                }

                internal override bool TryCreateKeyFromValues(object[] values, out V v)
                {
                    object obj2 = values[this.offset];
                    if ((obj2 == null) && !this.isKeyNullAssignable)
                    {
                        v = default(V);
                        return false;
                    }
                    v = (V)obj2;
                    return true;
                }

                // Properties
                internal override IEqualityComparer<V> Comparer
                {
                    get
                    {
                        if (this.comparer == null)
                        {
                            this.comparer = EqualityComparer<V>.Default;
                        }
                        return this.comparer;
                    }
                }

                internal override Type KeyType
                {
                    get
                    {
                        return typeof(V);
                    }
                }
            }
        }
    }


}
