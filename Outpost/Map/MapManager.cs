using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

using OutpostLibrary.Navigation;

namespace OutpostCore.Map
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

        

        public static MapManager Map
        {
            get 
            {
                return _Map;
            }
        }
        private static MapManager _Map;

        private MapManager(string folder)
        {
            worldFolder = folder;

            chunkStore = new ConcurrentDictionary<ChunkAddress,Chunk>();
            loadQueue = new ConcurrentQueue<Chunk>();
            unloadQueue = new ConcurrentQueue<Chunk>();

            loadAdded = new AutoResetEvent(false);
            unloadAdded = new AutoResetEvent(false);

            loadThread = new Thread(loadLoop);
            loadThread.Priority = ThreadPriority.BelowNormal;
            loadThread.Start();
            unloadThread = new Thread(unloadLoop);
            unloadThread.Priority = ThreadPriority.Lowest;
            unloadThread.Start();
        }

        public static void CreateMap(string folder)
        {
            //hopefully, this shouldn't come up
            if (_Map != null)
                DisposeMap();

            _Map = new MapManager(folder);
        }

        public void Dispose()
        {
            DisposeMap();
        }

        public static void DisposeMap()
        {
            _Map.beginDispose();
            lock (_Map.chunkStore)
            {
                foreach (KeyValuePair<ChunkAddress, Chunk> pair in _Map.chunkStore)
                {
                    Chunk chunk = pair.Value;

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
                try
                {
                    Chunk chunk = chunkStore[ca];
                    if(chunk != null)
                        return chunkStore[ca];

                    return null;
                }
                catch (KeyNotFoundException e)
                {
                    //that just means that chunk isn't loaded, certainly nothing to crash over
                    return null;
                }
                
                //TODO: if it doesn't exist, create shell and return that
                //maybe with a pattern, but we'll worry about that later
                //we could throw the empty shell in the dictionary but why the hell would we it has no information

                //no. this is a get. don't make it do things, even just loading. we'll have to check for null at the other end.

                //ofc, now Chunk (AssignNeighbors and GenerateVertices, possibly others) relies on it passing null
                //so, we'll need to overhaul that before changing this

                //okay, problem: we need the GraphicsDevice
                //i think we should probably just make that thing public i mean really
                //return new Chunk(ca, )
            }
        }

        public Chunk GetOrAdd(Chunk c)
        {
            return chunkStore.GetOrAdd(c.address, c);
        }

        public string worldFolder;

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
                bool succeeded = loadQueue.TryDequeue(out Chunk c);

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


        //wait, is there actually any difference between new and load map at the map manager level?
        //i think it might just not care
        //i mean it doesn't handle mapgen (?)
        /*
        public static void newMap(string folder)
        {

        }

        //TODO: this
        public static void loadMap(string folder)
        {

        }

        //*/

        //TODO: also this
        public void saveAndQuit()
        {

        }

        
    }

}
