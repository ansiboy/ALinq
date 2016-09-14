using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="EmployeeTerritories")]
    public partial class EmployeeTerritory
    {
        private System.Int32 _EmployeeID;
        private System.String _TerritoryID;
        private EntityRef<Employee> _Employee;
        private EntityRef<Territory> _Territory;

        public EmployeeTerritory()
        {
            this._Employee = default(EntityRef<Employee>);
            this._Territory = default(EntityRef<Territory>);
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.Never, Name="EmployeeID")]
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

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.Never, Name="TerritoryID", DbType="VarChar(20)")]
        public System.String TerritoryID
        {
            get
            {
                return _TerritoryID;
            }
            set
            {
                _TerritoryID = value;
            }
        }

        [Association(Storage="_Employee", ThisKey="EmployeeID", OtherKey="EmployeeID", IsForeignKey=true, Name="Employee_EmployeeTerritory")]
        public Employee Employee
        {
            get
            {
                return this._Employee.Entity;
            }
            set
            {
                this._Employee.Entity = value;
            }
        }

        [Association(Storage="_Territory", ThisKey="TerritoryID", OtherKey="TerritoryID", IsForeignKey=true, Name="Territory_EmployeeTerritory")]
        public Territory Territory
        {
            get
            {
                return this._Territory.Entity;
            }
            set
            {
                this._Territory.Entity = value;
            }
        }
    }
}
