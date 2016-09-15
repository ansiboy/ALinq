using System;

namespace ALinq.Dynamic.Parsers
{
    interface ISubtractSignatures : IAddSignatures
    {
        void F(DateTime x, DateTime y);
        void F(DateTime? x, DateTime? y);
    }
}