using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="Employees")]
    public partial class Employee
    {
        private System.Int32 _EmployeeID;
        private System.String _LastName;
        private System.String _FirstName;
        private System.String _Title;
        private System.String _TitleOfCourtesy;
        private Nullable<System.DateTime> _BirthDate;
        private Nullable<System.DateTime> _HireDate;
        private System.String _Address;
        private System.String _City;
        private System.String _Region;
        private System.String _PostalCode;
        private System.String _Country;
        private System.String _HomePhone;
        private System.String _Extension;
        private System.Byte[] _Photo;
        private System.String _Notes;
        private Nullable<System.Int32> _ReportsTo;
        private System.String _PhotoPath;
        private EntitySet<Employee> _Employees;
        private EntitySet<EmployeeTerritory> _EmployeeTerritories;
        private EntitySet<Order> _Orders;
        private EntityRef<Employee> _ReportsToEmployee;

        public Employee()
        {
            this._Employees = new EntitySet<Employee>();
            this._EmployeeTerritories = new EntitySet<EmployeeTerritory>();
            this._Orders = new EntitySet<Order>();
            this._ReportsToEmployee = default(EntityRef<Employee>);
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.OnInsert, IsDbGenerated=true, Name="EmployeeID")]
        public System.Int32 EmployeeID
        {
            get
            {
                return _EmployeeID;
            }
            set
            {
                _EmployeeID = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="LastName", DbType="VarChar(20)")]
        public System.String LastName
        {
            get
            {
                return _LastName;
            }
            set
            {
                _LastName = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="FirstName", DbType="VarChar(10)")]
        public System.String FirstName
        {
            get
            {
                return _FirstName;
            }
            set
            {
                _FirstName = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Title", DbType="VarChar(30)")]
        public System.String Title
        {
            get
            {
                return _Title;
            }
            set
            {
                _Title = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="TitleOfCourtesy", DbType="VarChar(25)")]
        public System.String TitleOfCourtesy
        {
            get
            {
                return _TitleOfCourtesy;
            }
            set
            {
                _TitleOfCourtesy = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="BirthDate")]
        public Nullable<System.DateTime> BirthDate
        {
            get
            {
                return _BirthDate;
            }
            set
            {
                _BirthDate = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="HireDate")]
        public Nullable<System.DateTime> HireDate
        {
            get
            {
                return _HireDate;
            }
            set
            {
                _HireDate = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Address", DbType="VarChar(60)")]
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="PostalCode", DbType="VarChar(10)")]
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="HomePhone", DbType="VarChar(24)")]
        public System.String HomePhone
        {
            get
            {
                return _HomePhone;
            }
            set
            {
                _HomePhone = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Extension", DbType="VarChar(4)")]
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Photo")]
        public System.Byte[] Photo
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Notes")]
        public System.String Notes
        {
            get
            {
                return _Notes;
            }
            set
            {
                _Notes = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ReportsTo")]
        public Nullable<System.Int32> ReportsTo
        {
            get
            {
                return _ReportsTo;
            }
            set
            {
                _ReportsTo = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="PhotoPath", DbType="VarChar(255)")]
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

        [Association(Storage="_Employees", ThisKey="EmployeeID", OtherKey="ReportsTo", IsForeignKey=false, Name="Employee_Employee")]
        public EntitySet<Employee> Employees
        {
            get
            {
                return this._Employees;
            }
            set
            {
                this._Employees.Assign(value);
            }
        }

        [Association(Storage="_EmployeeTerritories", ThisKey="EmployeeID", OtherKey="EmployeeID", IsForeignKey=false, Name="Employee_EmployeeTerritory")]
        public EntitySet<EmployeeTerritory> EmployeeTerritories
        {
            get
            {
                return this._EmployeeTerritories;
            }
            set
            {
                this._EmployeeTerritories.Assign(value);
            }
        }

        [System.Web.Script.Serialization.ScriptIgnore()]
        [Association(Storage="_Orders", ThisKey="EmployeeID", OtherKey="EmployeeID", IsForeignKey=false, Name="Employee_Order")]
        public EntitySet<Order> Orders
        {
            get
            {
                return this._Orders;
            }
            set
            {
                this._Orders.Assign(value);
            }
        }

        [Association(Storage="_ReportsToEmployee", ThisKey="ReportsTo", OtherKey="EmployeeID", IsForeignKey=true, Name="Employee_Employee")]
        public Employee ReportsToEmployee
        {
            get
            {
                return this._ReportsToEmployee.Entity;
            }
            set
            {
                this._ReportsToEmployee.Entity = value;
            }
        }
    }
}
