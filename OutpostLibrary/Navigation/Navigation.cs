using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using OutpostLibrary;

namespace OutpostLibrary.Navigation
{
    

    public class Sizes
    {
        public const int ChunkSize = 8;
        public const int VoxelsPerEdge = 5;
    }

    public class Directions
    {
        /*
         * +X = north
         * -X = south
         * +Y = up
         * -Y = down
         * +Z = east
         * -Z = west
         */

        public enum CompassDirection { N, S, U, D, E, W }
        public static IntVector3 north = new IntVector3(1, 0, 0);
        public static IntVector3 south = new IntVector3(-1, 0, 0);
        public static IntVector3 up = new IntVector3(0, 1, 0);
        public static IntVector3 down = new IntVector3(0, -1, 0);
        public static IntVector3 east = new IntVector3(0, 0, 1);
        public static IntVector3 west = new IntVector3(0, 0, -1);
    }

    
}
