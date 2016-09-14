using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="Class1s")]
    public partial class Class1
    {
        private System.Int32 _ID1;
        private System.Int32 _ID2;
        private EntitySet<Class2> _Class2s;

        public Class1()
        {
            this._Class2s = new EntitySet<Class2>();
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.Never, Name="ID1")]
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

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.Never, Name="ID2")]
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

        [Association(Storage="_Class2s", ThisKey="ID1,ID2", OtherKey="ID1,ID2", IsForeignKey=false, Name="Class1_Class2")]
        public EntitySet<Class2> Class2s
        {
            get
            {
                return this._Class2s;
            }
            set
            {
                this._Class2s.Assign(value);
            }
        }
    }
}
