using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OutpostEngine.Blocks;
using OutpostLibrary.Navigation;

namespace OutpostEngine.Map
{
    public static class MapExtensions
    {
        public static Block GetBlock(this BlockAddress blockAddress)
        {
            Chunk c = MapManager.Map[blockAddress.GetChunkAddress()];
            if (c == null)
                return null;

            return c[blockAddress.block];
        }

        public static bool ChangeBlock(this BlockAddress blockAddress, Block newBlock)
        {
            Chunk c = MapManager.Map[blockAddress.GetChunkAddress()];

            if (c == null)
                return false;

            return c.assignBlock(blockAddress.block, newBlock);
        }

        //a minor performance increase,
        //but mostly here to make it easier not to forgetting to copy to temp, or whatever
        public static (bool, Block) SwapBlock(this BlockAddress blockAddress, Block newBlock)
        {
            Chunk c = MapManager.Map[blockAddress.GetChunkAddress()];
            if (c == null)
                return (false, null);

            Block oldBlock = c[blockAddress.block];

            return (c.assignBlock(blockAddress.block, newBlock), oldBlock);
        }
    }
}
