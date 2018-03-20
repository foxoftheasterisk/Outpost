using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.IO;
using OutpostLibrary;
using OutpostLibrary.Content;
using OutpostLibrary.Navigation;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System.Xml.Serialization;
using OutpostCore.Blocks;
using ScreenManagement;

namespace OutpostCore
{
    //okay I'm very seriously thinking this should be split to two or more classes
    //like
    // -map management
    // -player and screen drawing
    // -basic graphics stuff that can be used by multiple screens that want to draw in 3D
    // -lua and game rules
    //--possibly other things too

    //okay okay so
    //GameShell should hold only game rule things that can keep when you unload one game and load another in the same modset
    //...not 100% sure this will work correctly but
    //something to aim for
    public class GameShell
    {
        IntVector3 mapOffset;

        Dictionary<IntVector3, patternOrChunk> mapGenHelper;
        //Is here to keep map-generation from loading quite so many files
        //since that's what's slowing it down so much
        //though it doesn't seem to have helped much...

        public Random random;
        Thread mapFiller;

        public static string WorldFolder = "";


        public static GameShell gameShell
        {
            get
            {
                return _gameShell;
            }
        }
        private static GameShell _gameShell;

        public static void MakeGameShell()
        {
            _gameShell = new GameShell();
        }

        public LuaBridge lua;

        // i.e. messing with these will just screw up the workings
        #region technical constants
        const int mapSize = 9;
        const int mapCenter = 4;
        const int allowedStray = 2;
        #endregion

        //but messing with these will change the game 'balance'.
        #region game constants
        public const float gravity = .05f;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">The content manager to use.</param>
        /// <param name="g">The graphics device to use.</param>
        private GameShell()
        {
            random = new Random();



            lua = new LuaBridge();
        }

        #region Mapgen and Loading



