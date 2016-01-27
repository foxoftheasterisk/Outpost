using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OutpostLibrary.Navigation;

namespace OutpostLibrary.Content
{
    public class Voxture
    {
        public OutpostColor[,,] texture;

        public Voxture()
        {
            texture = new OutpostColor[Sizes.VoxelsPerEdge, Sizes.VoxelsPerEdge, Sizes.VoxelsPerEdge];
        }

        /// <summary>
        /// Creates a new voxture of solid color.
        /// </summary>
        public Voxture(OutpostColor color)
        {
            texture = new OutpostColor[Sizes.VoxelsPerEdge, Sizes.VoxelsPerEdge, Sizes.VoxelsPerEdge];
            for(int x=0; x<Sizes.VoxelsPerEdge; x++)
            {
                for (int y = 0; y < Sizes.VoxelsPerEdge; y++)
                {
                    for (int z = 0; z < Sizes.VoxelsPerEdge; z++)
                    {
                        texture[x, y, z] = color;
                    }
                }
            }
        }

        public OutpostColor this[int i, int j, int k]
        {
            get
            {
                return texture[i, j, k];
            }
            set
            {
                texture[i, j, k] = value;
            }
        }
    }
}
