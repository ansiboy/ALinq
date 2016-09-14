#if !FREE
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using ALinq.Resources;
using Microsoft.Win32;
using System.Reflection;
using System.Linq;

namespace ALinq
{
    internal class ALinqLicenseProvider : LicenseProvider
    {
        #region DatabaseType
        [Flags]
        public enum DatabaseType
        {
            MSSQL = 1,
            //SQL2005 = 2,
            Access = 4,
            SQLite = 8,
            MySQL = 16,
            Oracle = 32,
            PostgreSQL = 64,
            DB2 = 128,
            Firebird = 256,
            SQLCE = Firebird * 2,
            ASE = SQLCE * 2
        }
        #endregion

        private const int DATABASE_TYPE_STAR_INDEX = 1;
        private const int DATABASE_TYPE_LENGTH = 4;
        private const int USER_NAME_STAR_INDEX = 5;

        //private static Dictionary<Type, LicFileLicense> licenses;

        //========================================================
        //用于反破解
        //internal static string acutalUserName;
        //========================================================

        //private const int DAYS = Constants.TrivalDates;
        ////private string licenseKey;
        //const string ENCRYPT_KEY = Constants.ENCRYPT_KEY;

        //static Dictionary<Type, LicFileLicense> Licenses
        //{
        //    get
        //    {
        //        if (licenses == null)
        //            licenses = new Dictionary<Type, LicFileLicense>(16);
        //        return licenses;
        //    }
        //}

        //void ValidateLicense(Type type, Object instance, LicFileLicense license)
        //{
        //    var key = license.LicenseKey;
        //    var username = license.UserName;

        //    if (license.LicenseType == LicenseType.Trial)
        //    {
        //        //license.IsTrial = true;
        //        Debug.Assert(license.LicenseType == LicenseType.Trial);
        //        var date = DateTime.Parse(key);
        //        var days = DAYS - (DateTime.Now - date).Days;
        //        license.ExpiredDays = days;
        //        if (days < 0)
        //            throw Error.LicenseFail(type, instance, Messages.SoftwareExpired);

        //        return;
        //    }

        //    if (license.LicenseType == LicenseType.Free)
        //        return;

        //    var validateFailMessage = string.Format(Messages.ValidateLicenseFail, type.Name);
        //    var typeName = type.Name;
        //    DatabaseType dbType;
        //    switch (typeName)
        //    {
        //        case "AccessDbProvider":
        //            dbType = DatabaseType.Access;
        //            break;
        //        case "DB2Provider":
        //            dbType = DatabaseType.DB2;
        //            break;
        //        case "FirebirdProvider":
        //            dbType = DatabaseType.Firebird;
        //            break;
        //        case "MySqlProvider":
        //            dbType = DatabaseType.MySQL;
        //            break;
        //        case "OracleProvider":
        //            dbType = DatabaseType.Oracle;
        //            break;
        //        case "PgsqlProvider":
        //            dbType = DatabaseType.PostgreSQL;
        //            break;
        //        case "SQLiteProvider":
        //            dbType = DatabaseType.SQLite;
        //            break;
        //        default:
        //            throw Error.LicenseFail(type, instance, validateFailMessage);
        //    }

        //    if (!ValidateLicense(dbType, username, key))
        //        throw Error.LicenseFail(type, instance, validateFailMessage);
        //}

        //internal static bool ValidateLicense(DatabaseType dbType, string userName, string licenseKey)
        //{

        //    var dbTypeText = GetDatabaseType(licenseKey);
        //    var supportedDbTypes = (DatabaseType)(Convert.ToInt32(dbTypeText));

        //    if ((supportedDbTypes & dbType) != dbType)
        //        return false;

        //    acutalUserName = GetUserName(licenseKey);
        //    if (acutalUserName.StartsWith(userName))
        //        return true;

        //    return false;
        //}

