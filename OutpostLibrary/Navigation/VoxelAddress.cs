using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace OutpostLibrary.Navigation
{
    class VoxelAddress
    {
        public string world;
        public IntVector3 chunk, block, voxel;

        public int x
        {
            get
            {
                return (chunk.X * Sizes.ChunkSize + block.X) * Sizes.VoxelsPerEdge + voxel.X;
            }
        }
        public int y
        {
            get
            {
                return (chunk.Y * Sizes.ChunkSize + block.Y) * Sizes.VoxelsPerEdge + voxel.Y;
            }
        }
        public int z
        {
            get
            {
                return (chunk.Z * Sizes.ChunkSize + block.Z) * Sizes.VoxelsPerEdge + voxel.Z;
            }
        }

        public override string ToString()
        {
            return world + ": " + chunk.X + ":" + block.X + ":" + voxel.X + ", " + chunk.Y + ":" + block.Y + ":" + voxel.Y + ", " + chunk.Z + ":" + block.Z + ":" + voxel.Z;
        }

        //the one real constructor
        //because who wants to rewrite all those safety checks
        public VoxelAddress(string _world, IntVector3 chunkBase, IntVector3 blockBase, IntVector3 voxelBase)
        {
            world = _world;
            chunk = new IntVector3(chunkBase);
            block = new IntVector3(blockBase);
            voxel = new IntVector3(voxelBase);

            while (voxel.X >= Sizes.VoxelsPerEdge)
            {
                voxel.X -= Sizes.VoxelsPerEdge;
                block.X += 1;
            }
            while (voxel.X < 0)
            {
                voxel.X += Sizes.VoxelsPerEdge;
                block.X -= 1;
            }
            while (block.X >= Sizes.ChunkSize)
            {
                block.X -= Sizes.ChunkSize;
                chunk.X += 1;
            }
            while (block.X < 0)
            {
                block.X += Sizes.ChunkSize;
                chunk.X -= 1;
            }

            while (voxel.Y >= Sizes.VoxelsPerEdge)
            {
                voxel.Y -= Sizes.VoxelsPerEdge;
                block.Y += 1;
            }
            while (voxel.Y < 0)
            {
                voxel.Y += Sizes.VoxelsPerEdge;
                block.Y -= 1;
            }
            while (block.Y >= Sizes.ChunkSize)
            {
                block.Y -= Sizes.ChunkSize;
                chunk.Y += 1;
            }
            while (block.Y < 0)
            {
                block.Y += Sizes.ChunkSize;
                chunk.Y -= 1;
            }

            while (voxel.Z >= Sizes.VoxelsPerEdge)
            {
                voxel.Z -= Sizes.VoxelsPerEdge;
                block.Z += 1;
            }
            while (voxel.Z < 0)
            {
                voxel.Z += Sizes.VoxelsPerEdge;
                block.Z -= 1;
            }
            while (block.Z >= Sizes.ChunkSize)
            {
                block.Z -= Sizes.ChunkSize;
                chunk.Z += 1;
            }
            while (block.Z < 0)
            {
                block.Z += Sizes.ChunkSize;
                chunk.Z -= 1;
            }

        }

        public VoxelAddress(string _world)
            : this(_world, new IntVector3(0), new IntVector3(0), new IntVector3(0))
        { }

        public VoxelAddress(ChunkAddress chunk, IntVector3 block, IntVector3 voxel)
            : this(chunk.world, chunk.position, block, voxel)
        { }

        public VoxelAddress(BlockAddress block, IntVector3 voxel)
            : this(block.world, block.chunk, block.block, voxel)
        { }

        public VoxelAddress(string _world, IntVector3 chunk, Vector3 block)
            : this(_world, chunk, new IntVector3(0), new IntVector3((int)Math.Floor(block.X * Sizes.VoxelsPerEdge), (int)Math.Floor(block.Y * Sizes.VoxelsPerEdge), (int)Math.Floor(block.Z * Sizes.VoxelsPerEdge)))
        { }

        public VoxelAddress(ChunkAddress chunk, Vector3 block)
            : this(chunk.world, chunk.position, new IntVector3(0), new IntVector3((int)Math.Floor(block.X * Sizes.VoxelsPerEdge), (int)Math.Floor(block.Y * Sizes.VoxelsPerEdge), (int)Math.Floor(block.Z * Sizes.VoxelsPerEdge)))
        { }
    }
}
