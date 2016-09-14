using System;
using System.Linq;
using System.Reflection;

namespace ALinq
{
    internal static class SubqueryRules
    {
        // Methods
        private static bool IsSequenceOperatorCall(MethodInfo mi)
        {
            Type declaringType = mi.DeclaringType;
            if ((declaringType != typeof(Enumerable)) && (declaringType != typeof(Queryable)))
            {
                return false;
            }
            return true;
        }

        internal static bool IsSupportedTopLevelMethod(MethodInfo mi)
        {
            string str;
            if (!IsSequenceOperatorCall(mi))
            {
                return false;
            }
            if (((str = mi.Name) == null) || (((!(str == "Where") && !(str == "OrderBy")) && (!(str == "OrderByDescending") && !(str == "ThenBy"))) && (!(str == "ThenByDescending") && !(str == "Take"))))
            {
                return false;
            }
            return true;
        }
    }
}