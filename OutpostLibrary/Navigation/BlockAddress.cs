﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace OutpostLibrary.Navigation
{
    public struct BlockAddress
    {
        public string world;
        public IntVector3 chunk, block;

        public int x
        {
            get
            {
                return chunk.X * Sizes.ChunkSize + block.X;
            }
        }
        public int y
        {
            get
            {
                return chunk.Y * Sizes.ChunkSize + block.Y;
            }
        }
        public int z
        {
            get
            {
                return chunk.Z * Sizes.ChunkSize + block.Z;
            }
        }

        public override string ToString()
        {
            return world + ": " + chunk.X + ":" + block.X + ", " + chunk.Y + ":" + block.Y + ", " + chunk.Z + ":" + block.Z;
        }

        public BlockAddress(ChunkAddress chunk, IntVector3 block)
            : this(chunk.world, chunk.position, block)
        { }

        public BlockAddress(string _world)
            :this(_world, new IntVector3(0), new IntVector3(0))
        { }

        //the one real constructor
        public BlockAddress(string _world, IntVector3 chunkBase, IntVector3 blockBase)
        {
            world = _world;
            chunk = new IntVector3(chunkBase);
            block = new IntVector3(blockBase);

            while (block.X >= 16)
            {
                block.X -= 16;
                chunk.X += 1;
            }
            while (block.X < 0)
            {
                block.X += 16;
                chunk.X -= 1;
            }
            while (block.Y >= 16)
            {
                block.Y -= 16;
                chunk.Y += 1;
            }
            while (block.Y < 0)
            {
                block.Y += 16;
                chunk.Y -= 1;
            }
            while (block.Z >= 16)
            {
                block.Z -= 16;
                chunk.Z += 1;
            }
            while (block.Z < 0)
            {
                block.Z += 16;
                chunk.Z -= 1;
            }

        }

        public BlockAddress(string _world, IntVector3 chunk, Vector3 block)
            : this(_world, chunk, new IntVector3((int)Math.Floor(block.X), (int)Math.Floor(block.Y), (int)Math.Floor(block.Z)))
        { }

        public BlockAddress(ChunkAddress chunk, Vector3 block)
            : this(chunk.world, chunk.position, new IntVector3((int)Math.Floor(block.X), (int)Math.Floor(block.Y), (int)Math.Floor(block.Z)))
        { }
    }
}
