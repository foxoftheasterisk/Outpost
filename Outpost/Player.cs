using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using OutpostLibrary;
using OutpostLibrary.Content;
using OutpostLibrary.Navigation;
using Microsoft.Xna.Framework.Input;

namespace OutpostCore
{
    public class Player
    {
        #region constants
        const int height = 5;
        const int width = 2;
        const int length = 2;
        #region movement constants

        const int climbableHeight = 4;
        const float jumpStartVel = .7f;
        const float climbRate = .2f;
        const float stepRate = .5f;
        const float surmountRate = .3f;
        const float walkSpeed = .2f;
        const float runSpeed = .5f;
        const float runVertSpeed = .3f;
        const float strafeSpeed = .15f;

        #endregion
        #region technical constants
        Vector3 baseDirection = new Vector3(0, 0, 1);
        Vector3 sideDirection = new Vector3(1, 0, 0);
        IntVector2 screenCenter;
        #endregion
        #endregion


        //TODO: probably make the following happen on the MapSection side
        public Map.MapSection data;
        public Map.MapSection graphics;

        public Player(ChunkAddress chunkIn, Vector3 posInChunk)
        {
            IntVector2 screenCenter = GameShell.gameShell.screenCenter;
            
            Mouse.SetPosition(screenCenter.x, screenCenter.y);

            chunk = chunkIn;
            this.posInChunk = posInChunk;

            emptyDefault = new Items.PlayerFist();
            activeItem = emptyDefault;
        }

        #region movement
        public ChunkAddress chunk;
        Vector3 posInChunk; 
        float pitch; //i.e. vertical rotation
        float yaw; //i.e. horizontal rotation

        /// <summary>
        /// TEMP METHOD FOR DEBUGGING
        /// </summary>
        /// <returns></returns>
        public Vector3 Pos()
        {
            return posInChunk;
        }

