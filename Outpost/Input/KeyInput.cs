using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScreenManagement;

namespace OutpostEngine.Input
{
    class KeyInput : InputItem
    {
        public Keys key;

        public KeyInput(Keys _key)
        {
            key = _key;
        }
    }

    //TODO: convert to a configurable version
    class KeyInputIdentifier : IInputIdentifier
    {
        Keys key;

        public KeyInputIdentifier(Keys _key)
        {
            key = _key;
        }

        public bool Matches(InputItem input)
        {
            if (!(input is KeyInput))
                return false;
            if(key == (input as KeyInput).key)
                return true;

            return false;
        }
    }
}
