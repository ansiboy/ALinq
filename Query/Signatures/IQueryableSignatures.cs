namespace ALinq.Dynamic.Parsers
{
    interface IQueryableSignatures
    {
        void Average(decimal? source);
        void Average(decimal source);
        void Average(double? source);
        void Average(double source);
        void Average(float? source);
        void Average(float source);
        void Average(long? source);
        void Average(long source);
        void Average(int? source);
        void Average(int source);

        void Average();
    }
}