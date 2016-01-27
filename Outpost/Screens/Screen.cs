using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Outpost.Screens
{
    public interface Screen
    {
        /// <summary>
        /// Updates the screen.
        /// Only the top screen currently uses input.  A more elegant screen solution will be devised later.
        /// </summary>
        /// <param name="useInput">Whether the screen is allowed to use input.</param>
        /// <returns>Whether the screen below should be updated.</returns>
        bool Update(bool useInput);

        /// <summary>
        /// Determines whether the screen under this one should be drawn.
        /// Screens are always drawn from the bottom of the stack to the top.
        /// </summary>
        /// <returns></returns>
        bool drawUnder();
        void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch drawer);

        /// <summary>
        /// Polling method to determine if the screen should be removed from the stack.
        /// As a stack, the ScreenManager will only remove screens from the top down.
        /// Therefore, as a general rule the screen should close when escape is pressed.
        /// 
        /// Screens are polled after the update call.  If true, they will be removed before the draw call.
        /// </summary>
        /// <returns>Whether the screen in question should close.</returns>
        bool shouldClose();
    }
}
