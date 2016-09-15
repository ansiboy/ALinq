using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ALinq.Dynamic.Parsers;

namespace ALinq.Dynamic
{
    internal class TokenCursor
    {
        private static Dictionary<string, Keyword> keywords;
        private static Dictionary<string, Function> functions;

        #region Keywords
        private const string KEYWORD_AS = "AS";
        private const string KEYWORD_ASC = "ASC";
        private const string KEYWORD_BY = "BY";
        private const string KEYWORD_DESC = "DESC";
        private const string KEYWORD_EXISTS = "EXISTS";
        private const string KEYWORD_FROM = "FROM";
        private const string KEYWORD_GROUP = "GROUP";
        private const string KEYWORD_HAVING = "HAVING";
        private const string KEYWORD_INNER = "INNER";
        private const string KEYWORD_JOIN = "JOIN";
        private const string KEYWORD_LEFT = "LEFT";
        private const string KEYWORD_LIMIT = "LIMIT";
        private const string KEYWORD_NEW = "NEW";
        private const string KEYWORD_NOT = "NOT";
        private const string KEYWORD_ON = "ON";
        private const string KEYWORD_ORDER = "ORDER";
        private const string KEYWORD_ROW = "ROW";
        private const string KEYWORD_SELECT = "SELECT";
        private const string KEYWORD_SKIP = "SKIP";
        private const string KEYWORD_VALUE = "VALUE";
        private const string KEYWORD_WHERE = "WHERE";
        private const string KEYWORD_USING = "USING";
        private const string KEYWORD_IN = "IN";
        #endregion

        string queryText;
        int textPos;
        int textLen;
        char ch;
        Token _current;
        private int line;
        private int column;

        public TokenCursor(string esql)
        {
            queryText = esql;
            textLen = queryText.Length;
            this._current = Activator.CreateInstance<Token>();

            Reset();

        }

        public string QueryText
        {
            get { return this.queryText; }
        }

        void NextChar()
        {
            if (textPos < textLen)
                textPos++;

            ch = textPos < textLen ? queryText[textPos] : '\0';
            if (ch == '\n')
            {
                line = line + 1;
                column = 0;
            }
            else// if (!Char.IsWhiteSpace(ch))
            {
                column = column + 1;
            }
        }

        #region Function NextToken


        public Token Current
        {
            get { return _current; }
        }