        /*

        /// <summary>
        /// After having at least one loaded chunk, loads the rest of the map[,,].
        /// Does this in another thread, because it's sloooooow.
        /// </summary>
        void fillMapThreaded()
        {
            if (mapFiller != null)
            {
                if (mapFiller.IsAlive)
                {
                    mapFiller.Abort();
                }
            }

            mapFiller = new Thread(fillMap);
            mapFiller.Priority = ThreadPriority.Lowest;
            mapFiller.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        void fillMap()
        {
            //oh my god what is even happening here
            //a Dictionary of IntVectors????
            //guh
            mapGenHelper = new Dictionary<IntVector3, patternOrChunk>();
            Queue<IntVector3> todo = new Queue<IntVector3>();

            //what
            //what
            //wwwwwwhhhhhaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaatttttttt
            {
                Queue<IntVector3> meta = new Queue<IntVector3>();
                meta.Enqueue(new IntVector3(mapCenter));
                bool[, ,] donified = new bool[mapSize, mapSize, mapSize];
                for (int x = 0; x < mapSize; x++)
                    for (int y = 0; y < mapSize; y++)
                        for (int z = 0; z < mapSize; z++)
                            donified[x, y, z] = false;

                while (meta.Count != 0)
                {
                    IntVector3 investigate = meta.Dequeue();
                    #region neighbors
                    IntVector3 temp = investigate + new IntVector3(-1, 0, 0);
                    if (temp >= new IntVector3(0) && temp < new IntVector3(mapSize))
                    {
                        if (map.get(temp) == null && map.get(investigate) != null)
                        {
                            todo.Enqueue(temp);
                        }
                        if (!donified.get(temp))
                        {
                            meta.Enqueue(temp);
                            donified.set(temp, true);
                        }
                    }
                    temp = investigate + new IntVector3(1, 0, 0);
                    if (temp >= new IntVector3(0) && temp < new IntVector3(mapSize))
                    {
                        if (map.get(temp) == null && map.get(investigate) != null)
                        {
                            todo.Enqueue(temp);
                        }
                        if (!donified.get(temp))
                        {
                            meta.Enqueue(temp);
                            donified.set(temp, true);
                        }
                    }
                    temp = investigate + new IntVector3(0, -1, 0);
                    if (temp >= new IntVector3(0) && temp < new IntVector3(mapSize))
                    {
                        if (map.get(temp) == null && map.get(investigate) != null)
                        {
                            todo.Enqueue(temp);
                        }
                        if (!donified.get(temp))
                        {
                            meta.Enqueue(temp);
                            donified.set(temp, true);
                        }
                    }
                    temp = investigate + new IntVector3(0, 1, 0);
                    if (temp >= new IntVector3(0) && temp < new IntVector3(mapSize))
                    {
                        if (map.get(temp) == null && map.get(investigate) != null)
                        {
                            todo.Enqueue(temp);
                        }
                        if (!donified.get(temp))
                        {
                            meta.Enqueue(temp);
                            donified.set(temp, true);
                        }
                    }
                    temp = investigate + new IntVector3(0, 0, -1);
                    if (temp >= new IntVector3(0) && temp < new IntVector3(mapSize))
                    {
                        if (map.get(temp) == null && map.get(investigate) != null)
                        {
                            todo.Enqueue(temp);
                        }
                        if (!donified.get(temp))
                        {
                            meta.Enqueue(temp);
                            donified.set(temp, true);
                        }
                    }
                    temp = investigate + new IntVector3(0, 0, 1);
                    if (temp >= new IntVector3(0) && temp < new IntVector3(mapSize))
                    {
                        if (map.get(temp) == null && map.get(investigate) != null)
                        {
                            todo.Enqueue(temp);
                        }
                        if (!donified.get(temp))
                        {
                            meta.Enqueue(temp);
                            donified.set(temp, true);
                        }
                    }
                    #endregion
                }
            }

            while (todo.Count != 0)
            {
                IntVector3 make = todo.Dequeue();
                if (map.get(make) != null)
                    continue;
                map.set(make, loadChunk(make + mapOffset));
                map.get(make).assignNeighbors(true);
                map.get(make).endFill();
                #region neighbors
                IntVector3 temp;
                temp = make + new IntVector3(-1, 0, 0);
                if (temp >= new IntVector3(0) && temp < new IntVector3(mapSize))
                {
                    if (map.get(temp) == null)
                    {
                        todo.Enqueue(temp);
                    }
                }
                temp = make + new IntVector3(1, 0, 0);
                if (temp >= new IntVector3(0) && temp < new IntVector3(mapSize))
                {
                    if (map.get(temp) == null)
                    {
                        todo.Enqueue(temp);
                    }
                }
                temp = make + new IntVector3(0, -1, 0);
                if (temp >= new IntVector3(0) && temp < new IntVector3(mapSize))
                {
                    if (map.get(temp) == null)
                    {
                        todo.Enqueue(temp);
                    }
                }
                temp = make + new IntVector3(0, 1, 0);
                if (temp >= new IntVector3(0) && temp < new IntVector3(mapSize))
                {
                    if (map.get(temp) == null)
                    {
                        todo.Enqueue(temp);
                    }
                }
                temp = make + new IntVector3(0, 0, -1);
                if (temp >= new IntVector3(0) && temp < new IntVector3(mapSize))
                {
                    if (map.get(temp) == null)
                    {
                        todo.Enqueue(temp);
                    }
                }
                temp = make + new IntVector3(0, 0, 1);
                if (temp >= new IntVector3(0) && temp < new IntVector3(mapSize))
                {
                    if (map.get(temp) == null)
                    {
                        todo.Enqueue(temp);
                    }
                }
                #endregion
            }
            mapGenHelper = null;
        }

        void recenterMap()
        {
            IntVector3 prevCenter = mapOffset + new IntVector3(mapCenter);
            IntVector3 difference = prevCenter - player.chunk;
            Chunk[, ,] newMap = new Chunk[mapSize, mapSize, mapSize];
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    for (int z = 0; z < mapSize; z++)
                    {
                        IntVector3 oldCoordinates = new IntVector3(x, y, z);
                        IntVector3 newCoordinates = oldCoordinates + difference;
                        if (newCoordinates >= new IntVector3(0) && newCoordinates < new IntVector3(mapSize))
                        {
                            newMap.set(newCoordinates, map.get(oldCoordinates));
                        }
                        else
                        {
                            Chunk unload = map.get(oldCoordinates);
                            if (unload != null)
                            {
                                unload.Dispose();
                            }
                        }
                    }
                }
            }
            mapOffset -= difference;
            map = newMap;
            fillMapThreaded();
        }


        /// <summary>
        /// NOT FULLY IMPLEMENTED
        /// Loads a chunk specified by position in the world map.
        /// Will generate the chunk if it does not already exist.
        /// </summary>
        /// <param name="position">The position to load from.</param>
        /// <returns>An Octree representation of the chunk.</returns>
        Chunk loadChunk(IntVector3 position)
        {
            LoadingScreen.ChangeMessage("Loading chunk " + position.ToString());
            String filename = position.ToString();
            mapV1 mapmaker;
            try
            {
                //mapmaker = content.Load<mapV1>(filename);
            }
            catch (ContentLoadException)
            {
                //mapmaker = Mapgen.generatePatterns(this, position);
            }
            Chunk building = new Chunk(position, graphics);
            //if (mapmaker.layers[0] == "ungenerated")
            {

                lua.buildChunk(position, building);
            }
            //else
            {
                //Chunk loading from file code goes here.
            }
            return building;
        }


        /// <summary>
        /// NOT FULLY IMPLEMENTED
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public patternOrChunk getPatternOrChunk(IntVector3 position)
        {
            patternOrChunk toMake;


            //map first
            if ((position >= mapOffset) && position < mapOffset + new IntVector3(mapSize))
            {
                Chunk possible = map.get(position - mapOffset);
                if (possible != null)
                {
                    toMake = new patternOrChunk(possible);
                    if (mapGenHelper != null)
                    {
                        mapGenHelper[position] = toMake;
                    }
                    return toMake;
                }
            }

            //then helper
            //because if you do helper first, then ones generated during a run don't get acknowledged
            if (mapGenHelper != null)
            {
                if (mapGenHelper.ContainsKey(position))
                {
                    return mapGenHelper[position];
                }
            }

            //then, check for a file
            String filename = position.ToString();
            mapV1 mapmaker;
            try
            {
                //for some reason this is slow as fuck
                //probably because hard drives are slow
                //so it's commented for now
                //I wonder if it would be better if it was actually loading things?
                //mapmaker = content.Load<mapV1>(filename);
            }
            catch (ContentLoadException)
            {
                //if no file, then generate the pattern
                //mapmaker = Mapgen.generatePatterns(this, position);
            }
            //beyond here lies TEMP SHIT CODE

            //if (mapmaker.layers[0] == "ungenerated")
            {
                toMake = new patternOrChunk(OutpostLibrary.Structure.field);
                if (mapGenHelper != null)
                {
                    mapGenHelper[position] = toMake;
                }
                return toMake;
            }
            Chunk building = new Chunk(position, graphics);

            //chunk loading from file goes here

            building.endFill();
            toMake = new patternOrChunk(building);
            if (mapGenHelper != null)
            {
                mapGenHelper[position] = toMake;
            }
            return toMake;
        }

        /// <summary>
        /// Loads a chunk specified by filename.
        /// Assumes that the file exists and the chunk is generated.
        /// If there is ANY doubt about this (i.e. if the chunk is part of the main map), use loadChunk(IntVector) instead.
        /// </summary>
        /// <param name="filename">The file that the chunk is described in.</param>
        /// <returns>An Octree representation of the chunk.</returns>
        Chunk loadChunk(string filename)
        {
            mapV1 mapmaker = content.Load<mapV1>(filename);
            Chunk building = new Chunk(4, filename, graphics);

            //Actual chunk loading code goes here.

            building.endFill();
            return building;
        }


        //TODO: this actually needs an equivalent in GameManager
        void loadEntities(mapV1 source)
        {
            //This code is complete temporary bullshit.
            string[] pos = source.layers[16].Split(' ');
            //playerPos = new Vector3(Int16.Parse(pos[0]), Int16.Parse(pos[1]), Int16.Parse(pos[2]));
        }
        //*/

        #endregion Mapgen and Loading

    }
}
