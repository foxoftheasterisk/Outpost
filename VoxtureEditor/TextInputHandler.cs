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

        public TextInputHandler(GameWindow w)
        {
            EventInput.EventInput.Initialize(w);
            EventInput.EventInput.CharEntered += new EventInput.CharEnteredHandler(CharEntered);
            EventInput.EventInput.KeyDown += new EventInput.KeyEventHandler(EventInput_KeyDown);
        }

        public void editString(StringBuilder str)
        {
            editingString = str;
        }

        public void stopEditing()
        {
            editingString = null;
        }

        //????
        void EventInput_KeyDown(object sender, EventInput.KeyEventArgs e)
        {
            return;
        }

        void CharEntered(object sender, EventInput.CharacterEventArgs e)
        {
            if (editingString == null)
                return;

            char c = e.Character;

            if(Char.IsControl(c))
            {
                switch(c)
                {
                    case (char)0x16:
                        //some voodoo to get clipboard input, since XNA doesn't like it
                        //(copied from StackOverflow)
                        Thread thread = new Thread(PasteThread);
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                        thread.Join();

                        editingString.Append(pasteResult);
                        break;
                    default:
                        Console.Write(c);
                        break;
                }
            }
            else
            {
                editingString.Append(c);
            }
        }

        //more copy-pasted voodoo for getting clipboard
        string pasteResult = "";
        [STAThread]
        void PasteThread()
        {
            if (Clipboard.ContainsText())
            {
                pasteResult = Clipboard.GetText();
            }
            else
            {
                pasteResult = "";
            }
        }

    }



}
