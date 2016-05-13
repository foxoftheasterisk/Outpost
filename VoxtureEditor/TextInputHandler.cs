using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Xna.Framework;

using System.Threading;
using System.Windows.Forms;

//some of the code in this class is copied from http://stackoverflow.com/a/10222878/1778528

namespace VoxtureEditor
{
    class TextInputHandler
    {

        StringBuilder editingString;
        EventHandler<TextInputEventArgs> eh;
        GameWindow w;

        public TextInputHandler(GameWindow _w)
        {
            w = _w;
            eh = new EventHandler<TextInputEventArgs>(TextEntered);
        }

        public void editString(StringBuilder str)
        {
            w.TextInput += eh;
            editingString = str;
        }

        public void stopEditing()
        {
            w.TextInput -= eh;
            editingString = null;
        }

        void TextEntered(object sender, TextInputEventArgs e)
        {
            if (editingString == null)
                return;

            char c = e.Character;

            //OpenGL doesn't pass special characters this way,
            //so we have to handle them elsewhere
            //but we have to handle them here for other platforms
            #if OpenGL
            editingString.Append(c);
            #else
            switch (c)
            {
                case '\b':
                    //backspace
                    if (editingString.Length == 0)
                        break;
                    editingString.Remove(editingString.Length - 1, 1);
                    break;
                case '\n':
                    //enter is handled elsewhere
                    break;
                default:
                    editingString.Append(c);
                    break;
            }
            #endif
        }

        #if OpenGL
        public enum SpecialCharacters{
            Backspace
        }

        public void specialCharacter(SpecialCharacters c)
        {
            switch(c)
            {
                case SpecialCharacters.Backspace:
                    if (editingString.Length == 0)
                        break;
                    editingString.Remove(editingString.Length - 1, 1);
                    break;
            }
        }

        #endif

    }



}
