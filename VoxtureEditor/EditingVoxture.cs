using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OutpostLibrary.Content;
using OutpostLibrary.Navigation;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using Outpost;

namespace VoxtureEditor
{
    class EditingVoxture
    {

        string name;
        Voxture vox;

         

        public EditingVoxture(string _name, OutpostColor baseColor, GraphicsDevice graphics)
        {
            name = _name;
            vox = new Voxture(baseColor);

            vBuff = new DynamicVertexBuffer(graphics, typeof(VertexPositionColorNormal), numVerts, BufferUsage.WriteOnly);
            iBuff = new IndexBuffer(graphics, IndexElementSize.SixteenBits, numInds, BufferUsage.WriteOnly);
            wfVBuff = new DynamicVertexBuffer(graphics, typeof(VertexPositionColor), numWfVerts, BufferUsage.WriteOnly);
            wfIBuff = new IndexBuffer(graphics, IndexElementSize.SixteenBits, numWfInds, BufferUsage.WriteOnly);

            makeIndices();

            _rotation = Matrix.Identity;
        }


        #region graphics

        public Matrix rotation
        {
            get
            {
                return _rotation;
            }
        }
        Matrix _rotation;
        public void rotate(Matrix rotation)
        {
            _rotation = _rotation * rotation;
        }

        const float voxelSize = 0.1f;

        #region VertsAndInds
        VertexPositionColorNormal[] vertices;
        short[] indices;

        public DynamicVertexBuffer vertexBuffer
        {
            get
            {
                return vBuff;
            }
        }
        private DynamicVertexBuffer vBuff;
        public IndexBuffer indexBuffer
        {
            get
            {
                return iBuff;
            }
        }
        private IndexBuffer iBuff;

        public const int numVerts = Sizes.VoxelsPerEdge * Sizes.VoxelsPerEdge * Sizes.VoxelsPerEdge * 24;  //4 per face, can't reuse because normals
        //although, I suppose we could, if we just make the normals diagonal, if we turn off specular lighting it'll barely matter right?
        //MNEH.  sides we probably want to be able to see the specular in the editor.
        public const int numInds = Sizes.VoxelsPerEdge * Sizes.VoxelsPerEdge * Sizes.VoxelsPerEdge * 36;  //6 per face

        public VertexPositionColor[] wireframeVertices;
        public short[] wireframeIndices;
        //TODO: these should really not be public

        public DynamicVertexBuffer wireframeVertexBuffer
        {
            get
            {
                return wfVBuff;
            }
        }
        private DynamicVertexBuffer wfVBuff;
        public IndexBuffer wireframeIndexBuffer
        {
            get
            {
                return wfIBuff;
            }
        }
        private IndexBuffer wfIBuff;

        public const int numWfVerts = Sizes.VoxelsPerEdge * Sizes.VoxelsPerEdge * Sizes.VoxelsPerEdge * 8; //no duplication needed here! just one per vertex
        public const int numWfInds = Sizes.VoxelsPerEdge * Sizes.VoxelsPerEdge * Sizes.VoxelsPerEdge * 24; //2 per edge

        public void makeVertices(float spread, OutpostLibrary.IntVector3 selected)
        {
            float midpoint = (voxelSize * Sizes.VoxelsPerEdge + spread * (Sizes.VoxelsPerEdge - 1)) * 0.5f;

            List<VertexPositionColorNormal> verts = new List<VertexPositionColorNormal>();

            List<VertexPositionColor> wfVerts = new List<VertexPositionColor>();

            for (int x = 0; x < Sizes.VoxelsPerEdge; x++)
            {
                float xPos = (x * (voxelSize + spread)) - midpoint;

                for (int y = 0; y < Sizes.VoxelsPerEdge; y++)
                {
                    float yPos = (y * (voxelSize + spread)) - midpoint;

                    for (int z = 0; z < Sizes.VoxelsPerEdge; z++)
                    {
                        float zPos = (z * (voxelSize + spread)) - midpoint;

                        Vector3 pos = new Vector3(xPos, yPos, zPos);

                        bool isSelected = false;
                        if (selected.X == x && selected.Y == y && selected.Z == z)
                            isSelected = true;

                        makeVoxelVerts(verts, wfVerts, vox[x, y, z].color, pos, isSelected);
                    }
                }
            }

            vertices = verts.ToArray();
            wireframeVertices = wfVerts.ToArray();

            vBuff.SetData<VertexPositionColorNormal>(vertices);
            wfVBuff.SetData<VertexPositionColor>(wireframeVertices);
        }

