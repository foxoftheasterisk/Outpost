using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Neo.IronLua;

namespace Outpost
{
    public class LuaBridge
    {
        private Lua compiler;
        public LuaGlobalPortable global; //TODO: make this private
        //will need to convert playerFist to Lua first.
        private LuaTable game;
        private LuaCompileOptions lc;

        public const string GAME_DIR_DEFAULT = ".\\Content\\";

        public LuaBridge()
        {
            compiler = new Lua();
            lc = new LuaCompileOptions();
        }

        /// <summary>
        /// Creates a new Lua environment and registers various classes and functions from the game to it.
        /// </summary>
        public void initializeLua()
        {
            global = compiler.CreateEnvironment();
            game = new LuaTable();
            global["Game"] = game;

            global["Log"] = new Action<string>(Logger.Log);

            global["GAME_DIR"] = GAME_DIR_DEFAULT;
            //TODO: make this load from some kind of config file and only fail to this default

            global["runFile"] = new Action<string, LuaTable>(runLuaFile);

            
            LuaTable loadingInterface = new LuaTable();
            loadingInterface["display"] = new Action<string>(Screens.LoadingScreen.Display);
            loadingInterface["change"] = new Action<string>(Screens.LoadingScreen.ChangeMessage);
            game["LoadingScreen"] = loadingInterface;

            game["loadVoxtures"] = new Func<string, LuaTable>(loadVoxtures);

            LuaType.RegisterTypeAlias("material", typeof(OutpostLibrary.Content.Material));
            LuaTable mat = new LuaTable();
            game["Material"] = LuaType.GetType(typeof(OutpostLibrary.Content.Material));
            game["Transparency"] = LuaType.GetType(typeof(OutpostLibrary.Content.Transparency));
            game["Solidity"] = LuaType.GetType(typeof(OutpostLibrary.Content.Solidity));
            //TODO: solidity should be a lua-side thing

            game["Sizes"] = LuaType.GetType(typeof(OutpostLibrary.Navigation.Sizes));
            game["Directions"] = LuaType.GetType(typeof(OutpostLibrary.Navigation.Directions));

            game["map"] = MainGame.mainGame;
            //TODO: this clearly needs reworked
            

            global["twoDArray"] = new Func<int, LuaTable[,]>(delegate(int size) { return new LuaTable[size, size]; });
            global["threeDArray"] = new Func<int, LuaTable[, ,]>(delegate(int size) { return new LuaTable[size, size, size]; });

            LuaType.RegisterTypeAlias("IntVector3", typeof(OutpostLibrary.IntVector3));
            global["IntVector3"] = LuaType.GetType(typeof(OutpostLibrary.IntVector3));

            LuaType.RegisterTypeAlias("Vector3", typeof(Microsoft.Xna.Framework.Vector3));
            global["Vector3"] = LuaType.GetType(typeof(Microsoft.Xna.Framework.Vector3));

            LuaType.RegisterTypeAlias("Chunk", typeof(Chunk));
            global["Chunk"] = LuaType.GetType(typeof(Chunk));

            LuaType.RegisterTypeAlias("SolidBlock", typeof(Blocks.SolidBlock));
            global["SolidBlock"] = LuaType.GetType(typeof(Blocks.SolidBlock));

            LuaType.RegisterTypeAlias("Structure", typeof(MapStructure));
            global["Structure"] = LuaType.GetType(typeof(MapStructure));

            //global["Double"] = LuaType.GetType(typeof(Double));
            
            runLuaFile("init.lua");
        }

        public void runLuaFile(string filename)
        {
            runLuaFile(filename, global);
        }

        public void runLuaFile(string filename, LuaTable environment)
        {
            try
            {
                Logger.Log("Compiling Lua file: " + filename);
                LuaChunk c = compiler.CompileChunk(global["GAME_DIR"] + filename, lc);
                Logger.Log("Running " + filename + "...");
                c.Run(environment);
                Logger.Log(filename + " run successfully!");
                //MainGame.mainGame.env.DoChunk(OutpostLibrary.Misc.GAME_DIR + "vanilla.set");
            }
            catch (LuaParseException e)
            {
                Logger.Log("Lua compile error while compiling " + filename + "!");
                Logger.Log(e.Message);
                Logger.Log("At " + e.FileName + " line " + e.Line);
            }
            catch (LuaRuntimeException e)
            {
                Logger.Log("Lua error while running " + filename + "!");
                Logger.Log(e.Message);
                Logger.Log("In " + e.FileName + " line " + e.Line);
                Logger.Log(e.ToString());
                LuaExceptionData d = LuaExceptionData.GetData(e);
                Logger.Log(d.GetStackTrace(0, false));
            }
        }

        //*/

        //I should maybe move voxture loading to its own class
        //
        #region loadVoxtures

        private enum Mode { none, colors, voxtures }

