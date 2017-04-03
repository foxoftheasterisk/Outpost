using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outpost.Map
{
    public struct LoadState
    {
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
            foreach (LoadState state in collection)
            {
                result = max(result, state);
            }
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
    }
}
