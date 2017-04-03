using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

using OutpostLibrary.Navigation;

namespace Outpost.Map
{
    public class MapManager : IDisposable
    {



        //okay, so, I need to figure a way to manage what chunks are loaded or not
        //such that:
        //*each chunk is never loaded more than once
        //*chunks are loaded and unloaded properly
        //*chunks are easily accessed from... well wherever really
        //*chunks are properly loaded and unloaded into graphical state when needed
        //*chunks that don't exist yet are put into a queue for generation (preferably in some priority order)

        //actually MapStructures should probably be handled in a similar way, since they can be in multiple chunks.

        //of course then there's the question of possible other semi-load modes
        //but the graphical/no solution should extend for those too.

        //the big questions is, are the chunks stored here in the MapManager,
        //or in the individual MapSections?

        //probably in MapManager

        //...should all the map management be in Lua?
        //......nah.

        public static MapManager map
        {
            get 
            {
                return _map;
            }
        }
        private static MapManager _map;

        private MapManager()
        {
            chunkStore = new ConcurrentDictionary<ChunkAddress,Chunk>();
            loadQueue = new ConcurrentQueue<Chunk>();
            unloadQueue = new ConcurrentQueue<Chunk>();

            loadThread = new Thread(loadLoop);
            loadThread.Priority = ThreadPriority.BelowNormal;
            loadThread.Start();
            unloadThread = new Thread(unloadLoop);
            unloadThread.Priority = ThreadPriority.Lowest;
            unloadThread.Start();
        }

        public static void CreateMap()
        {
            //hopefully, this shouldn't come up
            if (_map != null)
                DisposeMap();

            _map = new MapManager();
        }

        public static void DisposeMap()
        {
            map.beginDispose();
            lock (_map.chunkStore)
            {
                foreach (KeyValuePair<ChunkAddress, Chunk> pair in _map.chunkStore)
                {
                    Chunk chunk = pair.Value;

                    //TODO: save and dispose all chunks
                    chunk.forceUnload();
                    chunk.Dispose();
                }
            }
            
        }

        private ConcurrentDictionary<ChunkAddress, Chunk> chunkStore;

        public Chunk this[ChunkAddress ca]
        {
            get
            {
                Chunk chunk = chunkStore[ca];
                if(chunk != null)
                    return chunkStore[ca];

                //TODO: work out what exactly goes here
                //cause it's not this
                //NEEDS TO HAVE: ChunkAddress, LoadStateManager (with no load states)
                return null;
            }
        }


        private ConcurrentQueue<Chunk> loadQueue;
        private ConcurrentQueue<Chunk> unloadQueue;
        Thread loadThread;
        Thread unloadThread;
        AutoResetEvent loadAdded;
        AutoResetEvent unloadAdded;
        bool disposing = false;

        public void registerToLoad(Chunk c)
        {
            loadQueue.Enqueue(c);
            loadAdded.Set();
        }

        public void registerToUnload(Chunk c)
        {
            unloadQueue.Enqueue(c);
            unloadAdded.Set();
        }

        void loadLoop()
        {
            if (loadQueue.IsEmpty)
                loadAdded.WaitOne();

            while(!disposing)
            {
                Chunk c;
                bool succeeded = loadQueue.TryDequeue(out c);

                if (!succeeded)
                {
                    Logger.Log("Load dequeue failed????");
                    break;
                }

                c.doLoad();

                if (loadQueue.IsEmpty)
                    loadAdded.WaitOne();
            }
        }

        void unloadLoop()
        {
            if (unloadQueue.IsEmpty)
                unloadAdded.WaitOne();

            while (!disposing)
            {
                Chunk c;
                bool succeeded = unloadQueue.TryDequeue(out c);

                if (!succeeded)
                {
                    Logger.Log("Unload dequeue failed????");
                    break;
                }

                c.doUnload();

                if (unloadQueue.IsEmpty)
                    unloadAdded.WaitOne();
            }
        }

        public void beginDispose()
        {
            disposing = true;
            loadAdded.Set();
            unloadAdded.Set();
        }
    }
}