        public void NextToken()
        {
            while (Char.IsWhiteSpace(ch))
            {
                NextChar();

            }

            //var preIdentity = this.Current.Identity;
            TokenId t;
            int startPos = textPos;
            var tokenPosition = new TokenPosition { Column = column, Line = line, Sequence = startPos };

            switch (ch)
            {
                case '!':
                    NextChar();
                    if (ch == '=')
                    {
                        NextChar();
                        t = TokenId.ExclamationEqual;
                    }
                    else
                    {
                        t = TokenId.Exclamation;
                    }
                    break;
                case '%':
                    NextChar();
                    t = TokenId.Percent;
                    break;
                case '&':
                    NextChar();
                    if (ch == '&')
                    {
                        NextChar();
                        t = TokenId.DoubleAmphersand;
                    }
                    else
                    {
                        t = TokenId.Amphersand;
                    }
                    break;
                case '(':
                    NextChar();
                    t = TokenId.OpenParen;
                    break;
                case ')':
                    NextChar();
                    t = TokenId.CloseParen;
                    break;
                case '*':
                    NextChar();
                    t = TokenId.Asterisk;
                    break;
                case '+':
                    NextChar();
                    t = TokenId.Plus;
                    break;
                case ',':
                    NextChar();
                    t = TokenId.Comma;
                    break;
                case ';':
                    NextChar();
                    t = TokenId.Semicolon;
                    break;
                case '-':
                    NextChar();
                    if (ch == '-')
                    {
                        do
                        {
                            NextChar();
                        } while (ch != '\n' && textPos < textLen);
                        t = TokenId.Comment;
                    }
                    else
                    {
                        t = TokenId.Minus;
                    }
                    break;
                case '.':
                    NextChar();
                    t = TokenId.Dot;
                    break;
                case '/':
                    NextChar();
                    t = TokenId.Slash;
                    break;
                case ':':
                    NextChar();
                    t = TokenId.Colon;
                    break;
                case '<':
                    NextChar();
                    if (ch == '=')
                    {
                        NextChar();
                        t = TokenId.LessThanEqual;
                    }
                    else if (ch == '>')
                    {
                        NextChar();
                        t = TokenId.LessGreater;
                    }
                    else
                    {
                        t = TokenId.LessThan;
                    }
                    break;
                case '=':
                    NextChar();
                    if (ch == '=')
                    {
                        NextChar();
                        t = TokenId.DoubleEqual;
                    }
                    else
                    {
                        t = TokenId.Equal;
                    }
                    break;
                case '>':
                    NextChar();
                    if (ch == '=')
                    {
                        NextChar();
                        t = TokenId.GreaterThanEqual;
                    }
                    else
                    {
                        t = TokenId.GreaterThan;
                    }
                    break;
                case '?':
                    NextChar();
                    t = TokenId.Question;
                    break;
                case '[':
                    NextChar();
                    if (Char.IsNumber(ch) || ch == '\'')
                    {
                        t = TokenId.OpenBracket;
                    }
                    else
                    {
                        while (ch != ']' && textPos < textLen)
                        {
                            NextChar();
                        }

                        if (ch != ']')
                            throw Error.TokenExpected(tokenPosition, TokenId.CloseBracket);

                        NextChar();
                        t = TokenId.Identifier;
                    }

                    break;
                case ']':
                    NextChar();
                    t = TokenId.CloseBracket;
                    break;
                case '|':
                    NextChar();
                    if (ch == '|')
                    {
                        NextChar();
                        t = TokenId.DoubleBar;
                    }
                    else
                    {
                        t = TokenId.Bar;
                    }
                    break;
                case '"':
                case '\'':
                    char quote = ch;
                    do
                    {
                        NextChar();
                        while (textPos < textLen && ch != quote) NextChar();
                        if (textPos == textLen)
                            throw Error.UnterminatedStringLiteral(tokenPosition);
                        //throw ParseError(textPos, Res.UnterminatedStringLiteral);
                        NextChar();
                    } while (ch == quote);
                    t = TokenId.StringLiteral;
                    break;
                case '#':
                    char sharp = ch;
                    do
                    {
                        NextChar();
                        while (textPos < textLen && ch != sharp)
                            NextChar();

                        if (textPos == textLen)
                            throw Error.UnterminatedDateTimeLiteral(tokenPosition);

                        NextChar();
                    } while (ch == sharp);
                    t = TokenId.Sharp;
                    break;
                case '{':
                    NextChar();
                    t = TokenId.OpenCurlyBrace;
                    break;
                case '}':
                    NextChar();
                    t = TokenId.CloseCurlyBrace;
                    break;
                default:
                    if (Char.IsLetter(ch) || ch == '@' || ch == '_')
                    {
                        var ch1 = ch;
                        var pos1 = this.textPos;
                        do
                        {
                            NextChar();
                            //ch1 = ch;
                        }
                        while (Char.IsLetterOrDigit(ch) || ch == '_');

                        if (ch1 == 'X' && ch == '\'' && (this.textPos - pos1) == 1)
                        {
                            //NextChar();
                            do
                            {
                                NextChar();

                            } while (textPos < textLen && ch != '\'');

                            if (textPos == textLen)
                                throw Error.UnterminatedBinaryLiteral(tokenPosition);

                            NextChar();
                            t = TokenId.BinaryLiteral;
                            break;
                        }

                        if (ch1 == 'G' && ch == '\'' && (this.textPos - pos1) == 1)
                        {
                            do
                            {
                                NextChar();
                            } while (textPos < textLen && ch != '\'');

                            if (textPos == textLen)
                                throw Error.UnterminatedBinaryLiteral(tokenPosition);

                            NextChar();
                            t = TokenId.GuidLiteral;
                            break;
                        }
                        t = TokenId.Identifier;
                        break;
                    }
                    if (Char.IsDigit(ch))
                    {
                        t = TokenId.IntegerLiteral;
                        do
                        {
                            NextChar();
                        } while (Char.IsDigit(ch));
                        if (ch == '.')
                        {
                            t = TokenId.RealLiteral;
                            NextChar();
                            ValidateDigit();
                            do
                            {
                                NextChar();
                            } while (Char.IsDigit(ch));
                        }
                        if (ch == 'E' || ch == 'e')
                        {
                            t = TokenId.RealLiteral;
                            NextChar();
                            if (ch == '+' || ch == '-') NextChar();
                            ValidateDigit();
                            do
                            {
                                NextChar();
                            } while (Char.IsDigit(ch));
                        }

                        //if (ch == LiteralPostfix.FloatUpper || ch == LiteralPostfix.FloatLower ||
                        //    ch == LiteralPostfix.LongUpper || ch == LiteralPostfix.LongLower ||
                        //    ch == LiteralPostfix.DecimalUpper || ch == LiteralPostfix.DecimalLower)
                        //{
                        //    NextChar();
                        //}
                        //else if (ch == LiteralPostfix.UnsignedUpper || ch == LiteralPostfix.UnsignedLower)
                        //{
                        //    NextChar();
                        //    if (ch == LiteralPostfix.LongUpper || ch == LiteralPostfix.LongLower)
                        //        NextChar();
                        //}
                        ParseSign(ref t);


                        break;
                    }
                    if (textPos == textLen)
                    {
                        t = TokenId.End;
                        break;
                    }
                    throw ParseError(textPos, Res.InvalidCharacter, ch);
            }

            if (t == TokenId.Comment)
            {
                NextToken();
                return;
            }

            var tokenText = queryText.Substring(startPos, textPos - startPos);//queryText[textPos]

            if (typeof(Token).IsValueType == false)
                this._current = Activator.CreateInstance<Token>();


            _current.Keyword = Keyword.None;
            _current.Function = Function.None;
            _current.SetValues(t, tokenText, tokenPosition);

            if (t == TokenId.Identifier)
            {

                Keyword k;

                if (Keywords.TryGetValue(tokenText, out k))
                    this._current.Keyword = k;


                //=============================================================
                // 對于具有相同名稱的 Keyword 和 Function，必須特殊處理
                //-------------------------------------------------------------
                //if (new[] { Keyword.Left, Keyword.Right }.Contains(k))
                //{
                for (var j = textPos; j < queryText.Length; j++)
                {
                    if (Char.IsWhiteSpace(queryText[j]) == false)
                    {
                        this._current.IsMethod = queryText[j] == '(';
                        Function f;
                        if (this._current.IsMethod && Functions.TryGetValue(tokenText, out f))
                        {
                            this._current.Keyword = Keyword.None;
                            this._current.Function = f;
                        }
                        break;
                    }
                }
                //}
                //===========================================================


            }

            if (!string.IsNullOrEmpty(_current.Text) && _current.Text[0] == '@' && _current.Text.Length == 1)
            {
                throw Error.ParameterNameRequried(_current);
            }



        }

