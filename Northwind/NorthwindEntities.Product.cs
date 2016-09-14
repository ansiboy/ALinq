using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="Products")]
    public partial class Product
    {
        private System.Int32 _ProductID;
        private System.String _ProductName;
        private Nullable<System.Int32> _SupplierID;
        private Nullable<System.Int32> _CategoryID;
        private System.String _QuantityPerUnit;
        private Nullable<System.Decimal> _UnitPrice;
        private Nullable<System.Int16> _UnitsInStock;
        private Nullable<System.Int16> _UnitsOnOrder;
        private Nullable<System.Int16> _ReorderLevel;
        private System.Boolean _Discontinued;
        private EntitySet<OrderDetail> _OrderDetails;
        private EntityRef<Category> _Category;
        private EntityRef<Supplier> _Supplier;

        public Product()
        {
            this._OrderDetails = new EntitySet<OrderDetail>();
            this._Category = default(EntityRef<Category>);
            this._Supplier = default(EntityRef<Supplier>);
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.OnInsert, IsDbGenerated=true, Name="ProductID")]
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

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ProductName", DbType="VarChar(40)")]
        public System.String ProductName
        {
            get
            {
                return _ProductName;
            }
            set
            {
                _ProductName = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="SupplierID")]
        public Nullable<System.Int32> SupplierID
        {
            get
            {
                return _SupplierID;
            }
            set
            {
                _SupplierID = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="CategoryID")]
        public Nullable<System.Int32> CategoryID
        {
            get
            {
                return _CategoryID;
            }
            set
            {
                _CategoryID = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="QuantityPerUnit", DbType="VarChar(20)")]
        public System.String QuantityPerUnit
        {
            get
            {
                return _QuantityPerUnit;
            }
            set
            {
                _QuantityPerUnit = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="UnitPrice")]
        public Nullable<System.Decimal> UnitPrice
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="UnitsInStock")]
        public Nullable<System.Int16> UnitsInStock
        {
            get
            {
                return _UnitsInStock;
            }
            set
            {
                _UnitsInStock = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="UnitsOnOrder")]
        public Nullable<System.Int16> UnitsOnOrder
        {
            get
            {
                return _UnitsOnOrder;
            }
            set
            {
                _UnitsOnOrder = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ReorderLevel")]
        public Nullable<System.Int16> ReorderLevel
        {
            get
            {
                return _ReorderLevel;
            }
            set
            {
                _ReorderLevel = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Discontinued")]
        public System.Boolean Discontinued
        {
            get
            {
                return _Discontinued;
            }
            set
            {
                _Discontinued = value;
            }
        }

        [Association(Storage="_OrderDetails", ThisKey="ProductID", OtherKey="ProductID", IsForeignKey=false, Name="Product_OrderDetail")]
        public EntitySet<OrderDetail> OrderDetails
        {
            get
            {
                return this._OrderDetails;
            }
            set
            {
                this._OrderDetails.Assign(value);
            }
        }

        [Association(Storage="_Category", ThisKey="CategoryID", OtherKey="CategoryID", IsForeignKey=true, Name="Category_Product")]
        public Category Category
        {
            get
            {
                return this._Category.Entity;
            }
            set
            {
                this._Category.Entity = value;
            }
        }

        [Association(Storage="_Supplier", ThisKey="SupplierID", OtherKey="SupplierID", IsForeignKey=true, Name="Supplier_Product")]
        public Supplier Supplier
        {
            get
            {
                return this._Supplier.Entity;
            }
            set
            {
                this._Supplier.Entity = value;
            }
        }
    }
}