        public void makeVertices(float spread, bool isSelected)
        {
           float midpoint = (voxelSize * Sizes.VoxelsPerEdge + spread * (Sizes.VoxelsPerEdge - 1)) * 0.5f;

            List<VertexPositionColorNormal> verts = new List<VertexPositionColorNormal>();

            List<VertexPositionColor> wfVerts = new List<VertexPositionColor>();

            for (int x = 0; x < Sizes.VoxelsPerEdge; x++)
            {
                float xPos = (x * (voxelSize + spread)) - midpoint;

                for (int y = 0; y < Sizes.VoxelsPerEdge; y++)
                {
                    float yPos = (y * (voxelSize + spread)) - midpoint;

                    for (int z = 0; z < Sizes.VoxelsPerEdge; z++)
                    {
                        float zPos = (z * (voxelSize + spread)) - midpoint;

                        Vector3 pos = new Vector3(xPos, yPos, zPos);

                        makeVoxelVerts(verts, vox[x, y, z].color, pos);
                    }
                }
            }

            {
                Color c = Color.Black;
                if (isSelected)
                    c = Color.Red;

                float low = -midpoint;
                float high = (Sizes.VoxelsPerEdge * voxelSize + (Sizes.VoxelsPerEdge - 1) * spread) - midpoint;

                wfVerts.Add(new VertexPositionColor(new Vector3(low, low, low), c));
                wfVerts.Add(new VertexPositionColor(new Vector3(high, low, low), c));
                wfVerts.Add(new VertexPositionColor(new Vector3(high, high, low), c));
                wfVerts.Add(new VertexPositionColor(new Vector3(low, high, low), c));
                wfVerts.Add(new VertexPositionColor(new Vector3(low, low, high), c));
                wfVerts.Add(new VertexPositionColor(new Vector3(high, low, high), c));
                wfVerts.Add(new VertexPositionColor(new Vector3(high, high, high), c));
                wfVerts.Add(new VertexPositionColor(new Vector3(low, high, high), c));
            }

            vertices = verts.ToArray();
            wireframeVertices = wfVerts.ToArray();

            vBuff.SetData<VertexPositionColorNormal>(vertices);
            wfVBuff.SetData<VertexPositionColor>(wireframeVertices);
        }

        //public DynamicVertexBuffer getVertexBuffer

        private void makeIndices()
        {

            List<short> inds = new List<short>();
            List<short> wfInds = new List<short>();

            short currentIndex = 0;
            short currentWfIndex = 0;

            for (int x = 0; x < Sizes.VoxelsPerEdge; x++)
            {
                for (int y = 0; y < Sizes.VoxelsPerEdge; y++)
                {
                    for (int z = 0; z < Sizes.VoxelsPerEdge; z++)
                    {
                        makeVoxelInds(inds, currentIndex, wfInds, currentWfIndex);

                        currentIndex += 24;
                        currentWfIndex += 8;
                    }
                }
            }

            indices = inds.ToArray();
            wireframeIndices = wfInds.ToArray();

            iBuff.SetData<short>(indices);
            wfIBuff.SetData<short>(wireframeIndices);
        }

