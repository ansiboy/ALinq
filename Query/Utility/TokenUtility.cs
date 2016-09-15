using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ALinq.Dynamic.Parsers;

namespace ALinq.Dynamic
{
    static class TokenUtility
    {
        public static bool IsIdentifier(this Token token)
        {
            return token.Identity == TokenId.Identifier;
        }

        public static bool Is(this Token token, TokenId tokenId)
        {
            return token.Identity == tokenId;
        }

        public static string GetIdentifier(this Token token)
        {
            token.Validate(TokenId.Identifier, Res.IdentifierExpected);
            string id = token.Text;
            if (id.Length > 1 && id[0] == '@')
                id = id.Substring(1);

            if (id[0] == '[')
                return id.Substring(1, id.Length - 2);

            return id;
        }

        public static bool IdentifierIs(this Token token, string id)
        {
            return token.Identity == TokenId.Identifier
                                && String.Equals(id, token.Text, StringComparison.OrdinalIgnoreCase);

        }

        public static void Validate(this Token token, TokenId t, string errorMessage)
        {
            if (!token.Is(t))
                throw Error.GenericSyntaxError(token, errorMessage); 
        }

        public static void Validate(this Token token, TokenId t, Exception exc)
        {
            if (!token.Is(t))
                throw exc;
        }

        public static void Validate(this Token token, Keyword k, Exception exc)
        {
            if (token.Keyword != k)
                throw exc;
        }

        public static void Validate(this Token token, Keyword k, Func<TokenPosition, Keyword, Exception> func)
        {
            var exc = func(token.Position, k);
            Validate(token, k, exc);
        }

        public static void Validate(this Token token, TokenId t)
        {
            Validate(token, t, Error.GenericSyntaxError(token));
        }

        public static void Validate(this Token token, Keyword t)
        {
            Validate(token, t, Error.GenericSyntaxError(token));
        }

        private static Exception ParseError(Token token, string format, params object[] args)
        {
            return ParseError(token.Position, format, args);
        }

        static Exception ParseError(TokenPosition pos, string format, params object[] args)
        {
            return ParseError(pos.Sequence, format, args);
        }

        static Exception ParseError(int pos, string format, params object[] args)
        {
            return new ParseException(string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args), pos);
        }



    }
}
