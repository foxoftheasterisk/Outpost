using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OutpostLibrary;
using OutpostLibrary.Navigation;

namespace Outpost.Map
{
    class MapSection
    {
        //TODO?: make this not always load cubes?

        LoadState targetState;
        ChunkAddress center;
        int radius;

        public MapSection(ChunkAddress _center, int _radius, LoadState _targetState)
        {
            center = _center;
            radius = _radius;
            targetState = _targetState;

            //something something register with mapManager and chunkManagers
        }

        public MapSection(string world, IntVector3 _center, int _radius, LoadState _targetState) 
            : this(new ChunkAddress(world, _center), _radius, _targetState)
        { }

        /// <summary>
        /// WARNING: In this constructor _center is in worldspace!
        /// To construct with chunk coordinates, use an IntVector3 or ChunkAddress.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="_center"></param>
        /// <param name="_radius"></param>
        /// <param name="graphical"></param>
        public MapSection(string world, Microsoft.Xna.Framework.Vector3 _center, int _radius, LoadState _targetState)
            : this(new ChunkAddress(world, _center), _radius, _targetState)
        { }


        public void Move(ChunkAddress newCenter)
        {
            if (newCenter.world != center.world)
                changeWorld(newCenter);
            else
                Move(newCenter.position);

        }

        public void changeWorld(ChunkAddress newCenter)
        {

        }

        public void Move(IntVector3 newCenter)
        {

        }
    }
}
