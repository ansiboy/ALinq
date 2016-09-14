using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="Categories")]
    public partial class Category
    {
        private System.Int32 _CategoryID;
        private System.String _CategoryName;
        private System.String _Description;
        private ALinq.Binary _Picture;
        private EntitySet<Product> _Products;

        public Category()
        {
            this._Products = new EntitySet<Product>();
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.OnInsert, IsDbGenerated=true, Name="CategoryID")]
        public System.Int32 CategoryID
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

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="CategoryName", DbType="VarChar(15)")]
        public System.String CategoryName
        {
            get
            {
                return _CategoryName;
            }
            set
            {
                _CategoryName = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Description")]
        public System.String Description
        {
            get
            {
                return _Description;
            }
            set
            {
                _Description = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Picture")]
        public ALinq.Binary Picture
        {
            get
            {
                return _Picture;
            }
            set
            {
                _Picture = value;
            }
        }

        [Association(Storage="_Products", ThisKey="CategoryID", OtherKey="CategoryID", IsForeignKey=false, Name="Category_Product")]
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
