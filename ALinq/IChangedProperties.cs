using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq
{
    internal interface IUpdateProperties
    {
        IEnumerable<string> Properties
        {
            get;
        }
    }
}