        internal static DatabaseType GetDatabaseType(string licenseKey)
        {
            try
            {
                var descrypt = DecryptDES(licenseKey, ENCRYPT_KEY);

                var licenseType = descrypt.Substring(0, 1);
                var dbTypeText = descrypt.Substring(DATABASE_TYPE_STAR_INDEX, DATABASE_TYPE_LENGTH);
                var dbType = (DatabaseType)(Convert.ToInt32(dbTypeText));
                return dbType;
            }
            catch
            {
                return default(DatabaseType);
            }
        }

        internal static string GetUserName(string licenseKey)
        {
            try
            {
                var descrypt = DecryptDES(licenseKey, ENCRYPT_KEY);
                var username = descrypt.Substring(USER_NAME_STAR_INDEX);
                return username;
            }
            catch
            {
                return string.Empty;
            }
        }

        internal static LicenseType GetLicenseType(string licenseKey)
        {
            try
            {
                var descrypt = DecryptDES(licenseKey, ENCRYPT_KEY);
                // var str = DecryptDES(encryptString, encryptKey);
                var licenseType = (LicenseType)Convert.ToInt32(descrypt.Substring(0, 1));
                return licenseType;
            }
            catch
            {
                return LicenseType.Free;
            }
        }



        //protected abstract string LicenseName
        //{
        //    get;
        //}


        public override License GetLicense(LicenseContext c, Type type, object instance, bool allowExceptions)
        {
            ALinqLicenseContext context;
            if (c is ALinqLicenseContext)
                context = (ALinqLicenseContext)c;
            else
                context = new ALinqLicenseContext();

            LicenseManager.CurrentContext = context;
            LicFileLicense license = context.GetSavedLicense(type);

            if (license == null)
            {
                #region Read license from Attribute
                if (instance is DataContext)
                {
                    var attributes = instance.GetType().GetCustomAttributes(true)
                                             .Where(o => o is LicenseAttribute).Cast<LicenseAttribute>();
                    var count = attributes.Count();
                    if (count == 1)
                    {
                        var attribute = attributes.Single();
                        license = new LicFileLicense(this, attribute.UserName, attribute.Key, null);
                    }

                }
                #endregion

                #region Read license from alinq.lic
                if (license == null)
                {
                    Assembly assembly = null;
                    if (instance is DataContext && instance.GetType() != typeof(DataContext))
                        assembly = instance.GetType().Assembly;

                    Assembly licenseFileAssembly;
                    var stream = SearchLicenseStream(assembly, out licenseFileAssembly);
                    if (stream != null)
                    {
                        string userName;
                        string key;
                        string assemblyName;
                        CultureInfo culture;
                        ParseLicense(type, instance, stream, out userName, out key, out assemblyName, out culture);
                        stream.Dispose();

                        //==============================================================================
                        // 说明：key 允许为空，当为 Free 版本时
                        // if (key == null)
                        //     throw Error.LicenseFail(type, instance, string.Format("The {0} license key could not found in the ALinq.lic file.", type));
                        //==============================================================================

                        if (assemblyName != licenseFileAssembly.GetName().Name)
                            throw Error.AssemblyNameNotMatch(type, instance, assemblyName, licenseFileAssembly.GetName().Name);

                        license = new LicFileLicense(this, userName, key,culture);
                    }
                }
                #endregion

                #region Read license from registry.
                //if (license == null)
                //{
                //    var platform = Environment.OSVersion.Platform;
                //    if (platform == PlatformID.Win32NT || platform == PlatformID.Win32S ||
                //        platform == PlatformID.Win32Windows || platform == PlatformID.Win32Windows)
                //    {
                //        try
                //        {
                //            var regALinq = Registry.ClassesRoot.OpenSubKey("SOFTWARE\\ALinq", true);
                //            if (regALinq != null)
                //            {
                //                var obj = regALinq.GetValue("PreLoad");
                //                if (obj == null)
                //                {
                //                    obj = DateTime.Now.ToString("yyyy-MM-dd");
                //                    var bs = Encoding.ASCII.GetBytes((string)obj);
                //                    obj = Convert.ToBase64String(bs);
                //                    regALinq.SetValue("PreLoad", obj);
                //                }

                //                var preLoad = (string)obj;
                //                var bytes = Convert.FromBase64String(preLoad);
                //                var strDate = Encoding.ASCII.GetString(bytes);
                //                var date = DateTime.Parse(strDate);
                //                if (date > DateTime.Now)
                //                    throw Error.LicenseFail(type, instance, "This software is expired.");
                //                else
                //                {
                //                    var data = Encoding.ASCII.GetBytes(DateTime.Now.ToString());
                //                    var str = Convert.ToBase64String(data);
                //                    regALinq.SetValue("PreLoad", str);
                //                }

                //                obj = regALinq.GetValue("Date");
                //                if (obj == null)
                //                {
                //                    obj = DateTime.Now.ToString("yyyy-MM-dd");
                //                    var bs = Encoding.ASCII.GetBytes((string)obj);
                //                    obj = Convert.ToBase64String(bs);
                //                    regALinq.SetValue("Date", obj);
                //                }

                //                var encryptDate = (string)obj;
                //                bytes = Convert.FromBase64String(encryptDate);
                //                strDate = Encoding.ASCII.GetString(bytes);
                //                date = DateTime.Parse(strDate);

                //                var days = DAYS - (DateTime.Now - date).Days;
                //                license = new LicFileLicense(this, Constants.TrialUserName, date.ToString(), null) { ExpiredDays = days };
                //            }
                //        }
                //        catch (Exception exc)
                //        {
                //            if (exc is LicenseException)
                //                throw;
                //        }
                //    }
                //}
                #endregion

                if (license != null)
                {
                    //licenses[type] = license;
                    context.SetLicense(type, license);//.SetSavedLicenseKey(type, license.LicenseKey);
                }
            }

            if (license == null)
                throw new LicenseException(type, instance, string.Format("Found license fail!"));

            //验证授权
            ValidateLicense(type, instance, license);
            return license;
        }

