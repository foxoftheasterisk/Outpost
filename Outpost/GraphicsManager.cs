using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OutpostEngine.Map;
using OutpostLibrary;
using OutpostLibrary.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutpostEngine
{
    class GraphicsManager
    {

        #region Singleton and constructor

        public static GraphicsManager graphicsManager => _graphicsManager;
        private static GraphicsManager _graphicsManager;

        public static void CreateGraphicsManager(GraphicsDevice g, ContentManager c)
        {
            _graphicsManager = new GraphicsManager(g, c);
        }


        private GraphicsManager(GraphicsDevice g, ContentManager content)
        {
            graphics = g;

            #region setup tasks

            screenCenter = new IntVector2(graphics.Viewport.Width / 2, graphics.Viewport.Height / 2);

            world = Matrix.Identity;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), (float)graphics.Viewport.Width / (float)graphics.Viewport.Height, .01f, 50.0f);

            drawingEngine = new BasicEffect(graphics);

            drawingEngine.World = world;
            drawingEngine.Projection = projection;


            drawingEngine.TextureEnabled = false;
            drawingEngine.VertexColorEnabled = true;

            drawingEngine.EnableDefaultLighting();
            drawingEngine.SpecularColor = new Vector3(0, 0, 0);

            #region selection

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

            #endregion selection

            voxelEffect = content.Load<Effect>("CubeShader");

            voxelEffect.Parameters["World"].SetValue(world);
            voxelEffect.Parameters["Projection"].SetValue(projection);

            #endregion setup tasks

            #region content

            font = content.Load<SpriteFont>("someFont");
            reticule = content.Load<Texture2D>("reticule");
            blank = content.Load<Texture2D>("blank");

            #endregion content

        }

        #endregion singleton and constructor

        public GraphicsDevice GraphicsDevice => graphics;
        private GraphicsDevice graphics;

        public SpriteFont font;
        Texture2D reticule;
        public Texture2D blank;

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

        public IntVector2 ScreenCenter => screenCenter;
        private IntVector2 screenCenter;

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

        
        /*
         * These are, apparently, NOT needed?
        void indicesLostHandler(object sender, EventArgs e)
        {
            sIBuff.SetData(selectionIndices);
        }

        void verticesLostHandler(object sender, EventArgs e)
        {
            sVBuff.SetData(selectionVertices);
        }
        //*/

        //this should maybe be in a different class?
        public void DrawStandardProjection(SpriteBatch drawer, Matrix view, MapSection areaToDraw, BlockAddress? selectedBlock)
        {
            drawingEngine.View = view;
            voxelEffect.Parameters["View"].SetValue(view);

            voxelEffect.Parameters["ambientColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 0.3f));
            voxelEffect.Parameters["sunColor"].SetValue(new Vector4(1.0f, 1.0f, 0.8f, 0.9f));
            voxelEffect.Parameters["sunDir"].SetValue(new Vector3(-0.1f, -1.0f, -0.5f));

            foreach (Chunk drawee in areaToDraw)
            {
                if (drawee == null)
                    continue;
                if (drawee.isTransparent())
                    continue;
                if (drawee.isDisposed)
                    continue;

                #region chunkdrawer

                DynamicVertexBuffer vertexBuffer = drawee.getVertexBuffer();
                DynamicIndexBuffer indexBuffer = drawee.getIndexBuffer();
                int numTriangles = drawee.getNumTriangles();

                graphics.SetVertexBuffer(vertexBuffer);
                graphics.Indices = indexBuffer;

                foreach (EffectPass p in voxelEffect.CurrentTechnique.Passes)  //Not sure I entirely understand this
                {
                    //mmm... so... let's see
                    p.Apply();
                    graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numTriangles);

                }
                #endregion chunkdrawer
            }



            //TODO: test if this is still necessary
            drawer.End();
            drawer.Begin();
            //for whatever reason if I don't reboot the SpriteBatch it does weird things with transparency and whatnot.

            #region GUI

            drawer.Draw(reticule, new Vector2(ScreenCenter.x - reticule.Width, ScreenCenter.y - reticule.Height), Color.White);

            #region selectedBlock

            if (selectedBlock != null)
            {
                BlockAddress selected = (BlockAddress)selectedBlock;

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
                    graphics.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 12);
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
        }

    }
}