        void ParseSign(ref TokenId t)
        {
            if (ch == LiteralPostfix.UnsignedUpper || ch == LiteralPostfix.UnsignedLower)
            {
                NextToken();
            }
            ParseNumberType(ref t);
        }

        void ParseNumberType(ref TokenId t)
        {
            //TokenId t;
            if (ch == LiteralPostfix.FloatUpper || ch == LiteralPostfix.FloatLower)
            {
                t = TokenId.RealLiteral;
                NextChar();
            }
            else if (ch == LiteralPostfix.LongUpper || ch == LiteralPostfix.LongLower)
            {
                t = TokenId.IntegerLiteral;
                NextChar();
            }
            else if (ch == LiteralPostfix.DecimalUpper || ch == LiteralPostfix.DecimalLower)
            {
                t = TokenId.RealLiteral;
                NextChar();
            }
        }

        #endregion



        public void Reset()
        {
            textPos = 0;
            ch = (textPos < textLen) ? queryText[textPos] : '\0';

            NextToken();
        }

        protected Exception ParseError(string format, params object[] args)
        {
            return ParseError(_current.Position, format, args);
        }

        Exception ParseError(int pos, string format, params object[] args)
        {
            return new ParseException(string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args), pos);
        }

        void ValidateDigit()
        {
            if (!Char.IsDigit(ch)) throw ParseError(textPos, Res.DigitExpected);
        }

        internal void MoveTo(TokenPosition pos)
        {
            this.line = pos.Line;
            this.column = pos.Column;

            Debug.Assert(pos >= 0);
            textPos = pos.Sequence;
            ch = (textPos < textLen) ? queryText[textPos] : '\0';
            NextToken();

        }

        public Dictionary<string, Keyword> Keywords
        {
            get
            {
                if (keywords == null)
                {
                    var d = Enum.GetValues(typeof(Keyword)).Cast<Keyword>()
                                .ToDictionary(o => Enum.GetName(typeof(Keyword), o).ToUpper(), StringComparer.CurrentCultureIgnoreCase);

                    keywords = d;
                }
                return keywords;
            }
        }

        public Dictionary<string, Function> Functions
        {
            get
            {
                if (functions == null)
                {
                    var d = Enum.GetValues(typeof(Function)).Cast<Function>()
                                .ToDictionary(delegate(Function o)
                                {
                                    if (o == Function.Average)
                                        return "AVG";

                                    return Enum.GetName(typeof(Function), o).ToUpper();
                                }, StringComparer.CurrentCultureIgnoreCase);

                    functions = d;
                }

                return functions;
            }
        }
    }
}
