using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OutpostLibrary.Content;
using Microsoft.Xna.Framework.Input;

namespace VoxtureEditor
{
    class VoxtureEditor : Outpost.Screens.Screen
    {

        string filename;

        List<EditingVoxture> voxes;
        
        List<Tuple<string, OutpostColor>> colors;

        int currentVoxture;
        int currentColor;

        public VoxtureEditor(GraphicsDevice g)
        {
            graphics = g;
            filename = null;

            voxes = new List<EditingVoxture>();
            colors = new List<Tuple<string, OutpostColor>>();
            colors.Add(new Tuple<string, OutpostColor>("clr", new OutpostColor(0, 0, 0, 0)));
            
            currentColor = 0;
            voxes.Add(new EditingVoxture("NewVox1", colors[0].Item2, graphics));
            currentVoxture = 0;
            


            initializeGraphics();

            //TODO: TEMP
            voxes[0].makeVertices(0.2f, new OutpostLibrary.IntVector3(-1,-1,-1));
            colors.Add(new Tuple<string, OutpostColor>("tmp", new OutpostColor(1, 24, 128, 255)));
            voxes.Add(new EditingVoxture("TEMP", colors[1].Item2, graphics));
            voxes[1].makeVertices(0.0f, false);
        }

        bool m2Down;
        Vector2 lastMousePos;

        public bool Update(bool useInput)
        {
            if (currentVoxture - 1 >= 0)
            {
                voxes[currentVoxture - 1].rotate(Matrix.CreateRotationY(0.005f));
            }

            if (currentVoxture + 1 < voxes.Count)
            {
                voxes[currentVoxture + 1].rotate(Matrix.CreateRotationY(0.005f));
            }

            if (!useInput)
                return false;

            MouseState mouse = Mouse.GetState();
            Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
            if(mouse.RightButton == ButtonState.Pressed)
            {
                if(m2Down)
                {
                    Vector2 diff = mousePos - lastMousePos;

                    Matrix rot = Matrix.CreateRotationY(diff.X / 100);
                    rot = rot * Matrix.CreateRotationZ(diff.Y / 100);

                    voxes[currentVoxture].rotate(rot);
                }

                m2Down = true;
            }
            else
            {
                m2Down = false;
            }

            lastMousePos = mousePos;
            return false;
        }

        public bool drawUnder()
        {
            return false;
        }

        #region graphics

        GraphicsDevice graphics;

        Matrix world; //totally not just the identity matrix
        //actually i should actually use this for rotating the active chunk
        //and maybe also rotating the other ones, in a different way
        Matrix projection;
        Matrix view;

        Matrix transLeft;
        Matrix transRight;

        BasicEffect drawingEngine;
        //Effect voxelEffect;

        short[] selectionIndices;
        DynamicIndexBuffer sIBuff;
        VertexPositionColor[] selectionVertices;
        DynamicVertexBuffer sVBuff;

        void initializeGraphics()
        {
            world = Matrix.Identity;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), (float)graphics.Viewport.Width / (float)graphics.Viewport.Height, .01f, 10.0f);
            view = Matrix.CreateLookAt(new Vector3(-4, 0, 0), new Vector3(0, 0, 0), Vector3.Up);

            
            drawingEngine = new BasicEffect(graphics);

            drawingEngine.World = world;
            drawingEngine.Projection = projection;
            drawingEngine.View = view;

            drawingEngine.TextureEnabled = false;
            drawingEngine.VertexColorEnabled = true;

            drawingEngine.EnableDefaultLighting();
            drawingEngine.SpecularColor = new Vector3(0, 0, 0);
            //*/

            /*
            voxelEffect = content.Load<Effect>("CubeShader");

            voxelEffect.Parameters["World"].SetValue(world);
            voxelEffect.Parameters["Projection"].SetValue(projection);
            voxelEffect.Parameters["View"].SetValue(view);

            voxelEffect.Parameters["ambientColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            voxelEffect.Parameters["sunColor"].SetValue(new Vector4(1.0f, 1.0f, 0.8f, 0.0f));
            voxelEffect.Parameters["sunDir"].SetValue(new Vector3(-0.1f, -1.0f, -0.5f));
            //*/

            #region selected

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

            #endregion selected

