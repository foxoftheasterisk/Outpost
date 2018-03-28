using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutpostEngine.SetLoading
{
    public class RegistryManager
    {

        internal RegistryManager()
        {
            registries = new Dictionary<Type, RegistryBase>();
        }

        Dictionary<Type, RegistryBase> registries;

        /// <summary>
        /// Adds a new item to the registry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set"></param>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <returns>If the item was successfully registered (currently always true)</returns>
        public bool Register<T>(SetIdentifier set, string name, T item)
        {
            Type type = typeof(T);

            Registry<T> registry;

            if(registries.TryGetValue(type, out RegistryBase rb))
            {
                registry = rb as Registry<T>;
            }
            else
            {
                registry = new Registry<T>();
                registries.Add(type, registry);
            }

            return registry.Register(set, name, item);
        }

        /// <summary>
        /// Attempts to find the item specified.
        /// If the item cannot be found under the current set, will search upwards for it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set"></param>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Get<T>(SetIdentifier set, string name, out T item)
        {
            Type type = typeof(T);

            if (!registries.TryGetValue(type, out RegistryBase rb))
            {
                //there are no items of the type registered - obviously this one won't be found
                item = default(T);
                return false;
            }
            Registry<T> registry = rb as Registry<T>;

            while (set != null)
            {
                if (registry.Get(set, name, out item))
                    return true;

                set = set.parent;
            }

            item = default(T);
            return false;
        }


        private abstract class RegistryBase
        { }

        private class Registry<T> : RegistryBase
        {
            private Dictionary<SetIdentifier, Dictionary<string, T>> registry;

            public Registry()
            {
                registry = new Dictionary<SetIdentifier, Dictionary<string, T>>();
            }

            public bool Register(SetIdentifier set, string name, T item)
            {
                if(name.Contains('.'))
                {
                    int last = name.LastIndexOf('.');
                    string itemName = name.Substring(last + 1);
                    string setName = name.Remove(last);
                    return Register(set.GetOrCreateChild(setName), itemName, item);
                }

                if (registry.TryGetValue(set, out Dictionary<string, T> setContents))
                {
                    setContents.Add(name, item);
                    return true;
                }

                Dictionary<string, T> newRegistry = new Dictionary<string, T>();
                newRegistry.Add(name, item);
                registry.Add(set, newRegistry);
                return true;
            }

            public bool Get(SetIdentifier set, string name, out T item)
            {
                if (name.Contains('.'))
                {
                    int last = name.LastIndexOf('.');
                    string itemName = name.Substring(last + 1);
                    string setName = name.Remove(last);
                    if (set.GetChild(setName, out SetIdentifier rebasedSet))
                        return Get(rebasedSet, itemName, out item);
                    else
                    {
                        item = default(T);
                        return false;
                    }
                }

                if (registry.TryGetValue(set, out Dictionary<string, T> setContents))
                    if (setContents.TryGetValue(name, out item))
                        return true;

                item = default(T);
                return false;
            }
        }
    }
}
