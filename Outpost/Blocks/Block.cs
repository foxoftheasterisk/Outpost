using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using OutpostLibrary;
using OutpostLibrary.Content;
using OutpostLibrary.Navigation;
using Microsoft.Xna.Framework;

namespace Outpost.Blocks
{
    public interface Block
    {
        //Indexing starts at 0 in the south, west, bottom corner.
        //X is north-south, Y up-down, Z east-west.

        #region opacity

        bool opaqN
        {
            get;
        }

        bool opaqS
        {
            get;
        }

        bool opaqE
        {
            get;
        }

        bool opaqW
        {
            get;
        }

        bool opaqU
        {
            get;
        }

        bool opaqD
        {
            get;
        }

        bool isTransparent
        {
            get;
        }

        #endregion opacity

        #region traits

        Material primary
        {
            get;
        }

        Solidity solidity
        {
            get;
        }

        #endregion traits

        #region neighbors

        Block neighborN
        {
            get; set;
        }

        Block neighborS
        {
            get; set;
        }

        Block neighborE
        {
            get; set;
        }

        Block neighborW
        {
            get; set;
        }

        Block neighborU
        {
            get; set;
        }

        Block neighborD
        {
            get; set;
        }

        #endregion neighbors

        /// <summary>
        /// An identifier for use in mapgen
        /// Hopefully temporary
        /// </summary>
        String mapGenName
        {
            get;
        }

        //void createVertices(List<VertexPositionColorNormal> verts, List<short> inds, Vector3 offset);

        /// <summary>
        /// Gets the material at the given position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        Material this[int x, int y, int z]
        {
            get;
        }

        /// <summary>
        /// Gets the material at the given position.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        Material this[IntVector3 location]
        {
            get;
        }

        //for wearing and such
        //void neighborChanged(Block previous, Block current);

        Item[] drops();

    }

    public static class BlockMethods
    {
        