        internal static Stream SearchLicenseStream(Assembly resourceAssembly, out Assembly licenseFileAssembly)
        {
            Stream licenseStream = null;
            if (resourceAssembly != null)
            {
                licenseStream = GetLicenseStream(resourceAssembly);
            }

            if (licenseStream != null)
            {
                licenseFileAssembly = resourceAssembly;
                return licenseStream;
            }

            //if (licenseStream == null)
            //{
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (assembly != null)
                    {
                        //licenseFileAssembly = assembly;
                        licenseStream = GetLicenseStream(assembly);
                        if (licenseStream != null)
                        {
                            licenseFileAssembly = assembly;
                            return licenseStream;
                        }
                    }
                }
                catch
                {
                }
            }

            licenseFileAssembly = null;
            return null;
        }

#if DEBUG
        internal
#endif
 static Stream GetLicenseStream(Assembly resourceAssembly)
        {
            const string LICENSE_NAME = "ALinq.lic";

            var names = resourceAssembly.GetManifestResourceNames();
            foreach (string str8 in names)
            {
                if (str8.EndsWith(LICENSE_NAME))
                {
                    var result = resourceAssembly.GetManifestResourceStream(str8);
                    return result;
                }
            }
            return null;
        }
#if DEBUG
        internal static void ParseLicense(Stream stream, out string userName, out string licenseKey, out string assemblyName, out CultureInfo culture)
        {
            ParseLicense(null, null, stream, out userName, out licenseKey, out assemblyName, out culture);
        }
