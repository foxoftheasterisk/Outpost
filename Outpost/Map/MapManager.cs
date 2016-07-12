﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OutpostLibrary.Navigation;

namespace Outpost.Map
{
    public class MapManager
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

        public static void CreateMap()
        {
            //hopefully, this shouldn't come up
            if (_map != null)
                DisposeMap();

            _map = new MapManager();
        }

        public static void DisposeMap()
        {
            foreach(KeyValuePair<ChunkAddress, Chunk> pair in _map.chunkStore)
            {
                Chunk chunk = pair.Value;
                
                //TODO: save and dispose all chunks
                chunk.ForceUnload();
            }
        }

        private Dictionary<ChunkAddress, Chunk> chunkStore;

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
        

    }
}