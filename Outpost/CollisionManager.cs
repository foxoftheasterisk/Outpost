using Microsoft.Xna.Framework;
using OutpostCore.Blocks;
using OutpostCore.Map;
using OutpostLibrary;
using OutpostLibrary.Content;
using OutpostLibrary.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutpostCore
{
    class CollisionManager
    {


        //TODO: Review and most likely rewrite collision algorithms

        /// <summary>
        /// NOT FULLY IMPLEMENTED: Does not use the x/z size.  Or the direction, for that matter.
        /// ALSO, does not wrap outside of the chunk.
        /// </summary>
        /// <param name="chunkBase">The chunk containing the entity</param>
        /// <param name="posInChunk">The entity's position in the chunk</param>
        /// <param name="direction">The direction the entity is facing, for bounding-box purposes</param>
        /// <param name="size">The size of the entity</param>
        /// <returns></returns>
        public static Solidity[][][] detectCollision(ChunkAddress chunkBase, Vector3 posInChunk, Vector3 direction, IntVector3 size)
        {

            if (posInChunk.X < 0)
            {
                posInChunk.X += 16;
                chunkBase.position += new IntVector3(-1, 0, 0);
            }
            if (posInChunk.X > 16)
            {
                posInChunk.X -= 16;
                chunkBase.position += new IntVector3(1, 0, 0);
            }
            if (posInChunk.Y < 0)
            {
                posInChunk.Y += 16;
                chunkBase.position += new IntVector3(0, -1, 0);
            }
            if (posInChunk.Y > 16)
            {
                posInChunk.Y -= 16;
                chunkBase.position += new IntVector3(0, 1, 0);
            }
            if (posInChunk.Z < 0)
            {
                posInChunk.Z += 16;
                chunkBase.position += new IntVector3(0, 0, -1);
            }
            if (posInChunk.Z < 0)
            {
                posInChunk.Z += 16;
                chunkBase.position += new IntVector3(0, 0, 1);
            }

            Chunk chunk = MapManager.Map[chunkBase];
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
    }
}
