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
using Microsoft.Xna.Framework.Storage;
using System.Xml.Serialization;
using Outpost.Blocks;
using Outpost.Screens;

namespace Outpost
{
    //okay I'm very seriously thinking this should be split to two or more classes
    //like
    // -map management
    // -player and screen drawing
    // -basic graphics stuff that can be used by multiple screens that want to draw in 3D
    // -lua and game rules
    //--possibly other things too
    public class MainGame : Screen
    {
        IntVector3 mapOffset; // 
        Chunk[, ,] map;
        Dictionary<IntVector3, patternOrChunk> mapGenHelper;
        //Is here to keep map-generation from loading quite so many files
        //since that's what's slowing it down so much
        //though it doesn't seem to have helped much...
        Player player;
        public Random random;
        Thread mapFiller;

        Texture2D reticule;
        public Texture2D blank;

        public static string WorldFolder = "firstSavedWorld/";
        public static StorageContainer cont;
        //you know, for once I actually get saving a-workin'

        public static MainGame mainGame;
        //is this a good idea? prevents another MainGame instance being around.  Is there any reason there should be another?
        //No, I don't think so - although I might have to make a child class for multiplayer.  Multiple maps (that might share chunks)? Sure.  But not multiple GAMES.

        public LuaBridge lua;

        // i.e. messing with these will just screw up the workings
        #region technical constants
        ContentManager content;
        GraphicsDevice graphics;

        const int mapSize = 9;
        const int mapCenter = 4;
        const int allowedStray = 2;

        IntVector2 screenCenter;
        #endregion

        //but messing with these will change the game 'balance'.
        #region game constants
        public const float gravity = .05f;
        #endregion

        /// <summary>
        /// Creates a game with a new world.
        /// </summary>
        /// <param name="c">The content manager to use.</param>
        /// <param name="g">The graphics device to use.</param>
        public MainGame(ContentManager c, GraphicsDevice g)
        {
            //what is this I don't even
            /*
            IAsyncResult result = StorageDevice.BeginShowSelector(null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageDevice harddrive = StorageDevice.EndShowSelector(result);
            result.AsyncWaitHandle.Close();

            result = harddrive.BeginOpenContainer("StorageDemo", null, null);
            result.AsyncWaitHandle.WaitOne();
            cont = harddrive.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();

            cont.CreateDirectory(WorldFolder);
            //*/
            
            MainGame.mainGame = this;

            content = c;
            graphics = g;
            random = new Random();

            fontling = content.Load<SpriteFont>("someFont");
            reticule = content.Load<Texture2D>("reticule");
            blank = content.Load<Texture2D>("blank");
            
            initializeGraphics();

            lua = new LuaBridge();
        }

        public void newMap()
        {
            LoadingScreen.Display("Creating Map");
            map = new Chunk[mapSize, mapSize, mapSize];

            IntVector3 playerChunk = new IntVector3(0);
            mapOffset = new IntVector3(-mapCenter) + playerChunk;

            map.set(playerChunk - mapOffset, loadChunk(playerChunk));
            map.get(playerChunk - mapOffset).endFill();

            screenCenter = new IntVector2(graphics.Viewport.Width / 2, graphics.Viewport.Height / 2);
            player = new Player(this, screenCenter, new IntVector3(0), new Vector3(2, 15, 2));

            fillMap();
        }

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

        void loadEntities(mapV1 source)
        {
            //This code is complete temporary bullshit.
            string[] pos = source.layers[16].Split(' ');
            //playerPos = new Vector3(Int16.Parse(pos[0]), Int16.Parse(pos[1]), Int16.Parse(pos[2]));
        }

//TODO: i'm pretty sure there are times it should close
        public bool shouldClose()
        {
            return false;
        }

//TODO: make it use input correctly
        public bool Update(bool useInput)
        {
            KeyboardState keys;
            try
            {
                keys = Keyboard.GetState();
            }
            catch (InvalidOperationException)
            {
                keys = new KeyboardState();//probably a very bad idea
            }

            if(keys.IsKeyDown(Keys.Enter))
            {
                ScreenManager.screenManager.push(new PauseScreen());
            }
            
            player.Move();

            if (!(player.chunk >= mapOffset + new IntVector3(mapCenter) - new IntVector3(allowedStray)) || !(player.chunk <= mapOffset + new IntVector3(mapCenter) + new IntVector3(allowedStray)))
            {
                recenterMap();
            }

            player.Action();

            return false;
        }

