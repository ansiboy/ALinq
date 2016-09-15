using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ALinq.Dynamic.Parsers
{
    interface IVariableCollection : IEnumerable<KeyValuePair<string, object>>
    {
        object this[string name] { get; }
        bool ContainsKey(string name);
    }

    class VariablesStore
    {
        class GlobalVariableCollection : IVariableCollection
        {
            private VariablesStore variableStore;

            public GlobalVariableCollection(VariablesStore variableStore)
            {
                this.variableStore = variableStore;
            }

            public object this[string key]
            {
                get
                {
                    var n = this.variableStore.locLink.Last;
                    while (n != null)
                    {
                        var obj = n.Value[key];
                        if (obj != null)
                            return obj;

                        n = n.Previous;
                    }

                    return null;
                }
            }

            public bool ContainsKey(string name)
            {
                var n = this.variableStore.locLink.Last;
                while (n != null)
                {
                    if (n.Value.ContainsKey(name))
                        return true;

                    n = n.Previous;
                }

                return false;
            }

            #region Implementation of IEnumerable

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                var n = this.variableStore.locLink.Last;
                while (n != null)
                {
                    foreach (var item in n.Value)
                        yield return item;

                    n = n.Previous;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        class LocalVariableCollection : IVariableCollection
        {
            private Dictionary<string, object> items;

            public LocalVariableCollection()
            {
                this.items = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            }


            public object this[string name]
            {
                get
                {
                    object value;
                    if (items.TryGetValue(name, out value))
                        return value;

                    return null;
                }
                set { items[name] = value; }
            }

            public bool ContainsKey(string name)
            {
                return items.ContainsKey(name);
            }

            #region Implementation of IEnumerable

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                return items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        private LinkedList<LocalVariableCollection> locLink;

        public VariablesStore()
        {
            locLink = new LinkedList<LocalVariableCollection>();
            this.GlobalVariables = new GlobalVariableCollection(this);
        }

        public void CreateLocalVariables()
        {
            //Dictionary<string, object> symbols = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            locLink.AddLast(new LocalVariableCollection());
            //return symbols;
        }

        public void SetLocalVariable(string name, object value)
        {
            AppendLocalVariable(name, value, false);
        }

        public void AppendLocalVariable(string name, object value, bool replaceIfExists)
        {
            //if (LocalVariables.ContainsKey(name) && !replaceIfExists)
            //    throw new Exception(String.Format("Parameter '{0}' is exists.", name));

            ((LocalVariableCollection)LocalVariables)[name] = value;
        }

        public void ReleaseLocalVariables()
        {
            Debug.Assert(locLink.Count > 0);
            locLink.RemoveLast();
        }

        public IVariableCollection LocalVariables
        {
            get
            {
                if (locLink.Count == 0)
                    return null;

                return this.locLink.Last.Value;
            }
        }

        public IVariableCollection GlobalVariables
        {
            get;
            private set;
        }


        public bool GetAvailableVariable(string variableName, out object variable)
        {
            var n = locLink.Last;
            while (n != null)
            {
                //if (n.Value.TryGetValue(variableName, out variable))
                var value = n.Value[variableName];
                if (value != null)
                {
                    variable = value;
                    return true;
                }

                n = n.Previous;
            }
            variable = null;
            return false;
        }
    }



}
