using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OutpostLibrary.Navigation;
using OutpostLibrary.Content;
using OutpostEngine.Blocks;
using OutpostEngine.Entities;

using Neo.IronLua;
using OutpostEngine.Map;

namespace OutpostEngine.Items
{
    class PlayerFist : Item
    {
        //remember that the same PlayerFist is used for ALL of the player's inventory slots!
        //well I suppose it doesn't have to be
        //but if it can be that would probably be good for space efficiency
        public TestingOrder Order => TestingOrder.onOnly;

        public int Range => range;
        public const int range = 11;

        BlockAddress? mining;
        int timer;

        Block justMined;

        public PlayerFist()
        {
            timer = 0;
            mining = null;
            justMined = null;
        }

        public bool Test(BlockAddress testing)
        {
            return StandardBlockTests.isSolid(testing);
        }

        public bool ActionStart(BlockAddress target)
        {
            Block newBlock = new SolidBlock(((GameShell.gameShell.lua.global["vanilla"] as LuaTable)["basics"] as LuaTable)["air"] as Material);
            (_, justMined) = target.SwapBlock(newBlock);  //probably should actually pay attention to that check...
            return true;
        }

        public bool ActionHold(BlockAddress target)
        {
            throw new NotImplementedException();
        }

        public bool ActionEnd(BlockAddress target)
        {
            throw new NotImplementedException();
        }

        public void PerformActionsOnWielder(Entity wielder)
        {
            if (!(wielder is Player))
                Logger.Log("PlayerFist wielded by non-player??");

            Player player = wielder as Player;
            player.replaceActiveItem(justMined.drops()[0]);
            justMined = null;
        }
    }
}
