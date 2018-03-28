using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using OutpostLibrary.Content;
using OutpostLibrary;
using Microsoft.Xna.Framework;
using OutpostEngine.Items;

namespace OutpostEngine.Blocks
{
    /// <summary>
    /// Defines a block that consists entirely of a single material.
    /// </summary>
    class SolidBlock : Block
    {
        Material material;

        #region opaq*

        public bool opaqN
        {
            get { return material.transparency == Transparency.opaque; }
        }

        public bool opaqS
        {
            get { return material.transparency == Transparency.opaque; }
        }

        public bool opaqE
        {
            get { return material.transparency == Transparency.opaque; }
        }

        public bool opaqW
        {
            get { return material.transparency == Transparency.opaque; }
        }

        public bool opaqU
        {
            get { return material.transparency == Transparency.opaque; }
        }

        public bool opaqD
        {
            get { return material.transparency == Transparency.opaque; }
        }

        public bool isTransparent
        {
            get
            {
                return material.transparency == Transparency.transparent;
            }
        }

        #endregion opaq*

        #region neighbors

        public Block neighborN
        {
            set { _neighborN = value; }
            get { return _neighborN; }
        }

        public Block neighborS
        {
            set { _neighborS = value; }
            get { return _neighborS; }
        }

        public Block neighborE
        {
            set { _neighborE = value; }
            get { return _neighborE; }
        }

        public Block neighborW
        {
            set { _neighborW = value; }
            get { return _neighborW; }
        }

        public Block neighborU
        {
            set { _neighborU = value; }
            get { return _neighborU; }
        }

        public Block neighborD
        {
            set { _neighborD = value; }
            get { return _neighborD; }
        }

        Block _neighborN, _neighborS, _neighborE, _neighborW, _neighborU, _neighborD;

        #endregion neighbors

        #region traits

        public Material primary
        {
            get
            {
                return material;
            }
        }

        public Solidity solidity
        {
            get
            {
                return material.solidity;
            }
        }

        public String mapGenName
        {
            get
            {
                return "Solid " + material.name;
            }
        }

        #endregion traits

        //Indexing starts at 0 in the north, west, bottom corner.
        //X is north-south, Y up-down, Z east-west.

        public SolidBlock(Material _material)
        {
            material = _material;
        }

        public override string ToString()
        {
            return "SolidBlock: " + mapGenName;
        }

