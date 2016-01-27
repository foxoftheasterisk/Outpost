using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Outpost.Screens;

//this should be removable
//using OutpostLibrary.Content;

namespace Outpost
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TopLevel : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public TopLevel()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //whatever it is I have to do to declare this a multithreaded application
            //I should probably do that
            //it might help performance
            //er okay so
            //the apartment shit is something completely different
            //and I think we're good as is

            MainGame mainGame = new MainGame(this.Content, GraphicsDevice);

            ScreenManager.screenManager.push(mainGame);

            LoadingScreen.Display("Starting up");

            contentLoader = new System.Threading.Thread(LoadContentInNewThread);
            contentLoader.Priority = System.Threading.ThreadPriority.Normal;
            contentLoader.Start();

            base.Initialize();
        }

        System.Threading.Thread contentLoader;

        private void LoadContentInNewThread()
        {
            //okay so i'm not entirely sure why this is new thread
            //i mean what is going on in the main thread at this point?
            //but w/e

            //ohhhh it's so we can have a loading screen
            //okay cool

            MainGame.mainGame.lua.initializeLua();

            MainGame.mainGame.lua.runLuaFile("vanilla.set");
            
            //TODO: make the filename be loaded from a config file
            

            //Material.materials = new Dictionary<String, Material>();
            //when you remove this also remove the using OutpostLibrary.Content;

            //SetLoader.loadSet(OutpostLibrary.Misc.GAME_DIR + "vanilla.set");

            //MainGame.mainGame.Log(Material.materials["dirt"].solidity.ToString());

            MainGame.mainGame.newMap();

            LoadingScreen.Close();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            try
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    this.Exit();
                }
            }
            catch (InvalidOperationException e)
            {
                MainGame.mainGame.Log(e.ToString());
            }

            ScreenManager.screenManager.update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //spriteBatch.Begin();

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rs;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, rs);
            
            ScreenManager.screenManager.draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
