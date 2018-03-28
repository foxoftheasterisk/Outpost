using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OutpostLibrary.Navigation;
using OutpostEngine.Entities;
using Microsoft.Xna.Framework.Input;
using OutpostEngine.Input;
using Microsoft.Xna.Framework;

namespace OutpostEngine
{
    class GameManager
    {
        //manages a single game
        //where GameShell will hold the rules and such that will carry over if you quit one game and load another

        public static GameManager Game
        {
            get
            {
                return _Game;
            }
        }
        private static GameManager _Game;

        private GameManager()
        {
            //not sure what needs constructing, yet
        }

        Player player;

        public static void NewGame(string folderPath)
        {
            ScreenManagement.LoadingScreen.Display("Setting up new game");

            Map.MapManager.CreateMap(folderPath);

            ChunkAddress worldCenter = new ChunkAddress("Earth", new OutpostLibrary.IntVector3(0));

            _Game = new GameManager();

            _Game.player = new Player(worldCenter, new Microsoft.Xna.Framework.Vector3(2, 15, 2));

            //TODO: mapgen
            //this works for now, if inelegantly, but will NOT when we start having two-stage mapgen
            //also,
            //TODO: make radii be set instead of magic numbers
            Map.FollowingMapSection playerData = new Map.FollowingMapSection(worldCenter, 7, new Map.LoadState(Map.LoadState.GraphicalLoadState.None, Map.LoadState.DataLoadState.Full), _Game.player, 1);
            Map.FollowingMapSection playerGraphics = new Map.FollowingMapSection(worldCenter, 5, new Map.LoadState(Map.LoadState.GraphicalLoadState.Full, Map.LoadState.DataLoadState.Full), _Game.player, 1);

            _Game.player.data = playerData;
            _Game.player.graphics = playerGraphics;

        }

        /// <summary>
        /// NOT YET IMPLEMENTED
        /// </summary>
        /// <param name="folderPath"></param>
        public static void LoadGame(string folderPath)
        {
            //TODO: this
        }

        public void endGame()
        {
            //TODO: there is more to this
            string playerData = player.encodeForSave();
            //among other things we need to actually do something with that player data

            Map.MapManager.Map.saveAndQuit();
        }

        public void Update(ScreenManagement.InputSet input)
        {
            ScreenManagement.InputItem item;

            if (input.Consume(out item, new KeyInputIdentifier(Keys.Escape)))
            {
                ScreenManagement.ScreenManager.screenManager.Push(new ScreenManagement.PauseScreen(null, new Color(Color.Black, 0.5f), new KeyInputIdentifier(Keys.Enter)));
            }

            player.Move();

            player.Action();
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch drawer)
        {
            //TODO: this
        }
    }
}
