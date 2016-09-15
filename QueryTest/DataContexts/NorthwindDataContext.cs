

using System;
using System.Data.Common;
using System.Data.OleDb;
using System.Reflection;


namespace NorthwindDemo
{
    partial class NorthwindDataContext
    {
        public string GetFullName(string firstName, string lastName)
        {
            return firstName + " " + lastName;
        }

        public string Version
        {
            get { return "1.5"; }
        }
    }
}
