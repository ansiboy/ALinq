using System.Linq.Expressions;

namespace ALinq.Dynamic.Parsers
{
    internal interface IParser
    {
        Expression ParseExpression();

        TokenCursor TokenCursor { get; }
    }

}
