using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OutpostLibrary.Navigation;
using OutpostEngine.Blocks;
using OutpostEngine.Map;

namespace OutpostEngine.Items
{
    class Tile : Item
    {
        public TestingOrder Order => TestingOrder.beforeOnly;

        public int Range => range;
        public const int range = 11;

        Block block;

        public Tile(Block type)
        {
            block = type;
        }

        public bool Test(BlockAddress testing)
        {
            return StandardBlockTests.isSolid(testing);
        }

        public bool ActionStart(BlockAddress target)
        {
            target.ChangeBlock(block);
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

        public void PerformActionsOnWielder(Entities.Entity wielder)
        {
            //TODO: make this work again
            //wielder.discardActiveItem();
        }
    }
}
