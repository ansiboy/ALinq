using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DynamicQuery")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("ALinq Team")]
[assembly: AssemblyProduct("DynamicQuery")]
[assembly: AssemblyCopyright("Copyright @ ALinq Team")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("0a32c7b3-d666-4831-814d-1464a5016608")]

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
[assembly: AssemblyVersion(ALinq.Constants.ALinqVersion)]
[assembly: AssemblyFileVersion(ALinq.Constants.ALinqVersion)]

[assembly: InternalsVisibleTo(@"ALinq.Inject, PublicKey=0024000004800000940000000602000000240000525341310004000001000100f14b7dbca54c8340376ac4827d4f1e294016be3297f9d13cadf30a65077617ae31be414b3a0a77973919443bb20593881c8b01f87a4a6306048f25056b403c52e1dffc09c137628f7dd22b401cdb9780ad2ad79501ec9f45c26f0dd07446082826f03722f8c71725afdcae4b950984fe6f42399ce38bc991016cadf7749ab49a")]
[assembly: InternalsVisibleTo(@"System.Data.Linq.Inject, PublicKey=0024000004800000940000000602000000240000525341310004000001000100f14b7dbca54c8340376ac4827d4f1e294016be3297f9d13cadf30a65077617ae31be414b3a0a77973919443bb20593881c8b01f87a4a6306048f25056b403c52e1dffc09c137628f7dd22b401cdb9780ad2ad79501ec9f45c26f0dd07446082826f03722f8c71725afdcae4b950984fe6f42399ce38bc991016cadf7749ab49a")]
#if DEBUG
[assembly: InternalsVisibleTo(@"ALinq.Dynamic.LinqToSql.Test, PublicKey=002400000480000094000000060200000024000052534131000400000100010027b1bd78c8c9f1b7789cc2e20f5d3777484b3fcbffb31cff6db0a269bc3403f0b1aaf93d4d3802f4a2cd278630416fbfa8a63501256786b3266fb2ef8531335ab59ed624fe66106322cf10bcfc10aa88c62a23d17062e12912bde90d1e7e8bb7786b0150ee3122d7dd5d64b334388a9315557e03284adfd9b40bf5f35d6eb09b")]
[assembly: InternalsVisibleTo(@"ALinq.Dynamic.Test, PublicKey=002400000480000094000000060200000024000052534131000400000100010027b1bd78c8c9f1b7789cc2e20f5d3777484b3fcbffb31cff6db0a269bc3403f0b1aaf93d4d3802f4a2cd278630416fbfa8a63501256786b3266fb2ef8531335ab59ed624fe66106322cf10bcfc10aa88c62a23d17062e12912bde90d1e7e8bb7786b0150ee3122d7dd5d64b334388a9315557e03284adfd9b40bf5f35d6eb09b")]
#endif