using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OutpostLibrary;
using OutpostLibrary.Navigation;
using OutpostLibrary.Content;
using Outpost.Items;
using Microsoft.Xna.Framework;

namespace Outpost.Blocks
{
    class WornBlock : Block
    {
        Material _primary;

        Material[, ,] exceptions;

        public WornBlock(Material primary)
        {
            _primary = primary;
            exceptions = new Material[Sizes.VoxelsPerEdge, Sizes.VoxelsPerEdge, Sizes.VoxelsPerEdge];
            opaqUpdated = false;
        }

        #region opaq*

        public bool opaqN
        {
            get
            {
                if (!opaqUpdated)
                    updateOpacity();
                return _opaqN;
            }
            private set
            {
                _opaqN = value;
            }
        }

        public bool opaqS
        {
            get
            {
                if (!opaqUpdated)
                    updateOpacity();
                return _opaqS;
            }
            private set
            {
                _opaqS = value;
            }
        }

        public bool opaqE
        {
            get
            {
                if (!opaqUpdated)
                    updateOpacity();
                return _opaqE;
            }
            private set
            {
                _opaqE = value;
            }
        }

        public bool opaqW
        {
            get
            {
                if (!opaqUpdated)
                    updateOpacity();
                return _opaqW;
            }
            private set
            {
                _opaqW = value;
            }
        }

        public bool opaqU
        {
            get
            {
                if (!opaqUpdated)
                    updateOpacity();
                return _opaqU;
            }
            private set
            {
                _opaqU = value;
            }
        }

        public bool opaqD
        {
            get 
            {
                if (!opaqUpdated)
                    updateOpacity();
                return _opaqD;
            }
            private set
            {
                _opaqD = value;
            }
        }

        public bool isTransparent
        {
            get
            {
                if (!opaqUpdated)
                    updateOpacity();
                return _isTransparent;
            }
            private set
            {
                _isTransparent = value;
            }
        }

        private bool _opaqN, _opaqS, _opaqE, _opaqW, _opaqU, _opaqD, _isTransparent, opaqUpdated;

        #endregion opaq*

        #region traits

        public Solidity solidity
        {
            get { return primary.solidity; }
        }

        public Material primary
        {
            get
            {
                return _primary;
            }
        }

        #endregion traits

        #region neighbors

        public Block neighborN
        {
            set { _neighborN = value; }
            get { return _neighborN; }
        }

        public Block neighborS
        {
            set { _neighborS = value; }
            get { return _neighborS; }
        }

        public Block neighborE
        {
            set { _neighborE = value; }
            get { return _neighborE; }
        }

        public Block neighborW
        {
            set { _neighborW = value; }
            get { return _neighborW; }
        }

        public Block neighborU
        {
            set { _neighborU = value; }
            get { return _neighborU; }
        }

        public Block neighborD
        {
            set { _neighborD = value; }
            get { return _neighborD; }
        }

        Block _neighborN, _neighborS, _neighborE, _neighborW, _neighborU, _neighborD;

        #endregion neighbors

        public string mapGenName
        {
            get
            {
                return "Worn " + primary.name;
            }
        }

        public Material this[int x, int y, int z]
        {
            get
            {
                if (exceptions[x, y, z] != null)
                    return exceptions[x, y, z];
                return primary;
            }
            set
            {
                if (value != primary)
                    exceptions[x, y, z] = value;
                else
                    exceptions[x, y, z] = null;
                opaqUpdated = false;
            }
        }

        public Material this[IntVector3 loc]
        {
            get
            {
                return this[loc.X, loc.Y, loc.Z];
            }
            set
            {
                this[loc.X, loc.Y, loc.Z] = value;
            }
        }

        public Item[] drops()
        {
            Item drop = new Tile(new SolidBlock(primary));
            return new Item[] { drop };
        }

