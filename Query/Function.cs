namespace ALinq.Dynamic
{

    enum Function
    {
     
        Cast,
       
        
        None,
        
        OfType,
    
        PI,
        MultiSet,

        #region 聚合函数
        Average,
        BigCount,
        Count,
        Max,
        Min,
        Sum,
        #endregion

        #region 字符串函数
        Concat,
        Contains,
        EndsWith,
        IndexOf,
        Left,
        Length,
        LTrim,
        Replace,
        Reverse,
        Right,
        RTrim,
        StartsWith,
        Substring,
        ToLower,
        ToUpper,
        Treat,
        Trim,
        #endregion

        #region 日期函數
        Year,
        Month,
        Day,
        DayOfYear,
        Hour,
        Minute,
        Second,
        #endregion

        #region 位函数
        BitWiseAnd,
        BitWiseNot,
        BitWiseOr,
        BitWiseXor,
        #endregion

        #region 数字函数
        Abs,
        Round,
        Ceiling,
        Floor,
        Power,
        Truncate,
        #endregion

        #region 其它函数
        NewGuid,
        IIF,
        #endregion

    }


}
