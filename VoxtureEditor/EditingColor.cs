using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace VoxtureEditor
{
    class EditingColor : OutpostLibrary.Content.OutpostColor
    {
        public string name;

        public EditingColor(string _name, int r, int g, int b, int a) : base(r,g,b,a)
        {
            name = _name;
        }

        public EditingColor(string _name, Color _c) : base(_c)
        {
            name = _name;
        }

        public override string ToString()
        {
            return name + ":\n" + color.R + " " + color.G + " " + color.B + " " + color.A;
        }
    }
}
