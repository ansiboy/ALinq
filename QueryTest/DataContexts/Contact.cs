using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name = "Contacts")]
    [InheritanceMapping(Code = "Unknown", Type = typeof(Contact), IsDefault = true)]
    [InheritanceMapping(Code = "Full", Type = typeof(FullContact))]
    [InheritanceMapping(Code = "EmployeeContact", Type = typeof(EmployeeContact))]
    [InheritanceMapping(Code = "Supplier", Type = typeof(SupplierContact))]
    [InheritanceMapping(Code = "Customer", Type = typeof(CustomerContact))]
    [InheritanceMapping(Code = "Shipper", Type = typeof(ShipperContact))]
    public partial class Contact : INotifyPropertyChanging, INotifyPropertyChanged
    {

        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);

        private int _ContactID = default(int);

        private string _ContactType;

        private string _CompanyName;

        private string _Phone;

        private System.Nullable<System.Guid> _GUID;

        #region Extensibility Method Definitions
        partial void OnLoaded();
        partial void OnValidate(ALinq.ChangeAction action);
        partial void OnCreated();
        partial void OnContactTypeChanging(string value);
        partial void OnContactTypeChanged();
        partial void OnCompanyNameChanging(string value);
        partial void OnCompanyNameChanged();
        partial void OnPhoneChanging(string value);
        partial void OnPhoneChanged();
        partial void OnGUIDChanging(System.Nullable<System.Guid> value);
        partial void OnGUIDChanged();
        #endregion

        public Contact()
        {
            OnCreated();
        }

        [Column(Storage = "_ContactID", AutoSync = AutoSync.OnInsert, CanBeNull = false, IsPrimaryKey = true, IsDbGenerated = true, UpdateCheck = UpdateCheck.Never)]
        public int ContactID
        {
            get
            {
                return this._ContactID;
            }
        }

        [Column(Storage = "_ContactType", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never, IsDiscriminator = true)]
        public string ContactType
        {
            get
            {
                return this._ContactType;
            }
            set
            {
                if ((this._ContactType != value))
                {
                    this.OnContactTypeChanging(value);
                    this.SendPropertyChanging();
                    this._ContactType = value;
                    this.SendPropertyChanged("ContactType");
                    this.OnContactTypeChanged();
                }
            }
        }

        [Column(Storage = "_CompanyName", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never)]
        public string CompanyName
        {
            get
            {
                return this._CompanyName;
            }
            set
            {
                if ((this._CompanyName != value))
                {
                    this.OnCompanyNameChanging(value);
                    this.SendPropertyChanging();
                    this._CompanyName = value;
                    this.SendPropertyChanged("CompanyName");
                    this.OnCompanyNameChanged();
                }
            }
        }

        [Column(Storage = "_Phone", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never)]
        public string Phone
        {
            get
            {
                return this._Phone;
            }
            set
            {
                if ((this._Phone != value))
                {
                    this.OnPhoneChanging(value);
                    this.SendPropertyChanging();
                    this._Phone = value;
                    this.SendPropertyChanged("Phone");
                    this.OnPhoneChanged();
                }
            }
        }

        [Column(Storage = "_GUID", UpdateCheck = UpdateCheck.Never)]
        public System.Nullable<System.Guid> GUID
        {
            get
            {
                return this._GUID;
            }
            set
            {
                if ((this._GUID != value))
                {
                    this.OnGUIDChanging(value);
                    this.SendPropertyChanging();
                    this._GUID = value;
                    this.SendPropertyChanged("GUID");
                    this.OnGUIDChanged();
                }
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SendPropertyChanging()
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, emptyChangingEventArgs);
            }
        }

        protected virtual void SendPropertyChanged(String propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public partial class FullContact : Contact
    {

        private string _ContactName;

        private string _ContactTitle;

        private string _Address;

        private string _City;

        private string _Region;

        private string _PostalCode;

        private string _Country;

        private string _Fax;

        #region Extensibility Method Definitions
        partial void OnLoaded();
        partial void OnValidate(ALinq.ChangeAction action);
        partial void OnCreated();
        partial void OnContactNameChanging(string value);
        partial void OnContactNameChanged();
        partial void OnContactTitleChanging(string value);
        partial void OnContactTitleChanged();
        partial void OnAddressChanging(string value);
        partial void OnAddressChanged();
        partial void OnCityChanging(string value);
        partial void OnCityChanged();
        partial void OnRegionChanging(string value);
        partial void OnRegionChanged();
        partial void OnPostalCodeChanging(string value);
        partial void OnPostalCodeChanged();
        partial void OnCountryChanging(string value);
        partial void OnCountryChanged();
        partial void OnFaxChanging(string value);
        partial void OnFaxChanged();
        #endregion

        public FullContact()
        {
            OnCreated();
        }

        [Column(Storage = "_ContactName", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never)]
        public string ContactName
        {
            get
            {
                return this._ContactName;
            }
            set
            {
                if ((this._ContactName != value))
                {
                    this.OnContactNameChanging(value);
                    this.SendPropertyChanging();
                    this._ContactName = value;
                    this.SendPropertyChanged("ContactName");
                    this.OnContactNameChanged();
                }
            }
        }

        [Column(Storage = "_ContactTitle", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never)]
        public string ContactTitle
        {
            get
            {
                return this._ContactTitle;
            }
            set
            {
                if ((this._ContactTitle != value))
                {
                    this.OnContactTitleChanging(value);
                    this.SendPropertyChanging();
                    this._ContactTitle = value;
                    this.SendPropertyChanged("ContactTitle");
                    this.OnContactTitleChanged();
                }
            }
        }

        [Column(Storage = "_Address", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never)]
        public string Address
        {
            get
            {
                return this._Address;
            }
            set
            {
                if ((this._Address != value))
                {
                    this.OnAddressChanging(value);
                    this.SendPropertyChanging();
                    this._Address = value;
                    this.SendPropertyChanged("Address");
                    this.OnAddressChanged();
                }
            }
        }

        [Column(Storage = "_City", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never)]
        public string City
        {
            get
            {
                return this._City;
            }
            set
            {
                if ((this._City != value))
                {
                    this.OnCityChanging(value);
                    this.SendPropertyChanging();
                    this._City = value;
                    this.SendPropertyChanged("City");
                    this.OnCityChanged();
                }
            }
        }

        [Column(Storage = "_Region", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never)]
        public string Region
        {
            get
            {
                return this._Region;
            }
            set
            {
                if ((this._Region != value))
                {
                    this.OnRegionChanging(value);
                    this.SendPropertyChanging();
                    this._Region = value;
                    this.SendPropertyChanged("Region");
                    this.OnRegionChanged();
                }
            }
        }

        [Column(Storage = "_PostalCode", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never)]
        public string PostalCode
        {
            get
            {
                return this._PostalCode;
            }
            set
            {
                if ((this._PostalCode != value))
                {
                    this.OnPostalCodeChanging(value);
                    this.SendPropertyChanging();
                    this._PostalCode = value;
                    this.SendPropertyChanged("PostalCode");
                    this.OnPostalCodeChanged();
                }
            }
        }

        [Column(Storage = "_Country", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never)]
        public string Country
        {
            get
            {
                return this._Country;
            }
            set
            {
                if ((this._Country != value))
                {
                    this.OnCountryChanging(value);
                    this.SendPropertyChanging();
                    this._Country = value;
                    this.SendPropertyChanged("Country");
                    this.OnCountryChanged();
                }
            }
        }

        [Column(Storage = "_Fax", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never)]
        public string Fax
        {
            get
            {
                return this._Fax;
            }
            set
            {
                if ((this._Fax != value))
                {
                    this.OnFaxChanging(value);
                    this.SendPropertyChanging();
                    this._Fax = value;
                    this.SendPropertyChanged("Fax");
                    this.OnFaxChanged();
                }
            }
        }
    }

    public partial class EmployeeContact : FullContact
    {

        private string _PhotoPath;

        private string _Photo;

        private string _Extension;

        #region Extensibility Method Definitions
        partial void OnLoaded();
        partial void OnValidate(ALinq.ChangeAction action);
        partial void OnCreated();
        partial void OnPhotoPathChanging(string value);
        partial void OnPhotoPathChanged();
        partial void OnPhotoChanging(string value);
        partial void OnPhotoChanged();
        partial void OnExtensionChanging(string value);
        partial void OnExtensionChanged();
        #endregion

        public EmployeeContact()
        {
            OnCreated();
        }

        [Column(Storage = "_PhotoPath", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never)]
        public string PhotoPath
        {
            get
            {
                return this._PhotoPath;
            }
            set
            {
                if ((this._PhotoPath != value))
                {
                    this.OnPhotoPathChanging(value);
                    this.SendPropertyChanging();
                    this._PhotoPath = value;
                    this.SendPropertyChanged("PhotoPath");
                    this.OnPhotoPathChanged();
                }
            }
        }

        [Column(Storage = "_Photo", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never)]
        public string Photo
        {
            get
            {
                return this._Photo;
            }
            set
            {
                if ((this._Photo != value))
                {
                    this.OnPhotoChanging(value);
                    this.SendPropertyChanging();
                    this._Photo = value;
                    this.SendPropertyChanged("Photo");
                    this.OnPhotoChanged();
                }
            }
        }

        [Column(Storage = "_Extension", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never)]
        public string Extension
        {
            get
            {
                return this._Extension;
            }
            set
            {
                if ((this._Extension != value))
                {
                    this.OnExtensionChanging(value);
                    this.SendPropertyChanging();
                    this._Extension = value;
                    this.SendPropertyChanged("Extension");
                    this.OnExtensionChanged();
                }
            }
        }
    }

    public partial class SupplierContact : FullContact
    {

        private string _HomePage;

        #region Extensibility Method Definitions
        partial void OnLoaded();
        partial void OnValidate(ALinq.ChangeAction action);
        partial void OnCreated();
        partial void OnHomePageChanging(string value);
        partial void OnHomePageChanged();
        #endregion

        public SupplierContact()
        {
            OnCreated();
        }

        [Column(Storage = "_HomePage", DbType = "VarChar(40)", UpdateCheck = UpdateCheck.Never)]
        public string HomePage
        {
            get
            {
                return this._HomePage;
            }
            set
            {
                if ((this._HomePage != value))
                {
                    this.OnHomePageChanging(value);
                    this.SendPropertyChanging();
                    this._HomePage = value;
                    this.SendPropertyChanged("HomePage");
                    this.OnHomePageChanged();
                }
            }
        }
    }

    public partial class CustomerContact : FullContact
    {

        #region Extensibility Method Definitions
        partial void OnLoaded();
        partial void OnValidate(ALinq.ChangeAction action);
        partial void OnCreated();
        #endregion

        public CustomerContact()
        {
            OnCreated();
        }
    }

    public partial class ShipperContact : Contact
    {

        #region Extensibility Method Definitions
        partial void OnLoaded();
        partial void OnValidate(ALinq.ChangeAction action);
        partial void OnCreated();
        #endregion

        public ShipperContact()
        {
            OnCreated();
        }
    }
}
