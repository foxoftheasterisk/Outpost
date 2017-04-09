using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outpost.Screens
{
    class LoadingScreen : Screen
    {

        

        public static void Display(string message)
        {
            if(activeScreen == null)
            {
                activeScreen = new LoadingScreen(message);
                ScreenManager.screenManager.push(activeScreen);
            }
            else
            {
                activeScreen.message = message;
            }
        }
        private static LoadingScreen activeScreen = null;

        /// <summary>
        /// Changes the message displayed,
        /// but doesn't create a loading screen if one doesn't already exist.
        /// </summary>
        /// <param name="message"></param>
        public static void ChangeMessage(string message)
        {
            if (activeScreen != null)
                activeScreen.message = message;
        }

        public static void Close()
        {
            activeScreen.hardClose = true;
        }

        private string message;
        private bool isActive = true; //what??? is this???? doing???
        private bool hardClose = false;

        private LoadingScreen(string _message)
        {
            message = _message;
        }

        public bool update(bool useInput)
        {
            isActive = true;
            return false; //...i think
        }

        public bool drawUnder()
        {
            return false;
        }

        public void draw(Microsoft.Xna.Framework.Graphics.SpriteBatch drawer)
        {
            drawer.GraphicsDevice.Clear(new Microsoft.Xna.Framework.Color(0, 0, 200));
            drawer.DrawString(GameShell.gameShell.fontling, message, new Microsoft.Xna.Framework.Vector2(100, 100), new Microsoft.Xna.Framework.Color(0, 0, 0));
        }

        public bool shouldClose()
        {
            if(!hardClose && isActive)
            {
                isActive = false;
                return false;
            }
            activeScreen = null;
            return true;
        }
    }
}
