using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ALinq.Dynamic.Parsers
{
    partial class QueryParser
    {
/*
        class ConditionalParser : IParser
        {
            private QueryParser parser;
            private TokenCursor tokenCursor;

            public ConditionalParser(QueryParser parser)
            {
                this.parser = parser;

                this.tokenCursor = tokenCursor;
                this.tokenCursor = parser.TokenCursor;

            }


            public Expression ParseExpression()
            {
                Expression expr;
                //if (Token.Keyword == Keyword.Not)
                //{
                //    NextToken();
                //    expr = ParseExpression();
                //    expr = Expression.Not(expr);
                //    return expr;
                //}

                expr = this.parser.ParseExpression();
                //expr = ParseWherePostKeyword(expr);

                return expr;
            }

            TokenCursor IParser.TokenCursor
            {
                get { return this.tokenCursor; }
            }

            private Token Token
            {
                get { return this.tokenCursor.Current; }
            }

            void NextToken()
            {
                this.tokenCursor.NextToken();
            }
        }
*/
    }

}
