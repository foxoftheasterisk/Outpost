using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

/*
namespace Outpost
{
    class SetLoader
    {

        public static void loadSet(string filename)
        {
            MainGame.mainGame.setOfEverything = new Set();
            loadSet(filename, MainGame.mainGame.setOfEverything);
        }

        private static void loadSet(string filename, Set currentSet)
        {
            Outpost.Screens.LoadingScreen.Display("Loading sets from " + filename);

            Mode mode = Mode.none;
            StreamReader input = new StreamReader(filename);

            while(input.Peek() >= 0)
            {
                string line = input.ReadLine();

                if (line.Length == 0)
                    continue;

                if (line[0] == '>')
                {
                    if (line.Contains("sets"))
                    {
                        mode = Mode.sets;
                    }
                    else if (line.Contains("files"))
                    {
                        mode = Mode.files;
                    }
                    else if (line.Contains("colors"))
                    {
                        mode = Mode.colors;
                    }
                    else if (line.Contains("voxtures"))
                    {
                        mode = Mode.voxtures;
                    }
                    else if (line.Contains("materials"))
                    {
                        mode = Mode.materials;
                    }
                }
                else
                {
                    switch(mode)
                    {
                        case Mode.sets:
                            currentSet = currentSet.enterSet(line);
                            break;
                        case Mode.files:
                            loadSet(OutpostLibrary.Misc.GAME_DIR + line, currentSet);
                            break;
                        default:
                            line = line.Remove(line.IndexOf(':'));
                            switch (mode)
                            {
                                case Mode.colors:
                                    currentSet.addItem(loadColor(line, input));
                                    break;
                                case Mode.voxtures:
                                    currentSet.addItem(loadVoxture(line, currentSet, input));
                                    break;
                                case Mode.materials:
                                    currentSet.addItem(loadMaterial(line, currentSet, input));
                                    break;
                                default:
                                    MainGame.mainGame.Log("Modeless input: " + line);
                                    break;
                            }
                            break;
                    }
                }
            }
        }


/*
        private static OutpostLibrary.Content.Voxture loadVoxture(string name, Set set, StreamReader input)
        {
            OutpostLibrary.Content.Voxture vox = new OutpostLibrary.Content.Voxture(name);
            string line = "";
            bool overflow = false;
            for(int i = 0; i < OutpostLibrary.Navigation.Sizes.VoxelsPerEdge; i++)
            {
                if (!overflow)
                    line = input.ReadLine();
                if (!line.StartsWith("["))
                {
                    MainGame.mainGame.Log("Malformed voxture input: no opening bracket ([) at line reading " + line + ".\n  Attempting to compensate...");
                    overflow = true;
                }
                for(int j = 0; j < OutpostLibrary.Navigation.Sizes.VoxelsPerEdge; j++)
                {
                    if (!overflow)
                        line = input.ReadLine();
                    string[] colors = line.Split(' ');
                    if (colors.Length < OutpostLibrary.Navigation.Sizes.VoxelsPerEdge)
                    {
                        MainGame.mainGame.Log("Malformed voxture input: Line reading " + line + " does not contain sufficient inputs ("
                            + OutpostLibrary.Navigation.Sizes.VoxelsPerEdge + " required)!\n Attempting to compensate... (This may result in problems later!)");
                        j--;
                        continue;
                    }
                    int itor = 0;
                    for(int k = 0; k < OutpostLibrary.Navigation.Sizes.VoxelsPerEdge; k++)
                    {
                        if (colors[itor].StartsWith("["))
                        {
                            //handle this later
                        }
                        else
                        {
                            vox[i, j, k] = set.getItem(colors[itor]) as OutpostLibrary.Content.OutpostColor;
                            itor++;
                        }
                    }

                }
                if (!overflow)
                    line = input.ReadLine();
                if (!line.StartsWith("]"))
                {
                    MainGame.mainGame.Log("Malformed voxture input: no closing bracket (]) at line reading " + line + ".\n  Attempting to compensate...");
                    overflow = true;
                }
            }

            return vox;
        }

        private static OutpostLibrary.Content.Material loadMaterial(string name, Set set, StreamReader input)
        {
            string line = input.ReadLine();
            if (!line.StartsWith("["))
                MainGame.mainGame.Log("Malformed material definition: Line reading " + line + " does not start with opening bracket ([).\nAttempting to compensate...");
            else
                line = input.ReadLine();

            OutpostLibrary.Content.Material mat = new OutpostLibrary.Content.Material(name);

            while(!line.StartsWith("]"))
            {
                line = line.Remove(line.IndexOf(':'));
                line = line.ToLower();
                switch(line)
                {
                    case "vox":
                        line = input.ReadLine();
                        mat.voxture = set.getItem(line) as OutpostLibrary.Content.Voxture;
                        break;
                    case "trans":
                        line = input.ReadLine();
                        switch(line)
                        {
                            case "opaque":
                                mat.transparency = OutpostLibrary.Content.Transparency.opaque;
                                break;
                            case "transparent":
                                mat.transparency = OutpostLibrary.Content.Transparency.transparent;
                                break;
                            case "translucent":
                                mat.transparency = OutpostLibrary.Content.Transparency.translucent;
                                break;
                            default:
                                MainGame.mainGame.Log(("Did not recognize transparency: " + line));
                                break;
                        }
                        break;
                    case "solid":
                        line = input.ReadLine();
                        switch(line)
                        {
                            case "solid":
                                mat.solidity = OutpostLibrary.Content.Solidity.solid;
                                break;
                            case "liquid":
                                mat.solidity = OutpostLibrary.Content.Solidity.liquid;
                                break;
                            case "gas":
                                mat.solidity = OutpostLibrary.Content.Solidity.gas;
                                break;
                            case "plasma":
                                mat.solidity = OutpostLibrary.Content.Solidity.plasma;
                                break;
                            case "vacuum":
                                mat.solidity = OutpostLibrary.Content.Solidity.vacuum;
                                break;
                            default:
                                MainGame.mainGame.Log(("Did not recognize solidity: " + line));
                                break;
                        }
                        break;
                    default:
                        //something something lua table
                        break;
                }

                line = input.ReadLine();
            }

            //TEMP
            OutpostLibrary.Content.Material.materials.Add(name, mat);

            return mat;
        }
    }
}

//*/