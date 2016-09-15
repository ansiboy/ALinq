namespace ALinq.Dynamic.Parsers
{
    interface IEnumerableSignatures
    {
        void Where(bool predicate);
        void Any();
        void Any(bool predicate);
        void All(bool predicate);
        void Count();
        void Count(bool predicate);
        void Min(object selector);
        void Max(object selector);
        void Sum(int selector);
        void Sum(int? selector);
        void Sum(long selector);
        void Sum(long? selector);
        void Sum(float selector);
        void Sum(float? selector);
        void Sum(double selector);
        void Sum(double? selector);
        void Sum(decimal selector);
        void Sum(decimal? selector);
        void Average(int selector);
        void Average(int? selector);
        void Average(long selector);
        void Average(long? selector);
        void Average(float selector);
        void Average(float? selector);
        void Average(double selector);
        void Average(double? selector);
        void Average(decimal selector);
        void Average(decimal? selector);
    }
}