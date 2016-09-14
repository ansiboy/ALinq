using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{
    [TestClass]
    public class DB2Test : SqlTest
    {
        public override NorthwindDemo.NorthwindDatabase CreateDataBaseInstace()
        {
            return new DB2Northwind() { Log = Console.Out };
        }

        [TestMethod]
        public void CRUD_Insert2()
        {
            var db = base.db as DB2Northwind;
            db.Log = Console.Out;

            db.DataTypes.Insert(o => new DataType { Guid = Guid.NewGuid(), Enum = NorthwindDatabase.Enum.Item1, DateTime = db.Now() });
            var id = db.DataTypes.Max(o => o.ID) + 1;
            db.DataTypes.Insert(o => new DataType { ID = id, Guid = Guid.NewGuid(), Enum = NorthwindDatabase.Enum.Item1 });
        }

        [TestMethod]
        public void Bitwise()
        {
            var q = db.DataTypes.Select(o => o.ID & 1);
            var command = db.GetCommand(q);
            Console.WriteLine(command.CommandText);

            q = db.DataTypes.Select(o => o.ID | 1);//OR
            command = db.GetCommand(q);
            Console.WriteLine(command.CommandText);

            q = db.DataTypes.Select(o => ~o.ID);//NOT
            command = db.GetCommand(q);
            Console.WriteLine(command.CommandText);

            q = db.DataTypes.Select(o => o.ID ^ 1);//XOR 
            command = db.GetCommand(q);
            Console.WriteLine(command.CommandText);

            //q = db.DataTypes.Select(o => o.ID << 1);
            //command = db.GetCommand(q);
            //Console.WriteLine(command.CommandText);
        }
    }
}
