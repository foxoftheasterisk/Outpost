using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OutpostLibrary;

namespace Outpost.Map
{
    class MapSection
    {

        bool graphicsEnabled;
        IntVector3 center;
        int radius;
        string world;

        //we only have one world YET
        //but I don't want theoretical modders held up on creating their own worlds by that.
        //also, having the structure will help future me too.

    }
}
