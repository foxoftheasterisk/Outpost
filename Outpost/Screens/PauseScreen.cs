using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Input;

namespace Outpost.Screens
{
    class PauseScreen : Screen
    {
        bool close = false;

        //pauses the game... forEVer!
        public bool Update(bool useInput)
        {
            KeyboardState keys;
            try
            {
                keys = Keyboard.GetState();
            }
            catch (InvalidOperationException)
            {
                keys = new KeyboardState();//bad idea??
            }

            if (keys.IsKeyDown(Keys.Enter))
                close = true;

            return false;
        }

        public bool drawUnder()
        {
            return true;
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch drawer)
        {
            drawer.Draw(MainGame.mainGame.blank, drawer.GraphicsDevice.Viewport.Bounds, new Microsoft.Xna.Framework.Color(0, 0, 0, 140));
        }

        public bool shouldClose()
        {
            return close;
        }
    }
}
