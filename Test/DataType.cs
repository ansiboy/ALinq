using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{
    abstract partial class SqlTest
    {
        [TestMethod]
        public void DataType_Guid()
        {
            //db.Log = Console.Out;


            //var guid = new Guid("67a70d6f-36a7-4683-bca4-0bcd82524ef3");
            //var dataType = new DataType() { Guid = guid };
            //db.DataTypes.InsertOnSubmit(dataType);
            //db.SubmitChanges();

            //var command = db.GetCommand(db.DataTypes.Where(o => o.Guid == guid));
            var q = db.DataTypes.Select(o => new { o.Guid }).ToList();
            //Assert.IsTrue(q.Count() > 0);


            //db.Contacts.ToArray();
        }

        [TestMethod]
        public void DataType_Int64()
        {
            var dataType = new DataType { Int64 = Int64.MaxValue };
            db.DataTypes.InsertOnSubmit(dataType);
            db.SubmitChanges();

            var id = dataType.ID;
            dataType = (from item in db.DataTypes
                        where item.ID == id
                        select item).First();
            Assert.AreEqual(Int64.MaxValue, dataType.Int64);


            dataType = new DataType { Int64 = Int64.MinValue };
            db.DataTypes.InsertOnSubmit(dataType);
            db.SubmitChanges();

            id = dataType.ID;
            dataType = (from item in db.DataTypes
                        where item.ID == id
                        select item).First();
            Assert.AreEqual(Int64.MinValue, dataType.Int64);


            dataType = new DataType { Int64 = 0 };
            db.DataTypes.InsertOnSubmit(dataType);
            db.SubmitChanges();

            id = dataType.ID;
            dataType = (from item in db.DataTypes
                        where item.ID == id
                        select item).First();
            Assert.AreEqual(0, dataType.Int64);


            //db.DataTypes.DeleteAllOnSubmit(db.DataTypes);
            //db.SubmitChanges();
        }

        [TestMethod]
        public void DataType_UInt64()
        {
            DataType dataType = new DataType();
            dataType.UInt64 = UInt64.MaxValue;
            db.DataTypes.InsertOnSubmit(dataType);
            db.SubmitChanges();

            var id = dataType.ID;
            dataType = (from item in db.DataTypes
                        where item.ID == id
                        select item).First();
            Assert.AreEqual(UInt64.MaxValue, dataType.UInt64);


            dataType = new DataType();
            dataType.UInt64 = UInt64.MinValue;
            db.DataTypes.InsertOnSubmit(dataType);
            db.SubmitChanges();

            id = dataType.ID;
            dataType = (from item in db.DataTypes
                        where item.ID == id
                        select item).First();
            Assert.AreEqual(UInt64.MinValue, dataType.UInt64);


            dataType = new DataType();
            var value = new Random().Next(0, Int32.MaxValue); ;
            dataType.UInt64 = (ulong)value;
            db.DataTypes.InsertOnSubmit(dataType);
            db.SubmitChanges();

            id = dataType.ID;
            dataType = (from item in db.DataTypes
                        where item.ID == id
                        select item).First();
            Assert.AreEqual((UInt64)value, dataType.UInt64);
        }

        [TestMethod]
        public void DataType_Enum()
        {
            //var list = db.DataTypes.ToList();
            //list.ForEach(o => db.DataTypes.Attach(o));
            //db.DataTypes.DeleteAllOnSubmit(list);
            //db.SubmitChanges();
            var item = new DataType() { Enum = NorthwindDatabase.Enum.Item1 };
            db.DataTypes.InsertOnSubmit(item);
            db.SubmitChanges();
            var items = db.DataTypes.Where(o => o.Enum == NorthwindDatabase.Enum.Item1)
                                    .Select(o => o.ID)
                                    .ToList();
            Assert.IsTrue(items.Count > 0);
            db.DataTypes.Delete(o => o.ID == item.ID);
            db.SubmitChanges();
        }


        [TestMethod]
        public void DataType_Char()
        {
            db.Log = Console.Out;
            var items = db.DataTypes.Where(o => o.Char == 'a').ToList();
        }

        [TestMethod]
        public void DataType_XDocument()
        {
            var data = new DataType();
            XNamespace ns = "http://schemas.microsoft.com/linqtosql/mapping/2007";
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(ns + "Database", new XAttribute("Name", "NorthwindDatabase"), new XAttribute("Provider", "ALinq.Access.AccessDbProvider"),
                    new XElement(ns + "Table", new XAttribute("Name", "User"), new XAttribute("Member", "Users")),
                        new XElement(ns + "Type", new XAttribute("Name", "NorthwindDemo.User")),
                            new XElement(ns + "Column", new XAttribute("Name", "ID")),
                            new XElement(ns + "Column", new XAttribute("Member", "ID")))
                );
            data.XDocument = doc;
            db.DataTypes.InsertOnSubmit(data);
            db.SubmitChanges();
        }

        [TestMethod]
        public void DataType_XElement()
        {
            if (db.DataTypes.Where(o => o.XDocument != null).Count() == 0)
                DataType_XDocument();

            var doc = db.DataTypes.Where(o => o.XDocument != null)
                                  .Select(o => o.XDocument).First();
            var data = new DataType() { XElement = doc.Root };
            db.DataTypes.InsertOnSubmit(data);
            db.SubmitChanges();
        }
    }
}