        private LuaTable loadVoxtures(string filename)
        {
            Outpost.Screens.LoadingScreen.Display("Loading voxtures from " + filename);

            LuaTable table = new LuaTable();
            Dictionary<string, OutpostLibrary.Content.OutpostColor> colors = new Dictionary<string, OutpostLibrary.Content.OutpostColor>();

            Mode mode = Mode.none;
            StreamReader input;
            try
            {
                input = new StreamReader(global["GAME_DIR"] + filename);
            }
            catch (FileNotFoundException e)
            {
                Logger.Log("File " + filename + " does not exist!");
                return new LuaTable();
            }


            while (input.Peek() >= 0)
            {
                string line = input.ReadLine();

                if (line.Length == 0)
                    continue;

                if (line[0] == '>')
                {
                    if (line.Contains("colors"))
                    {
                        mode = Mode.colors;
                    }
                    else if (line.Contains("voxtures"))
                    {
                        mode = Mode.voxtures;
                    }
                    else
                    {
                        Logger.Log("Non-recognized input mode: " + line);
                    }
                }
                else
                {
                    line = line.Remove(line.IndexOf(':'));
                    switch (mode)
                    {
                        case Mode.colors:
                            colors.Add(line, loadColor(line, input));
                            break;
                        case Mode.voxtures:
                            table[line] = loadVoxture(line, colors, input);
                            break;
                        default:
                            Logger.Log("Non-recognized input: " + line);
                            break;
                    }
                }
            }

            return table;
        }

        //this is not a very safe method
        //actually none of these three are
        //but this one is the worst
        //also they don't handle set traversing
        //which is important
        private static OutpostLibrary.Content.OutpostColor loadColor(string name, StreamReader input)
        {
            string[] values = input.ReadLine().Split(' ');
            int[] ints = new int[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                ints[i] = Int32.Parse(values[i]);
            }
            Microsoft.Xna.Framework.Color surfaceColor = new Microsoft.Xna.Framework.Color(ints[0], ints[1], ints[2], ints[3]);
            values = input.ReadLine().Split(' ');
            ints = new int[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                ints[i] = Int32.Parse(values[i]);
            }
            Microsoft.Xna.Framework.Color specularColor = new Microsoft.Xna.Framework.Color(ints[0], ints[1], ints[2], ints[3]);
            return new OutpostLibrary.Content.OutpostColor(surfaceColor, specularColor);
        }

        private static OutpostLibrary.Content.Voxture loadVoxture(string name, Dictionary<string, OutpostLibrary.Content.OutpostColor> set, StreamReader input)
        {
            OutpostLibrary.Content.Voxture vox = new OutpostLibrary.Content.Voxture();
            string line = "";
            bool overflow = false;
            for (int i = 0; i < OutpostLibrary.Navigation.Sizes.VoxelsPerEdge; i++)
            {
                if (!overflow)
                    line = input.ReadLine();
                if (!line.StartsWith("["))
                {
                    Logger.Log("Malformed voxture input: no opening bracket ([) at line reading " + line + ".\n  Attempting to compensate...");
                    overflow = true;
                }
                for (int j = 0; j < OutpostLibrary.Navigation.Sizes.VoxelsPerEdge; j++)
                {
                    if (!overflow)
                        line = input.ReadLine();
                    string[] colors = line.Split(' ');
                    if (colors.Length < OutpostLibrary.Navigation.Sizes.VoxelsPerEdge)
                    {
                        Logger.Log("Malformed voxture input: Line reading " + line + " does not contain sufficient inputs ("
                            + OutpostLibrary.Navigation.Sizes.VoxelsPerEdge + " required)!\n Attempting to compensate... (This may result in problems later!)");
                        j--;
                        continue;
                    }
                    int itor = 0;
                    for (int k = 0; k < OutpostLibrary.Navigation.Sizes.VoxelsPerEdge; k++)
                    {
                        if (colors[itor].StartsWith("["))
                        {
                            //handle this later
                        }
                        else
                        {
                            vox[i, j, k] = set[colors[itor]];
                            itor++;
                        }
                    }

                }
                if (!overflow)
                    line = input.ReadLine();
                if (!line.StartsWith("]"))
                {
                    Logger.Log("Malformed voxture input: no closing bracket (]) at line reading " + line + ".\n  Attempting to compensate...");
                    overflow = true;
                }
            }

            return vox;
        }

        #endregion loadVoxtures

        #region mapgen

        public void buildChunk(OutpostLibrary.IntVector3 location, Chunk chunk)
        {
            Func<Chunk, OutpostLibrary.IntVector3, LuaResult> doBuild = game["buildChunk"] as Func<Chunk, OutpostLibrary.IntVector3, LuaResult>;

            try
            {
                doBuild(chunk, location);
            }
            catch (LuaRuntimeException e)
            {
                Logger.Log(e.ToString());
                LuaExceptionData d = LuaExceptionData.GetData(e);
                Logger.Log(d.GetStackTrace(0, false));

            }
        }

        #endregion mapgen
    }
}