        private static void makeVoxelVerts(List<VertexPositionColorNormal> verts, Color color, Vector3 offset)
        {
            #region north
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, 0, 0), color, new Vector3(1, 0, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, 0, voxelSize), color, new Vector3(1, 0, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, voxelSize, 0), color, new Vector3(1, 0, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, voxelSize, voxelSize), color, new Vector3(1, 0, 0)));
            #endregion north

            #region south
            verts.Add(new VertexPositionColorNormal(offset, color, new Vector3(-1, 0, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, voxelSize), color, new Vector3(-1, 0, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, voxelSize, 0), color, new Vector3(-1, 0, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, voxelSize, voxelSize), color, new Vector3(-1, 0, 0)));
            #endregion south

            #region east
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, voxelSize), color, new Vector3(0, 0, 1)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, 0, voxelSize), color, new Vector3(0, 0, 1)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, voxelSize, voxelSize), color, new Vector3(0, 0, 1)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, voxelSize, voxelSize), color, new Vector3(0, 0, 1)));
            #endregion east

            #region west
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, 0), color, new Vector3(0, 0, -1)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, 0, 0), color, new Vector3(0, 0, -1)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, voxelSize, 0), color, new Vector3(0, 0, -1)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, voxelSize, 0), color, new Vector3(0, 0, -1)));
            #endregion west

            #region up
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, voxelSize, 0), color, new Vector3(0, 1, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, voxelSize, voxelSize), color, new Vector3(0, 1, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, voxelSize, 0), color, new Vector3(0, 1, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, voxelSize, voxelSize), color, new Vector3(0, 1, 0)));
            #endregion up

            #region down
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, 0), color, new Vector3(0, -1, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, voxelSize), color, new Vector3(0, -1, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, 0, 0), color, new Vector3(0, -1, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, 0, voxelSize), color, new Vector3(0, -1, 0)));
            #endregion north

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="inds"></param>
        /// <param name="color"></param>
        /// <param name="offset">The voxel's position in 3D space.</param>
        /// <param name="face"></param>
        private static void makeVoxelVerts(List<VertexPositionColorNormal> verts, List<VertexPositionColor> wfVerts, Color color, Vector3 offset, bool isSelected)
        {
            #region north
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, 0, 0), color, new Vector3(1, 0, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, 0, voxelSize), color, new Vector3(1, 0, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, voxelSize, 0), color, new Vector3(1, 0, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, voxelSize, voxelSize), color, new Vector3(1, 0, 0)));
            #endregion north

            #region south
            verts.Add(new VertexPositionColorNormal(offset, color, new Vector3(-1, 0, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, voxelSize), color, new Vector3(-1, 0, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, voxelSize, 0), color, new Vector3(-1, 0, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, voxelSize, voxelSize), color, new Vector3(-1, 0, 0)));
            #endregion south

            #region east
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, voxelSize), color, new Vector3(0, 0, 1)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, 0, voxelSize), color, new Vector3(0, 0, 1)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, voxelSize, voxelSize), color, new Vector3(0, 0, 1)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, voxelSize, voxelSize), color, new Vector3(0, 0, 1)));
            #endregion east

            #region west
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, 0), color, new Vector3(0, 0, -1)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, 0, 0), color, new Vector3(0, 0, -1)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, voxelSize, 0), color, new Vector3(0, 0, -1)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, voxelSize, 0), color, new Vector3(0, 0, -1)));
            #endregion west

            #region up
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, voxelSize, 0), color, new Vector3(0, 1, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, voxelSize, voxelSize), color, new Vector3(0, 1, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, voxelSize, 0), color, new Vector3(0, 1, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, voxelSize, voxelSize), color, new Vector3(0, 1, 0)));
            #endregion up
            
            #region down
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, 0), color, new Vector3(0, -1, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(0, 0, voxelSize), color, new Vector3(0, -1, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, 0, 0), color, new Vector3(0, -1, 0)));
            verts.Add(new VertexPositionColorNormal(offset + new Vector3(voxelSize, 0, voxelSize), color, new Vector3(0, -1, 0)));
            #endregion north


            #region wireframe

            Color c = new Color(1, 1, 1, 254);
            if (isSelected)
                c = Color.Red;
            

            wfVerts.Add(new VertexPositionColor(offset + new Vector3(0, 0, 0), c));
            wfVerts.Add(new VertexPositionColor(offset + new Vector3(voxelSize, 0, 0), c));
            wfVerts.Add(new VertexPositionColor(offset + new Vector3(voxelSize, voxelSize, 0), c));
            wfVerts.Add(new VertexPositionColor(offset + new Vector3(0, voxelSize, 0), c));
            wfVerts.Add(new VertexPositionColor(offset + new Vector3(0, 0, voxelSize), c));
            wfVerts.Add(new VertexPositionColor(offset + new Vector3(voxelSize, 0, voxelSize), c));
            wfVerts.Add(new VertexPositionColor(offset + new Vector3(voxelSize, voxelSize, voxelSize), c));
            wfVerts.Add(new VertexPositionColor(offset + new Vector3(0, voxelSize, voxelSize), c));

            #endregion wireframe
        }

        private static void makeVoxelInds(List<short> inds, short currentIndex, List<short> wfInds, short currentWfIndex)
        {
            #region solid
            for (int i = 0; i < 6; i++)
            {
                inds.Add(currentIndex);
                inds.Add((short)(currentIndex + 1));
                inds.Add((short)(currentIndex + 3));
                inds.Add(currentIndex);
                inds.Add((short)(currentIndex + 2));
                inds.Add((short)(currentIndex + 3));

                currentIndex += 4;
            }
            #endregion solid

            #region wireframe
            wfInds.Add(currentWfIndex);
            wfInds.Add((short)(currentWfIndex + 1));

            wfInds.Add((short)(currentWfIndex + 1));
            wfInds.Add((short)(currentWfIndex + 2));

            wfInds.Add((short)(currentWfIndex + 2));
            wfInds.Add((short)(currentWfIndex + 3));

            wfInds.Add((short)(currentWfIndex + 3));
            wfInds.Add(currentWfIndex);

            wfInds.Add(currentWfIndex);
            wfInds.Add((short)(currentWfIndex + 4));

            wfInds.Add((short)(currentWfIndex + 1));
            wfInds.Add((short)(currentWfIndex + 5));

            wfInds.Add((short)(currentWfIndex + 2));
            wfInds.Add((short)(currentWfIndex + 6));

            wfInds.Add((short)(currentWfIndex + 3));
            wfInds.Add((short)(currentWfIndex + 7));

            wfInds.Add((short)(currentWfIndex + 4));
            wfInds.Add((short)(currentWfIndex + 5));

            wfInds.Add((short)(currentWfIndex + 5));
            wfInds.Add((short)(currentWfIndex + 6));

            wfInds.Add((short)(currentWfIndex + 6));
            wfInds.Add((short)(currentWfIndex + 7));

            wfInds.Add((short)(currentWfIndex + 7));
            wfInds.Add((short)(currentWfIndex + 4));
            #endregion wireframe
        }
        #endregion VertsAndInds

        #endregion graphics

    }
}