            transLeft = Matrix.CreateTranslation(0, 0, -2);
            transRight = Matrix.CreateTranslation(0, 0, 2);
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch drawer)
        {
            //TODO: maybe something with the world matrix
            //probably
            //for rotations

            if(currentVoxture - 1 >= 0)
            {
                EditingVoxture drawee = voxes[currentVoxture - 1];
                drawingEngine.World = drawee.rotation * transLeft;


                graphics.SetVertexBuffer(drawee.vertexBuffer);
                graphics.Indices = drawee.indexBuffer;

                //TODO: this one should become the VoxelEffect, eventually, when it's put into the main project.
                foreach (EffectPass p in drawingEngine.CurrentTechnique.Passes)
                {
                    p.Apply();
                    graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, EditingVoxture.numVerts, 0, EditingVoxture.numInds / 3);
                }



                RasterizerState standard = graphics.RasterizerState;
                RasterizerState depthBiased = new RasterizerState();
                depthBiased.DepthBias = 1;
                depthBiased.CullMode = CullMode.None;

                graphics.RasterizerState = depthBiased;
                drawingEngine.LightingEnabled = false;

                graphics.SetVertexBuffer(drawee.wireframeVertexBuffer);
                graphics.Indices = drawee.wireframeIndexBuffer;

                foreach (EffectPass p in drawingEngine.CurrentTechnique.Passes)
                {
                    p.Apply();
                    graphics.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, EditingVoxture.numWfVerts, 0, 12); //12 lines = 1 cube
                }

                drawingEngine.LightingEnabled = true;
            }

            { //the current one must exist, so no if here
                EditingVoxture drawee = voxes[currentVoxture];
                drawingEngine.World = drawee.rotation;

                graphics.SetVertexBuffer(drawee.vertexBuffer);
                graphics.Indices = drawee.indexBuffer;

                //TODO: this one should become the VoxelEffect, eventually, when it's put into the main project.
                foreach(EffectPass p in drawingEngine.CurrentTechnique.Passes)
                {
                    p.Apply();
                    graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, EditingVoxture.numVerts, 0, EditingVoxture.numInds / 3);
                }



                //this makes it draw over the cubes when they're in the same place
                RasterizerState standard = graphics.RasterizerState;
                RasterizerState depthBiased = new RasterizerState();
                depthBiased.DepthBias = 1;
                depthBiased.CullMode = CullMode.None;

                graphics.RasterizerState = depthBiased;
                drawingEngine.LightingEnabled = false;

                graphics.SetVertexBuffer(drawee.wireframeVertexBuffer);
                graphics.Indices = drawee.wireframeIndexBuffer;

                foreach (EffectPass p in drawingEngine.CurrentTechnique.Passes)
                {
                    p.Apply();
                    graphics.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, EditingVoxture.numWfVerts, 0, EditingVoxture.numWfInds / 2);
                }

                //graphics.RasterizerState = standard;
                drawingEngine.LightingEnabled = true;
            }

            if(currentVoxture + 1 < voxes.Count)
            {
                EditingVoxture drawee = voxes[currentVoxture + 1];
                drawingEngine.World = drawee.rotation * transRight;

                graphics.SetVertexBuffer(drawee.vertexBuffer);
                graphics.Indices = drawee.indexBuffer;

                //TODO: this one should become the VoxelEffect, eventually, when it's put into the main project.
                foreach (EffectPass p in drawingEngine.CurrentTechnique.Passes)
                {
                    p.Apply();
                    graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, EditingVoxture.numVerts, 0, EditingVoxture.numInds / 3);
                }



                RasterizerState standard = graphics.RasterizerState;
                RasterizerState depthBiased = new RasterizerState();
                depthBiased.DepthBias = 1;
                depthBiased.CullMode = CullMode.None;

                graphics.RasterizerState = depthBiased;
                drawingEngine.LightingEnabled = false;

                graphics.SetVertexBuffer(drawee.wireframeVertexBuffer);
                graphics.Indices = drawee.wireframeIndexBuffer;

                foreach (EffectPass p in drawingEngine.CurrentTechnique.Passes)
                {
                    p.Apply();
                    graphics.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, EditingVoxture.numWfVerts, 0, 12); //12 lines = 1 cube
                }

                drawingEngine.LightingEnabled = true;
            }
        }

        #endregion graphics

        public bool shouldClose()
        {
            throw new NotImplementedException();
        }

        
    }
}
