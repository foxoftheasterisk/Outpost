using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using System.Xml.Serialization;

using Outpost.Blocks;
using OutpostLibrary;
using OutpostLibrary.Navigation;

namespace Outpost
{
    public struct VertexPositionColorNormal : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        public VertexPositionColorNormal(Vector3 pos, Color col, Vector3 norm)
        {
            Position = pos;
            Color = col;
            Normal = norm;
        }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexPositionColorNormal.VertexDeclaration; }
        }

        public string ToString()
        {
            return "Position: " + Position.ToString() + "; Color: " + Color.ToString() + "; Normal: " + Normal.ToString();
        }
    }
    //when I write that shader, I should also be able to include a geometry shader which calculates per-poly normals
    //and thus obviate the need to include a normal here.
    //although, it may make some normals backwards?  not sure if this is a problem.
    //it probably is, but all I'd need to do is change the order of the indices, nbd.

    public class Chunk : IDisposable
    {
        int size;
        Block[,,] blocks;

        List<MapStructure> structures;

        IntVector3 position;
        String filename;

        public bool isDisposed = false;

        VertexPositionColorNormal[] vertices;  
        //If I want to do custom specular
        //(which I do, eventually)
        //I'll need to write my own shader
        //...but not yet
        short[] indices;

        DynamicVertexBuffer vBuff;
        DynamicIndexBuffer iBuff;

        int maxVerts = 20000;
        int maxInds = 30000;

        /// <summary>
        /// Creates a new Chunk of size chunkSize x chunkSize x chunkSize, filled with null blocks.
        /// This is for chunks that are part of the map.
        /// It will assume that its north-west-bottom corner is at pos * chunkSize,
        /// and (when implemented...) it will save its data to "pos.x,pos.y,pos.z".xml
        /// </summary>
        /// <param name="layers"></param>
        /// <param name="pos"></param>
        public Chunk(IntVector3 pos, GraphicsDevice graphics)
        {
            size = Sizes.ChunkSize;
            blocks = new Block[Sizes.ChunkSize, Sizes.ChunkSize, Sizes.ChunkSize];
            structures = new List<MapStructure>();
            position = pos;
            filename = MainGame.WorldFolder + position.X + "," + position.Y + "," + position.Z;

            try
            {
                vBuff = new DynamicVertexBuffer(graphics, typeof(VertexPositionColorNormal), maxVerts, BufferUsage.WriteOnly);
                //vBuff.ContentLost += new EventHandler<EventArgs>(verticesLostHandler);
                iBuff = new DynamicIndexBuffer(graphics, IndexElementSize.SixteenBits, maxInds, BufferUsage.WriteOnly);
                //iBuff.ContentLost += new EventHandler<EventArgs>(indicesLostHandler);
                //so do I need to add a custom test for content lost before drawing?
            }
            catch(OpenTK.Graphics.GraphicsContextException e)
            {
                MainGame.mainGame.Log(e.ToString());
            }
            

            vertices = new VertexPositionColorNormal[0];
            indices = new short[0];
        }

        /// <summary>
        /// Creates a new Chunk of size size x size x size, filled with null blocks
        /// This was intended for chunks that are part of blueprints, but may still be useful (vehicles?)
        /// </summary>
        /// <param name="layers"></param>
        /// <param name="filename"></param>
        public Chunk(int size, String filename, GraphicsDevice graphics)
        {
            this.size = size;
            blocks = new Block[size, size, size];
            this.filename = MainGame.WorldFolder + filename;

            vBuff = new DynamicVertexBuffer(graphics, typeof(VertexPositionColorNormal), maxVerts, BufferUsage.WriteOnly);
            //vBuff.ContentLost += new EventHandler<EventArgs>(verticesLostHandler);

            iBuff = new DynamicIndexBuffer(graphics, IndexElementSize.SixteenBits, maxInds, BufferUsage.WriteOnly);
            //iBuff.ContentLost += new EventHandler<EventArgs>(indicesLostHandler);
        }

        public void Dispose()
        {
            if (vBuff != null)
                vBuff.Dispose();
            if (iBuff != null)
                iBuff.Dispose();
            isDisposed = true;
        }

        /// <summary>
        /// Saves the block's data upon deconstruction.
        /// Does not work, probably due to being called at deconstruction time.
        /// Try calling it manually, or at Dispose().
        /// </summary>
        /*
        ~Chunk()
        {
            //BLUH BLUH HUGE EXCEPTION
            int breadth = (int)Math.Pow(2, levels);

            mapV1 dataHolder = new mapV1();
            dataHolder.layers = new string[breadth];

            for (int y = 0; y < breadth; y++)
            {
                string layer = "";
                for (int x = 0; x < breadth; x++)
                {
                    for (int z = 0; z < breadth; z++)
                    {
                        layer += (int)getBlock(x, y, z) + " ";
                    }
                    layer += "\n";
                }
                dataHolder.layers[y] = layer;
            }

            //this seems like a bad-type idea
            if (MainGame.cont.FileExists(filename))
                MainGame.cont.DeleteFile(filename);

            Stream outs = MainGame.cont.CreateFile(filename);

            XmlSerializer transcriber = new XmlSerializer(typeof(mapV1));

            transcriber.Serialize(outs, dataHolder);

            outs.Close();
        }

        //*/

        /// <summary>
        /// Assigns a block, generating new vertices for the chunk afterwards.
        /// Use when changing only one block at a time.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool assignBlock(int x, int y, int z, Block block)
        {
            if (x >= size || y >= size || z >= size || x < 0 || y < 0 || z < 0)
                return false;
            blocks[x,y,z] = block;

            assignNeighbors(x, y, z, false);
            generateVertices();
            return true;
        }

        public bool assignBlock(IntVector3 loc, Block block)
        {
            return assignBlock(loc.X, loc.Y, loc.Z, block);
        }

        /// <summary>
        /// Block assignment function for batch assignments.
        /// Sets the given block to the given material.
        /// After using this function, endFill() or assignBlock() MUST be called before the next draw step!
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        /// <param name="type">The material to set the block to.</param>
        /// <returns></returns>
        public bool fillAssign(int x, int y, int z, Block block)
        {
            if (x >= size || y >= size || z >= size || x < 0 || y < 0 || z < 0)
                return false;
            blocks[x,y,z] = block;
            return true;
        }

        public void assignNeighbors(bool isFilling)
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        assignNeighbors(x, y, z, true);
                    }
                }
            }

            if (!isFilling)
            {
                patternOrChunk adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(1, 0, 0));
                if (adjChunk.chunk != null)
                    adjChunk.chunk.generateVertices();

                adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(-1, 0, 0));
                if (adjChunk.chunk != null)
                    adjChunk.chunk.generateVertices();

                adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(0, 1, 0));
                if (adjChunk.chunk != null)
                    adjChunk.chunk.generateVertices();

                adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(0, -1, 0));
                if (adjChunk.chunk != null)
                    adjChunk.chunk.generateVertices();

                adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(0, 0, 1));
                if (adjChunk.chunk != null)
                    adjChunk.chunk.generateVertices();

                adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(0, 0, -1));
                if (adjChunk.chunk != null)
                    adjChunk.chunk.generateVertices();
            }
        }

        public void assignNeighbors(int x, int y, int z, bool isFilling)
        {
            Block block = blocks[x, y, z];

            #region north
            if (x + 1 < size)
            {
                if (blocks[x + 1, y, z] != null)
                {
                    blocks[x + 1, y, z].neighborS = block;
                    block.neighborN = blocks[x + 1, y, z];
                }
            }
            else
            {
                IntVector3 chunkAddr = position + new IntVector3(1, 0, 0);
                Block adj = MainGame.mainGame.getBlock(new BlockAddress(chunkAddr, new IntVector3(0, y, z)));
                if (adj != null)
                {
                    adj.neighborS = block;
                    block.neighborN = adj;
                }
                if (!isFilling)
                {
                    patternOrChunk adjChunk = MainGame.mainGame.getPatternOrChunk(chunkAddr);
                    if (adjChunk.chunk != null)
                        adjChunk.chunk.generateVertices();
                }
            }
            #endregion north

            #region south
            if (x - 1 >= 0)
            {
                if (blocks[x - 1, y, z] != null)
                {
                    blocks[x - 1, y, z].neighborN = block;
                    block.neighborS = blocks[x - 1, y, z];
                }
            }
            else
            {
                IntVector3 chunkAddr = position + new IntVector3(-1, 0, 0);
                Block adj = MainGame.mainGame.getBlock(new BlockAddress(chunkAddr, new IntVector3(Sizes.ChunkSize - 1, y, z)));
                if (adj != null)
                {
                    adj.neighborN = block;
                    block.neighborS = adj;
                }
                if (!isFilling)
                {
                    patternOrChunk adjChunk = MainGame.mainGame.getPatternOrChunk(chunkAddr);
                    if (adjChunk.chunk != null)
                        adjChunk.chunk.generateVertices();
                }
            }
            #endregion south

            #region up
            if (y + 1 < size)
            {
                if (blocks[x, y + 1, z] != null)
                {
                    blocks[x, y + 1, z].neighborD = block;
                    block.neighborU = blocks[x, y + 1, z];
                }
            }
            else
            {
                IntVector3 chunkAddr = position + new IntVector3(0, 1, 0);
                Block adj = MainGame.mainGame.getBlock(new BlockAddress(chunkAddr, new IntVector3(x, 0, z)));
                if (adj != null)
                {
                    adj.neighborD = block;
                    block.neighborU = adj;
                }
                if (!isFilling)
                {
                    patternOrChunk adjChunk = MainGame.mainGame.getPatternOrChunk(chunkAddr);
                    if (adjChunk.chunk != null)
                        adjChunk.chunk.generateVertices();
                }
            }
            #endregion up

            #region down

            if (y - 1 >= 0)
            {
                if (blocks[x, y - 1, z] != null)
                {
                    blocks[x, y - 1, z].neighborU = block;
                    block.neighborD = blocks[x, y - 1, z];
                }
            }
            else
            {
                IntVector3 chunkAddr = position + new IntVector3(0, -1, 0);
                Block adj = MainGame.mainGame.getBlock(new BlockAddress(chunkAddr, new IntVector3(x, Sizes.ChunkSize - 1, z)));
                if (adj != null)
                {
                    adj.neighborU = block;
                    block.neighborD = adj;
                }
                if (!isFilling)
                {
                    patternOrChunk adjChunk = MainGame.mainGame.getPatternOrChunk(chunkAddr);
                    if (adjChunk.chunk != null)
                        adjChunk.chunk.generateVertices();
                }
            }
            #endregion down

            #region east
            if (z + 1 < size)
            {
                if (blocks[x, y, z + 1] != null)
                {
                    blocks[x, y, z + 1].neighborW = block;
                    block.neighborE = blocks[x, y, z + 1];
                }
            }
            else
            {
                IntVector3 chunkAddr = position + new IntVector3(0, 0, 1);
                Block adj = MainGame.mainGame.getBlock(new BlockAddress(chunkAddr, new IntVector3(x, y, 0)));
                if (adj != null)
                {
                    adj.neighborW = block;
                    block.neighborE = adj;
                }
                if (!isFilling)
                {
                    patternOrChunk adjChunk = MainGame.mainGame.getPatternOrChunk(chunkAddr);
                    if (adjChunk.chunk != null)
                        adjChunk.chunk.generateVertices();
                }
            }
            #endregion east

            #region west
            if (z - 1 >= 0)
            {
                if (blocks[x, y, z - 1] != null)
                {
                    blocks[x, y, z - 1].neighborE = block;
                    block.neighborW = blocks[x, y, z - 1];
                }
            }
            else
            {
                IntVector3 chunkAddr = position + new IntVector3(0, 0, -1);
                Block adj = MainGame.mainGame.getBlock(new BlockAddress(chunkAddr, new IntVector3(x, y, Sizes.ChunkSize - 1)));
                if (adj != null)
                {
                    adj.neighborE = block;
                    block.neighborW = adj;
                }
                if (!isFilling)
                {
                    patternOrChunk adjChunk = MainGame.mainGame.getPatternOrChunk(chunkAddr);
                    if (adjChunk.chunk != null)
                        adjChunk.chunk.generateVertices();
                }
            }
            #endregion west
        }

        public bool isTransparent()
        {
            return indices.Length == 0;
        }
        //*/

        public void endFill()
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        assignNeighbors(x, y, z, true);
                    }
                }
            }

            patternOrChunk adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(1,0,0));
            if (adjChunk.chunk != null)
                adjChunk.chunk.generateVertices();

            adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(-1, 0, 0));
            if (adjChunk.chunk != null)
                adjChunk.chunk.generateVertices();

            adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(0, 1, 0));
            if (adjChunk.chunk != null)
                adjChunk.chunk.generateVertices();

            adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(0, -1, 0));
            if (adjChunk.chunk != null)
                adjChunk.chunk.generateVertices();

            adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(0, 0, 1));
            if (adjChunk.chunk != null)
                adjChunk.chunk.generateVertices();

            adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(0, 0, -1));
            if (adjChunk.chunk != null)
                adjChunk.chunk.generateVertices();

            generateVertices();
        }

        public Block getBlock(IntVector3 loc)
        {
            return getBlock(loc.X, loc.Y, loc.Z);
        }

        public Block getBlock(int x, int y, int z)
        {
            if (x >= size || y >= size || z >= size || x < 0 || y < 0 || z < 0)
                return null;
            return blocks[x, y, z];
        }

        #region graphics

        public void generateVertices()
        {
            #region neighborChecks
            patternOrChunk adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(1, 0, 0));
            if (adjChunk.chunk == null)
            {
                //MainGame.mainGame.Log("Skipping " + position + ", north (" + (position + new IntVector3(1,0,0)) + ") unset");
                return;
            }

            adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(-1, 0, 0));
            if (adjChunk.chunk == null)
            {
                //MainGame.mainGame.Log("Skipping " + position + ", south (" + (position + new IntVector3(-1, 0, 0)) + ") unset");
                return;
            }

            adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(0, 1, 0));
            if (adjChunk.chunk == null)
            {
                //MainGame.mainGame.Log("Skipping " + position + ", top (" + (position + new IntVector3(0, 1, 0)) + ") unset");
                return;
            }

            adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(0, -1, 0));
            if (adjChunk.chunk == null)
            {
                //MainGame.mainGame.Log("Skipping " + position + ", bottom (" + (position + new IntVector3(0, -1, 0)) + ") unset");
                return;
            }

            adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(0, 0, 1));
            if (adjChunk.chunk == null)
            {
                //MainGame.mainGame.Log("Skipping " + position + ", east (" + (position + new IntVector3(0, 0, 1)) + ") unset");
                return;
            }

            adjChunk = MainGame.mainGame.getPatternOrChunk(position + new IntVector3(0, 0, -1));
            if (adjChunk.chunk == null)
            {
                //MainGame.mainGame.Log("Skipping " + position + ", west (" + (position + new IntVector3(0, 0, -1)) + ") unset");
                return;
            }
            #endregion neighborChecks

            MainGame.mainGame.Log("Drawing " + position);
            Screens.LoadingScreen.ChangeMessage("Generating vertices for " + position);
            //there SHOULD be something set up so that it only needs to rebuild the vertices for changed sections
            //but i don't believe there currently is

            //ofc that does mean significantly more memory cost... maybe it's not worth it.


            List<VertexPositionColorNormal> verts = new List<VertexPositionColorNormal>(4096);
            List<short> inds = new List<short>(4096);
            //Having the 4096 should make it not have to increase the capacity often, since 4096 is the number of blocks in a chunk,
            //and not all of them will be distinct, probably.
            //(Of course there's... 125 * 24 potential vertices per block, but still.)

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        blocks[x, y, z].createVertices(verts, inds, new Vector3(position.X * Sizes.ChunkSize + x, position.Y * Sizes.ChunkSize + y, position.Z * Sizes.ChunkSize + z));
                    }
                }
            }


            vertices = verts.ToArray();

            //this could be problematic when you remove the last block in a chunk...
            if (vertices.Length > 0)
            {
                if (vertices.Length > maxVerts)
                {
                    GraphicsDevice graphics = vBuff.GraphicsDevice;
                    if (vBuff != null)
                        vBuff.Dispose();
                    maxVerts = vertices.Length + 1000;
                    vBuff = new DynamicVertexBuffer(graphics, typeof(VertexPositionColorNormal), maxVerts, BufferUsage.WriteOnly);
                    //vBuff.ContentLost += new EventHandler<EventArgs>(verticesLostHandler);
                    //MainGame.mainGame.Log("Vertices extended to " + maxVerts);
                }
                vBuff.SetData<VertexPositionColorNormal>(vertices, 0, vertices.Length);
                MainGame.mainGame.Log("Vertices: " + vertices.Length);
            }

            indices = inds.ToArray();

            if (indices.Length > 0)
            {
                if (indices.Length > maxInds)
                {
                    GraphicsDevice graphics = iBuff.GraphicsDevice;
                    if (iBuff != null)
                        iBuff.Dispose();
                    maxInds = indices.Length + 1500;
                    iBuff = new DynamicIndexBuffer(graphics, IndexElementSize.SixteenBits, maxInds, BufferUsage.WriteOnly);
                    //iBuff.ContentLost += new EventHandler<EventArgs>(indicesLostHandler);
                    //MainGame.mainGame.Log("Indices extended to " + maxInds);
                }
                iBuff.SetData<short>(indices, 0, indices.Length);
                MainGame.mainGame.Log("Indices: " + indices.Length);
            }


            Screens.LoadingScreen.ChangeMessage("Finished generating vertices for " + position);
        }

        void verticesLostHandler(object sender, EventArgs e)
        {
            vBuff.SetData<VertexPositionColorNormal>(vertices);
        }

        /// <summary>
        /// Note: Assumes that endFill() or assignBlock() was called since the last fillAssign() call.
        /// </summary>
        /// <returns></returns>
        public DynamicVertexBuffer getVertexBuffer()
        {
            return vBuff;
        }

        public int getNumVertices()
        {
            return vertices.Length;
        }

        void indicesLostHandler(object sender, EventArgs e)
        {
            iBuff.SetData<short>(indices);
        }

        public DynamicIndexBuffer getIndexBuffer()
        {
            return iBuff;
        }

        public int getNumIndices()
        {
            return indices.Length;
        }

        #endregion graphics
        
    }
}
