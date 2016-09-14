using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ALinq.SqlClient
{
    internal class Keywords
    {
        private readonly SortedList<string, string> list;

        internal Keywords()
        {
            list = new SortedList<string, string>(380);
        }

        internal void AddRange(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                Debug.Assert(list.ContainsKey(item) == false);
                this.list.Add(item, item);
            }
        }

        internal bool Contains(string item)
        {
            item = item.ToUpper();
            return list.ContainsKey(item);
        }
    }

    internal class Keywords<T> : Keywords where T : Keywords, new()
    {
        internal new static bool Contains(string item)
        {
            return Instance.Contains(item);
        }

        private static Keywords instance;

        static Keywords Instance
        {
            get
            {
                if (instance == null)
                    instance = new T();
                return instance;
            }
        }
    }
}
