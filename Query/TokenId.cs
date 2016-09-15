namespace ALinq.Dynamic
{
    enum TokenId
    {
        Unknown = 1,
        End,
        StringLiteral,
        IntegerLiteral,
        RealLiteral,
        Exclamation,
        Percent,
        Amphersand,
        OpenParen,
        CloseParen,
        Asterisk,
        Plus,
        Comma,
        Minus,
        Dot,
        Slash,
        Colon,
        LessThan,
        Equal,
        GreaterThan,
        Question,
        OpenBracket,    //[
        CloseBracket,   //]
        DoubleOpenBracket,
        DoubleCloseBracket,
        Bar,
        ExclamationEqual,
        DoubleAmphersand,
        LessThanEqual,
        LessGreater,
        DoubleEqual,
        GreaterThanEqual,
        DoubleBar,
        Sharp,
        OpenCurlyBrace,
        CloseCurlyBrace,
        Semicolon,
        Comment,
        Identifier,
        BinaryLiteral,
        GuidLiteral
    }
}