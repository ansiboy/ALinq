using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [System.ComponentModel.DataAnnotations.ScaffoldTableAttribute(false)]
    [Table(Name="Contacts")]
    [InheritanceMapping(Code="Unknown", Type=typeof(Contact), IsDefault=true)]
    [InheritanceMapping(Code="Shipper", Type=typeof(ShipperContact), IsDefault=false)]
    [InheritanceMapping(Code="Full", Type=typeof(FullContact), IsDefault=false)]
    [InheritanceMapping(Code="Customer", Type=typeof(CustomerContact), IsDefault=false)]
    [InheritanceMapping(Code="Supplier", Type=typeof(SupplierContact), IsDefault=false)]
    [InheritanceMapping(Code="EmployeeContact", Type=typeof(EmployeeContact), IsDefault=false)]
    public partial class Contact
    {
        private System.Int32 _ContactID;
        private System.String _ContactType;
        private System.String _CompanyName;
        private System.String _Phone;
        private Nullable<System.Guid> _GUID;

        public Contact()
        {
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.OnInsert, IsDbGenerated=true, Name="ContactID")]
        public System.Int32 ContactID
        {
            get
            {
                return _ContactID;
            }
            set
            {
                _ContactID = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ContactType", DbType="VarChar(40)", IsDiscriminator=true)]
        public System.String ContactType
        {
            get
            {
                return _ContactType;
            }
            set
            {
                _ContactType = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="CompanyName", DbType="VarChar(40)")]
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Phone", DbType="VarChar(40)")]
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="GUID")]
        public Nullable<System.Guid> GUID
        {
            get
            {
                return _GUID;
            }
            set
            {
                _GUID = value;
            }
        }
    }

    public partial class FullContact : Contact
    {
        private System.String _ContactName;
        private System.String _ContactTitle;
        private System.String _Address;
        private System.String _City;
        private System.String _Region;
        private System.String _PostalCode;
        private System.String _Country;
        private System.String _Fax;

        public FullContact()
        {
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ContactName", DbType="VarChar(40)")]
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ContactTitle", DbType="VarChar(40)")]
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Address", DbType="VarChar(40)")]
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="City", DbType="VarChar(40)")]
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Region", DbType="VarChar(40)")]
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="PostalCode", DbType="VarChar(40)")]
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Country", DbType="VarChar(40)")]
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Fax", DbType="VarChar(40)")]
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
    }

    public partial class EmployeeContact : FullContact
    {
        private System.String _PhotoPath;
        private System.String _Photo;
        private System.String _Extension;

        public EmployeeContact()
        {
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="PhotoPath", DbType="VarChar(40)")]
        public System.String PhotoPath
        {
            get
            {
                return _PhotoPath;
            }
            set
            {
                _PhotoPath = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Photo", DbType="VarChar(40)")]
        public System.String Photo
        {
            get
            {
                return _Photo;
            }
            set
            {
                _Photo = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Extension", DbType="VarChar(40)")]
        public System.String Extension
        {
            get
            {
                return _Extension;
            }
            set
            {
                _Extension = value;
            }
        }
    }

    public partial class SupplierContact : FullContact
    {
        private System.String _HomePage;

        public SupplierContact()
        {
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="HomePage", DbType="VarChar(40)")]
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
    }

    public partial class CustomerContact : FullContact
    {

        public CustomerContact()
        {
        }
    }

    public partial class ShipperContact : Contact
    {

        public ShipperContact()
        {
        }
    }
}
