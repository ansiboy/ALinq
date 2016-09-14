using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ALinq Firebird Provider")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(ALinq.Constants.Company)]
[assembly: AssemblyProduct("ALinq.Firebird")]
[assembly: AssemblyCopyright(ALinq.Constants.Copyright)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("ebb6a676-446a-4e2e-8f45-d60761bcd2f9")]

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
[assembly: AssemblyVersion(ALinq.Constants.FirebirdVersion)]
[assembly: AssemblyFileVersion(ALinq.Constants.FirebirdVersion)]
[assembly: AllowPartiallyTrustedCallers]
//[assembly: InternalsVisibleTo(@"ALinq.CodeGenerator, PublicKey=0024000004800000940000000602000000240000525341310004000001000100771F637FD8797E88AF34E14166BDA8C7C2C2B9F95439820593E4AF889F41A480AA9A8C043CF5CB8B2777024D683A44E4C2CAA43DCB70EFFED0063752D33F1455FD2CA4C6CC5A164A1AF4DC0D94DCBA2F6EF06B4592B6FE13232447AC2F3CF73DAAE75D22C76E8B97F242F17C38C93707E693E8251A3CDE64DFCD52FA3BF3CBF1")]
#if DEBUG
[assembly: InternalsVisibleTo("Test, PublicKey=002400000480000094000000060200000024000052534131000400000100010013a944798686e03018e0fee039bb6bace1b23150c4a55cc97ffd87177072b39ea0c607dc98bfcbfc0084750f8b4e8c19cf9e25a8d97f58b3100cfbd8b49824a683efd03fe9e35fec045441c6a39624c0fe2cc45dd29a4f181572eda0cbbac5649361ca1ab2cc85a5935c2c168310b019ba46ada80499f2bb09a3a89f6a3602a1")]
#endif