using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq
{
    internal class Constants
    {
        private const string LAST0 = "1";
#if FREE
#if NET4
        private const string LAST1 = ".48";
#else
        private const string LAST1 = ".38";
#endif
#else
#if NET4
        private const string LAST1 = ".40";
#else
        private const string LAST1 = ".35";
#endif
#endif

        internal const string Company = "ALinq Team";
        internal const string Copyright = "Copyright @ ALinq Team";
        internal const string ALinqVersion = "3.0." + LAST0 + LAST1;
        internal const string AccessVersion = ALinqVersion;//"1.9." + LAST0 + LAST1;
        internal const string SQLiteVersion = ALinqVersion;//"1.9." + LAST0 + LAST1;
        internal const string DB2Version = ALinqVersion;//"1.3." + LAST0 + LAST1;
        internal const string EffiProzVersion = ALinqVersion;//"1.1." + LAST0 + LAST1;
        internal const string FirebirdVersion = ALinqVersion;//"1.5." + LAST0 + LAST1;
        internal const string MySqlVersion = ALinqVersion;//"1.7." + LAST0 + LAST1;
        internal const string OdpOracleVersion = ALinqVersion;//OracleVersion;
        internal const string OracleVersion = ALinqVersion;//"1.7." + LAST0 + LAST1;
        internal const string PostgreSQLVersion = ALinqVersion;//"1.2." + LAST0 + LAST1;

        internal const string WebVersion = ALinqVersion; //"1.0." + LAST0 + LAST1;
        //internal const int TrivalDates = 180;
        //internal const string TrialUserName = "Trial";
        //internal const string FreeUserName = "Free";
        //internal const string ENCRYPT_KEY = "VERSION3";


        //internal const int FreeEdition_LimitedTablesCount = 18;
        //internal static string FreeEditionLimited = string.Format("Free Edition is limited to {0} tables in a database.",
        //                                                Constants.FreeEdition_LimitedTablesCount);

        //public const int TeamEdition_LimitedTablesCount = 300;

        //public const int StandardEdition_LimitedTablesCount = 200;

        //public const int SingleEdition_LimitedTablesCount = 100;

        //public const int ExperienceEdition_LimitedTablesCount = 50;
    }
}
