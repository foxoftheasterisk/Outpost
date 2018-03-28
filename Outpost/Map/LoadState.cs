using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutpostEngine.Map
{
    public struct LoadState : IEquatable<LoadState>
    {
        //careful when adding new; these enums are ordered
        public enum GraphicalLoadState { None, Low, Full }
        public enum DataLoadState { None, Full }

        public GraphicalLoadState graphical;
        public DataLoadState data;

        public LoadState(GraphicalLoadState graphicalState, DataLoadState dataState)
        {
            graphical = graphicalState;
            data = dataState;
        }

        public static LoadState max(LoadState a, LoadState b)
        {
            LoadState result = new LoadState();
            result.graphical = (GraphicalLoadState)Math.Max((int)a.graphical, (int)b.graphical);
            result.data = (DataLoadState)Math.Max((int)a.data, (int)b.data);
            return result;
        }

        public static LoadState max(IEnumerable<LoadState> collection)
        {
            LoadState result = new LoadState(0, 0);
            do
            {
                try
                {
                    foreach (LoadState state in collection)
                    {
                        result = max(result, state);
                    }
                }
                catch(System.InvalidOperationException e)
                {
                    //TODO: this seems like a bad idea... investigate better solutions
                    if (e.Message == "Collection was modified; enumeration operation may not execute.")
                        continue;
                    else
                        throw e;
                }
            }
            while (false);
            return result;
        }

        /// <summary>
        /// Returns true if ANY of a's load states are greater than b's corresponding states.
        /// Be careful! It is possible for both a > b and b > a to be true!
        /// It is also possible for a > b and a < b to be true!
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >(LoadState a, LoadState b)
        {
            if (a.data > b.data)
                return true;
            if (a.graphical > b.graphical)
                return true;
            return false;
        }

        /// <summary>
        /// Returns true if ANY of a's load states are less than b's corresponding states.
        /// Be careful! It is possible for both a < b and b < a to be true!
        /// It is also possible for a < b and a > b to be true!
        /// (However, a > b does guarantee that b < a.)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <(LoadState a, LoadState b)
        {
            if (a.data < b.data)
                return true;
            if (a.graphical < b.graphical)
                return true;
            return false;
        }

        #region iEquatable
        public override bool Equals(Object compTo)
        {
            if (compTo is LoadState)
                return Equals((LoadState)compTo);


            return false;
        }

        public bool Equals(LoadState other)
        {
            if (this.data != other.data)
                return false;
            if (this.graphical != other.graphical)
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return 3*((int)data) + 4*((int)graphical);
        }

        public static bool operator ==(LoadState a, LoadState b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(LoadState a, LoadState b)
        {
            return !(a.Equals(b));
        }

        #endregion iEquatable
    }
}
