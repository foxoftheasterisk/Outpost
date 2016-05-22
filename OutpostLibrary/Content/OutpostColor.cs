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
        protected Color c;
        //In contrast to the specular, this color value is assumed to be NOT premultiplied alpha.

        public Color color
        {
            get
            {
                return c;
            }
        }

        protected Color s;

        public Color specular
        {
            get
            {
                return s;
            }
        }

        //for specular, it's assumed to be premultiplied alpha with the brightest channel being the alpha
        //therefore, to get "clearer" specular, simply use a darker color
        //the alpha channel is instead used for the power, or "shinyness" factor, of the specular

        /// <summary>
        /// Creates a new OutpostColor.  All parameters scale from 0 to 255.
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <param name="a">Alpha</param>
        /// <param name="spec_r">Red in the specular reflection</param>
        /// <param name="spec_g">Green in the specular reflection</param>
        /// <param name="spec_b">Blue in the specular reflection</param>
        /// <param name="spec_p">'Shinyness' of the specular reflection</param>
        public OutpostColor(int r, int g, int b, int a, int spec_r, int spec_g, int spec_b, int spec_p)
        {
            c = new Color(r, g, b, a);
            s = new Color(spec_r, spec_g, spec_b, spec_p);
        }

        public OutpostColor(Color _c, Color _specular)
        {
            c = _c;
            s = _specular;
        }

        
    }
}