        Vector3 lastMove;
        /// <summary>
        /// Rotation and movement
        /// </summary>
        public void Move()
        {
            //god this needs a refactor
            //it's pretty terrible

            #region view rotation
            MouseState mouser = Mouse.GetState();

            int diffX = mouser.X - screenCenter.x;
            int diffY = mouser.Y - screenCenter.y;

            Mouse.SetPosition(screenCenter.x, screenCenter.y);

            yaw -= (float)(diffX * (Math.PI / 64));
            if(yaw > 2 * Math.PI)
            {
                yaw -= (float)(2 * Math.PI);
            }
            if(yaw < 0)
            {
                yaw += (float)(2 * Math.PI);
            }

            pitch += (float)(diffY * (Math.PI / 64));
            if (pitch > Math.PI / 2)
            {
                pitch = (float)((Math.PI - .001) / 2);
            }
            if (pitch < -Math.PI / 2)
            {
                pitch = -(float)((Math.PI - .001) / 2);
            }

            Matrix rot = Matrix.CreateRotationY(yaw);
            Vector3 direction = Vector3.Transform(baseDirection, rot);
            direction.Normalize();

            #endregion
            #region movement
            Solidity[][][] temp = parent.detectCollision(chunk, posInChunk, direction, new IntVector3(0, 1, 0));
            Solidity standingOn = temp[0][0][0];

            Vector3 movement = new Vector3(0,0,0);
            KeyboardState keys;
            try
            {
                keys = Keyboard.GetState();
            }
            catch (InvalidOperationException)
            {
                keys = new KeyboardState();//bad idea??
            }


            if (posInChunk.Y == Math.Floor(posInChunk.Y) && standingOn != null && standingOn == Solidity.solid)
            {
                #region ground movement
                #region create hor. direction
                if (keys.IsKeyDown(Keys.LeftShift))
                {
                    direction *= runSpeed;
                    direction.Y = runVertSpeed;
                }
                else
                {
                    direction *= walkSpeed;
                }

                if (keys.IsKeyDown(Keys.S))
                {
                    movement -= direction;
                }
                if (keys.IsKeyDown(Keys.W))
                {
                    movement += direction;
                }
                direction = Vector3.Transform(sideDirection, rot);
                direction.Normalize();

                direction *= strafeSpeed;

                if (keys.IsKeyDown(Keys.A))
                {
                    movement += direction;
                }
                if (keys.IsKeyDown(Keys.D))
                {
                    movement -= direction;
                }

                #endregion

                if (movement != Vector3.Zero)
                {
                    #region movementTypeCheck
                    Solidity[][][] collisions = parent.detectCollision(chunk, posInChunk + movement, direction, new IntVector3(length, height + 1, width));

                    int xSize = collisions.Length;
                    int zSize = collisions[0][0].Length;
                    bool walkable = true;
                    bool stepPresent = false;
                    bool clearToStep = true;
                    bool climbable = false;

                    for (int x = 0; x < xSize; x++)
                    {
                        for (int z = 0; z < zSize; z++)
                        {

                            bool lastWasSolid = false;

                            if (collisions[x][1][z] == Solidity.solid)
                            {
                                walkable = false;
                                lastWasSolid = true;
                                stepPresent = true;
                            }

                            for (int y = 2; y <= height; y++)
                            {
                                if (collisions[x][y][z] == Solidity.gas)
                                {
                                    if (lastWasSolid && y <= climbableHeight)
                                    {
                                        climbable = true;
                                    }
                                }
                                if (collisions[x][y][z] == Solidity.solid)
                                {
                                    walkable = false;
                                    clearToStep = false;
                                    lastWasSolid = true;
                                }
                                else
                                {
                                    lastWasSolid = false;
                                }
                            }

                            if (collisions[x][height + 1][z] == Solidity.solid)
                                clearToStep = false;
                        }
                    }
                    bool steppable = clearToStep && stepPresent;
                    if (keys.IsKeyUp(Keys.W))
                    {
                        climbable = false;
                    }
                    #endregion
                    #region movement
                    int xTo = (int)Math.Floor(posInChunk.X + movement.X);
                    int zTo = (int)Math.Floor(posInChunk.Z + movement.Z);
                    //recalculate these because now we want the raw difference from the current position.

                    if (!walkable)
                    {
                        //these corrections could use work, when I go for actual bounding-box collision detection
                        //but for now they do I s'pose
                        if (posInChunk.X < xTo)
                        {
                            movement.X = xTo - (posInChunk.X + .1f);
                        }
                        if (posInChunk.X > xTo + 1)
                        {
                            movement.X = xTo + 1 - (posInChunk.X - .1f);
                        }
                        if (posInChunk.Z < zTo)
                        {
                            movement.Z = zTo - (posInChunk.Z + .1f);
                        }
                        if (posInChunk.Z > zTo + 1)
                        {
                            movement.Z = zTo + 1 - (posInChunk.Z - .1f);
                        }

                        if (climbable)
                        {
                            if (keys.IsKeyDown(Keys.W) && keys.IsKeyUp(Keys.S) && keys.IsKeyUp(Keys.A) && keys.IsKeyUp(Keys.D))
                                movement.Y = climbRate;
                        }
                        if (steppable)
                            movement.Y = stepRate;
                    }
                    #endregion
                }
                if (keys.IsKeyDown(Keys.Space))
                {
                    movement.Y = jumpStartVel;
                }
                #endregion
            }
            else
            {
                bool foundMovement = false;
                if (keys.IsKeyDown(Keys.W) || keys.IsKeyDown(Keys.S) || keys.IsKeyDown(Keys.A) || keys.IsKeyDown(Keys.D))
                {
                    #region climbing movement
                    #region create hor. direction
                    direction *= walkSpeed;

                    if (keys.IsKeyDown(Keys.S))
                    {
                        movement -= direction;
                    }
                    if (keys.IsKeyDown(Keys.W))
                    {
                        movement += direction;
                    }
                    direction = Vector3.Transform(sideDirection, rot);
                    direction.Normalize();

                    direction *= strafeSpeed;

                    if (keys.IsKeyDown(Keys.A))
                    {
                        movement += direction;
                    }
                    if (keys.IsKeyDown(Keys.D))
                    {
                        movement -= direction;
                    }
                    #endregion
                    #region movementTypeCheck
                    Solidity[][][] collisions = parent.detectCollision(chunk, posInChunk + movement, direction, new IntVector3(length, height + 1, width));

                    int xSize = collisions.Length;
                    int zSize = collisions[0][0].Length;

                    bool clearToSurmount = true;
                    bool somethingToSurmount = false;
                    bool climbable = false;

                    for(int x = 0; x < xSize; x++)
                    {
                        for (int z = 0; z < zSize; z++)
                        {

                            bool lastWasSolid = false;

                            if (collisions[x][1][z] == Solidity.solid)
                            {
                                lastWasSolid = true;
                                somethingToSurmount = true;
                            }

                            for (int y = 2; y <= height; y++)
                            {
                                if (collisions[x][y][z] == Solidity.gas)
                                {
                                    if (lastWasSolid && y <= climbableHeight)
                                    {
                                        climbable = true;
                                    }
                                }
                                if (collisions[x][y][z] == Solidity.solid)
                                {
                                    clearToSurmount = false;
                                    lastWasSolid = true;
                                }
                                else
                                {
                                    lastWasSolid = false;
                                }
                            }

                            if (collisions[x][height + 1][z] == Solidity.solid)
                                clearToSurmount = false;

                        }
                    }
                    if (keys.IsKeyUp(Keys.W))
                    {
                        climbable = false;
                    }
                    bool surmountable = clearToSurmount && somethingToSurmount;
                    #endregion
                    #region move if applicable

                    
                    if (surmountable)
                    {
                        int y = (int)Math.Floor(posInChunk.Y);
                        if (keys.IsKeyDown(Keys.Space) && y + 1 < posInChunk.Y + jumpStartVel)
                        {
                            if (keys.IsKeyDown(Keys.LeftShift))
                            {
                                movement.Normalize();
                                movement *= runSpeed;
                            }
                            movement.Y = jumpStartVel;
                        }
                        else if (y + 1 <= posInChunk.Y + surmountRate)
                        {
                            //movement *= .5f;
                            movement.Y = (y + 1) - posInChunk.Y;
                        }
                        else
                        {
                            movement = new Vector3(0, surmountRate, 0);
                        }
                        foundMovement = true;
                    }
                    else if (climbable)
                    {
                        if (keys.IsKeyDown(Keys.W) && keys.IsKeyUp(Keys.S) && keys.IsKeyUp(Keys.A) && keys.IsKeyUp(Keys.D))
                        {
                            movement = new Vector3(0, climbRate, 0);
                            foundMovement = true;
                        }
                    }
                    #endregion
                    #endregion
                }
                if (!foundMovement)
                {
                    #region airborne movement
                    movement = lastMove;
                    movement -= new Vector3(0, GameShell.gravity, 0);

                    Solidity[][][] flyingInto = parent.detectCollision(chunk, posInChunk + movement, direction, new IntVector3(length, height + 1, width));

                    int xSize = flyingInto.Length;
                    int zSize = flyingInto[0][0].Length;
                    
                    bool landable = false;
                    bool colliding = false;

                    for (int x = 0; x < xSize; x++)
                    {
                        for (int z = 0; z < zSize; z++)
                        {
                            bool temp_landable = false;

                            if (flyingInto[x][1][z] == Solidity.solid)
                            {
                                temp_landable = true;

                            }
                            for (int y = 2; y < height + 1; y++)
                            {
                                if (flyingInto[x][y][z] == Solidity.solid)
                                {
                                    colliding = true;
                                    temp_landable = false;
                                }
                            }

                            if (temp_landable)
                                landable = true;

                        }
                    }

                    int xTo = (int)Math.Floor(posInChunk.X + movement.X);
                    int zTo = (int)Math.Floor(posInChunk.Z + movement.Z);

                    //I suppose what I should actually do is
                    //first check to see if there are collisions
                    //if there are, correct the movement
                    //and THEN see if there's something to land on
                    if (landable)
                    {
                        float yMovement = (float)Math.Ceiling(posInChunk.Y + movement.Y);
                        yMovement -= posInChunk.Y;
                        movement.Y = yMovement;
                    }
                    if (colliding)
                    {
                        //these corrections could use work, when I go for actual bounding-box collision detection
                        //but for now they'll do I s'pose
                        if (posInChunk.X < xTo)
                        {
                            movement.X = xTo - (posInChunk.X + .1f);
                        }
                        if (posInChunk.X > xTo + 1)
                        {
                            movement.X = xTo + 1 - (posInChunk.X - .1f);
                        }
                        if (posInChunk.Z < zTo)
                        {
                            movement.Z = zTo - (posInChunk.Z + .1f);
                        }
                        if (posInChunk.Z > zTo + 1)
                        {
                            movement.Z = zTo + 1 - (posInChunk.Z - .1f);
                        }
                    }
                    #endregion
                }
            }
           

            posInChunk += movement;
            lastMove = movement;
            #endregion
            #region DEVMODE: teleport
            if (keys.IsKeyDown(Keys.Q))
            {
                posInChunk = new Vector3(10, 16, 10);
                lastMove = new Vector3(0, 0, 0);
            }
            #endregion
            #region chunk correction
            if (posInChunk.X > 16)
            {
                posInChunk.X -= 16;
                chunk.position.X += 1;
            }
            if (posInChunk.X < 0)
            {
                posInChunk.X += 16;
                chunk.position.X -= 1;
            }
            if (posInChunk.Y > 16)
            {
                posInChunk.Y -= 16;
                chunk.position.Y += 1;
            }
            if (posInChunk.Y < 0)
            {
                posInChunk.Y += 16;
                chunk.position.Y -= 1;
            }
            if (posInChunk.Z > 16)
            {
                posInChunk.Z -= 16;
                chunk.position.Z += 1;
            }
            if (posInChunk.Z < 0)
            {
                posInChunk.Z += 16;
                chunk.position.Z -= 1;
            }


            #endregion

            //TODO: probably change this to MapSections that automatically follow the player
            data.Move(chunk);
            graphics.Move(chunk);

        }
        #endregion

