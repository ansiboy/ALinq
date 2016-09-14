using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ALinq.Mapping
{
    //[StructLayout(LayoutKind.Sequential)]
    //internal struct MetaPosition : IEqualityComparer<MetaPosition>, IEqualityComparer
    //{
    //    private int metadataToken;
    //    private Assembly assembly;
    //    internal MetaPosition(MemberInfo mi) : this(mi.DeclaringType.Assembly, mi.MetadataToken)
    //    {
    //    }

    //    private MetaPosition(Assembly assembly, int metadataToken)
    //    {
    //        this.assembly = assembly;
    //        this.metadataToken = metadataToken;
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        if (obj == null)
    //        {
    //            return false;
    //        }
    //        if (obj.GetType() != base.GetType())
    //        {
    //            return false;
    //        }
    //        return AreEqual(this, (MetaPosition) obj);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return this.metadataToken;
    //    }

    //    public bool Equals(MetaPosition x, MetaPosition y)
    //    {
    //        return AreEqual(x, y);
    //    }

    //    public int GetHashCode(MetaPosition obj)
    //    {
    //        return obj.metadataToken;
    //    }

    //    bool IEqualityComparer.Equals(object x, object y)
    //    {
    //        return this.Equals((MetaPosition) x, (MetaPosition) y);
    //    }

    //    int IEqualityComparer.GetHashCode(object obj)
    //    {
    //        return this.GetHashCode((MetaPosition) obj);
    //    }

    //    private static bool AreEqual(MetaPosition x, MetaPosition y)
    //    {
    //        return ((x.metadataToken == y.metadataToken) && (x.assembly == y.assembly));
    //    }

    //    public static bool operator ==(MetaPosition x, MetaPosition y)
    //    {
    //        return AreEqual(x, y);
    //    }

    //    public static bool operator !=(MetaPosition x, MetaPosition y)
    //    {
    //        return !AreEqual(x, y);
    //    }

    //    internal static bool AreSameMember(MemberInfo x, MemberInfo y)
    //    {
    //        return ((x.MetadataToken == y.MetadataToken) && (x.DeclaringType.Assembly == y.DeclaringType.Assembly));
    //    }
    //}
}