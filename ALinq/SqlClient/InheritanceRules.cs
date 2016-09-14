using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ALinq.SqlClient
{
    internal static class InheritanceRules
    {
        // Methods
        internal static bool AreSameMember(MemberInfo mi1, MemberInfo mi2)
        {
            return DistinguishedMemberName(mi1).Equals(DistinguishedMemberName(mi2));
        }

        public static object DistinguishedMemberName(MemberInfo mi)
        {
            var info = mi as PropertyInfo;
            if (!(mi is FieldInfo))
            {
                if (info == null)
                {
                    throw Error.ArgumentOutOfRange("mi");
                }
                MethodInfo getMethod = null;
                if (info.CanRead)
                {
                    getMethod = info.GetGetMethod();
                }
                if ((getMethod == null) && info.CanWrite)
                {
                    getMethod = info.GetSetMethod();
                }
                if ((getMethod != null) && getMethod.IsVirtual)
                {
                    return mi.Name;
                }
            }
            return new MetaPosition(mi);
        }

        internal static object InheritanceCodeForClientCompare(object rawCode, IProviderType providerType)
        {
            if (!providerType.IsFixedSize || (rawCode.GetType() != typeof (string)))
            {
                return rawCode;
            }
            var str = (string) rawCode;
            if (providerType.Size.HasValue)
            {
                if (str.Length != providerType.Size)
                {
                    str = str.PadRight(providerType.Size.Value).Substring(0, providerType.Size.Value);
                }
            }
            return str;
        }
    }
}