using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutpostEngine.SetLoading
{
    public class SetIdentifier
    {
        public readonly SetIdentifier parent;
        readonly string name;
        Dictionary<string, SetIdentifier> children;

        internal SetIdentifier(string _name)
        {
            if (_name is null)
                throw new ArgumentNullException("_name");
            if (_name.Contains('.'))
                throw new ArgumentException("_name cannot contain a period!", "_name");
            name = _name;
        }

        private SetIdentifier(SetIdentifier _parent, string _name)
        {
            parent = _parent;
            name = _name;
        }

        /// <summary>
        /// finds a set under the current set, or creates it if it doesn't exist
        /// intermediate sets can be specified; they, too, will be created if they do not yet exist
        /// </summary>
        /// <param name="childName">the name of the set to find, possibly including intermediate sets seperated by periods (.)</param>
        /// <returns>the set found or created</returns>
        public SetIdentifier GetOrCreateChild(string childName)
        {
            if (childName is null)
                throw new ArgumentNullException("childName");
            if (childName.Contains('.'))
            {
                string[] names = childName.Split(new char['.'], 2);
                SetIdentifier midChild = GetOrCreateChild(names[0]);
                return midChild.GetOrCreateChild(names[1]);
            }

            if (children.TryGetValue(childName, out SetIdentifier child))
                return child;

            SetIdentifier si = new SetIdentifier(this, childName);
            children.Add(childName, si);
            return si;
        }

        /// <summary>
        /// finds a set that is strictly a child of the current set
        /// </summary>
        /// <param name="childName">the name of the set to find, possibly including intermediate sets seperated by periods (.)</param>
        /// <param name="child">returns the set found, or null if none is</param>
        /// <returns>whether any set was found</returns>
        public bool GetChild(string childName, out SetIdentifier child)
        {
            if (childName is null)
                throw new ArgumentNullException("childName");
            if (childName.Contains('.'))
            {
                string[] names = childName.Split(new char['.'], 2);
                if (GetChild(names[0], out SetIdentifier midChild))
                    return midChild.GetChild(names[1], out child);
                else
                {
                    child = null;
                    return false;
                }
            }

            return children.TryGetValue(childName, out child);
        }

        /// <summary>
        /// attempts to find a set, searching first under this set, then under its parent, and so on until reaching the top level
        /// </summary>
        /// <param name="setName">the name of the set to find, possibly including intermediate sets seperated by periods (.)</param>
        /// <param name="set">returns the set found, or null if none is</param>
        /// <returns>whether any set was found</returns>
        public bool FindSet(string setName, out SetIdentifier set)
        {
            if (setName is null)
                throw new ArgumentNullException("setName");

            if (GetChild(setName, out set))
                return true;
            else if (parent != null)
                return parent.FindSet(setName, out set);
            else
                return false;

        }

        /// <summary>
        /// attempts to find a set, searching downward through all sets
        /// if it fails, it then searches downward through all sets under its parent, and so on to the top
        /// will find the set if it exists anywhere in the structure
        /// USE SPARINGLY - SLOW AND MAY RETURN UNEXPECTED RESULTS.
        /// Save the found set after use if at all possible.
        /// </summary>
        /// <param name="setName"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public bool SearchForSet(string setName, out SetIdentifier set)
        {
            if (setName is null)
                throw new ArgumentNullException("setName");

            //inefficient - searches down from parent, so searches this set twice
            //(more, actually, since it then searches down to the parent from its parent)

            if (GetChild(setName, out set))
                return true;
            else if (SearchDown(setName, out set))
                return true;
            else if (parent != null)
                return parent.SearchForSet(setName, out set);
            else
                return false;

        }

        private bool SearchDown(string setName, out SetIdentifier set)
        {
            foreach(KeyValuePair<string, SetIdentifier> pair in children)
            {
                SetIdentifier child = pair.Value;
                if (child.GetChild(setName, out set))
                    return true;
            }
            foreach (KeyValuePair<string, SetIdentifier> pair in children)
            {
                SetIdentifier child = pair.Value;
                if (child.SearchDown(setName, out set))
                    return true;
            }
            set = null;
            return false;
        }

        /// <summary>
        /// Returns the fully qualified name of this set.
        /// </summary>
        /// <returns></returns>
        public string GetFullName()
        {
            if (parent != null)
                return parent.GetFullName() + name;
            else
                return name;
        }
    }
}