        /* old createVertices
        /// <summary>
        /// Creates vertices and indices for the chunk.  This will only be called top-level; other functions recurse.
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="inds"></param>
        /// <param name="xStart"></param>
        /// <param name="yStart"></param>
        /// <param name="zStart"></param>
        public void createVertices(List<VertexPositionColorNormal> verts, List<short> inds, Vector3 offset)
        {
            if (material.transparency != Transparency.opaque)
            {
                //????
                return;
            }

            short currentIndex = (short)verts.Count;

            //bluh
            float xStart = offset.X;
            float yStart = offset.Y;
            float zStart = offset.Z;

            #region north face
            if (neighborN != null && !neighborN.opaqS)
            {
                for (float i = 0; i < Navigation.VoxelsPerEdge; i++)
                {
                    for (float j = 0; j < Navigation.VoxelsPerEdge; j++)
                    {
                        if (neighborN[0, (int)i, (int)j].material.transparency == Transparency.opaque)
                            continue;

                        verts.Add(new VertexPositionColorNormal(offset + new Vector3(1, i / Navigation.VoxelsPerEdge, j / Navigation.VoxelsPerEdge), material[Navigation.VoxelsPerEdge - 1, (int)i, (int)j], new Vector3(1, 0, 0)));
                        verts.Add(new VertexPositionColorNormal(offset + new Vector3(1, i / Navigation.VoxelsPerEdge, (j + 1) / Navigation.VoxelsPerEdge), material[Navigation.VoxelsPerEdge - 1, (int)i, (int)j], new Vector3(1, 0, 0)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + 1, yStart + (i + 1) / Navigation.VoxelsPerEdge, zStart + j / Navigation.VoxelsPerEdge), material[Navigation.VoxelsPerEdge - 1, (int)i, (int)j], new Vector3(1, 0, 0)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + 1, yStart + (i + 1) / Navigation.VoxelsPerEdge, zStart + (j + 1) / Navigation.VoxelsPerEdge), material[Navigation.VoxelsPerEdge - 1, (int)i, (int)j], new Vector3(1, 0, 0)));
                        //

                        /*
                        verts.Add(new VertexPositionColor(new Vector3(xStart, yStart + i / Navigation.VoxelsPerEdge, zStart + j / Navigation.VoxelsPerEdge), material[0, (int)i, (int)j]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart, yStart + i / Navigation.VoxelsPerEdge, zStart + (j + 1) / Navigation.VoxelsPerEdge), material[0, (int)i, (int)j]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart, yStart + (i + 1) / Navigation.VoxelsPerEdge, zStart + j / Navigation.VoxelsPerEdge), material[0, (int)i, (int)j]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart, yStart + (i + 1) / Navigation.VoxelsPerEdge, zStart + (j + 1) / Navigation.VoxelsPerEdge), material[0, (int)i, (int)j]));
                        //

                        inds.Add(currentIndex);
                        inds.Add((short)(currentIndex + 1));
                        inds.Add((short)(currentIndex + 3));
                        inds.Add(currentIndex);
                        inds.Add((short)(currentIndex + 2));
                        inds.Add((short)(currentIndex + 3));

                        currentIndex = (short)verts.Count;
                    }
                }
            }
            #endregion north face

            #region south face
            if (neighborS != null && !neighborS.opaqN)
            {
                for (float i = 0; i < Navigation.VoxelsPerEdge; i++)
                {
                    for (float j = 0; j < Navigation.VoxelsPerEdge; j++)
                    {
                        if (neighborS[Navigation.VoxelsPerEdge - 1, (int)i, (int)j].material.transparency == Transparency.opaque)
                            continue;

                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart, yStart + i / Navigation.VoxelsPerEdge, zStart + j / Navigation.VoxelsPerEdge), material[0, (int)i, (int)j], new Vector3(-1, 0, 0)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart, yStart + i / Navigation.VoxelsPerEdge, zStart + (j + 1) / Navigation.VoxelsPerEdge), material[0, (int)i, (int)j], new Vector3(-1, 0, 0)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart, yStart + (i + 1) / Navigation.VoxelsPerEdge, zStart + j / Navigation.VoxelsPerEdge), material[0, (int)i, (int)j], new Vector3(-1, 0, 0)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart, yStart + (i + 1) / Navigation.VoxelsPerEdge, zStart + (j + 1) / Navigation.VoxelsPerEdge), material[0, (int)i, (int)j], new Vector3(-1, 0, 0)));
                        //

                        /*
                        verts.Add(new VertexPositionColor(new Vector3(xStart + 1, yStart + i / Navigation.VoxelsPerEdge, yStart + j / Navigation.VoxelsPerEdge), material[Navigation.VoxelsPerEdge, (int)i, (int)j]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + 1, yStart + i / Navigation.VoxelsPerEdge, yStart + (j + 1) / Navigation.VoxelsPerEdge), material[Navigation.VoxelsPerEdge, (int)i, (int)j]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + 1, yStart + (i + 1) / Navigation.VoxelsPerEdge, yStart + j / Navigation.VoxelsPerEdge), material[Navigation.VoxelsPerEdge, (int)i, (int)j]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + 1, yStart + (i + 1) / Navigation.VoxelsPerEdge, yStart + (j + 1) / Navigation.VoxelsPerEdge), material[Navigation.VoxelsPerEdge, (int)i, (int)j]));
                        //

                        inds.Add(currentIndex);
                        inds.Add((short)(currentIndex + 1));
                        inds.Add((short)(currentIndex + 3));
                        inds.Add(currentIndex);
                        inds.Add((short)(currentIndex + 2));
                        inds.Add((short)(currentIndex + 3));

                        currentIndex = (short)verts.Count;
                    }
                }
            }
            #endregion south face

            #region east face
            if (neighborE != null && !neighborE.opaqW)
            {
                for (float i = 0; i < Navigation.VoxelsPerEdge; i++)
                {
                    for (float j = 0; j < Navigation.VoxelsPerEdge; j++)
                    {
                        if (neighborE[(int)i, (int)j, 0].material.transparency == Transparency.opaque)
                            continue;

                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart + j / Navigation.VoxelsPerEdge, zStart + 1), material[(int)i, (int)j, Navigation.VoxelsPerEdge - 1], new Vector3(0, 0, 1)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart + j / Navigation.VoxelsPerEdge, zStart + 1), material[(int)i, (int)j, Navigation.VoxelsPerEdge - 1], new Vector3(0, 0, 1)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart + (j + 1) / Navigation.VoxelsPerEdge, zStart + 1), material[(int)i, (int)j, Navigation.VoxelsPerEdge - 1], new Vector3(0, 0, 1)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart + (j + 1) / Navigation.VoxelsPerEdge, zStart + 1), material[(int)i, (int)j, Navigation.VoxelsPerEdge - 1], new Vector3(0, 0, 1)));


                        //

                        /*
                        verts.Add(new VertexPositionColor(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart + j / Navigation.VoxelsPerEdge, zStart + 1), material[(int)i, (int)j, Navigation.VoxelsPerEdge - 1]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart + j / Navigation.VoxelsPerEdge, zStart + 1), material[(int)i, (int)j, Navigation.VoxelsPerEdge - 1]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart + (j + 1) / Navigation.VoxelsPerEdge, zStart + 1), material[(int)i, (int)j, Navigation.VoxelsPerEdge - 1]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart + (j + 1) / Navigation.VoxelsPerEdge, zStart + 1), material[(int)i, (int)j, Navigation.VoxelsPerEdge - 1]));
                        //

                        inds.Add(currentIndex);
                        inds.Add((short)(currentIndex + 1));
                        inds.Add((short)(currentIndex + 3));
                        inds.Add(currentIndex);
                        inds.Add((short)(currentIndex + 2));
                        inds.Add((short)(currentIndex + 3));

                        currentIndex = (short)verts.Count;
                    }
                }
            }
            #endregion east face

            #region west face

            if (neighborW != null && !neighborW.opaqE)
            {
                for (float i = 0; i < Navigation.VoxelsPerEdge; i++)
                {
                    for (float j = 0; j < Navigation.VoxelsPerEdge; j++)
                    {
                        if (neighborW[(int)i, (int)j, Navigation.VoxelsPerEdge - 1].material.transparency == Transparency.opaque)
                            continue;

                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart + j / Navigation.VoxelsPerEdge, zStart), material[(int)i, (int)j, 0], new Vector3(0, 0, -1)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart + j / Navigation.VoxelsPerEdge, zStart), material[(int)i, (int)j, 0], new Vector3(0, 0, -1)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart + (j + 1) / Navigation.VoxelsPerEdge, zStart), material[(int)i, (int)j, 0], new Vector3(0, 0, -1)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart + (j + 1) / Navigation.VoxelsPerEdge, zStart), material[(int)i, (int)j, 0], new Vector3(0, 0, -1)));
                        //

                        /*
                        verts.Add(new VertexPositionColor(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart + j / Navigation.VoxelsPerEdge, zStart), material[(int)i, (int)j, 0]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart + j / Navigation.VoxelsPerEdge, zStart), material[(int)i, (int)j, 0]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart + (j + 1) / Navigation.VoxelsPerEdge, zStart), material[(int)i, (int)j, 0]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart + (j + 1) / Navigation.VoxelsPerEdge, zStart), material[(int)i, (int)j, 0]));
                        //

                        inds.Add(currentIndex);
                        inds.Add((short)(currentIndex + 1));
                        inds.Add((short)(currentIndex + 3));
                        inds.Add(currentIndex);
                        inds.Add((short)(currentIndex + 2));
                        inds.Add((short)(currentIndex + 3));

                        currentIndex = (short)verts.Count;
                    }
                }
            }
            #endregion west face

            #region top face

            if (neighborU != null && !neighborU.opaqD)
            {
                for (float i = 0; i < Navigation.VoxelsPerEdge; i++)
                {
                    for (float j = 0; j < Navigation.VoxelsPerEdge; j++)
                    {
                        if (neighborU[(int)i, 0, (int)j].material.transparency == Transparency.opaque)
                            continue;

                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart + 1, zStart + j / Navigation.VoxelsPerEdge), material[(int)i, Navigation.VoxelsPerEdge - 1, (int)j], new Vector3(0, 1, 0)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart + 1, zStart + (j + 1) / Navigation.VoxelsPerEdge), material[(int)i, Navigation.VoxelsPerEdge - 1, (int)j], new Vector3(0, 1, 0)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart + 1, zStart + j / Navigation.VoxelsPerEdge), material[(int)i, Navigation.VoxelsPerEdge - 1, (int)j], new Vector3(0, 1, 0)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart + 1, zStart + (j + 1) / Navigation.VoxelsPerEdge), material[(int)i, Navigation.VoxelsPerEdge - 1, (int)j], new Vector3(0, 1, 0)));
                        //

                        /*
                        verts.Add(new VertexPositionColor(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart + 1, zStart + j / Navigation.VoxelsPerEdge), material[(int)i, Navigation.VoxelsPerEdge, (int)j]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart + 1, zStart + (j + 1) / Navigation.VoxelsPerEdge), material[(int)i, Navigation.VoxelsPerEdge, (int)j]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart + 1, zStart + j / Navigation.VoxelsPerEdge), material[(int)i, Navigation.VoxelsPerEdge, (int)j]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart + 1, zStart + (j + 1) / Navigation.VoxelsPerEdge), material[(int)i, Navigation.VoxelsPerEdge, (int)j]));
                        //

                        inds.Add(currentIndex);
                        inds.Add((short)(currentIndex + 1));
                        inds.Add((short)(currentIndex + 3));
                        inds.Add(currentIndex);
                        inds.Add((short)(currentIndex + 2));
                        inds.Add((short)(currentIndex + 3));

                        currentIndex = (short)verts.Count;
                    }
                }
            }
            #endregion top face

            #region bottom face
            if (neighborD != null && !neighborD.opaqU)
            {
                for (float i = 0; i < Navigation.VoxelsPerEdge; i++)
                {
                    for (float j = 0; j < Navigation.VoxelsPerEdge; j++)
                    {
                        if (neighborD[(int)i, Navigation.VoxelsPerEdge - 1, (int)j].material.transparency == Transparency.opaque)
                            continue;

                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart, zStart + j / Navigation.VoxelsPerEdge), material[(int)i, 0, (int)j], new Vector3(0, -1, 0)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart, zStart + (j + 1) / Navigation.VoxelsPerEdge), material[(int)i, 0, (int)j], new Vector3(0, -1, 0)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart, zStart + j / Navigation.VoxelsPerEdge), material[(int)i, 0, (int)j], new Vector3(0, -1, 0)));
                        verts.Add(new VertexPositionColorNormal(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart, zStart + (j + 1) / Navigation.VoxelsPerEdge), material[(int)i, 0, (int)j], new Vector3(0, -1, 0)));
                        //

                        /*
                        verts.Add(new VertexPositionColor(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart, zStart + j / Navigation.VoxelsPerEdge), material[(int)i, 0, (int)j]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + i / Navigation.VoxelsPerEdge, yStart, zStart + (j + 1) / Navigation.VoxelsPerEdge), material[(int)i, 0, (int)j]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart, zStart + j / Navigation.VoxelsPerEdge), material[(int)i, 0, (int)j]));
                        verts.Add(new VertexPositionColor(new Vector3(xStart + (i + 1) / Navigation.VoxelsPerEdge, yStart, zStart + (j + 1) / Navigation.VoxelsPerEdge), material[(int)i, 0, (int)j]));
                        //

                        inds.Add(currentIndex);
                        inds.Add((short)(currentIndex + 1));
                        inds.Add((short)(currentIndex + 3));
                        inds.Add(currentIndex);
                        inds.Add((short)(currentIndex + 2));
                        inds.Add((short)(currentIndex + 3));

                        currentIndex = (short)verts.Count;
                    }
                }
            }
            #endregion bottom face

            
        }
        //*/

        public Material this[int x, int y, int z]
        {
            get
            {
                return material;
            }
        }

        public Material this[IntVector3 loc]
        {
            get
            {
                return material;
            }
        }

        public Item[] drops()
        {
            Item drop = new Tile(new SolidBlock(material));
            return new Item[] { drop };
        }
    }
}
