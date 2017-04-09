using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OutpostLibrary.Navigation;
using OutpostLibrary.Content;
using Outpost.Blocks;

using Neo.IronLua;

namespace Outpost.Items
{
    class PlayerFist : Item
    {
        //remember that the same PlayerFist is used for ALL of the player's inventory slots!
        //well I suppose it doesn't have to be
        //but if it can be that would probably be good for space efficiency
        public TestingOrder order()
        {
            return TestingOrder.onOnly;
        }

        public int range()
        {
            return 11;
        }

        BlockAddress mining;
        int timer;

        Block justMined;

        public PlayerFist()
        {
            timer = 0;
            mining = null;
            justMined = null;
        }

        public bool beforeTest(BlockAddress testing)
        {
            throw new NotImplementedException();
        }

        public bool onTest(BlockAddress testing)
        {
            return standardBlockTests.isSolid(testing);
        }

        public bool actionStart(BlockAddress target)
        {
            justMined = GameShell.gameShell.getBlock(target);
            GameShell.gameShell.changeBlock(target, new SolidBlock(((GameShell.gameShell.lua.global["vanilla"] as LuaTable)["basics"] as LuaTable)["air"] as Material));
            return true;
        }

        public bool actionHold(BlockAddress target)
        {
            throw new NotImplementedException();
        }

        public bool actionEnd(BlockAddress target)
        {
            throw new NotImplementedException();
        }

        public void performActionsOnWielder(Player wielder)
        {
            wielder.replaceActiveItem(justMined.drops()[0]);
            justMined = null;
        }
    }
}
