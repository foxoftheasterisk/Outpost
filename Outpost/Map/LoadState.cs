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
    }

    
}
