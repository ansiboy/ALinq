using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq.SqlClient
{
    [Flags]
    internal enum ConverterStrategy
    {
        CanOutputFromInsert = 0x20,
        CanUseJoinOn = 0x10,
        CanUseOuterApply = 4,
        CanUseRowStatus = 8,
        CanUseScopeIdentity = 2,
        Default = 0,
        SkipWithRowNumber = 1
    }
}