        public Block getBlock(BlockAddress address)
        {
            IntVector3 chunk = address.chunk;
            chunk -= mapOffset;
            if (chunk.X < 0 || chunk.X >= map.GetLength(0) || chunk.Y < 0 || chunk.Y >= map.GetLength(1) || chunk.Z < 0 || chunk.Z >= map.GetLength(2))
                return null;
            if(map.get(chunk) == null)
                return null;

            return map.get(chunk).getBlock(address.block);
        }

        public void changeBlock(BlockAddress address, Block changeTo)
        {
            IntVector3 chunk = address.chunk;
            chunk -= mapOffset;
            map.get(chunk).assignBlock(address.block, changeTo);
        }

        //possible efficiency increaser: create a combined get+change block function

        /// <summary>
        /// NOT FULLY IMPLEMENTED: Does not use the x/z size.  Or the direction, for that matter.
        /// ALSO, does not wrap outside of the chunk.
        /// </summary>
        /// <param name="chunkBase">The chunk containing the entity</param>
        /// <param name="posInChunk">The entity's position in the chunk</param>
        /// <param name="direction">The direction the entity is facing, for bounding-box purposes</param>
        /// <param name="size">The size of the entity</param>
        /// <returns></returns>
        public Solidity[][][] detectCollision(IntVector3 chunkBase, Vector3 posInChunk, Vector3 direction, IntVector3 size)
        {
            IntVector3 mappedChunk = chunkBase - mapOffset;

            if (posInChunk.X < 0)
            {
                posInChunk.X += 16;
                mappedChunk = mappedChunk + new IntVector3(-1, 0, 0);
            }
            if (posInChunk.X > 16)
            {
                posInChunk.X -= 16;
                mappedChunk = mappedChunk + new IntVector3(1, 0, 0);
            }
            if (posInChunk.Y < 0)
            {
                posInChunk.Y += 16;
                mappedChunk = mappedChunk + new IntVector3(0, -1, 0);
            }
            if (posInChunk.Y > 16)
            {
                posInChunk.Y -= 16;
                mappedChunk = mappedChunk + new IntVector3( 0, 1,0);
            }
            if (posInChunk.Z < 0)
            {
                posInChunk.Z += 16;
                mappedChunk = mappedChunk + new IntVector3(0, 0, -1);
            }
            if (posInChunk.Z < 0)
            {
                posInChunk.Z += 16;
                mappedChunk = mappedChunk + new IntVector3(0, 0, 1);
            }

            Chunk chunk = map.get(mappedChunk);
            Solidity[][][] collidingWith = new Solidity[1][][];
            for (int i = 0; i < collidingWith.Length; i++)
            {
                collidingWith[i] = new Solidity[size.Y + 1][];
                for (int j = 0; j < collidingWith[i].Length; j++)
                {
                    collidingWith[i][j] = new Solidity[1];
                }
            }
            //well that is a mess, but I can't really think of a better way to actually pass the dimensions...

            for (int y = 0; y < size.Y + 1; y++)
            {
                if (posInChunk.Y + y > 15)
                {
                    collidingWith[0][y][0] = Solidity.vacuum;
                    //Chunk temp = map.get(mappedChunk + new IntVector3(0,1,0));
                    //collidingWith[0][y][0] = temp.getBlock((int)Math.Floor(posInChunk.X), (int)Math.Floor(posInChunk.Y) + y - 16, (int)Math.Floor(posInChunk.Z));
                }
                else
                {
                    Block got = chunk.getBlock((int)Math.Floor(posInChunk.X), (int)Math.Floor(posInChunk.Y) + y, (int)Math.Floor(posInChunk.Z));
                    if (got == null)
                        collidingWith[0][y][0] = Solidity.vacuum;
                    else
                        collidingWith[0][y][0] = got.solidity;
                }
            }

            return collidingWith;
        }

