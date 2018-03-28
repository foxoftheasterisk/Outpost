using Microsoft.Xna.Framework;
using OutpostEngine.Blocks;
using OutpostEngine.Map;
using OutpostLibrary;
using OutpostLibrary.Content;
using OutpostLibrary.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutpostEngine
{
    static class CollisionManager
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

       

        //possible efficiency increaser: create a combined get+change block function
        //probably not actually very helpful

        public delegate bool blockChecker(BlockAddress blockToCheck);

        public static (BlockAddress? blockFound, BlockAddress? blockBefore) FindBlock(ChunkAddress chunk, Vector3 posInChunk, Vector3 directionToSeek, float stopDist, blockChecker isAcceptable)
        {
            Vector3 unitX, unitY, unitZ;
            Vector3 nextX, nextY, nextZ;
            float xDist, yDist, zDist;
            float fTemp;
            Vector3 vTemp;
            BlockAddress bTemp;
            BlockAddress previous = new BlockAddress(chunk, posInChunk);

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
                    return (null, null);
                if (fTemp == xDist)
                {
                    vTemp = new Vector3(unitX.X / 10, 0, 0);
                    bTemp = new BlockAddress(chunk, nextX + vTemp);
                    if (isAcceptable(bTemp))
                        return (bTemp, previous);
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
                        return (bTemp, previous);
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
                        return (bTemp, previous);
                    nextZ += unitZ;
                    zDist += unitZ.Length();
                    previous = bTemp;
                    continue;
                }

                Logger.Log("findBlock reached unreachable point?");
                throw new Exception("findBlock reached unreachable point?");
            }
            #endregion
        }
    }
}
