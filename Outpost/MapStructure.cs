using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Neo.IronLua;

namespace Outpost
{
    /// <summary>
    /// Stores a mapgen structure, including its name, whether and how often to update it, and how to save it
    /// Basically it's all Lua-side, though.
    /// </summary>
    class MapStructure : LuaTable
    {
        //uh, hm, there should probably be something to intercept block interactions... for some structures, anyway.
        //...no idea how to do that effectively, atm.
        //figure it out later

        //maybe, also, have a draw for the structure? i dunno that could be weird.

        [LuaMember("name")]
        public string name {get;set;}

        [LuaMember("living")]
        public bool living { get; set; }
        //if false, structure will no longer be stored when all chunks within radius have been generated
        //must be true in order for the structure to be updated

        [LuaMember("radius")]
        public int radius {get;set;}
        //how many chunks away this structure should be considered (for mapgen, updates, etc.)

        public enum UpdateMode { None, ActiveOnly, CatchUp, Maintain, ActivateChunk}
        //when this structure should be updated
        //None: Does not update.
        //ActiveOnly: Updates only when the chunk is active.  Preferred mode for animations, etc.
        //CatchUp: When the chunk is made active, updates a number of times required to "catch up" to the current game time.  Preferred mode for plants and other growing structures.
        //Maintain: Continues to update even while the chunk is inactive.  Use sparingly.
        //ActivateChunk: Prevents the chunk containing this structure from ever being made inactive.  Use extremely sparingly.

        [LuaMember("updateMode")]
        public UpdateMode updateMode { get; set; }

        [LuaMember("updateTime")]
        public int updateTime { get; set; }
        //The number of frames between each update.

    }
}
