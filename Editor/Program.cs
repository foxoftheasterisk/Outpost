using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
//using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
//using Microsoft.Xna.Framework;
using OutpostLibrary;

namespace VoxtureEditor
{
    class Program
    {
        static void Main(string[] args)
        {

            const string GAMEDIR = ".\\";

            StreamWriter set1 = new StreamWriter(GAMEDIR + "basicset.vtx", false);

            NamedColor c1 = new NamedColor("dt1", 136, 101, 66, 255);
            NamedColor c2 = new NamedColor("dt2", 145, 107, 70, 255);
            NamedColor c3 = new NamedColor("dt3", 159, 118, 77, 255);

            NamedColor clear = new NamedColor("clr", 0, 0, 0, 0);

            set1.WriteLine(c1);
            set1.WriteLine(c2);
            set1.WriteLine(c3);
            set1.WriteLine(clear);

            NamedColor[, ,] dirtTex = new NamedColor[,,] { { { c1, c2, c2, c1, c3 },
                                                       { c1, c3, c1, c2, c2 },
                                                       { c2, c3, c1, c1, c3 },
                                                       { c2, c1, c2, c2, c1 },
                                                       { c1, c1, c2, c3, c3 } },
                                                     { { c3, c1, c2, c1, c1 },
                                                       { c1, c2, c3, c2, c1 },
                                                       { c2, c3, c2, c2, c1 },
                                                       { c1, c2, c3, c1, c1 },
                                                       { c3, c2, c1, c2, c2 } },
                                                     { { c2, c3, c1, c2, c2 },
                                                       { c1, c3, c2, c3, c3 },
                                                       { c2, c2, c3, c2, c2 },
                                                       { c2, c1, c2, c2, c1 },
                                                       { c3, c2, c1, c3, c3 } },
                                                     { { c1, c1, c2, c3, c2 },
                                                       { c1, c3, c1, c2, c2 },
                                                       { c3, c1, c2, c1, c1 },
                                                       { c1, c3, c2, c3, c3 },
                                                       { c1, c2, c3, c1, c1 } },
                                                     { { c2, c2, c3, c1, c3 },
                                                       { c3, c1, c2, c2, c3 },
                                                       { c2, c2, c3, c1, c1 },
                                                       { c1, c3, c1, c2, c3 },
                                                       { c2, c1, c3, c2, c2 } } };


            set1.WriteLine("dirt:");
            for (int i = 0; i < 5; i++)
            {
                set1.WriteLine("[ ");
                for (int j = 0; j < 5; j++)
                {
                    set1.Write("[ ");
                    for (int k = 0; k < 5; k++)
                    {
                        set1.Write(dirtTex[i, j, k].name + " ");
                    }
                    set1.WriteLine("]");
                }
                set1.WriteLine("]");
            }

            set1.WriteLine("clear:");
            for (int i = 0; i < 5; i++)
            {
                set1.WriteLine("[ ");
                for (int j = 0; j < 5; j++)
                {
                    set1.Write("[ ");
                    for (int k = 0; k < 5; k++)
                    {
                        set1.Write(clear.name + " ");
                    }
                    set1.WriteLine("]");
                }
                set1.WriteLine("]");
            }

            set1.Flush();
            set1.Close();
        }

        private class NamedColor
        {
            public string name;
            Microsoft.Xna.Framework.Color color;

            public NamedColor(string _name, int r, int g, int b, int a)
            {
                name = _name;
                color = new Microsoft.Xna.Framework.Color(r, g, b, a);
            }

            public override string ToString()
            {
                return name + "= " + color.R + " " + color.G + " " + color.B + " " + color.A;
            }
        }
    }
}