        public delegate bool blockChecker(BlockAddress blockToCheck);

        public BlockAddress findBlock(IntVector3 chunk, Vector3 posInChunk, Vector3 directionToSeek, int lengthToStop, blockChecker isAcceptable)
        {
            Vector3 unitX, unitY, unitZ;
            Vector3 nextX, nextY, nextZ;
            float xDist, yDist, zDist, stopDist;
            float fTemp;
            Vector3 vTemp;
            BlockAddress bTemp;

            stopDist = lengthToStop;

            #region initialization
            fTemp = Math.Abs(directionToSeek.X);
            if (fTemp != 0)
            {
                unitX = directionToSeek / fTemp;
                if (unitX.X > 0)
                {
                    fTemp = (float)Math.Ceiling(posInChunk.X) - posInChunk.X;
                }
                else
                {
                    fTemp = (float)Math.Floor(posInChunk.X) - posInChunk.X;
                }
                vTemp = unitX * fTemp;
                xDist = vTemp.Length();
                nextX = vTemp + posInChunk;
            }
            else
            {
                xDist = stopDist + 1;
                unitX = new Vector3();
                nextX = new Vector3();
            }

            fTemp = Math.Abs(directionToSeek.Y);
            if (fTemp != 0)
            {
                unitY = directionToSeek / fTemp;
                if (unitY.Y > 0)
                {
                    fTemp = (float)Math.Ceiling(posInChunk.Y) - posInChunk.Y;
                }
                else
                {
                    fTemp = (float)Math.Floor(posInChunk.Y) - posInChunk.Y;
                }
                vTemp = unitY * fTemp;
                yDist = vTemp.Length();
                nextY = vTemp + posInChunk;
            }
            else
            {
                yDist = stopDist + 1;
                unitY = new Vector3();
                nextY = new Vector3();
            }

            fTemp = Math.Abs(directionToSeek.Z);
            if (fTemp != 0)
            {
                unitZ = directionToSeek / fTemp;
                if (unitZ.Z > 0)
                {
                    fTemp = (float)Math.Ceiling(posInChunk.Z) - posInChunk.Z;
                }
                else
                {
                    fTemp = (float)Math.Floor(posInChunk.Z) - posInChunk.Z;
                }
                vTemp = unitZ * fTemp;
                zDist = vTemp.Length();
                nextZ = vTemp + posInChunk;
            }
            else
            {
                zDist = stopDist + 1;
                unitZ = new Vector3();
                nextZ = new Vector3();
            }
            #endregion
            #region seekLoop
            while (true)
            {
                fTemp = Math.Min(Math.Min(xDist, yDist), Math.Min(zDist, stopDist));
                if (fTemp == stopDist)
                    return null;
                if (fTemp == xDist)
                {
                    vTemp = new Vector3(unitX.X / 10, 0, 0);
                    bTemp = new BlockAddress(chunk, nextX + vTemp);
                    if (isAcceptable(bTemp))
                        return bTemp;
                    nextX += unitX;
                    xDist += unitX.Length();
                    continue;
                }
                if (fTemp == yDist)
                {
                    vTemp = new Vector3(0, unitY.Y / 10, 0);
                    bTemp = new BlockAddress(chunk, nextY + vTemp);
                    if (isAcceptable(bTemp))
                        return bTemp;
                    nextY += unitY;
                    yDist += unitY.Length();
                    continue;
                }
                if (fTemp == zDist)
                {
                    vTemp = new Vector3(0, 0, unitZ.Z / 10);
                    bTemp = new BlockAddress(chunk, nextZ + vTemp);
                    if (isAcceptable(bTemp))
                        return bTemp;
                    nextZ += unitZ;
                    zDist += unitZ.Length();
                    continue;
                }
                return null;
            }
            #endregion
        }

