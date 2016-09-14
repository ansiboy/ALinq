using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Provider(typeof(ALinq.EffiProz.EfzProvider))]
    public class EfzNorthwind : NorthwindDemo.NorthwindDatabase
    {
        public EfzNorthwind(string connection, MappingSource mapping)
            : base(connection, mapping)
        {
        }

        public EfzNorthwind(string connection)
            : base(connection, GetMapping())
        {
        }

        private static MappingSource GetMapping()
        {
            var mapping = XmlMappingSource.FromStream(typeof(EfzNorthwind).Assembly.GetManifestResourceStream("NorthwindDemo.Northwind.Efz.map"));
            return mapping;
        }


        protected override void ImportData()
        {
            var data = new NorthwindData();

            this.Regions.InsertAllOnSubmit(data.regions);
            this.Employees.InsertAllOnSubmit(data.employees);

            this.Territories.InsertAllOnSubmit(data.territories);
            this.EmployeeTerritories.InsertAllOnSubmit(data.employeeTerritories.Select(o => new EmployeeTerritory { EmployeeID = o.EmployeeID - 1, TerritoryID = o.TerritoryID }));

            this.Customers.InsertAllOnSubmit(data.customers);
            this.Shippers.InsertAllOnSubmit(data.shippers);

            this.Categories.InsertAllOnSubmit(data.categories);
            this.Suppliers.InsertAllOnSubmit(data.suppliers);

            foreach (var product in data.products)
                product.SupplierID = product.SupplierID - 1;
            this.Products.InsertAllOnSubmit(data.products);

            foreach (var order in data.orders)
            {
                if (order.ShipVia == null)
                    continue;
                order.EmployeeID = order.EmployeeID - 1;
                order.ShipVia = order.ShipVia - 1;
            }
            this.Orders.InsertAllOnSubmit(data.orders);

            foreach (var orderDetail in data.orderDetails)
                orderDetail.ProductID = orderDetail.ProductID - 1;
            this.OrderDetails.InsertAllOnSubmit(data.orderDetails);
            /**/
            this.SubmitChanges();
        }
    }
}