        /// <summary>
        /// Wears the block down based on its neighbors.
        /// NOT FULLY IMPLEMENTED - currently is a big mess.
        /// NOTE: Algorithm is based on assumption that neighboring faces are composed of this block's material and one other.  Other compositions may yield strange results.
        /// ALSO, assumes that the block is currently a single material.
        /// </summary>
        public void wearDown()
        {
            

            //okay... significantly better, but less dramatic of effects than I was thinking/hoping/expecting.
            //let's try tweaking distance?
            //seems better
            //though I think it would also help if it could cross blocks

            IntVector3 nue = new IntVector3(Sizes.VoxelsPerEdge - 1, Sizes.VoxelsPerEdge - 1, Sizes.VoxelsPerEdge - 1);
            wearDownRecursive(nue, nue);

            IntVector3 sue = new IntVector3(0, Sizes.VoxelsPerEdge - 1, Sizes.VoxelsPerEdge - 1);
            wearDownRecursive(sue, sue);

            IntVector3 nde = new IntVector3(Sizes.VoxelsPerEdge - 1, 0, Sizes.VoxelsPerEdge - 1);
            wearDownRecursive(nde, nde);

            IntVector3 nuw = new IntVector3(Sizes.VoxelsPerEdge - 1, Sizes.VoxelsPerEdge - 1, 0);
            wearDownRecursive(nuw, nuw);

            IntVector3 sde = new IntVector3(0, 0, Sizes.VoxelsPerEdge - 1);
            wearDownRecursive(sde, sde);

            IntVector3 suw = new IntVector3(0, Sizes.VoxelsPerEdge - 1, 0);
            wearDownRecursive(suw, suw);

            IntVector3 ndw = new IntVector3(Sizes.VoxelsPerEdge - 1, 0, 0);
            wearDownRecursive(ndw, ndw);

            IntVector3 sdw = new IntVector3(0, 0, 0);
            wearDownRecursive(sdw, sdw);

            /*
            
            Material[, ,] keys = new Material[3, 3, 3];
            int middle = 2; //don't care about calculating it but having it a variable in case of changing VoxelsPerBlock.
            int highEdge = Sizes.VoxelsPerEdge - 1;
            
            #region antinull

            Block n, s, u, d, e, w;

            if (neighborN == null)
                n = new SolidBlock(primary);
            else
                n = neighborN;

            if (neighborS == null)
                s = new SolidBlock(primary);
            else
                s = neighborS;

            if (neighborU == null)
                u = new SolidBlock(primary);
            else
                u = neighborU;

            if (neighborD == null)
                d = new SolidBlock(primary);
            else
                d = neighborD;

            if (neighborE == null)
                e = new SolidBlock(primary);
            else
                e = neighborE;

            if (neighborW == null)
                w = new SolidBlock(primary);
            else
                w = neighborW;

            #endregion antinull

            keys[1, 1, 1] = primary;
            
            #region faces

            if (n[0, middle, middle] == primary)
                keys[2, 1, 1] = primary;
            else
            {
                int matches = 0;
                if (n[0, middle - 1, middle] == primary)
                    matches++;
                if (n[0, middle + 1, middle] == primary)
                    matches++;
                if (n[0, middle, middle - 1] == primary)
                    matches++;
                if (n[0, middle, middle + 1] == primary)
                    matches++;

                if (MainGame.mainGame.random.Next(2) < matches)
                    keys[2, 1, 1] = primary;
                else
                    keys[2, 1, 1] = n[0, middle, middle];
            }

            if (s[highEdge, middle, middle] == primary)
                keys[0, 1, 1] = primary;
            else
            {
                int matches = 0;
                if (s[highEdge, middle - 1, middle] == primary)
                    matches++;
                if (s[highEdge, middle + 1, middle] == primary)
                    matches++;
                if (s[highEdge, middle, middle - 1] == primary)
                    matches++;
                if (s[highEdge, middle, middle + 1] == primary)
                    matches++;

                if (MainGame.mainGame.random.Next(2) < matches)
                    keys[0, 1, 1] = primary;
                else
                    keys[0, 1, 1] = s[highEdge, middle, middle];
            }

            if (u[middle, 0, middle] == primary)
                keys[1, 2, 1] = primary;
            else
            {
                int matches = 0;
                if (u[middle - 1, 0, middle] == primary)
                    matches++;
                if (u[middle + 1, 0, middle] == primary)
                    matches++;
                if (u[middle, 0, middle - 1] == primary)
                    matches++;
                if (u[middle, 0, middle + 1] == primary)
                    matches++;

                if (MainGame.mainGame.random.Next(2) < matches)
                    keys[1, 2, 1] = primary;
                else
                    keys[1, 2, 1] = u[middle, 0, middle];
            }

            if (d[middle, highEdge, middle] == primary)
                keys[1, 0, 1] = primary;
            else
            {
                int matches = 0;
                if (d[middle - 1, highEdge, middle] == primary)
                    matches++;
                if (d[middle + 1, highEdge, middle] == primary)
                    matches++;
                if (d[middle, highEdge, middle - 1] == primary)
                    matches++;
                if (d[middle, highEdge, middle + 1] == primary)
                    matches++;

                if (MainGame.mainGame.random.Next(2) < matches)
                    keys[1, 0, 1] = primary;
                else
                    keys[1, 0, 1] = d[middle, highEdge, middle];
            }

            if (e[0, middle, middle] == primary)
                keys[1, 1, 2] = primary;
            else
            {
                int matches = 0;
                if (e[middle - 1, middle, 0] == primary)
                    matches++;
                if (e[middle + 1, middle, 0] == primary)
                    matches++;
                if (e[middle, middle - 1, 0] == primary)
                    matches++;
                if (e[middle, middle + 1, 0] == primary)
                    matches++;

                if (MainGame.mainGame.random.Next(2) < matches)
                    keys[1, 1, 2] = primary;
                else
                    keys[1, 1, 2] = e[middle, middle, 0];
            }

            if (w[highEdge, middle, middle] == primary)
                keys[1, 1, 0] = primary;
            else
            {
                int matches = 0;
                if (w[middle - 1, middle, highEdge] == primary)
                    matches++;
                if (w[middle + 1, middle, highEdge] == primary)
                    matches++;
                if (w[middle, middle - 1, highEdge] == primary)
                    matches++;
                if (w[middle, middle + 1, highEdge] == primary)
                    matches++;

                if (MainGame.mainGame.random.Next(2) < matches)
                    keys[1, 1, 0] = primary;
                else
                    keys[1, 1, 0] = w[middle, middle, highEdge];
            }

            #endregion faces

            #region edges

            //ne
            if (n[0, middle, highEdge] == e[highEdge, middle, 0])
                keys[2, 1, 2] = n[0, middle, highEdge];
            else
            {
                switch (MainGame.mainGame.random.Next(3))
                {
                    case 0:
                        keys[2, 1, 2] = n[0, middle, highEdge];
                        break;
                    case 1:
                        keys[2, 1, 2] = e[highEdge, middle, 0];
                        break;
                    case 2:
                        keys[2, 1, 2] = primary;
                        break;
                }
            }
            //nw
            if (n[0, middle, 0] == w[highEdge, middle, highEdge])
                keys[2, 1, 0] = n[0, middle, 0];
            else
            {
                switch (MainGame.mainGame.random.Next(3))
                {
                    case 0:
                        keys[2, 1, 0] = n[0, middle, 0];
                        break;
                    case 1:
                        keys[2, 1, 0] = w[highEdge, middle, highEdge];
                        break;
                    case 2:
                        keys[2, 1, 0] = primary;
                        break;
                }
            }
            //se
            if (s[highEdge, middle, highEdge] == e[0, middle, 0])
                keys[0, 1, 2] = s[highEdge, middle, highEdge];
            else
            {
                switch (MainGame.mainGame.random.Next(3))
                {
                    case 0:
                        keys[0, 1, 2] = s[highEdge, middle, highEdge];
                        break;
                    case 1:
                        keys[0, 1, 2] = e[0, middle, 0];
                        break;
                    case 2:
                        keys[0, 1, 2] = primary;
                        break;
                }
            }
            //sw
            if (s[highEdge, middle, 0] == w[0, middle, highEdge])
                keys[0, 1, 0] = s[highEdge, middle, 0];
            else
            {
                switch (MainGame.mainGame.random.Next(3))
                {
                    case 0:
                        keys[0, 1, 0] = s[highEdge, middle, 0];
                        break;
                    case 1:
                        keys[0, 1, 0] = w[0, middle, highEdge];
                        break;
                    case 2:
                        keys[0, 1, 0] = primary;
                        break;
                }
            }
            //nu
            if (n[0, highEdge, middle] == u[highEdge, 0, middle])
                keys[2, 2, 1] = n[0, highEdge, middle];
            else
            {
                switch (MainGame.mainGame.random.Next(3))
                {
                    case 0:
                        keys[2, 2, 1] = n[0, highEdge, middle];
                        break;
                    case 1:
                        keys[2, 2, 1] = u[highEdge, 0, middle];
                        break;
                    case 2:
                        keys[2, 2, 1] = primary;
                        break;
                }
            }
            //nd
            if (n[0, 0, middle] == d[highEdge, highEdge, middle])
                keys[2, 0, 1] = n[0, 0, middle];
            else
            {
                switch (MainGame.mainGame.random.Next(3))
                {
                    case 0:
                        keys[2, 0, 1] = n[0, 0, middle];
                        break;
                    case 1:
                        keys[2, 0, 1] = u[highEdge, highEdge, middle];
                        break;
                    case 2:
                        keys[2, 0, 1] = primary;
                        break;
                }
            }
            //su
            if (s[highEdge, highEdge, middle] == u[0, 0, middle])
                keys[0, 2, 1] = s[highEdge, highEdge, middle];
            else
            {
                switch (MainGame.mainGame.random.Next(3))
                {
                    case 0:
                        keys[0, 2, 1] = s[highEdge, highEdge, middle];
                        break;
                    case 1:
                        keys[0, 2, 1] = u[0, 0, middle];
                        break;
                    case 2:
                        keys[0, 2, 1] = primary;
                        break;
                }
            }
            //sd
            if (s[highEdge, 0, middle] == d[0, highEdge, middle])
                keys[0, 0, 1] = s[highEdge, 0, middle];
            else
            {
                switch (MainGame.mainGame.random.Next(3))
                {
                    case 0:
                        keys[0, 0, 1] = s[highEdge, 0, middle];
                        break;
                    case 1:
                        keys[0, 0, 1] = d[0, highEdge, middle];
                        break;
                    case 2:
                        keys[0, 0, 1] = primary;
                        break;
                }
            }
            
            //ue
            if (u[middle, 0, highEdge] == e[middle, highEdge, 0])
                keys[1, 2, 2] = u[middle, 0, highEdge];
            else
            {
                switch (MainGame.mainGame.random.Next(3))
                {
                    case 0:
                        keys[1, 2, 2] = u[middle, 0, highEdge];
                        break;
                    case 1:
                        keys[1, 2, 2] = e[middle, highEdge, 0];
                        break;
                    case 2:
                        keys[1, 2, 2] = primary;
                        break;
                }
            }
            //uw
            if (u[middle, 0, 0] == w[middle, highEdge, highEdge])
                keys[1, 2, 0] = u[middle, 0, 0];
            else
            {
                switch (MainGame.mainGame.random.Next(3))
                {
                    case 0:
                        keys[1, 2, 0] = u[middle, 0, 0];
                        break;
                    case 1:
                        keys[1, 2, 0] = w[middle, highEdge, highEdge];
                        break;
                    case 2:
                        keys[1, 2, 0] = primary;
                        break;
                }
            }
            //de
            if (d[middle, highEdge, highEdge] == e[middle, 0, 0])
                keys[1, 0, 2] = d[middle, highEdge, highEdge];
            else
            {
                switch (MainGame.mainGame.random.Next(3))
                {
                    case 0:
                        keys[1, 0, 2] = d[middle, highEdge, highEdge];
                        break;
                    case 1:
                        keys[1, 0, 2] = e[middle, 0, 0];
                        break;
                    case 2:
                        keys[1, 0, 2] = primary;
                        break;
                }
            }
            //dw
            if (d[middle, highEdge, 0] == w[middle, 0, highEdge])
                keys[1, 0, 0] = d[middle, highEdge, 0];
            else
            {
                switch (MainGame.mainGame.random.Next(3))
                {
                    case 0:
                        keys[1, 0, 0] = d[middle, highEdge, 0];
                        break;
                    case 1:
                        keys[1, 0, 0] = w[middle, 0, highEdge];
                        break;
                    case 2:
                        keys[1, 0, 0] = primary;
                        break;
                }
            }

            #endregion edges


            //corners section is still old
            #region corners

            switch (MainGame.mainGame.random.Next(3))
            {
                case 0:
                    keys[0, 0, 0] = keys[1, 0, 0];
                    break;
                case 1:
                    keys[0, 0, 0] = keys[0, 1, 0];
                    break;
                case 2:
                    keys[0, 0, 0] = keys[0, 0, 1];
                    break;
            }

            switch (MainGame.mainGame.random.Next(3))
            {
                case 0:
                    keys[2, 0, 0] = keys[1, 0, 0];
                    break;
                case 1:
                    keys[2, 0, 0] = keys[2, 1, 0];
                    break;
                case 2:
                    keys[2, 0, 0] = keys[2, 0, 1];
                    break;
            }

            switch (MainGame.mainGame.random.Next(3))
            {
                case 0:
                    keys[0, 2, 0] = keys[1, 2, 0];
                    break;
                case 1:
                    keys[0, 2, 0] = keys[0, 1, 0];
                    break;
                case 2:
                    keys[0, 2, 0] = keys[0, 2, 1];
                    break;
            }

            switch (MainGame.mainGame.random.Next(3))
            {
                case 0:
                    keys[0, 0, 2] = keys[1, 0, 2];
                    break;
                case 1:
                    keys[0, 0, 2] = keys[0, 1, 2];
                    break;
                case 2:
                    keys[0, 0, 2] = keys[0, 0, 1];
                    break;
            }

            switch (MainGame.mainGame.random.Next(3))
            {
                case 0:
                    keys[2, 2, 0] = keys[1, 2, 0];
                    break;
                case 1:
                    keys[2, 2, 0] = keys[2, 1, 0];
                    break;
                case 2:
                    keys[2, 2, 0] = keys[2, 2, 1];
                    break;
            }

            switch (MainGame.mainGame.random.Next(3))
            {
                case 0:
                    keys[2, 0, 2] = keys[1, 0, 2];
                    break;
                case 1:
                    keys[2, 0, 2] = keys[2, 1, 2];
                    break;
                case 2:
                    keys[2, 0, 2] = keys[2, 0, 1];
                    break;
            }

            switch (MainGame.mainGame.random.Next(3))
            {
                case 0:
                    keys[0, 2, 2] = keys[1, 2, 2];
                    break;
                case 1:
                    keys[0, 2, 2] = keys[0, 1, 2];
                    break;
                case 2:
                    keys[0, 2, 2] = keys[0, 2, 1];
                    break;
            }

            switch (MainGame.mainGame.random.Next(3))
            {
                case 0:
                    keys[2, 2, 2] = keys[1, 2, 2];
                    break;
                case 1:
                    keys[2, 2, 2] = keys[2, 1, 2];
                    break;
                case 2:
                    keys[2, 2, 2] = keys[2, 2, 1];
                    break;
            }

            #endregion corners

            for (int x = 0; x < Sizes.VoxelsPerEdge; x++)
            {
                for (int y = 0; y < Sizes.VoxelsPerEdge; y++)
                {
                    for (int z = 0; z < Sizes.VoxelsPerEdge; z++)
                    {
                        List<Material> nearbyKeys = new List<Material>();

                        

                        //I'm pretty sure there SHOULD be a better way than this.
                        //...but I don't know it.
                        
                        //well, I can at least do some if unrolling
                        //yes that's totally what you call it

                        //...not sure it actually is better but w/e
                        #region assignNearby
                        int xBase = x / middle;
                        int yBase = y / middle;
                        int zBase = z / middle;
                        
                        bool xBetween = x % middle != 0;
                        bool yBetween = y % middle != 0;
                        bool zBetween = z % middle != 0;

                        nearbyKeys.Add(keys[xBase, yBase, zBase]);
                        if (xBetween)
                        {
                            nearbyKeys.Add(keys[xBase + 1, yBase, zBase]);
                            if (yBetween)
                            {
                                nearbyKeys.Add(keys[xBase + 1, yBase + 1, zBase]);
                                if (zBetween)
                                {
                                    nearbyKeys.Add(keys[xBase + 1, yBase + 1, zBase + 1]);
                                }
                            }
                            if (zBetween)
                            {
                                nearbyKeys.Add(keys[xBase + 1, yBase, zBase + 1]);
                            }
                        }
                        if (yBetween)
                        {
                            nearbyKeys.Add(keys[xBase, yBase + 1, zBase]);
                            if (zBetween)
                            {
                                nearbyKeys.Add(keys[xBase, yBase + 1, zBase + 1]);
                            }
                        }
                        if (zBetween)
                        {
                            nearbyKeys.Add(keys[xBase, yBase, zBase + 1]);
                        }

                        #endregion assignNearby

                        IEnumerable<IGrouping<Material, Material>> groups = nearbyKeys.GroupBy<Material, Material>(i => i);

                        bool assigned = false;
                        List<Tuple<int, Material>> randomizerList = new List<Tuple<int, Material>>();
                        int randomizerTotal = 0;

                        foreach (IGrouping<Material, Material> group in groups)
                        {
                            if (group.Count() > nearbyKeys.Count / 2)
                            {
                                assigned = true;
                                this[x, y, z] = group.ElementAt(0);
                            }
                            else
                            {
                                if (group.Count() > 1)
                                {
                                    randomizerList.Add(new Tuple<int, Material>(group.Count() - 1, group.ElementAt(0)));
                                    randomizerTotal += group.Count() - 1;
                                }
                            }
                        }
                        if (!assigned)
                        {
                            if (randomizerList.Count == 0)
                            {
                                this[x, y, z] = primary;
                            }
                            else
                            {
                                int random = MainGame.mainGame.random.Next(randomizerTotal);
                                foreach (Tuple<int, Material> pair in randomizerList)
                                {
                                    if (random >= pair.Item1)
                                    {
                                        random -= pair.Item1;
                                    }
                                    else
                                    {
                                        this[x, y, z] = pair.Item2;
                                        break;
                                    }
                                }
                            }
                        }

                    }
                }
            }
            //*/

            opaqUpdated = false;
        }

