namespace ALinq.SqlClient
{
    internal enum SqlNodeType
    {
        Add,
        Alias,
        AliasRef,
        And,
        Assign,

        Avg,
        Between,
        BitAnd,
        BitNot,
        BitOr,
        
        BitXor,
        Block,
        Cast,
        ClientArray,
        ClientCase,
        
        ClientParameter,
        ClientQuery,
        ClrLength,
        Coalesce,
        Column,             //0x13 19
        
        ColumnRef,
        Concat,             //0x15 21
        Convert,
        Count,
        Covar,

        //====================================//
        
        Delete,
        DiscriminatedType,  //0x1a 26
        DiscriminatorOf,
        Div,
        DoNotVisit,
        
        Element,
        ExprSet,            //0x1f 31
        EQ,         
        EQ2V,
        Exists,             //0x22 34
        
        FunctionCall,
        In,
        IncludeScope,
        IsNotNull,          //0x26 38
        IsNull,         
        
        LE,
        Lift,
        Link,
        Like,
        LongCount,
        
        LT,                 
        GE,
        Grouping,           //0x2f 47
        GT,
        Insert,

        //====================================//

        Join,
        JoinedCollection,
        Max,
        MethodCall,
        Member,
        
        MemberAssign,
        Min,
        Mod,
        Mul,
        Multiset,
        
        NE,
        NE2V,
        Negate,
        New,
        Not,
        
        Not2V,
        Nop,
        ObjectType,         //0x43 67
        Or,
        OptionalValue,
        
        OuterJoinedValue,
        Parameter,
        Property,
        Row,
        RowNumber,

        //====================================//

        ScalarSubSelect,
        SearchedCase,
        Select,
        SharedExpression,
        SharedExpressionRef,
        
        SimpleCase,
        SimpleExpression,
        Stddev,
        StoredProcedureCall,
        Sub,
        
        Sum,
        Table,
        TableValuedFunctionCall,
        Treat,
        TypeCase,
        
        Union,
        Update,
        UserColumn,
        UserQuery,
        UserRow,
        
        
        Variable,
        Value,
        ValueOf,

    }
}