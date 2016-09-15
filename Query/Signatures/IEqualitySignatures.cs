using System;
namespace ALinq.Dynamic.Parsers
{
    interface IEqualitySignatures : IRelationalSignatures
    {
        void F(bool x, bool y);
        void F(bool? x, bool? y);

        void F(Guid x, Guid y);
        void F(Guid? x, Guid? y);
    }
}