        public BlockAddress findBlockBefore(IntVector3 chunk, Vector3 posInChunk, Vector3 directionToSeek, int lengthToStop, blockChecker isAcceptable)
        {
            Vector3 unitX, unitY, unitZ;
            Vector3 nextX, nextY, nextZ;
            float xDist, yDist, zDist, stopDist;
            float fTemp;
            Vector3 vTemp;
            BlockAddress bTemp;
            BlockAddress previous = new BlockAddress(chunk, posInChunk);

            stopDist = lengthToStop;

            #region initialization
            fTemp = Math.Abs(directionToSeek.X);
            if (fTemp != 0)
            {
                unitX = directionToSeek / fTemp;
                if (unitX.X > 0)
                {
                    fTemp = (float)Math.Ceiling(posInChunk.X) - posInChunk.X;
                }
                else
                {
                    fTemp = (float)Math.Floor(posInChunk.X) - posInChunk.X;
                }
                vTemp = unitX * fTemp;
                xDist = vTemp.Length();
                nextX = vTemp + posInChunk;
            }
            else
            {
                xDist = stopDist + 1;
                unitX = new Vector3();
                nextX = new Vector3();
            }

            fTemp = Math.Abs(directionToSeek.Y);
            if (fTemp != 0)
            {
                unitY = directionToSeek / fTemp;
                if (unitY.Y > 0)
                {
                    fTemp = (float)Math.Ceiling(posInChunk.Y) - posInChunk.Y;
                }
                else
                {
                    fTemp = (float)Math.Floor(posInChunk.Y) - posInChunk.Y;
                }
                vTemp = unitY * fTemp;
                yDist = vTemp.Length();
                nextY = vTemp + posInChunk;
            }
            else
            {
                yDist = stopDist + 1;
                unitY = new Vector3();
                nextY = new Vector3();
            }

            fTemp = Math.Abs(directionToSeek.Z);
            if (fTemp != 0)
            {
                unitZ = directionToSeek / fTemp;
                if (unitZ.Z > 0)
                {
                    fTemp = (float)Math.Ceiling(posInChunk.Z) - posInChunk.Z;
                }
                else
                {
                    fTemp = (float)Math.Floor(posInChunk.Z) - posInChunk.Z;
                }
                vTemp = unitZ * fTemp;
                zDist = vTemp.Length();
                nextZ = vTemp + posInChunk;
            }
            else
            {
                zDist = stopDist + 1;
                unitZ = new Vector3();
                nextZ = new Vector3();
            }
            #endregion
            #region seekLoop
            while (true)
            {
                fTemp = Math.Min(Math.Min(xDist, yDist), Math.Min(zDist, stopDist));
                if (fTemp == stopDist)
                    return null;
                if (fTemp == xDist)
                {
                    vTemp = new Vector3(unitX.X / 10, 0, 0);
                    bTemp = new BlockAddress(chunk, nextX + vTemp);
                    if (isAcceptable(bTemp))
                        return previous;
                    nextX += unitX;
                    xDist += unitX.Length();
                    previous = bTemp;
                    continue;
                }
                if (fTemp == yDist)
                {
                    vTemp = new Vector3(0, unitY.Y / 10, 0);
                    bTemp = new BlockAddress(chunk, nextY + vTemp);
                    if (isAcceptable(bTemp))
                        return previous;
                    nextY += unitY;
                    yDist += unitY.Length();
                    previous = bTemp;
                    continue;
                }
                if (fTemp == zDist)
                {
                    vTemp = new Vector3(0, 0, unitZ.Z / 10);
                    bTemp = new BlockAddress(chunk, nextZ + vTemp);
                    if (isAcceptable(bTemp))
                        return previous;
                    nextZ += unitZ;
                    zDist += unitZ.Length();
                    previous = bTemp;
                    continue;
                }
                return null;
            }
            #endregion
        }

