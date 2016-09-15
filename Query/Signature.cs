using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;

namespace ALinq.Dynamic
{
    class DynamicPropertyCollection : IEnumerable<DynamicProperty>
    {
        LinkedList<DynamicProperty> items;

        public DynamicPropertyCollection()
        {
            items = new LinkedList<DynamicProperty>();
        }

        public int Length
        {
            get { return items.Count; }
        }

        public int Count
        {
            get { return items.Count; }
        }

        public void Add(DynamicProperty item)
        {
            Debug.Assert(item != null);
            Debug.Assert(items.Select(o => o.Name).Contains(item.Name) == false);


            //按名称排序，小的放在前面。找到比 item 小的元素，
            var p = items.First;
            while (p != null)
            {
                if (string.Compare(item.Name, p.Value.Name) < 0)
                {
                    items.AddBefore(p, item);
                    return;
                }

                p = p.Next;
            }

            //找不到比 item 更大的，直接放后面。
            items.AddLast(item);
            return;
        }



        #region Implementation of IEnumerable

        public IEnumerator<DynamicProperty> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }


    internal class Signature : IEquatable<Signature>
    {
        public DynamicProperty[] properties;
        public int hashCode;
        private Type baseType;

        public Signature(IEnumerable<DynamicProperty> properties, Type baseType)
        {
            this.properties = properties.ToArray();
            hashCode = 0;
            foreach (DynamicProperty p in properties)
            {
                hashCode ^= p.Name.GetHashCode() ^ p.Type.GetHashCode();
            }

            this.baseType = baseType;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is Signature ? Equals((Signature)obj) : false;
        }

        public bool Equals(Signature other)
        {
            if (properties.Length != other.properties.Length)
                return false;

            if (baseType != other.baseType)
                return false;

            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].Name != other.properties[i].Name ||
                    properties[i].Type != other.properties[i].Type) return false;
            }
            return true;
        }
    }
}