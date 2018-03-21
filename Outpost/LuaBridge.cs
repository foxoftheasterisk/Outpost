using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Neo.IronLua;

namespace OutpostCore
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
        public void InitializeLua()
        {
            //well, this is messy
            //is there not a better way to do this?

            global = compiler.CreateEnvironment();
            game = new LuaTable();
            global["Game"] = game;

            global["Log"] = new Action<string>(Logger.Log);

            global["GAME_DIR"] = GAME_DIR_DEFAULT;
            //TODO: make this load from some kind of config file and only fail to this default

            global["runFile"] = new Action<string, LuaTable>(runLuaFile);

            
            LuaTable loadingInterface = new LuaTable();
            loadingInterface["display"] = new Action<string>(ScreenManagement.LoadingScreen.Display);
            loadingInterface["change"] = new Action<string>(ScreenManagement.LoadingScreen.ChangeMessage);
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

            game["map"] = GameShell.gameShell;
            //TODO: this clearly needs reworked
            

            global["twoDArray"] = new Func<int, LuaTable[,]>(delegate(int size) { return new LuaTable[size, size]; });
            global["threeDArray"] = new Func<int, LuaTable[, ,]>(delegate(int size) { return new LuaTable[size, size, size]; });

            LuaType.RegisterTypeAlias("IntVector3", typeof(OutpostLibrary.IntVector3));
            global["IntVector3"] = LuaType.GetType(typeof(OutpostLibrary.IntVector3));

            LuaType.RegisterTypeAlias("ChunkAddress", typeof(OutpostLibrary.Navigation.ChunkAddress));
            global["ChunkAddress"] = LuaType.GetType(typeof(OutpostLibrary.Navigation.ChunkAddress));

            LuaType.RegisterTypeAlias("Vector3", typeof(Microsoft.Xna.Framework.Vector3));
            global["Vector3"] = LuaType.GetType(typeof(Microsoft.Xna.Framework.Vector3));

            LuaType.RegisterTypeAlias("Chunk", typeof(Map.Chunk));
            global["Chunk"] = LuaType.GetType(typeof(Map.Chunk));

            LuaType.RegisterTypeAlias("SolidBlock", typeof(Blocks.SolidBlock));
            global["SolidBlock"] = LuaType.GetType(typeof(Blocks.SolidBlock));

            LuaType.RegisterTypeAlias("Structure", typeof(Map.MapStructure));
            global["Structure"] = LuaType.GetType(typeof(Map.MapStructure));

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
            ScreenManagement.LoadingScreen.Display("Loading voxtures from " + filename);

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
            return new OutpostLibrary.Content.OutpostColor(ints[0], ints[1], ints[2], ints[3]);
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

        //uh what
        //...
        //I... guess this makes sense?
        //sort of?

        //i dunno maybe LuaBridge should be distributed?

        public void buildChunk(OutpostLibrary.Navigation.ChunkAddress location, Map.Chunk chunk)
        {
            Func<Map.Chunk, OutpostLibrary.Navigation.ChunkAddress, LuaResult> doBuild = game["buildChunk"] as Func<Map.Chunk, OutpostLibrary.Navigation.ChunkAddress, LuaResult>;

            if(doBuild == null)
            {
                OutpostLibrary.Content.Material errormat = new OutpostLibrary.Content.Material("error");

                //do what???
                for(int x = 0; x < OutpostLibrary.Navigation.Sizes.ChunkSize; x++)
                {
                    for (int y = 0; y < OutpostLibrary.Navigation.Sizes.ChunkSize; y++)
                    {
                        for (int z = 0; z < OutpostLibrary.Navigation.Sizes.ChunkSize; z++)
                        {
                            chunk.fillAssign(x, y, z, new Blocks.SolidBlock(errormat));
                        }
                    }
                }
                return;
            }

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
