using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinqToSqlShared.Mapping;

namespace LinqToSqlShared.Mapping
{
    internal sealed class AssociationMapping : MemberMapping
    {
        // Fields
        private bool deleteOnNull;
        private bool isForeignKey;
        private bool isUnique;
        private string otherKey;
        private string thisKey;

        // Methods
        internal AssociationMapping()
        {
        }

        // Properties
        internal bool DeleteOnNull
        {
            get
            {
                return this.deleteOnNull;
            }
            set
            {
                this.deleteOnNull = value;
            }
        }

        internal string DeleteRule { get; set; }

        internal bool IsForeignKey
        {
            get
            {
                return this.isForeignKey;
            }
            set
            {
                this.isForeignKey = value;
            }
        }

        internal bool IsUnique
        {
            get
            {
                return this.isUnique;
            }
            set
            {
                this.isUnique = value;
            }
        }

        internal string OtherKey
        {
            get
            {
                return this.otherKey;
            }
            set
            {
                this.otherKey = value;
            }
        }

        internal string ThisKey
        {
            get
            {
                return this.thisKey;
            }
            set
            {
                this.thisKey = value;
            }
        }

        internal string XmlDeleteOnNull
        {
            get
            {
                if (!this.deleteOnNull)
                {
                    return null;
                }
                return "true";
            }
            set
            {
                this.deleteOnNull = (value != null) ? bool.Parse(value) : false;
            }
        }

        internal string XmlIsForeignKey
        {
            get
            {
                if (!this.isForeignKey)
                {
                    return null;
                }
                return "true";
            }
            set
            {
                this.isForeignKey = (value != null) ? bool.Parse(value) : false;
            }
        }

        internal string XmlIsUnique
        {
            get
            {
                if (!this.isUnique)
                {
                    return null;
                }
                return "true";
            }
            set
            {
                this.isUnique = (value != null) ? bool.Parse(value) : false;
            }
        }
    }
}