        #region Graphics

        public SpriteFont fontling;
        Matrix world; //totally not just the identity matrix
        //as long as this is actually always the identity matrix,
        //we can be lazy with the shader code.
        //and I see no real reason for it to not be.
        //(well, it could be we want to translate things, so we're not way off in like +1000, but as long as no rotates are going on we're still good.)
        //(I think.)

        //ehh I should probably not be lazy there
        //but
        //not yet
        //make that a TODO:
        Matrix projection;
        
        const int numTriangles = 4;

        short[] selectionIndices;
        DynamicIndexBuffer sIBuff;
        VertexPositionColor[] selectionVertices;
        DynamicVertexBuffer sVBuff;
        //this assumes that any selected area will be a cube (or at least a rectangular prism)
        //which uh
        //not necessarily true
        //say we want to select a tile in a wall and four around it?  yeah.  rectangular prism isn't gonna work there.

        BasicEffect drawingEngine;
        Effect voxelEffect;

        public void initializeGraphics()
        {
            world = Matrix.Identity;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), (float)graphics.Viewport.Width / (float)graphics.Viewport.Height, .01f, 50.0f);

            drawingEngine = new BasicEffect(graphics);

            drawingEngine.World = world;
            drawingEngine.Projection = projection;


            drawingEngine.TextureEnabled = false;
            drawingEngine.VertexColorEnabled = true;

            drawingEngine.EnableDefaultLighting();
            drawingEngine.SpecularColor= new Vector3(0, 0, 0);

            #region selected

            sIBuff = new DynamicIndexBuffer(graphics, IndexElementSize.SixteenBits, 24, BufferUsage.WriteOnly);
            //iBuff.ContentLost += new EventHandler<EventArgs>(indicesLostHandler);

            selectionIndices = new short[24]{0, 1,
                                    0, 2,
                                    1, 3,
                                    2, 3,
                                    0, 4,
                                    1, 5,
                                    2, 6,
                                    3, 7,
                                    4, 5,
                                    4, 6,
                                    5, 7,
                                    6, 7};

            sIBuff.SetData(selectionIndices);

            sVBuff = new DynamicVertexBuffer(graphics, typeof(VertexPositionColor), 8, BufferUsage.WriteOnly);
            //vBuff.ContentLost += new EventHandler<EventArgs>(verticesLostHandler);

            #endregion selected

            voxelEffect = content.Load<Effect>("CubeShader");