        private void wearDownRecursive(IntVector3 trying, IntVector3 root)
        {
            //not sure if I should have some checker for multiple tests
            //I think I don't need it because it only comes up again if a neighbor has changed
            if (this[trying] != primary)
                return;

            Material other = null;
            int otherFaces = 0;

            Material m;
            #region north

            if (trying.X + 1 < Sizes.VoxelsPerEdge)
            {
                m = this[trying.X + 1, trying.Y, trying.Z];
            }
            else
            {
                if (neighborN != null)
                {
                    m = neighborN[0, trying.Y, trying.Z];
                }
                else
                    m = primary;
            }
            if (m != primary)
            {
                other = m;
                otherFaces++;
            }

            #endregion north

            #region south

            if (trying.X - 1 >= 0)
            {
                m = this[trying.X - 1, trying.Y, trying.Z];
            }
            else
            {
                if (neighborS != null)
                {
                    m = neighborS[Sizes.VoxelsPerEdge - 1, trying.Y, trying.Z];
                }
                else
                    m = primary;
            }
            if (m != primary)
            {
                if (other != null)
                {
                    if (other != m)
                        Logger.Log("Unhandled case: three materials in WearDown.");
                }
                else
                    other = m;
                otherFaces++;
            }

            #endregion south

            #region up

            if (trying.Y + 1 < Sizes.VoxelsPerEdge)
            {
                m = this[trying.X, trying.Y + 1, trying.Z];
            }
            else
            {
                if (neighborU != null)
                {
                    m = neighborU[trying.X, 0, trying.Z];
                }
                else
                    m = primary;
            }
            if (m != primary)
            {
                if (other != null)
                {
                    if (other != m)
                        Logger.Log("Unhandled case: three materials in WearDown.");
                }
                else
                    other = m;
                otherFaces++;
            }

            #endregion up

            #region down

            if (trying.Y - 1 >= 0)
            {
                m = this[trying.X, trying.Y - 1, trying.Z];
            }
            else
            {
                if (neighborD != null)
                {
                    m = neighborD[trying.X, Sizes.VoxelsPerEdge - 1, trying.Z];
                }
                else
                    m = primary;
            }
            if (m != primary)
            {
                if (other != null)
                {
                    if (other != m)
                        Logger.Log("Unhandled case: three materials in WearDown.");
                }
                else
                    other = m;
                otherFaces++;
            }

            #endregion down

            #region east

            if (trying.Z + 1 < Sizes.VoxelsPerEdge)
            {
                m = this[trying.X, trying.Y, trying.Z + 1];
            }
            else
            {
                if (neighborE != null)
                {
                    m = neighborE[trying.X, trying.Y, 0];
                }
                else
                    m = primary;
            }
            if (m != primary)
            {
                if (other != null)
                {
                    if (other != m)
                        Logger.Log("Unhandled case: three materials in WearDown.");
                }
                else
                    other = m;
                otherFaces++;
            }

            #endregion east

            #region west

            if (trying.Z - 1 >= 0)
            {
                m = this[trying.X, trying.Y, trying.Z - 1];
            }
            else
            {
                if (neighborW != null)
                {
                    m = neighborW[trying.X, trying.Y, Sizes.VoxelsPerEdge - 1];
                }
                else
                    m = primary;
            }
            if (m != primary)
            {
                if (other != null)
                {
                    if (other != m)
                        Logger.Log("Unhandled case: three materials in WearDown.");
                }
                else
                    other = m;
                otherFaces++;
            }

            #endregion west


            const int borderValue = 3;
            bool switching = false;

            if (otherFaces > borderValue)
                switching = true;
            else if (otherFaces == borderValue)
            {

                const int maxDistance = 7; //yay magic numbers
                int distance = Math.Abs(trying.X - root.X) * Math.Abs(trying.Y - root.Y) * Math.Abs(trying.Z - root.Z);

                if (GameShell.gameShell.random.Next(maxDistance) > distance)
                    switching = true;
            }

            if (switching)
            {
                this[trying.X, trying.Y, trying.Z] = other;
                if (trying.X + 1 < Sizes.VoxelsPerEdge)
                    wearDownRecursive(new IntVector3(trying.X + 1, trying.Y, trying.Z), root);
                if (trying.X - 1 >= 0)
                    wearDownRecursive(new IntVector3(trying.X - 1, trying.Y, trying.Z), root);
                if (trying.Y + 1 < Sizes.VoxelsPerEdge)
                    wearDownRecursive(new IntVector3(trying.X, trying.Y + 1, trying.Z), root);
                if (trying.Y - 1 >= 0)
                    wearDownRecursive(new IntVector3(trying.X, trying.Y - 1, trying.Z), root);
                if (trying.Z + 1 < Sizes.VoxelsPerEdge)
                    wearDownRecursive(new IntVector3(trying.X, trying.Y, trying.Z + 1), root);
                if (trying.Z - 1 >= 0)
                    wearDownRecursive(new IntVector3(trying.X, trying.Y, trying.Z - 1), root);
            }
        }

