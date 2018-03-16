using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OutpostLibrary.Navigation;
using OutpostCore.Blocks;

namespace OutpostCore.Items
{
    class Tile : Item
    {
        public TestingOrder order()
        {
            return TestingOrder.beforeOnly;
        }

        public int range()
        {
            return 11;
        }

        Block block;

        public Tile(Block type)
        {
            block = type;
        }

        public bool beforeTest(BlockAddress testing)
        {
            return StandardBlockTests.isSolid(testing);
        }

        public bool onTest(BlockAddress testing)
        {
            throw new NotImplementedException();
        }

        public bool actionStart(BlockAddress target)
        {
            GameShell.gameShell.changeBlock(target, block);
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
            wielder.discardActiveItem();
        }
    }
}
