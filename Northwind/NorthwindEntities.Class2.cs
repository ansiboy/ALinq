using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="Class2s")]
    public partial class Class2
    {
        private System.Int32 _ID;
        private System.Int32 _ID1;
        private System.Int32 _ID2;
        private EntityRef<Class1> _Class1;

        public Class2()
        {
            this._Class1 = default(EntityRef<Class1>);
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.OnInsert, IsDbGenerated=true, Name="ID")]
        public System.Int32 ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ID1")]
        public System.Int32 ID1
        {
            get
            {
                return _ID1;
            }
            set
            {
                _ID1 = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ID2")]
        public System.Int32 ID2
        {
            get
            {
                return _ID2;
            }
            set
            {
                _ID2 = value;
            }
        }

        [Association(Storage="_Class1", ThisKey="ID1,ID2", OtherKey="ID1,ID2", IsForeignKey=true, Name="Class1_Class2")]
        public Class1 Class1
        {
            get
            {
                return this._Class1.Entity;
            }
            set
            {
                this._Class1.Entity = value;
            }
        }
    }
}
