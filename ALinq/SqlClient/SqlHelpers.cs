using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq.SqlClient
{
    /// <summary>
    /// Provides helper methods for operations that match string patterns.
    /// </summary>
    public static class SqlHelpers
    {
        // Methods
        private static string EscapeLikeText(string text, char escape)
        {
            if ((!text.Contains("%") && !text.Contains("_")) && (!text.Contains("[") && !text.Contains("^")))
            {
                return text;
            }
            var builder = new StringBuilder(text.Length);
            foreach (char ch in text)
            {
                if (((ch == '%') || (ch == '_')) || (((ch == '[') || (ch == '^')) || (ch == escape)))
                {
                    builder.Append(escape);
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Creates a search pattern string where the specified text can have other text before and following it.
        /// </summary>
        /// <param name="text">The string to insert into the search pattern string.</param>
        /// <param name="escape">The character to use to escape wildcard characters.</param>
        /// <returns>A search pattern string that contains the specified string and the '%' character before and after it.</returns>
        public static string GetStringContainsPattern(string text, char escape)
        {
            if (text == null)
            {
                throw Error.ArgumentNull("text");
            }
            return ("%" + EscapeLikeText(text, escape) + "%");
        }

        /// <summary>
        /// Creates a search pattern string where the specified text can have other text before it but not following it.
        /// </summary>
        /// <param name="text">The string to insert into the search pattern string.</param>
        /// <param name="escape">The character to use to escape wildcard characters.</param>
        /// <returns>A search pattern string that contains the '%' character followed by the specified string.</returns>
        public static string GetStringEndsWithPattern(string text, char escape)
        {
            if (text == null)
            {
                throw Error.ArgumentNull("text");
            }
            return ("%" + EscapeLikeText(text, escape));
        }

        /// <summary>
        /// Creates a search pattern string where the specified text can have other text after it but not before it.
        /// </summary>
        /// <param name="text">The string to insert into the search pattern string.</param>
        /// <param name="escape">The character to use to escape wildcard characters.</param>
        /// <returns>A search pattern string that contains the specified string followed by the '%' character.</returns>
        public static string GetStringStartsWithPattern(string text, char escape)
        {
            if (text == null)
            {
                throw Error.ArgumentNull("text");
            }
            return (EscapeLikeText(text, escape) + "%");
        }

        /// <summary>
        /// Translates a search pattern for the Visual Basic Like operator to a search pattern for the SQL Server LIKE operator.
        /// </summary>
        /// <param name="pattern">The Visual Basic Like search pattern to translate to a SQL Server LIKE search pattern.</param>
        /// <param name="escape">The character to use to escape special SQL characters or the escape character itself.</param>
        /// <returns>A search pattern for the SQL Server LIKE operator that corresponds to the specified Visual Basic Like search pattern.</returns>
        public static string TranslateVBLikePattern(string pattern, char escape)
        {
            if (pattern == null)
            {
                throw Error.ArgumentNull("pattern");
            }
            var builder = new StringBuilder();
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            int num = 0;
            foreach (char ch in pattern)
            {
                if (!flag)
                {
                    goto Label_010C;
                }
                num++;
                if (flag3)
                {
                    if (ch != ']')
                    {
                        builder.Append('^');
                        flag3 = false;
                    }
                    else
                    {
                        builder.Append('!');
                        flag3 = false;
                    }
                }
                char ch2 = ch;
                if (ch2 != '!')
                {
                    switch (ch2)
                    {
                        case ']':
                            flag = false;
                            flag3 = false;
                            builder.Append(']');
                            goto Label_01B1;

                        case '^':
                            goto Label_00C9;

                        case '-':
                            goto Label_00B0;
                    }
                    goto Label_00E4;
                }
                if (num == 1)
                {
                    flag3 = true;
                }
                else
                {
                    builder.Append(ch);
                }
                goto Label_01B1;
            Label_00B0:
                if (flag2)
                {
                    throw Error.VbLikeDoesNotSupportMultipleCharacterRanges();
                }
                flag2 = true;
                builder.Append('-');
                goto Label_01B1;
            Label_00C9:
                if (num == 1)
                {
                    builder.Append(escape);
                }
                builder.Append(ch);
                goto Label_01B1;
            Label_00E4:
                if (ch == escape)
                {
                    builder.Append(escape);
                    builder.Append(escape);
                }
                else
                {
                    builder.Append(ch);
                }
                goto Label_01B1;
            Label_010C:
                switch (ch)
                {
                    case '?':
                        builder.Append('_');
                        goto Label_01B1;

                    case '[':
                        flag = true;
                        flag2 = false;
                        num = 0;
                        builder.Append('[');
                        goto Label_01B1;

                    case '_':
                    case '%':
                        builder.Append(escape);
                        builder.Append(ch);
                        goto Label_01B1;

                    case '#':
                        builder.Append("[0-9]");
                        goto Label_01B1;

                    case '*':
                        builder.Append('%');
                        goto Label_01B1;

                    default:
                        if (ch == escape)
                        {
                            builder.Append(escape);
                            builder.Append(escape);
                        }
                        else
                        {
                            builder.Append(ch);
                        }
                        goto Label_01B1;
                }
            Label_01B1: ;
            }
            if (flag)
            {
                throw Error.VbLikeUnclosedBracket();
            }
            return builder.ToString();
        }
    }

 

}
