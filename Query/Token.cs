using System;
using System.Collections.Generic;
using System.Linq;
using ALinq.Dynamic.Parsers;

namespace ALinq.Dynamic
{
    [System.Diagnostics.DebuggerDisplay("{Text}")]
    struct Token
    {
        public static Token Empty;

        public TokenId Identity { get; private set; }

        public string Text { get; private set; }

        public TokenPosition Position { get; private set; }

        public Keyword Keyword { get; set; }

        public Function Function { get; set; }

        //public char NextChar { get; set; }

        public bool IsMethod { get; set; }

        public void SetValues(TokenId identity, string text, TokenPosition pos)
        {
            this.Identity = identity;
            this.Text = text;
            this.Position = pos;
        }
    }
}
