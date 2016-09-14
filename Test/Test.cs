using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ALinq;

namespace Test
{
    abstract partial class SqlTest
    {
        /*
        protected static Type DataManipulationType = DataContext.LinqAssembly.GetType("System.Data.Linq.Provider.DataManipulation");

        /// <summary>
        /// 构造表达式删除对象。
        /// </summary>
        [TestMethod]
        public void DeleteExpression()
        {
            var order = new Order() { OrderID = 100 };
            var expr = Expression.Call(DataManipulationType, "Delete", new[] { typeof(Order) },
                                       new Expression[] { Expression.Constant(order) });
            //db.Provider.Execute(expr);
        }*/
    }

    
}
