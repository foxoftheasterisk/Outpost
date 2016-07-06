using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OutpostLibrary.Navigation;
using OutpostLibrary.Content;
using OutpostLibrary;
using Outpost.Blocks;
using Outpost.Map;

//TODO: abolish the rest of this

namespace Outpost
{
    public struct patternOrChunk
    {
        public OutpostLibrary.Structure pattern;
        public Chunk chunk;

        public patternOrChunk(Chunk oct)
        {
            chunk = oct;
            pattern = OutpostLibrary.Structure.field;
        }

        public patternOrChunk(OutpostLibrary.Structure pat)
        {
            pattern = pat;
            chunk = null;
        }
    }

    /*
    public static class Mapgen
    {
        public static void generateChunk(Chunk chunk, MainGame buildingFor, IntVector3 location)
        {
            //TEMP SHIT CODE GO
            //Well, great minds think alike it seems, because this looks quite Minecraft-like!  I am 0kay with this.

            //starting to think that generation should not use SolidBlock ever
            //but just use WornBlock with no exceptions
            //...no, bad idea, it increases storage space too much

            Screens.LoadingScreen.ChangeMessage("Generating chunk " + location);

            int[,] checkpointHeights = new int[5, 5];  //corners, middle, in-between
            int interval = Sizes.ChunkSize / 4;  //the space between the checkpoint heights

            if (location.Y != 0)
            {
                for (int x = 0; x < Sizes.ChunkSize; x++)
                {
                    for (int y = 0; y < Sizes.ChunkSize; y++)
                    {
                        for (int z = 0; z < Sizes.ChunkSize; z++)
                        {
                            if (location.Y > 0)
                                chunk.fillAssign(x, y, z, new SolidBlock(Material.materials["air"]));
                            else
                                chunk.fillAssign(x, y, z, new SolidBlock(Material.materials["dirt"]));
                        }
                    }
                }
                chunk.assignNeighbors(true);
                return;
            }

            #region neighbors
            patternOrChunk top = buildingFor.getPatternOrChunk(location + new IntVector3(0, 1, 0));
            patternOrChunk bottom = buildingFor.getPatternOrChunk(location + new IntVector3(0, -1, 0));
            patternOrChunk west = buildingFor.getPatternOrChunk(location + new IntVector3(0, 0, -1));
            patternOrChunk north = buildingFor.getPatternOrChunk(location + new IntVector3(-1, 0, 0));
            patternOrChunk east = buildingFor.getPatternOrChunk(location + new IntVector3(0, 0, 1));
            patternOrChunk south = buildingFor.getPatternOrChunk(location + new IntVector3(1, 0, 0));

            for (int x = 0; x < 5; x++)
            {
                for (int z = 0; z < 5; z++)
                {
                    checkpointHeights[x, z] = -32;
                }
            }

            if (top.chunk != null)
            {
                bool[,] isAirAbove = new bool[Sizes.ChunkSize, Sizes.ChunkSize];
                for (int x = 0; x < Sizes.ChunkSize; x++)
                {
                    //bluh FINSH LATER
                }
            }
            if (bottom.chunk != null)
            {
                //yeah I dunno
            }

            bool xSet = false;
            bool zSet = false;
            if (west.chunk != null)
            {
                if (east.chunk != null)
                {
                    //not exactly sure what to do here, but
                    //I guess lerp and add in random?
                    //of course there's also the thought of, what if this AND south and/or north?
                    //yeah okay
                    //lerp
                    //if there's a north/south element, add that in
                    //then add random
                }
                else
                {
                    #region west
                    Chunk reading = west.chunk;
                    
                    for (int i = 0; i < 4; i++)
                    {
                        int baseHeight = Sizes.ChunkSize;
                        int slope = 0;

                        bool set = false;
                        for (int y = 0; y < Sizes.ChunkSize; y++)
                        {
                            if (reading.getBlock(i * interval, y, Sizes.ChunkSize - 4).mapGenName == "Solid air")
                            {
                                baseHeight = y;
                                set = true;
                                break;
                            }
                        }
                        if (!set)
                        {
                            baseHeight = Sizes.ChunkSize;
                        }

                        set = false;
                        for (int y = 0; y < Sizes.ChunkSize; y++)
                        {
                            if (reading.getBlock(i * interval, y, Sizes.ChunkSize - 2).mapGenName == "Solid air")
                            {
                                slope = (y - baseHeight);
                                set = true;
                                break;
                            }
                        }
                        if (!set)
                        {
                            slope = Sizes.ChunkSize - baseHeight;
                        }

                        set = false;
                        for (int y = 0; y < Sizes.ChunkSize; y++)
                        {
                            if (reading.getBlock(i * interval, y, Sizes.ChunkSize - 1).mapGenName == "Solid air")
                            {
                                checkpointHeights[i, 0] = y + (slope / 2);
                                set = true;
                                //checkpointHeights[i, 0] = y;
                                break;
                            }
                        }

                        if (!set)
                        {
                            checkpointHeights[i, 0] = Sizes.ChunkSize + (slope / 2);
                        }
                    }
                    {
                        int baseHeight = Sizes.ChunkSize;
                        int slope = 0;
                        bool set = false;
                        for (int y = 0; y < Sizes.ChunkSize; y++)
                        {
                            if (reading.getBlock(Sizes.ChunkSize - 4, y, Sizes.ChunkSize - 1).mapGenName == "Solid air")
                            {
                                baseHeight = y;
                                set = true;
                                break;
                            }
                        }
                        if (!set)
                        {
                            baseHeight = Sizes.ChunkSize;
                        }
                        for (int y = 0; y < Sizes.ChunkSize; y++)
                        {
                            if (reading.getBlock(Sizes.ChunkSize - 2, y, Sizes.ChunkSize - 1).mapGenName == "Solid air")
                            {
                                slope += (y - baseHeight);
                                set = true;
                                break;
                            }
                        }
                        if (!set)
                        {
                            slope = Sizes.ChunkSize - baseHeight;
                        }
                        for (int y = 0; y < Sizes.ChunkSize; y++)
                        {
                            if (reading.getBlock(Sizes.ChunkSize - 1, y, Sizes.ChunkSize - 1).mapGenName == "Solid air")
                            {
                                checkpointHeights[4, 0] = y + (slope / 2);
                                //checkpointHeights[4, 0] = y;
                                set = true;
                                break;
                            }
                        }
                        if (!set)
                        {
                            checkpointHeights[4, 0] = Sizes.ChunkSize + (slope / 2);
                        }
                    }
                    #endregion
                    xSet = true;
                }
            }
            else if (east.chunk != null)
            {
                #region east
                Chunk reading = east.chunk;
                for (int x = 0; x < 4; x++)
                {
                    bool set = false;
                    for (int y = 0; y < Sizes.ChunkSize; y++)
                    {
                        if (reading.getBlock(x * interval, y, 0).mapGenName == "Solid air")
                        {
                            checkpointHeights[x, 4] = y;
                            set = true;
                            break;
                        }
                    }
                    if (!set)
                    {
                        checkpointHeights[x, 4] = Sizes.ChunkSize;
                    }
                }
                {
                    int baseHeight = Sizes.ChunkSize;
                    int slope = 0;
                    bool set = false;
                    for (int y = 0; y < Sizes.ChunkSize; y++)
                    {
                        if (reading.getBlock(Sizes.ChunkSize - 4, y, 0).mapGenName == "Solid air")
                        {
                            baseHeight = y;
                            set = true;
                            break;
                        }
                    }
                    if (!set)
                    {
                        baseHeight = Sizes.ChunkSize;
                    }
                    set = false;
                    for (int y = 0; y < Sizes.ChunkSize; y++)
                    {
                        if (reading.getBlock(Sizes.ChunkSize - 2, y, 0).mapGenName == "Solid air")
                        {
                            slope += (y - baseHeight);
                            set = true;
                            break;
                        }
                    }
                    if (!set)
                    {
                        slope = (Sizes.ChunkSize - baseHeight);
                    }
                    set = false;
                    for (int y = 0; y < Sizes.ChunkSize; y++)
                    {
                        if (reading.getBlock(Sizes.ChunkSize - 1, y, 0).mapGenName == "Solid air")
                        {
                            checkpointHeights[4, 4] = y + (slope / 2);
                            set = true;
                            break;
                        }
                    }
                    if (!set)
                    {
                        checkpointHeights[4, 4] = Sizes.ChunkSize + (slope / 2);
                    }
                }
                #endregion
                xSet = true;
            }
            if (north.chunk != null)
            {
                if (south.chunk != null)
                {
                    //there'll probably be some kind of special case for in-between ones, but meh.
                }
                else
                {
                    #region north
                    Chunk reading = north.chunk;
                    for (int z = 0; z < 4; z++)
                    {
                        int baseHeight = Sizes.ChunkSize;
                        int slope = 0;
                        bool set = false;
                        for (int y = 0; y < Sizes.ChunkSize; y++)
                        {
                            if (reading.getBlock(Sizes.ChunkSize - 4, y, z * interval).mapGenName == "Solid air")
                            {
                                baseHeight = y;
                                set = true;
                                break;
                            }
                        }
                        if (!set)
                        {
                            baseHeight = Sizes.ChunkSize;
                        }
                        set = false;
                        for (int y = 0; y < Sizes.ChunkSize; y++)
                        {
                            if (reading.getBlock(Sizes.ChunkSize - 2, y, z * interval).mapGenName == "Solid air")
                            {
                                slope += (y - baseHeight);
                                set = true;
                                break;
                            }
                        }
                        if (!set)
                        {
                            slope = (Sizes.ChunkSize - baseHeight);
                        }
                        set = false;
                        for (int y = 0; y < Sizes.ChunkSize; y++)
                        {
                            if (reading.getBlock(Sizes.ChunkSize - 1, y, z * interval).mapGenName == "Solid air")
                            {
                                checkpointHeights[0, z] = y + (slope / 2);
                                set = true;
                                break;
                            }
                        }
                        if (!set)
                        {
                            checkpointHeights[0, z] = Sizes.ChunkSize + (slope / 2);
                        }
                    }
                    {
                        int baseHeight = Sizes.ChunkSize;
                        int slope = 0;
                        bool set = false;
                        for (int y = 0; y < Sizes.ChunkSize; y++)
                        {
                            if (reading.getBlock(Sizes.ChunkSize - 1, y, Sizes.ChunkSize - 4).mapGenName == "Solid air")
                            {
                                baseHeight = y;
                                set = true;
                                break;
                            }
                        }
                        if (!set)
                        {
                            baseHeight = Sizes.ChunkSize;
                        }
                        set = false;
                        for (int y = 0; y < Sizes.ChunkSize; y++)
                        {
                            if (reading.getBlock(Sizes.ChunkSize - 1, y, Sizes.ChunkSize - 2).mapGenName == "Solid air")
                            {
                                slope += (y - baseHeight);
                                set = true;
                                break;
                            }
                        }
                        if (!set)
                        {
                            slope = (Sizes.ChunkSize - baseHeight);
                        }
                        set = false;
                        for (int y = 0; y < Sizes.ChunkSize; y++)
                        {
                            if (reading.getBlock(Sizes.ChunkSize - 1, y, Sizes.ChunkSize - 1).mapGenName == "Solid air")
                            {
                                checkpointHeights[0, 4] = y + (slope / 2);
                                set = true;
                                break;
                            }
                        }
                        if (!set)
                        {
                            checkpointHeights[0, 4] = Sizes.ChunkSize + (slope / 2);
                        }
                    }
                    #endregion
                    zSet = true;
                }
            }
            else if (south.chunk != null)
            {
                #region south
                Chunk reading = south.chunk;
                for (int z = 0; z < 4; z++)
                {
                    bool set = false;
                    for (int y = 0; y < Sizes.ChunkSize; y++)
                    {
                        if (reading.getBlock(0, y, z * interval).mapGenName == "Solid air")
                        {
                            checkpointHeights[4, z] = y;
                            set = true;
                            break;
                        }
                    }
                    if(!set)
                        checkpointHeights[4, z] = Sizes.ChunkSize;
                }
                {
                    int baseHeight = Sizes.ChunkSize;
                    int slope = 0;
                    bool set = false;
                    for (int y = 0; y < Sizes.ChunkSize; y++)
                    {
                        if (reading.getBlock(0, y, Sizes.ChunkSize - 4).mapGenName == "Solid air")
                        {
                            baseHeight = y;
                            set = true;
                            break;
                        }
                    }
                    if (!set)
                        baseHeight = Sizes.ChunkSize;
                    set = false;
                    for (int y = 0; y < Sizes.ChunkSize; y++)
                    {
                        if (reading.getBlock(0, y, Sizes.ChunkSize - 2).mapGenName == "Solid air")
                        {
                            slope += (y - baseHeight);
                            set = true;
                            break;
                        }
                    }
                    if (!set)
                        slope = Sizes.ChunkSize - baseHeight;
                    set = false;
                    for (int y = 0; y < Sizes.ChunkSize; y++)
                    {
                        if (reading.getBlock(0, y, Sizes.ChunkSize - 1).mapGenName == "Solid air")
                        {
                            checkpointHeights[4, 4] = y + (slope / 2);
                            set = true;
                            break;
                        }
                    }
                    if (!set)
                        checkpointHeights[4,4] = Sizes.ChunkSize + (slope / 2);
                    set = false;
                }
                #endregion
                zSet = true;
            }
            #endregion
            #region filling checkpoints
            Random rand = buildingFor.random;
            if (!xSet && !zSet)
                checkpointHeights[0, 0] = rand.Next(Sizes.ChunkSize - 2) + 1;
            if (checkpointHeights[0, 0] == -32)
            {
                for (int i = 3; i >= 0; i--)
                {
                    if (!zSet)
                        checkpointHeights[0, i] = checkpointHeights[0, i + 1] + rand.Next(-1, 2);
                    if (!xSet)
                        checkpointHeights[i, 0] = checkpointHeights[i + 1, 0] + rand.Next(-1, 2);
                }
            }
            else
            {
                for (int i = 1; i < 5; i++)
                {
                    if (!zSet)
                        checkpointHeights[0, i] = checkpointHeights[0, i - 1] + rand.Next(-1, 2);
                    if (!xSet)
                        checkpointHeights[i, 0] = checkpointHeights[i - 1, 0] + rand.Next(-1, 2);
                }
            }

            {
                int xstart, zstart;
                int xend, zend;
                int xdir, zdir;
                if (checkpointHeights[1, 0] == -32)
                {
                    zstart = 3;
                    zend = -1;
                    zdir = -1;
                }
                else
                {
                    zstart = 1;
                    zend = 5;
                    zdir = 1;
                }
                

                if (checkpointHeights[0, 1] == -32)
                {
                    xstart = 3;
                    xend = -1;
                    xdir = -1;
                }
                else
                {
                    xstart = 1;
                    xend = 5;
                    xdir = 1;
                }

                for (int x = xstart; x != xend; x += xdir)
                {
                    for (int z = zstart; z != zend; z += zdir)
                    {
                        checkpointHeights[x, z] = ((checkpointHeights[x - xdir, z] + checkpointHeights[x, z - zdir] + rand.Next(-3, 3)) / 2);
                    }
                }
            }
            #endregion
            #region heights
            int[,] heights = new int[Sizes.ChunkSize, Sizes.ChunkSize];
            for (int bigX = 0; bigX < 4; bigX++)
            {
                for (int bigZ = 0; bigZ < 4; bigZ++)
                {
                    for (int smallX = 0; smallX < interval; smallX++)
                    {
                        for (int smallZ = 0; smallZ < interval; smallZ++)
                        {
                            int temp00 = checkpointHeights[bigX, bigZ] * (4 - smallX) * (4 - smallZ);
                            int temp01 = checkpointHeights[bigX + 1, bigZ] * (smallX) * (4 - smallZ);
                            int temp10 = checkpointHeights[bigX, bigZ + 1] * (4 - smallX) * (smallZ);
                            int temp11 = checkpointHeights[bigX + 1, bigZ + 1] * (smallX) * (smallZ);
                            heights[(bigX * interval) + smallX, (bigZ * interval) + smallZ] = (int)Math.Round((temp00 + temp01 + temp10 + temp11) / (float)Sizes.ChunkSize);
                        }
                    }
                }
            }
            #endregion
            #region terrain
            List<WornBlock> secondPass = new List<WornBlock>();

            for (int x = 0; x < Sizes.ChunkSize; x++)
            {
                for (int z = 0; z < Sizes.ChunkSize; z++)
                {
                    int height = heights[x, z];
                    for (int y = 0; y < Sizes.ChunkSize; y++)
                    {
                        if (y < height - 1)
                        {
                            chunk.fillAssign(x,y,z,new SolidBlock(Material.materials["dirt"]));
                        }
                        else if(y > height)
                        {
                            chunk.fillAssign(x,y,z,new SolidBlock(Material.materials["air"]));
                        }
                        else 
                        {
                            Material primary;
                            if (y == height)
                                primary = Material.materials["air"];
                            else
                                primary = Material.materials["dirt"];

                            WornBlock block = new WornBlock(primary);

                            secondPass.Add(block);
                            chunk.fillAssign(x,y,z,block);
                        }
                    }
                }
            }
            #endregion


            chunk.assignNeighbors(true);
            foreach (WornBlock block in secondPass)
            {
                block.wearDown();
            }
            //
        }

        public static mapV1 generatePatterns(MainGame buildingFor, IntVector3 center)
        {
            return null;
        }
    }
    //*/
}