        private void updateOpacity()
        {
            //TODO: this
            bool isPrimaryOpaque = primary.transparency == Transparency.opaque;

            int highEdge = Sizes.VoxelsPerEdge - 1;
            
            #region north
            opaqN = true;

            for (int i = 0; i < Sizes.VoxelsPerEdge; i++)
            {
                for (int j = 0; j < Sizes.VoxelsPerEdge; j++)
                {
                    if (exceptions[highEdge, i, j] == null)
                    {
                        if (!isPrimaryOpaque)
                            opaqN = false;
                    }
                    else
                    {
                        if (exceptions[highEdge, i, j].transparency != Transparency.opaque)
                            opaqN = false;
                    }
                }
            }
            #endregion north

            #region south
            opaqS = true;

            for (int i = 0; i < Sizes.VoxelsPerEdge; i++)
            {
                for (int j = 0; j < Sizes.VoxelsPerEdge; j++)
                {
                    if (exceptions[0, i, j] == null)
                    {
                        if (!isPrimaryOpaque)
                            opaqS = false;
                    }
                    else
                    {
                        if (exceptions[0, i, j].transparency != Transparency.opaque)
                            opaqS = false;
                    }
                }
            }
            #endregion south

            #region up
            opaqU = true;

            for (int i = 0; i < Sizes.VoxelsPerEdge; i++)
            {
                for (int j = 0; j < Sizes.VoxelsPerEdge; j++)
                {
                    if (exceptions[i, highEdge, j] == null)
                    {
                        if (!isPrimaryOpaque)
                            opaqU = false;
                    }
                    else
                    {
                        if (exceptions[i, highEdge, j].transparency != Transparency.opaque)
                            opaqU = false;
                    }
                }
            }
            #endregion up

            #region down
            opaqD = true;

            for (int i = 0; i < Sizes.VoxelsPerEdge; i++)
            {
                for (int j = 0; j < Sizes.VoxelsPerEdge; j++)
                {
                    if (exceptions[i, 0, j] == null)
                    {
                        if (!isPrimaryOpaque)
                            opaqD = false;
                    }
                    else
                    {
                        if (exceptions[i, 0, j].transparency != Transparency.opaque)
                            opaqD = false;
                    }
                }
            }
            #endregion down

            #region east
            opaqE = true;

            for (int i = 0; i < Sizes.VoxelsPerEdge; i++)
            {
                for (int j = 0; j < Sizes.VoxelsPerEdge; j++)
                {
                    if (exceptions[i, j, highEdge] == null)
                    {
                        if (!isPrimaryOpaque)
                            opaqE = false;
                    }
                    else
                    {
                        if (exceptions[i, j, highEdge].transparency != Transparency.opaque)
                            opaqE = false;
                    }
                }
            }
            #endregion east

            #region west
            opaqW = true;

            for (int i = 0; i < Sizes.VoxelsPerEdge; i++)
            {
                for (int j = 0; j < Sizes.VoxelsPerEdge; j++)
                {
                    if (exceptions[i, j, 0] == null)
                    {
                        if (!isPrimaryOpaque)
                            opaqW = false;
                    }
                    else
                    {
                        if (exceptions[i, j, 0].transparency != Transparency.opaque)
                            opaqW = false;
                    }
                }
            }
            #endregion west

            isTransparent = false;

            opaqUpdated = true;
        }
    }
}
