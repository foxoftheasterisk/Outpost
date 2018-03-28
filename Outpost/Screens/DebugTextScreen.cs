using ScreenManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace OutpostEngine.Screens
{
    class DebugTextScreen : Screen
    {
        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Draw(SpriteBatch drawer)
        {
            drawer.DrawString(GraphicsManager.graphicsManager.font, "I THINK THEREFORE I ARRRRR", new Vector2(10, 10), new Color(10, 20, 30));
            //drawer.DrawString(fontling, player.Pos().ToString(), new Vector2(10, 40), Color.AntiqueWhite);
            //no more creepiest glitch ever
            //instead we have working things
            //...I don't know WHY it's working, but it is.  Not going to question this further.

            //TODO: temporarily disabled
            /*
            drawer.DrawString(GraphicsManager.graphicsManager.font, "Position: " + player.Pos().ToString(), new Vector2(10, 40), new Color(7, 7, 7));

            if (selected != BlockAddress.noBlock)
                drawer.DrawString(GraphicsManager.graphicsManager.font, "Selected: " + selected.ToString(), new Vector2(10, 70), Color.CadetBlue);
            //*/
        }

        public bool DrawUnder()
        {
            throw new NotImplementedException();
        }

        public bool ShouldClose()
        {
            throw new NotImplementedException();
        }

        public (bool updateBelow, bool shouldClose) Update(InputSet input)
        {
            return (true, false);
        }
    }
}
