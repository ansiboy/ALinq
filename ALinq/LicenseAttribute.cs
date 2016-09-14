#if !FREE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class LicenseAttribute : Attribute
    {
        public LicenseAttribute(string username, string key)
        {
            UserName = username;
            Key = key;
        }

        internal string UserName
        {
            get;
            set;
        }

        internal string Key
        {
            get;
            set;
        }

    }
}
#endif
