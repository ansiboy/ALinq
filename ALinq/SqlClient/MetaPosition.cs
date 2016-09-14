using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ALinq.SqlClient
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MetaPosition : IEqualityComparer<MetaPosition>, IEqualityComparer
    {
        private readonly int metadataToken;
        private readonly Assembly assembly;

        internal MetaPosition(MemberInfo mi) : this(mi.DeclaringType.Assembly, mi.MetadataToken)
        {
        }

        internal MetaPosition(Assembly assembly, int metadataToken)
        {
            this.assembly = assembly;
            this.metadataToken = metadataToken;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            return AreEqual(this, (MetaPosition) obj);
        }

        public bool Equals(object x, object y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return metadataToken;
        }

        public bool Equals(MetaPosition x, MetaPosition y)
        {
            return AreEqual(x, y);
        }

        public int GetHashCode(MetaPosition obj)
        {
            return obj.metadataToken;
        }

        bool IEqualityComparer.Equals(object x, object y)
        {
            return Equals((MetaPosition) x, (MetaPosition) y);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return GetHashCode((MetaPosition) obj);
        }

        private static bool AreEqual(MetaPosition x, MetaPosition y)
        {
            return ((x.metadataToken == y.metadataToken) && (x.assembly == y.assembly));
        }

        public static bool operator ==(MetaPosition x, MetaPosition y)
        {
            return AreEqual(x, y);
        }

        public static bool operator !=(MetaPosition x, MetaPosition y)
        {
            return !AreEqual(x, y);
        }

        internal static bool AreSameMember(MemberInfo x, MemberInfo y)
        {
            return ((x.MetadataToken == y.MetadataToken) && (x.DeclaringType.Assembly == y.DeclaringType.Assembly));
        }
    }
}