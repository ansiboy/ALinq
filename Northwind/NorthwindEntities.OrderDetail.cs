using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="OrderDetails")]
    public partial class OrderDetail
    {
        private System.Int32 _OrderID;
        private System.Int32 _ProductID;
        private System.Decimal _UnitPrice;
        private System.Int16 _Quantity;
        private System.Single _Discount;
        private EntityRef<Order> _Order;
        private EntityRef<Product> _Product;

        public OrderDetail()
        {
            this._Order = default(EntityRef<Order>);
            this._Product = default(EntityRef<Product>);
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.Never, Name="OrderID")]
        public System.Int32 OrderID
        {
            get
            {
                return _OrderID;
            }
            set
            {
                _OrderID = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.Never, Name="ProductID")]
        public System.Int32 ProductID
        {
            get
            {
                return _ProductID;
            }
            set
            {
                _ProductID = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="UnitPrice")]
        public System.Decimal UnitPrice
        {
            get
            {
                return _UnitPrice;
            }
            set
            {
                _UnitPrice = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Quantity")]
        public System.Int16 Quantity
        {
            get
            {
                return _Quantity;
            }
            set
            {
                _Quantity = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Discount")]
        public System.Single Discount
        {
            get
            {
                return _Discount;
            }
            set
            {
                _Discount = value;
            }
        }

        [Association(Storage="_Order", ThisKey="OrderID", OtherKey="OrderID", IsForeignKey=true, Name="Order_OrderDetail")]
        public Order Order
        {
            get
            {
                return this._Order.Entity;
            }
            set
            {
                this._Order.Entity = value;
            }
        }

        [Association(Storage="_Product", ThisKey="ProductID", OtherKey="ProductID", IsForeignKey=true, Name="Product_OrderDetail")]
        public Product Product
        {
            get
            {
                return this._Product.Entity;
            }
            set
            {
                this._Product.Entity = value;
            }
        }
    }
}