        public static void createVertices(this Block block, List<VertexPositionColorNormal> verts, List<short> inds, Vector3 offset)
        {
            if (block.isTransparent)
                return;

            List<IntVector3> completedList = new List<IntVector3>();

            int highEdge = Sizes.VoxelsPerEdge - 1;

            #region north
            if (block.neighborN != null && !block.neighborN.opaqS)
            {
                for (int i = 0; i < Sizes.VoxelsPerEdge; i++)
                {
                    for (int j = 0; j < Sizes.VoxelsPerEdge; j++)
                    {
                        if (block.neighborN[0, i, j].transparency == Transparency.opaque)
                            continue;
                        Material m = block[highEdge, i, j];
                        if (m.transparency != Transparency.transparent)
                        {
                            renderVoxelFace(verts, inds, m[highEdge, i, j], offset + new Vector3((float)highEdge / Sizes.VoxelsPerEdge, ((float)i) / Sizes.VoxelsPerEdge, ((float)j) / Sizes.VoxelsPerEdge), Directions.CompassDirection.N);
                        }

                        if(m.transparency != Transparency.opaque)
                        {
                            createVerticesRecurse(block, verts, inds, offset, completedList, new IntVector3(highEdge, i, j));
                        }
                    }
                }
            }
            #endregion north

            #region south
            if (block.neighborS != null && !block.neighborS.opaqN)
            {
                for (int i = 0; i < Sizes.VoxelsPerEdge; i++)
                {
                    for (int j = 0; j < Sizes.VoxelsPerEdge; j++)
                    {
                        if (block.neighborS[highEdge, i, j].transparency == Transparency.opaque)
                            continue;
                        Material m = block[0, i, j];
                        if (m.transparency != Transparency.transparent)
                        {
                            renderVoxelFace(verts, inds, m[0, i, j], offset + new Vector3(0, ((float)i) / Sizes.VoxelsPerEdge, ((float)j) / Sizes.VoxelsPerEdge), Directions.CompassDirection.S);
                        }

                        if (m.transparency != Transparency.opaque)
                        {
                            createVerticesRecurse(block, verts, inds, offset, completedList, new IntVector3(0, i, j));
                        }
                    }
                }
            }
            #endregion south

            #region east
            if (block.neighborE != null && !block.neighborE.opaqW)
            {
                for (int i = 0; i < Sizes.VoxelsPerEdge; i++)
                {
                    for (int j = 0; j < Sizes.VoxelsPerEdge; j++)
                    {
                        if (block.neighborE[i, j, 0].transparency == Transparency.opaque)
                            continue;
                        Material m = block[i, j, highEdge];
                        if (m.transparency != Transparency.transparent)
                        {
                            renderVoxelFace(verts, inds, m[i, j, highEdge], offset + new Vector3(((float)i) / Sizes.VoxelsPerEdge, ((float)j) / Sizes.VoxelsPerEdge, (float)highEdge / Sizes.VoxelsPerEdge), Directions.CompassDirection.E);
                        }

                        if (m.transparency != Transparency.opaque)
                        {
                            createVerticesRecurse(block, verts, inds, offset, completedList, new IntVector3(i, j, highEdge));
                        }
                    }
                }
            }
            #endregion east

            #region west
            if (block.neighborW != null && !block.neighborW.opaqE)
            {
                for (int i = 0; i < Sizes.VoxelsPerEdge; i++)
                {
                    for (int j = 0; j < Sizes.VoxelsPerEdge; j++)
                    {
                        if (block.neighborW[i, j, highEdge].transparency == Transparency.opaque)
                            continue;
                        Material m = block[i, j, 0];
                        if (m.transparency != Transparency.transparent)
                        {
                            renderVoxelFace(verts, inds, m[i, j, 0], offset + new Vector3(((float)i) / Sizes.VoxelsPerEdge, ((float)j) / Sizes.VoxelsPerEdge, 0), Directions.CompassDirection.W);
                        }

                        if (m.transparency != Transparency.opaque)
                        {
                            createVerticesRecurse(block, verts, inds, offset, completedList, new IntVector3(i, j, 0));
                        }
                    }
                }
            }
            #endregion west

            #region up
            if (block.neighborU != null && !block.neighborU.opaqD)
            {
                for (int i = 0; i < Sizes.VoxelsPerEdge; i++)
                {
                    for (int j = 0; j < Sizes.VoxelsPerEdge; j++)
                    {
                        if (block.neighborU[i, 0, j].transparency == Transparency.opaque)
                            continue;
                        Material m = block[i, highEdge, j];
                        if (m.transparency != Transparency.transparent)
                        {
                            renderVoxelFace(verts, inds, m[i, highEdge, j], offset + new Vector3(((float)i) / Sizes.VoxelsPerEdge, (float)highEdge / Sizes.VoxelsPerEdge, ((float)j) / Sizes.VoxelsPerEdge), Directions.CompassDirection.U);
                        }

                        if (m.transparency != Transparency.opaque)
                        {
                            createVerticesRecurse(block, verts, inds, offset, completedList, new IntVector3(i, highEdge, j));
                        }
                    }
                }
            }
            #endregion up

            #region down
            if (block.neighborD != null && !block.neighborD.opaqU)
            {
                for (int i = 0; i < Sizes.VoxelsPerEdge; i++)
                {
                    for (int j = 0; j < Sizes.VoxelsPerEdge; j++)
                    {
                        if (block.neighborD[i, highEdge, j].transparency == Transparency.opaque)
                            continue;
                        Material m = block[i, 0, j];
                        if (m.transparency != Transparency.transparent)
                        {
                            renderVoxelFace(verts, inds, m[i, 0, j], offset + new Vector3(((float)i) / Sizes.VoxelsPerEdge, 0, ((float)j) / Sizes.VoxelsPerEdge), Directions.CompassDirection.D);
                        }

                        if (m.transparency != Transparency.opaque)
                        {
                            createVerticesRecurse(block, verts, inds, offset, completedList, new IntVector3(i, 0, j));
                        }
                    }
                }
            }
            #endregion down
            //*/
        }
        //*/
        private static void createVerticesRecurse(Block block, List<VertexPositionColorNormal> verts, List<short> inds, Vector3 offset, List<IntVector3> completed, IntVector3 adding)
        {
            if (completed.Contains(adding))
                return;

            completed.Add(adding);

            #region north
            if (adding.X + 1 < Sizes.VoxelsPerEdge)
            {
                Material m = block[adding.X + 1, adding.Y, adding.Z];
                if (m.transparency != Transparency.transparent)
                {
                    renderVoxelFace(verts, inds, m[adding.X + 1, adding.Y, adding.Z], offset + new Vector3((adding.X + 1) / (float)Sizes.VoxelsPerEdge, adding.Y / (float)Sizes.VoxelsPerEdge, adding.Z / (float)Sizes.VoxelsPerEdge), Directions.CompassDirection.S);
                }

                if (m.transparency != Transparency.opaque)
                {
                    createVerticesRecurse(block, verts, inds, offset, completed, adding + new IntVector3(1, 0, 0));
                }
            }
            #endregion north

            #region south
            if (adding.X - 1 >= 0)
            {
                Material m = block[adding.X - 1, adding.Y, adding.Z];
                if (m.transparency != Transparency.transparent)
                {
                    renderVoxelFace(verts, inds, m[adding.X - 1, adding.Y, adding.Z], offset + new Vector3((adding.X - 1) / (float)Sizes.VoxelsPerEdge, adding.Y / (float)Sizes.VoxelsPerEdge, adding.Z / (float)Sizes.VoxelsPerEdge), Directions.CompassDirection.N);
                }

                if (m.transparency != Transparency.opaque)
                {
                    createVerticesRecurse(block, verts, inds, offset, completed, adding + new IntVector3(-1, 0, 0));
                }
            }
            #endregion south

            #region up
            if (adding.Y + 1 < Sizes.VoxelsPerEdge)
            {
                Material m = block[adding.X, adding.Y + 1, adding.Z];
                if (m.transparency != Transparency.transparent)
                {
                    renderVoxelFace(verts, inds, m[adding.X, adding.Y + 1, adding.Z], offset + new Vector3(adding.X / (float)Sizes.VoxelsPerEdge, (adding.Y + 1) / (float)Sizes.VoxelsPerEdge, adding.Z / (float)Sizes.VoxelsPerEdge), Directions.CompassDirection.D);
                }

                if (m.transparency != Transparency.opaque)
                {
                    createVerticesRecurse(block, verts, inds, offset, completed, adding + new IntVector3(0, 1, 0));
                }
            }
            #endregion up

            #region down
            if (adding.Y - 1 >= 0)
            {
                Material m = block[adding.X, adding.Y - 1, adding.Z];
                if (m.transparency != Transparency.transparent)
                {
                    renderVoxelFace(verts, inds, m[adding.X, adding.Y - 1, adding.Z], offset + new Vector3((adding.X) / (float)Sizes.VoxelsPerEdge, (adding.Y - 1) / (float)Sizes.VoxelsPerEdge, adding.Z / (float)Sizes.VoxelsPerEdge), Directions.CompassDirection.U);
                }

                if (m.transparency != Transparency.opaque)
                {
                    createVerticesRecurse(block, verts, inds, offset, completed, adding + new IntVector3(0, -1, 0));
                }
            }
            #endregion down

            #region east
            if (adding.Z + 1 < Sizes.VoxelsPerEdge)
            {
                Material m = block[adding.X, adding.Y, adding.Z + 1];
                if (m.transparency != Transparency.transparent)
                {
                    renderVoxelFace(verts, inds, m[adding.X, adding.Y, adding.Z + 1], offset + new Vector3(adding.X / (float)Sizes.VoxelsPerEdge, (adding.Y) / (float)Sizes.VoxelsPerEdge, (adding.Z + 1) / (float)Sizes.VoxelsPerEdge), Directions.CompassDirection.W);
                }

                if (m.transparency != Transparency.opaque)
                {
                    createVerticesRecurse(block, verts, inds, offset, completed, adding + new IntVector3(0, 0, 1));
                }
            }
            #endregion east

            #region west
            if (adding.Z - 1 >= 0)
            {
                Material m = block[adding.X, adding.Y, adding.Z - 1];
                if (m.transparency != Transparency.transparent)
                {
                    renderVoxelFace(verts, inds, m[adding.X, adding.Y, adding.Z - 1], offset + new Vector3((adding.X) / (float)Sizes.VoxelsPerEdge, (adding.Y) / (float)Sizes.VoxelsPerEdge, (adding.Z - 1) / (float)Sizes.VoxelsPerEdge), Directions.CompassDirection.E);
                }

                if (m.transparency != Transparency.opaque)
                {
                    createVerticesRecurse(block, verts, inds, offset, completed, adding + new IntVector3(0, 0, -1));
                }
            }
            #endregion west
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="inds"></param>
        /// <param name="color"></param>
        /// <param name="offset">The voxel's position in 3D space.</param>
        /// <param name="face"></param>
        private static void renderVoxelFace(List<VertexPositionColorNormal> verts, List<short> inds, Color color, Vector3 offset, Directions.CompassDirection face)
        {
            short currentIndex = (short)verts.Count;

            float size = 1.0f / Sizes.VoxelsPerEdge;

            switch (face)
            {
                case Directions.CompassDirection.N:
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(size, 0, 0), color, new Vector3(1, 0, 0)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(size, 0, size), color, new Vector3(1, 0, 0)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(size, size, 0), color, new Vector3(1, 0, 0)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(size, size, size), color, new Vector3(1, 0, 0)));
                    break;
                case Directions.CompassDirection.S:
                    verts.Add(new VertexPositionColorNormal(offset, color, new Vector3(-1, 0, 0)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, size), color, new Vector3(-1, 0, 0)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, size, 0), color, new Vector3(-1, 0, 0)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, size, size), color, new Vector3(-1, 0, 0)));
                    break;
                case Directions.CompassDirection.E:
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, size), color, new Vector3(0, 0, 1)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(size, 0, size), color, new Vector3(0, 0, 1)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, size, size), color, new Vector3(0, 0, 1)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(size, size, size), color, new Vector3(0, 0, 1)));
                    break;
                case Directions.CompassDirection.W:
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, 0), color, new Vector3(0, 0, -1)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(size, 0, 0), color, new Vector3(0, 0, -1)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, size, 0), color, new Vector3(0, 0, -1)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(size, size, 0), color, new Vector3(0, 0, -1)));
                    break;
                case Directions.CompassDirection.U:
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, size, 0), color, new Vector3(0, 1, 0)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, size, size), color, new Vector3(0, 1, 0)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(size, size, 0), color, new Vector3(0, 1, 0)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(size, size, size), color, new Vector3(0, 1, 0)));
                    break;
                case Directions.CompassDirection.D:
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, 0), color, new Vector3(0, -1, 0)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, size), color, new Vector3(0, -1, 0)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(size, 0, 0), color, new Vector3(0, -1, 0)));
                    verts.Add(new VertexPositionColorNormal(offset + new Vector3(size, 0, size), color, new Vector3(0, -1, 0)));
                    break;
                default:
                    Logger.Log("Nonstandard CompassDirection??");
                    break;
            }

            inds.Add(currentIndex);
            inds.Add((short)(currentIndex + 1));
            inds.Add((short)(currentIndex + 3));
            inds.Add(currentIndex);
            inds.Add((short)(currentIndex + 2));
            inds.Add((short)(currentIndex + 3));
        }
    }
}
