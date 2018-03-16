using Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutpostCore
{
    /// <summary>
    /// simple bridge class to connect GameManager to ScreenManager
    /// </summary>
    class GameScreen : Screen
    {


        public (bool updateBelow, bool shouldClose) Update(InputSet input)
        {
            if (GameManager.Game == null)
                throw new InvalidOperationException("No game yet!");

            GameManager.Game.Update(input);

            return (false, false);
        }

        public bool DrawUnder()
        {
            return false;
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch drawer)
        {
            if (GameManager.Game == null)
                throw new InvalidOperationException("No game yet!");

            GameManager.Game.Draw(drawer);
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public bool ShouldClose()
        {
            return false;
            //TODO: something something pause menu exit option
        }
    }
}