            voxelEffect.Parameters["World"].SetValue(world);
            voxelEffect.Parameters["Projection"].SetValue(projection);


        }

        void indicesLostHandler(object sender, EventArgs e)
        {
            sIBuff.SetData(selectionIndices);
        }

        void verticesLostHandler(object sender, EventArgs e)
        {
            sVBuff.SetData(selectionVertices);
        }

        public bool drawUnder()
        {
            return false;
        }

        public void Draw(SpriteBatch drawer)
        {
            drawingEngine.View = player.createViewMatrix();
            voxelEffect.Parameters["View"].SetValue(player.createViewMatrix());

            voxelEffect.Parameters["ambientColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 0.3f));
            voxelEffect.Parameters["sunColor"].SetValue(new Vector4(1.0f, 1.0f, 0.8f, 0.9f));
            voxelEffect.Parameters["sunDir"].SetValue(new Vector3(-0.1f, -1.0f, -0.5f));

            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    for (int z = 0; z < mapSize; z++)
                    {
                        Chunk drawee = map.get(new IntVector3(x, y, z));
                        if (drawee == null)
                            continue;
                        if (drawee.isTransparent())
                            continue;
                        if (drawee.isDisposed)
                            continue;
                        #region chunkdrawer

                        DynamicVertexBuffer vertexBuffer = drawee.getVertexBuffer();
                        int numVertices = drawee.getNumVertices();
                        DynamicIndexBuffer indexBuffer = drawee.getIndexBuffer();
                        int numIndices = drawee.getNumIndices();

                        graphics.SetVertexBuffer(vertexBuffer);
                        graphics.Indices = indexBuffer;

                        foreach (EffectPass p in voxelEffect.CurrentTechnique.Passes)  //Not sure I entirely understand this
                        {
                            //mmm... so... let's see
                            p.Apply();
                            graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, 0, numIndices / 3);
                            
                        }
                        #endregion chunkdrawer
                    }
                }
            }
            

            drawer.End();
            drawer.Begin();
            //for whatever reason if I don't reboot the SpriteBatch it does weird things with transparency and whatnot.

            #region GUI

            drawer.Draw(reticule, new Vector2(screenCenter.x - reticule.Width, screenCenter.y - reticule.Height), Color.White);

            #region selectedBlock

            BlockAddress selected = player.getTarget();
            if (selected != null)
            {
                RasterizerState standard = graphics.RasterizerState;
                
                RasterizerState depthBiased = new RasterizerState();
                depthBiased.DepthBias = 1;
                graphics.RasterizerState = depthBiased;

                selectionVertices = new VertexPositionColor[8];

                selectionVertices[0] = new VertexPositionColor(new Vector3(selected.x, selected.y, selected.z), Color.Black);
                selectionVertices[1] = new VertexPositionColor(new Vector3(selected.x + 1, selected.y, selected.z), Color.Black);
                selectionVertices[2] = new VertexPositionColor(new Vector3(selected.x, selected.y, selected.z + 1), Color.Black);
                selectionVertices[3] = new VertexPositionColor(new Vector3(selected.x + 1, selected.y, selected.z + 1), Color.Black);
                selectionVertices[4] = new VertexPositionColor(new Vector3(selected.x, selected.y + 1, selected.z), Color.Black);
                selectionVertices[5] = new VertexPositionColor(new Vector3(selected.x + 1, selected.y + 1, selected.z), Color.Black);
                selectionVertices[6] = new VertexPositionColor(new Vector3(selected.x, selected.y + 1, selected.z + 1), Color.Black);
                selectionVertices[7] = new VertexPositionColor(new Vector3(selected.x + 1, selected.y + 1, selected.z + 1), Color.Black);

                sVBuff.SetData(selectionVertices);

                drawingEngine.LightingEnabled = false;

                graphics.SetVertexBuffer(sVBuff);
                graphics.Indices = sIBuff;

                foreach (EffectPass p in drawingEngine.CurrentTechnique.Passes)
                {
                    p.Apply();
                    graphics.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 8, 0, 12);
                }

                graphics.RasterizerState = standard;
            }
            else
            {
                graphics.SetVertexBuffer(null);
                graphics.Indices = null;
            }
            #endregion selectedBlock


            #endregion GUI

            #region randomtext

            drawer.DrawString(fontling, "I THINK THEREFORE I ARRRRR", new Vector2(10, 10), new Color(10, 20, 30));
            //drawer.DrawString(fontling, player.Pos().ToString(), new Vector2(10, 40), Color.AntiqueWhite);
            //no more creepiest glitch ever
            //instead we have working things
            //...I don't know WHY it's working, but it is.  Not going to question this further.
            
            drawer.DrawString(fontling, "Position: " + player.Pos().ToString(), new Vector2(10, 40), new Color(7, 7, 7));

            if(selected != null)
                drawer.DrawString(fontling, "Selected: " + selected.ToString(), new Vector2(10, 70), Color.CadetBlue);
            #endregion

            //drawer.Draw(reticule, new Vector2(centerX, centerY), Color.White);
        }

        #endregion
    }
}
