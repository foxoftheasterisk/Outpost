using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OutpostLibrary.Navigation;
using OutpostLibrary.Content;
using OutpostCore.Map;

namespace OutpostCore.Items
{
    public enum TestingOrder { noTest, onOnly, beforeOnly, onFirst, beforeFirst };
    
    public interface Item
    {
        TestingOrder Order { get; }
        int Range { get; }

        bool Test(BlockAddress testing);

        bool ActionStart(BlockAddress target);
        bool ActionHold(BlockAddress target);
        bool ActionEnd(BlockAddress target);
        //these three all assume that the block passed in is one that fulfills the tests ^
        //note, once items are fully implemented, all three of these will be required.
        //well, except on 'noTest' items, I guess.
        //i guess this is moot though cause the compiler requires them anyway.

        void PerformActionsOnWielder(Entities.Entity wielder);
        //this assumes that 'true' has been passed from one of those three ^
        //make SUPER SURE that it is not possible to switch items between that pass and this function.
    }

    public static class StandardBlockTests
    {
        public static bool isSolid(BlockAddress testing)
        {
            Blocks.Block block = testing.GetBlock();
            if (block == null)
                return false;
            if (block.solidity == Solidity.solid)
                return true;
            return false;
        }
    }
}
