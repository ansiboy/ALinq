using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="Suppliers")]
    public partial class Supplier
    {
        private System.Int32 _SupplierID;
        private System.String _CompanyName;
        private System.String _ContactName;
        private System.String _ContactTitle;
        private System.String _Address;
        private System.String _City;
        private System.String _Region;
        private System.String _PostalCode;
        private System.String _Country;
        private System.String _Phone;
        private System.String _Fax;
        private System.String _HomePage;
        private EntitySet<Product> _Products;

        public Supplier()
        {
            this._Products = new EntitySet<Product>();
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.OnInsert, IsDbGenerated=true, Name="SupplierID")]
        public System.Int32 SupplierID
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

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="CompanyName", DbType="VarChar(40)")]
        public System.String CompanyName
        {
            get
            {
                return _CompanyName;
            }
            set
            {
                _CompanyName = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ContactName", DbType="VarChar(30)")]
        public System.String ContactName
        {
            get
            {
                return _ContactName;
            }
            set
            {
                _ContactName = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ContactTitle", DbType="VarChar(30)")]
        public System.String ContactTitle
        {
            get
            {
                return _ContactTitle;
            }
            set
            {
                _ContactTitle = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Address", DbType="VarChar(60)")]
        public System.String Address
        {
            get
            {
                return _Address;
            }
            set
            {
                _Address = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="City", DbType="VarChar(15)")]
        public System.String City
        {
            get
            {
                return _City;
            }
            set
            {
                _City = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Region", DbType="VarChar(15)")]
        public System.String Region
        {
            get
            {
                return _Region;
            }
            set
            {
                _Region = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="PostalCode", DbType="VarChar(10)")]
        public System.String PostalCode
        {
            get
            {
                return _PostalCode;
            }
            set
            {
                _PostalCode = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Country", DbType="VarChar(15)")]
        public System.String Country
        {
            get
            {
                return _Country;
            }
            set
            {
                _Country = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Phone", DbType="VarChar(24)")]
        public System.String Phone
        {
            get
            {
                return _Phone;
            }
            set
            {
                _Phone = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Fax", DbType="VarChar(24)")]
        public System.String Fax
        {
            get
            {
                return _Fax;
            }
            set
            {
                _Fax = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="HomePage")]
        public System.String HomePage
        {
            get
            {
                return _HomePage;
            }
            set
            {
                _HomePage = value;
            }
        }

        [Association(Storage="_Products", ThisKey="SupplierID", OtherKey="SupplierID", IsForeignKey=false, Name="Supplier_Product")]
        public EntitySet<Product> Products
        {
            get
            {
                return this._Products;
            }
            set
            {
                this._Products.Assign(value);
            }
        }
    }
}
