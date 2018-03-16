using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OutpostLibrary.Navigation;

namespace OutpostCore
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
            Screens.LoadingScreen.Display("Setting up new game");

            Map.MapManager.CreateMap(folderPath);

            ChunkAddress worldCenter = new ChunkAddress("Earth", new OutpostLibrary.IntVector3(0));

            _Game = new GameManager();

            _Game.player = new Player(worldCenter, new Microsoft.Xna.Framework.Vector3(2, 15, 2));

            //TODO: mapgen
            //this works for now, if inelegantly, but will NOT when we start having two-stage mapgen
            //also,
            //TODO: make radii be set instead of magic numbers
            Map.MapSection playerData = new Map.MapSection(worldCenter, 7, new Map.LoadState(Map.LoadState.GraphicalLoadState.None, Map.LoadState.DataLoadState.Full));
            Map.MapSection playerGraphics = new Map.MapSection(worldCenter, 5, new Map.LoadState(Map.LoadState.GraphicalLoadState.Full, Map.LoadState.DataLoadState.Full));

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

        public void update(bool useInput)
        {
            //TODO: this
        }

        public void draw(Microsoft.Xna.Framework.Graphics.SpriteBatch drawer)
        {
            //TODO: this
        }
    }
}
