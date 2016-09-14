using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq.SqlClient
{
    class ConstColumns
    {
        private static ConstColumns firebirdInstance;
        private string test;
        private SqlProvider.ProviderMode mode;
        private string key;
        private string group;
        private string value;

        public ConstColumns(SqlProvider.ProviderMode mode)
        {
            this.mode = mode;
        }

        public static string Test
        {
            get
            {
                return "Test";
            }
        }

        public static string Key
        {
            get
            {
                //if (mode == SqlProvider.ProviderMode.Firebird)
                //    return FirebirdInstance.key;
                return "Key";
            }
        }

        public static string Group
        {
            get
            {
                //if (mode == SqlProvider.ProviderMode.Firebird)
                //    return FirebirdInstance.group;
                return "Group";
            }
        }

        private static string GetValue()
        {
            //if (mode == SqlProvider.ProviderMode.Firebird)
            //    return FirebirdInstance.value;
            return "Value";
        }

        public static string GetValue(SqlProvider.ProviderMode mode)
        {
            if (mode == SqlProvider.ProviderMode.Firebird)
                return "CValue";
            return GetValue();
        }
    }
}
