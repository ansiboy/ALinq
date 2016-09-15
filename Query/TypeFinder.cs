using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ALinq.Dynamic
{
    class TypeFinder
    {
        private ReaderWriterLock @lock = new ReaderWriterLock();
        private Dictionary<string, Type> types;

        //public static TypeFinder Instance = new TypeFinder();

        public TypeFinder()
        {
            types = new Dictionary<string, Type>();
        }

        internal Type FindType(string name, IEnumerable<string> namespaces)
        {
            Type type;
            foreach (var ns in namespaces)
            {
                type = this.FindType(name, ns);
                if (type != null)
                    return type;
            }

            type = FindType(name, null as string);

            return type;
        }

        private Type FindType(string name, string ns)
        {
            Type type = null;
            string key = null;
            Type type2 = null;
            this.@lock.AcquireReaderLock(-1);
            try
            {
                if (!this.types.TryGetValue(name, out type))
                {
                    key = name.Contains(".") ? null : (ns + "." + name);
                    if ((key == null) || !this.types.TryGetValue(key, out type))
                    {
                        goto Label_0069;
                    }
                }
                return type;
            }
            finally
            {
                this.@lock.ReleaseReaderLock();
            }
        Label_0069:
            if (key != null)
                type2 = SearchForType(key);

            if (type2 == null)
                type2 = SearchForType(name);

            if (type2 != null)
            {
                this.@lock.AcquireWriterLock(-1);
                try
                {
                    if (this.types.TryGetValue(name, out type))
                    {
                        return type;
                    }
                    this.types.Add(name, type2);
                    return type2;
                }
                finally
                {
                    this.@lock.ReleaseWriterLock();
                }
            }
            return null;
        }

        private Type SearchForType(string name)
        {
            Type type = SearchForType(name, false);
            return type;
        }

        private Type SearchForType(string name, bool ignoreCase)
        {
            Type type = Type.GetType(name, false, ignoreCase);
            if (type != null)
            {
                return type;
            }

            var exactMatch = name.Contains(".");

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Module module2 in assembly.GetLoadedModules())
                {
                    if (exactMatch)
                    {
                        type = module2.GetType(name, false, ignoreCase);
                    }
                    else
                    {
                        try
                        {
                            var types = module2.FindTypes(Module.FilterTypeName, name);
                            //Ambiguous type reference
                            if (types.Count() > 1)
                            {
                                throw Error.AmbiguousTypeReference(name, types[0].Namespace, types[1].Namespace);
                            }
                            type = types.FirstOrDefault();
                        }
                        catch (EntitySqlException exc)
                        {
                            throw exc;
                        }
                        catch (ReflectionTypeLoadException exc)
                        {

                        }
                    }
                    if (type != null)
                    {
                        return type;
                    }
                }
            }
            return null;
        }

    }
}
