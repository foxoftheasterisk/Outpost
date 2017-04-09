using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OutpostLibrary;
using OutpostLibrary.Navigation;

namespace Outpost.Map
{
    public class MapSection : IEnumerable<Chunk>
    {
        //TODO?: make this not always load cubes?

        LoadState targetState;
        ChunkAddress center;
        int radius; //inclusive

        public MapSection(ChunkAddress _center, int _radius, LoadState _targetState)
        {
            center = _center;
            radius = _radius;
            targetState = _targetState;

            //something something register with mapManager and chunkManagers
            foreach(Chunk chunk in this)
            {
                chunk.addLoadStateRequest(targetState);
            }
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


        /// <summary>
        /// Moves the MapSection to a new location.
        /// It's safest to always use the ChunkAddress version of this function,
        /// although it incurs an additional check.
        /// </summary>
        /// <param name="newCenter"></param>
        public void Move(ChunkAddress newCenter)
        {
            if (newCenter.world != center.world)
                changeWorld(newCenter);
            else if(newCenter.position != center.position)
                Move(newCenter.position);

        }

        public void changeWorld(ChunkAddress newCenter)
        {
            foreach(Chunk chunk in this)
            {
                chunk.removeLoadStateRequest(targetState);
            }
            center = newCenter;
            foreach(Chunk chunk in this)
            {
                chunk.addLoadStateRequest(targetState);
            }
        }

        public void Move(IntVector3 newCenter)
        {
            HashSet<IntVector3> hash = new HashSet<IntVector3>();
            IntVector3 currentChunk = newCenter - new IntVector3(radius);

            while (currentChunk.Z <= center.position.Z + radius)
            {
                currentChunk.X++;
                if (currentChunk.X > center.position.X + radius)
                {
                    currentChunk.X = center.position.X - radius;
                    currentChunk.Y++;
                    if (currentChunk.Y > center.position.Y + radius)
                    {
                        currentChunk.Y = center.position.Y - radius;
                        currentChunk.Z++;
                    }
                }

                hash.Add(currentChunk);
            }

            foreach(Chunk chunk in this)
            {
                if(hash.Contains(chunk.address.position))
                {
                    hash.Remove(chunk.address.position);
                }
                else
                {
                    chunk.removeLoadStateRequest(targetState);
                }
            }

            foreach(IntVector3 iv in hash)
            {
                Chunk chunk = MapManager.Map[new ChunkAddress(center.world, iv)];
                chunk.addLoadStateRequest(targetState);
            }

            center.position = newCenter;
        }

        /// <summary>
        /// I don't know why this would be needed, BUT HERE IT IS ANYWAY.
        /// </summary>
        /// <param name="ls"></param>
        public void changeTargetLoadState(LoadState ls)
        {
            foreach(Chunk chunk in this)
            {
                chunk.removeLoadStateRequest(targetState);
                chunk.addLoadStateRequest(ls);
            }
            targetState = ls;
        }

        public IEnumerator<Chunk> GetEnumerator()
        {
            return new MapSectionIterator(this);
        }

        //what is the point of this?
        //ah well whatever
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class MapSectionIterator : IEnumerator<Chunk>
        {
            ChunkAddress center;
            int radius;

            private ChunkAddress currentChunk;

            public MapSectionIterator(MapSection parent)
            {
                center = parent.center;
                radius = parent.radius;
                currentChunk = parent.center - new IntVector3(parent.radius);
                currentChunk.position.X--;
            }

            public Chunk Current
            {
                get
                {
                    if (currentChunk.position.Z > center.position.Z + radius)
                        throw new InvalidOperationException("Overiterated a MapSectionIterator.");
                    return MapManager.Map[currentChunk];
                }
            }

            public void Dispose()
            {
                //i don't believe anything actually needs to be done here.
            }

            //and again
            //why do i need to implement it explicitly and non-explicitly???
            //oh well
            object System.Collections.IEnumerator.Current
            {
                get 
                {
                    return Current;
                }
            }

            public bool MoveNext()
            {
                currentChunk.position.X++;
                if (currentChunk.position.X > center.position.X + radius)
                {
                    currentChunk.position.X = center.position.X - radius;
                    currentChunk.position.Y++;
                    if (currentChunk.position.Y > center.position.Y + radius)
                    {
                        currentChunk.position.Y = center.position.Y - radius;
                        currentChunk.position.Z++;
                    }
                }
                if (currentChunk.position.Z > center.position.Z + radius)
                {
                    return false;
                }
                return true;
            }

            public void Reset()
            {
                currentChunk = center - new IntVector3(radius);
                currentChunk.position.X--;
            }
        }

        
    }

    
}
