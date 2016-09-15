using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using ALinq.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicQueryTest
{
    [TestClass]
    public class ClassFactoryTest
    {
        //[TestMethod]
        //public void GetDynamicClassTest()
        //{
        //    var properties = new[] { new DynamicProperty("F", typeof(string)), new DynamicProperty("L", typeof(string)) };
        //    var newType = ClassFactory.Instance.GetDynamicClass(properties, typeof(DynamicObject));

        //    Assert.IsNotNull(newType);
        //    var newTypeProperties = newType.GetProperties().ToArray();

        //    Assert.AreEqual(properties.Length, newTypeProperties.Length);

        //    Assert.AreEqual("F", newTypeProperties[0].Name);
        //    Assert.AreEqual("L", newTypeProperties[1].Name);

        //    Console.WriteLine(newType.Name);
        //}
    }
}
