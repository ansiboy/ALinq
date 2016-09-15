using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ALinq.Dynamic.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Dynamic.Test.CoreTest
{
    [TestClass]
    public class VariablesStoreTest
    {
        [TestMethod]
        public void SetLocalVariable()
        {
            var store = new VariablesStore();
            var value = Expression.Constant("hello");
            store.CreateLocalVariables();

            //store.SetLocalVariable("aa", value);

            //Assert.IsNotNull(store.AvailableVarialbes["aa"]);
            //Assert.IsNotNull(store.LocalVariables["aa"]);
            //Assert.IsNull(store.GlobalVariables["aa"]);
        }

        [TestMethod]
        public void SetGlobalVariable()
        {
            var store = new VariablesStore();
            var value = Expression.Constant("hello");
            store.CreateLocalVariables();

            //store.SetGlobalVariable("aa", value);

            //Assert.IsNotNull(store.AvailableVarialbes["aa"]);
            //Assert.IsNull(store.LocalVariables["aa"]);
            //Assert.IsNotNull(store.GlobalVariables["aa"]);
        }
    }
}
