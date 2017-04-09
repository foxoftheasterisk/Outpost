using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outpost.Screens
{
    /// <summary>
    /// simple bridge class to connect GameManager to ScreenManager
    /// </summary>
    class GameScreen : Screen
    {
        

        public bool update(bool useInput)
        {
            if (GameManager.Game == null)
                throw new InvalidOperationException("No game yet!");

            GameManager.Game.update(useInput);

            return false;
        }

        public bool drawUnder()
        {
            return false;
        }

        public void draw(Microsoft.Xna.Framework.Graphics.SpriteBatch drawer)
        {
            if (GameManager.Game == null)
                throw new InvalidOperationException("No game yet!");

            GameManager.Game.draw(drawer);
        }

        public bool shouldClose()
        {
            throw new NotImplementedException();
        }
    }
}