#endif
        static void ParseLicense(Type type, object instance, Stream stream, out string userName, out string licenseKey, out string assemblyName, out CultureInfo culture)
        {
            object obj2;
            IFormatter formatter = new BinaryFormatter();
            new SecurityPermission(SecurityPermissionFlag.SerializationFormatter).PermitOnly();
            new SecurityPermission(SecurityPermissionFlag.SerializationFormatter).Assert();
            try
            {
                obj2 = formatter.Deserialize(stream);
                //var objArray = (object[])obj2;
                //userName = (string)objArray[0];
                //licenseKey = (string)objArray[1];
                var h = obj2 as System.Collections.Hashtable;
                if (h == null)
                    throw Error.LicenseFileErrorFormat(type, instance);

                userName = h["UserName"] as string;
                if (userName == null)
                    throw Error.CannotGetUserName(type, instance);

                //==============================================================================
                // 说明：key 允许为空，当为 Free 版本时
                licenseKey = h["LicenseKey"] as string;
                if (licenseKey == null && !string.Equals(userName, Constants.FreeUserName, StringComparison.CurrentCultureIgnoreCase))
                    throw Error.CannotGetLicenseKey(type, instance);
                //==============================================================================

                assemblyName = h["AssemblyName"] as string;
                if (assemblyName == null)
                    throw Error.CannotGetAssemblyName(type, instance);

                culture = h["Culture"] as CultureInfo;

            }
            finally
            {
                CodeAccessPermission.RevertAssert();
                CodeAccessPermission.RevertPermitOnly();
            }
        }


        static public byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                //Create a new instance of RSACryptoServiceProvider.
                var RSA = new RSACryptoServiceProvider();

                //Import the RSA Key information. This needs
                //to include the private key information.
                RSA.ImportParameters(RSAKeyInfo);

                //Decrypt the passed byte array and specify OAEP padding.  
                //OAEP padding is only available on Microsoft Windows XP or
                //later.  
                return RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }

        }

        internal static string DecryptDES(string decryptString, string decryptKey)
        {
            var des = new DESCryptoServiceProvider();

            var inputByteArray = new byte[decryptString.Length / 2];
            for (int x = 0; x < decryptString.Length / 2; x++)
            {
                int i = (Convert.ToInt32(decryptString.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            des.Key = Encoding.Default.GetBytes(decryptKey);
            des.IV = Encoding.Default.GetBytes(decryptKey);
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            return Encoding.Default.GetString(ms.ToArray());
        }


    }

    class ALinqLicenseContext : LicenseContext
    {
        private Dictionary<Type, LicFileLicense> licenses;

        public ALinqLicenseContext()
        {
            licenses = new Dictionary<Type, LicFileLicense>();
        }

        public void SetLicense(Type type, LicFileLicense license)
        {
            SetSavedLicenseKey(type, license.LicenseKey);
            licenses[type] = license;
        }

        public LicFileLicense GetSavedLicense(Type type)
        {
            LicFileLicense license;
            if (licenses.TryGetValue(type, out license))
                return license;

            return null;
        }
    }

    internal class LicFileLicense : License
    {
        // Fields
        private readonly string key;
        private LicenseProvider owner;

        // Methods
        public LicFileLicense(ALinqLicenseProvider owner, string userName, string key, CultureInfo culture)
        {
            this.owner = owner;
            this.key = key;
            this.UserName = userName;

            if (string.Equals(userName, Constants.TrialUserName, StringComparison.CurrentCulture))
            {
                this.LicenseType = LicenseType.Trial;
            }
            else if (string.Equals(userName, Constants.FreeUserName, StringComparison.CurrentCulture))
            {
                this.LicenseType = LicenseType.Free;
            }
            else
            {
                //this.LicenseType = LicenseType.Purchased;
                this.LicenseType = ALinqLicenseProvider.GetLicenseType(key);
            }
            this.Culture = culture;
        }

        public CultureInfo Culture { get;private set; }

        public string UserName { get; set; }

        public override void Dispose()
        {
        }

        // Properties
        public override string LicenseKey
        {
            get
            {
                return key;
            }
        }

        //internal bool IsTrial { get; set; }

        internal int ExpiredDays { get; set; }

        internal LicenseType LicenseType { get; private set; }

    }

    enum LicenseType
    {
        Single,
        Team,
        Site,
        Trial,
        Free,
        Experience,
        Standard
    }
}
#endif

