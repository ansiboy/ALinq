using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq.Mapping
{
    internal delegate V DGet<T, V>(T t);
    internal delegate void DRSet<T, V>(ref T t, V v);
    internal delegate void DSet<T, V>(T t, V v);

    internal delegate object DItemGet<T>(T t, string key);
    internal delegate void DRItemSet<T>(ref T t, string key, object v);
    internal delegate void DItemSet<T>(T t, string key, object v);
}
