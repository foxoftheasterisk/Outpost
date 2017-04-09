using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OutpostLibrary.Navigation;

namespace Outpost
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

            Map.MapManager.CreateMap();

            ChunkAddress worldCenter = new ChunkAddress("Earth", new OutpostLibrary.IntVector3(0));

            _Game = new GameManager();

            _Game.player = new Player(worldCenter, new Microsoft.Xna.Framework.Vector3(2, 15, 2));

            //TODO: mapgen

            /*
             *  public void newMap()
                {
                    LoadingScreen.Display("Creating Map");
                    Outpost.Map.MapManager.CreateMap();

                    IntVector3 playerChunk = new IntVector3(0);
                    mapOffset = new IntVector3(-mapCenter) + playerChunk;

                    map.set(playerChunk - mapOffset, loadChunk(playerChunk));
                    map.get(playerChunk - mapOffset).endFill();

                    screenCenter = new IntVector2(graphics.Viewport.Width / 2, graphics.Viewport.Height / 2);
                    player = new Player(this, screenCenter, new IntVector3(0), new Vector3(2, 15, 2));

                    fillMap();
                }
             * 
            //*/

        }

        public static void LoadGame(string folderPath)
        {

        }

        public static void endGame()
        {

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
