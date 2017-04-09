using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OutpostLibrary.Navigation;
using OutpostLibrary.Content;

namespace Outpost
{
    public enum TestingOrder { noTest, onOnly, beforeOnly, onFirst, beforeFirst };
    
    public interface Item
    {
        TestingOrder order();
        int range();
        //I guess it makes sense to have these not necessarily constant anyway

        bool beforeTest(BlockAddress testing);
        bool onTest(BlockAddress testing);

        bool actionStart(BlockAddress target);
        bool actionHold(BlockAddress target);
        bool actionEnd(BlockAddress target);
        //these three all assume that the block passed in is one that fulfills the tests ^
        //note, once items are fully implemented, all three of these will be required.
        //well, except on 'noTest' items, I guess.
        //i guess this is moot though cause the compiler requires them anyway.

        void performActionsOnWielder(Player wielder); //will have to change that "Player" to, iono, "Entity" or summat
        //this assumes that 'true' has been passed from one of those three ^
        //make SUPER SURE that it is not possible to switch items between that pass and this function.
    }

    public static class standardBlockTests
    {
        public static bool isSolid(BlockAddress testing)
        {
            if (testing == null)
                return false;
            if (GameShell.gameShell.getBlock(testing).solidity == Solidity.solid)
                return true;
            return false;
        }
    }
}
