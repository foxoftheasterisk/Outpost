﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OutpostLibrary.Content;
using OutpostLibrary;
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

        const float spread = 0.2f;

        public VoxtureEditor(GraphicsDevice g, SpriteFont f)
        {
            font = f;
            graphics = g;
            filename = null;

            voxes = new List<EditingVoxture>();
            colors = new List<Tuple<string, OutpostColor>>();
            colors.Add(new Tuple<string, OutpostColor>("clr", new OutpostColor(0, 0, 0, 0)));
            
            currentColor = 0;
            voxes.Add(new EditingVoxture("NewVox1", colors[0].Item2, graphics, font));
            currentVoxture = 0;
            


            initializeGraphics();

            //TODO: maybe temp?
            cd = new System.Windows.Forms.ColorDialog();
            cd.AllowFullOpen = true;

            //TODO: TEMP
            voxes[0].makeVertices(spread, new OutpostLibrary.IntVector3(-1, -1, -1));
            colors.Add(new Tuple<string, OutpostColor>("tmp", new OutpostColor(1, 24, 128, 255)));
            voxes.Add(new EditingVoxture("TEMP", colors[1].Item2, graphics, font));
            voxes[1].makeVertices(0.0f, false);
        }

        bool m1Down;
        bool m2Down;
        Vector2 lastMousePos;

        System.Windows.Forms.ColorDialog cd;
        System.Windows.Forms.DialogResult dr;
        bool wasInForm = false;
        bool unhandledForm = false;

        bool nameSelected = false;
        bool nameEditing = false;
        int nameEditFlashTimer = 0;
        const int nameEditFlashTime = 10;

        public bool Update(bool useInput)
        {
            //TODO: create visible buttons for functions
            //Also, make this into safe, central input.  When that's a thing.
            KeyboardState keys;
            try
            {
                keys = Keyboard.GetState();
            }
            catch (InvalidOperationException)
            {
                keys = new KeyboardState();//probably a very bad idea
            }

            

            //Hotkeys include:
            //C: create Color
            #region hotkeys
            if(keys.IsKeyDown(Keys.C) && !wasInForm)
            {
                cd.Color = colors[currentColor].Item2.color.toSystemColor();
                wasInForm = true;
                unhandledForm = true;

                //color creation
                //TODO: Create a dialog that includes specular?  (or, just throw a second one on, but that's less good.)
                dr = cd.ShowDialog();
            }
            else if(!keys.IsKeyDown(Keys.C))
            {
                wasInForm = false;
            }

            if (unhandledForm && dr == System.Windows.Forms.DialogResult.OK)
            {
                unhandledForm = false;
                Color createdColor = cd.Color.toXnaColor();
                bool colorAlreadyExists = false;

                for (int i = 0; i < colors.Count; i++)
                {

                    Color comp = colors[i].Item2.color;
                    if (comp == createdColor)
                    {
                        colorAlreadyExists = true;
                        currentColor = i;
                    }
                }

                if (!colorAlreadyExists)
                {
                    //dafuq there isn't a basic text input dialog box???
                    //i guess just make up names for now...

                    string colorname = voxes[currentVoxture].name.Substring(0, 3) + colors.Count;
                    currentColor = colors.Count;
                    colors.Add(new Tuple<string, OutpostColor>(colorname, new OutpostColor(createdColor)));
                }
            }
            #endregion hotkeys

            #region rotation
            //rotation stuff
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

            #endregion rotation

            //selection stuff
            //also includes on-click actions
            #region selections
            //leaving it here instead of in the non-rotate section, cause i don't want the selection persisting like it did
            //there's an alternate solution but it's more complexicated.
            int leftSelectCutoff = graphics.Viewport.Width / 4;
            int rightSelectCutoff = graphics.Viewport.Width - (graphics.Viewport.Width / 4);

            bool somethingSelected = false;

            //check for left voxture selection
            if (mouse.X < leftSelectCutoff)
            {
                //select left
                if(currentVoxture - 1 >= 0)
                {
                    voxes[currentVoxture - 1].makeSelection(true);
                    somethingSelected = true;

                    if (mouse.LeftButton == ButtonState.Pressed && !m1Down)
                    {
                        voxes[currentVoxture].makeVertices(0, false);
                        voxes[currentVoxture].resetRotation();
                        currentVoxture--;
                        voxes[currentVoxture].makeVertices(spread, new OutpostLibrary.IntVector3(-1, -1, -1));
                        voxes[currentVoxture].resetRotation();
                    }
                }
                
            }
            else if (lastMousePos.X < leftSelectCutoff)
            {
                //deselect left
                if (currentVoxture - 1 >= 0)
                {
                    voxes[currentVoxture - 1].makeSelection(false);
                }
            }

            //check for right voxture selection
            if (mouse.X > rightSelectCutoff)
            {
                //select right
                if (currentVoxture + 1 < voxes.Count)
                {
                    voxes[currentVoxture + 1].makeSelection(true);
                    somethingSelected = true;

                    //!m1down so that it doesn't just breeze through the whole stack of voxtures
                    if (mouse.LeftButton == ButtonState.Pressed && !m1Down)
                    {
                        voxes[currentVoxture].makeVertices(0, false);
                        voxes[currentVoxture].resetRotation();
                        currentVoxture++;
                        voxes[currentVoxture].makeVertices(spread, new OutpostLibrary.IntVector3(-1, -1, -1));
                        voxes[currentVoxture].resetRotation();
                    }
                }
                
            }
            else if (lastMousePos.X > rightSelectCutoff)
            {
                //deselect right
                if (currentVoxture + 1 < voxes.Count)
                {
                    voxes[currentVoxture + 1].makeSelection(false);
                }
            }

            //check for voxture name selection
            if(!somethingSelected)
            {
                int midX = graphics.Viewport.Width / 2;
                int baseY = graphics.Viewport.Height - nameDistFromScreenBottom;
                int distX = (int)voxes[currentVoxture].nameSize.X / 2;
                if (mouse.X > midX - distX && mouse.X < midX + distX &&
                   mouse.Y > baseY - voxes[currentVoxture].nameSize.Y && mouse.Y < baseY)
                {
                    nameSelected = true;
                    somethingSelected = true;

                    if(mouse.LeftButton == ButtonState.Pressed && !m1Down)
                    {
                        nameEditing = true;
                    }
                }
                else
                {
                    nameSelected = false;
                    if (mouse.LeftButton == ButtonState.Pressed && !m1Down)
                    {
                        nameEditing = false;
                    }
                }

            }
            else
            {
                nameSelected = false;
            }

            if (!somethingSelected)
            {
                //check for selection in the editing one

                #region initialization
                //start with finding a ray
                Viewport viewp = graphics.Viewport;
                Vector3 front = viewp.Unproject(new Vector3(mouse.X, mouse.Y, 0), projection, view, voxes[currentVoxture].rotation);
                Vector3 dir = viewp.Unproject(new Vector3(mouse.X, mouse.Y, 1), projection, view, voxes[currentVoxture].rotation) - front;
                dir.Normalize();

                
                float voxSize = EditingVoxture.voxelSize;
                int voxPerEdge = OutpostLibrary.Navigation.Sizes.VoxelsPerEdge;
                //TODO-ish: If we ever get variable amounts of spread, it'll need to be able to read the current spread amount

                float midpoint = (voxSize * voxPerEdge + spread * (voxPerEdge - 1)) * 0.5f;

                float minEdge = -midpoint;
                float maxEdge = midpoint;

                Vector3 currentPos = front;

                //using the edge is safe despite possibly starting inside the block on one or more coordinates
                //it will resolve to a negative distance, and back itself up.

                int highestVox = OutpostLibrary.Navigation.Sizes.VoxelsPerEdge - 1;

                int xDir = 0, yDir = 0, zDir = 0;
                float xDist, yDist, zDist;
                int xVoxPos = 0, yVoxPos = 0, zVoxPos = 0;

                if (dir.X > 0)
                {
                    xDir = 1;
                    xDist = (minEdge - currentPos.X) / dir.X;
                    xVoxPos = 0;
                }
                else if (dir.X < 0)
                {
                    xDir = -1;
                    xDist = (maxEdge - currentPos.X) / dir.X;
                    xVoxPos = highestVox;
                }
                else
                {
                    //dir.X = 0, so we don't want this to come up
                    xDist = 10000000;
                    //that should be sufficient
                }

                if (dir.Y > 0)
                {
                    yDir = 1;
                    yDist = (minEdge - currentPos.Y) / dir.Y;
                    yVoxPos = 0;
                }
                else if (dir.Y < 0)
                {
                    yDir = -1;
                    yDist = (maxEdge - currentPos.Y) / dir.Y;
                    yVoxPos = highestVox;
                }
                else
                {
                    //dir.Y = 0, so we don't want this to come up
                    yDist = 10000000;
                    //that should be sufficient
                }

                if (dir.Z > 0)
                {
                    zDir = 1;
                    zDist = (minEdge - currentPos.Z) / dir.Z;
                    zVoxPos = 0;
                }
                else if (dir.Z < 0)
                {
                    zDir = -1;
                    zDist = (maxEdge - currentPos.Z) / dir.Z;
                    zVoxPos = highestVox;
                }
                else
                {
                    //dir.Z = 0, so we don't want this to come up
                    zDist = 10000000;
                    //that should be sufficient
                }

                int x = 0;  //break excuse
                #endregion initialization
                #region seek

                bool stillGoing = true;

                bool xInVox = false, yInVox = false, zInVox = false;

                while(stillGoing)
                {
                    if(xDist < yDist && xDist < zDist)
                    {
                        //x is lowest
                        currentPos = currentPos * xDist;
                        yDist -= xDist;
                        zDist -= xDist;
                        if(xInVox)
                        {
                            xInVox = false;
                            xVoxPos += xDir;
                            if (xVoxPos < 0 || xVoxPos > highestVox)
                                stillGoing = false;
                            xDist = spread / (dir.X * xDir);  //this *should* always be positive
                        }
                        else
                        {
                            xInVox = true;
                            xDist = voxSize / (dir.X * xDir); //this also should always be positive
                        }
                    }
                    else if(yDist < zDist)
                    {
                        //y is lowest
                        currentPos = currentPos * yDist;
                        xDist -= yDist;
                        zDist -= yDist;
                        if (yInVox)
                        {
                            yInVox = false;
                            yVoxPos += yDir;
                            if (yVoxPos < 0 || yVoxPos > highestVox)
                                stillGoing = false;
                            yDist = spread / (dir.Y * yDir);  //this *should* always be positive
                        }
                        else
                        {
                            yInVox = true;
                            yDist = voxSize / (dir.Y * yDir); //this also should always be positive
                        }
                    }
                    else
                    {
                        //z is lowest
                        currentPos = currentPos * zDist;
                        xDist -= zDist;
                        yDist -= zDist;
                        if (zInVox)
                        {
                            zInVox = false;
                            zVoxPos += zDir;
                            if (zVoxPos < 0 || zVoxPos > highestVox)
                                stillGoing = false;
                            zDist = spread / (dir.Z * zDir);  //this *should* always be positive
                        }
                        else
                        {
                            zInVox = true;
                            zDist = voxSize / (dir.Z * zDir); //this also should always be positive
                        }
                    }

                    if(xInVox && yInVox && zInVox)
                        stillGoing = false;

                }
                #endregion seek
                OutpostLibrary.IntVector3 selected = new OutpostLibrary.IntVector3(xVoxPos, yVoxPos, zVoxPos);
                //this is safe, passing it voxel coordinates outside the voxture is already its failure condition
                voxes[currentVoxture].makeSelection(selected);

                if(mouse.LeftButton == ButtonState.Pressed)
                {
                    voxes[currentVoxture][selected] = colors[currentColor].Item2;
                }
            }

            #endregion selections


            if (mouse.LeftButton == ButtonState.Pressed)
            {
                m1Down = true;
            }
            else
            {
                m1Down = false;
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

        private SpriteFont font;

        const int nameDistFromScreenBottom = 10;

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
                
                //draws voxels
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


                //then draws outlines
                //this makes it draw over the cubes when they're in the same place
                //theoretically
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
                
                //Reboot SpriteBatch because transitioning from 3D to 2D
                drawer.End();
                drawer.Begin();

                //now, display the name
                int target = graphics.Viewport.Width / 2;
                Vector2 drawPos = new Vector2(target - (drawee.nameSize.X / 2), graphics.Viewport.Height - (nameDistFromScreenBottom + drawee.nameSize.Y));
                string drawString = drawee.name;
                if (nameEditing)
                {
                    nameEditFlashTimer++;
                    if (nameEditFlashTimer > 2 * nameEditFlashTime)
                        nameEditFlashTimer = 0;
                    if (nameEditFlashTimer < nameEditFlashTime)
                        drawString = drawString + "|";
                }

                if(nameSelected)
                    drawer.DrawString(font, drawString, drawPos, Color.Red);
                else
                    drawer.DrawString(font, drawString, drawPos, Color.Black);
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
