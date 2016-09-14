using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ALinq DB2 Provider")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(ALinq.Constants.Company)]
[assembly: AssemblyProduct("ALinq.DB2")]
[assembly: AssemblyCopyright(ALinq.Constants.Copyright)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("73a36cc2-3e7b-40a9-99d6-aa007e2d5f58")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(ALinq.Constants.DB2Version)]
[assembly: AssemblyFileVersion(ALinq.Constants.DB2Version)]
[assembly: AllowPartiallyTrustedCallers] 
#if DEBUG
[assembly: InternalsVisibleTo("Test, PublicKey=002400000480000094000000060200000024000052534131000400000100010013a944798686e03018e0fee039bb6bace1b23150c4a55cc97ffd87177072b39ea0c607dc98bfcbfc0084750f8b4e8c19cf9e25a8d97f58b3100cfbd8b49824a683efd03fe9e35fec045441c6a39624c0fe2cc45dd29a4f181572eda0cbbac5649361ca1ab2cc85a5935c2c168310b019ba46ada80499f2bb09a3a89f6a3602a1")]
#endif
