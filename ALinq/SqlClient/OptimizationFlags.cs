using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq.SqlClient
{
    [Flags]
    internal enum OptimizationFlags
    {
        None,
        SimplifyCaseStatements,
        OptimizeLinkExpansions,
        All
    }
}