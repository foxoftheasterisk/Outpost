using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using OutpostLibrary.Navigation;

namespace OutpostLibrary.Content
{
    public class Material
    {
        public Transparency transparency;
        public Solidity solidity;
        public Voxture voxture;

        public string name;  //for toString purposes, mostly

        public Material(string _name)
        {
            name = _name;
        }

        public OutpostColor this[int i, int j, int k]
        {
            get
            {
                return voxture[i, j, k];
            }
        }

        public override string ToString()
        {
            return name;
        }
    }

    public enum Transparency { transparent, translucent, opaque }
    public enum Solidity { solid, liquid, gas, vacuum, plasma }
    
}
