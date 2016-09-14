using System.Runtime.InteropServices;
using ALinq.Mapping;

namespace ALinq
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RelatedItem
    {
        internal MetaType Type;
        internal object Item;

        internal RelatedItem(MetaType type, object item)
        {
            this.Type = type;
            this.Item = item;
        }
    }
}