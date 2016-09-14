using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ALinq.SqlClient
{
    internal partial class ObjectReaderCompiler
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct NamedColumn
        {
            private readonly string name;
            private readonly bool isRequired;

            internal NamedColumn(string name, bool isRequired)
            {
                this.name = name;
                this.isRequired = isRequired;
            }

            internal string Name
            {
                get { return name; }
            }

            internal bool IsRequired
            {
                get { return isRequired; }
            }
        }
    }
}