        #region actions
        MouseState lastMouseState;
        
        Item activeItem;//this variable will eventually become a small int (like, 4 bits would suffice) that points to the hotbar
        Item emptyDefault;
        public void Action()
        {
            //you know what fuck figuring this out right now
            
            MouseState currentMouseState = Mouse.GetState();

            BlockAddress target = getTarget();
            if (target != null)
            {
                bool changesRequired = false;
                if (currentMouseState.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton != ButtonState.Pressed)
                {
                    changesRequired = activeItem.actionStart(target);
                    if (changesRequired)
                        activeItem.performActionsOnWielder(this);
                }
            }
            
            lastMouseState = currentMouseState;
            
            //*/
        }

        public BlockAddress getTarget()
        {
            Vector3 direction = getDirection();

            Vector3 headPos = posInChunk + new Vector3(0, height, 0);

            BlockAddress? found = null;
            switch (activeItem.order())
            {
                case TestingOrder.onOnly:
                    found = GameShell.gameShell.findBlock(chunk, headPos, direction, activeItem.range(), activeItem.onTest);
                    break;
                case TestingOrder.beforeOnly:
                    found = GameShell.gameShell.findBlockBefore(chunk, headPos, direction, activeItem.range(), activeItem.beforeTest);
                    break;
                case TestingOrder.beforeFirst:
                    found = GameShell.gameShell.findBlockBefore(chunk, headPos, direction, activeItem.range(), activeItem.beforeTest);
                    if (found == null)
                        found = GameShell.gameShell.findBlock(chunk, headPos, direction, activeItem.range(), activeItem.onTest);
                    break;
                case TestingOrder.onFirst:
                    found = GameShell.gameShell.findBlock(chunk, headPos, direction, activeItem.range(), activeItem.onTest);
                    if (found == null)
                        found = GameShell.gameShell.findBlockBefore(chunk, headPos, direction, activeItem.range(), activeItem.beforeTest);
                    break;
                default:
                    break;
            }
            return found;

        }

        public void replaceActiveItem(Item replaceWith)
        {
            activeItem = replaceWith;
        }
        public void discardActiveItem()
        {
            activeItem = emptyDefault;
        }
        #endregion

        public Matrix createViewMatrix()
        {
            Vector3 headPos = posInChunk + new Vector3(0, height, 0) + new Vector3(chunk.X * 16, chunk.Y * 16, chunk.Z * 16);
            Vector3 lookingAt = getDirection() + headPos;
            return Matrix.CreateLookAt(headPos, lookingAt, Vector3.Up);
        }

        public Vector3 getDirection()
        {
            Matrix rots = Matrix.CreateFromYawPitchRoll(yaw, pitch, 0);
            return Vector3.Transform(baseDirection, rots);
        }

        public string encodeForSave()
        {
            string encoding = (new OutpostLibrary.Navigation.VoxelAddress(chunk, posInChunk)).ToString();

            //TODO: inventory

            return encoding;
        }
    }
}
