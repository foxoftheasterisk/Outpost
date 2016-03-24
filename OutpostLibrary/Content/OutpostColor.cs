using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace OutpostLibrary.Content
{
    //eventually I'll add custom specular and make this more than a pointless wrapper
    public class OutpostColor
    {
        private Color c;

        public Color color
        {
            get
            {
                return c;
            }
        }

        public OutpostColor(int r, int g, int b, int a)
        {
            c = new Color(r, g, b, a);
        }

        public OutpostColor(Color _c)
        {
            c = _c;
        }
    }
}
