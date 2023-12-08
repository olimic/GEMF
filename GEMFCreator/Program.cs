using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEMFCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("GEMFCreator -o output.gemf -i input-path [-m max-zoom] [-s tile-size] [-n name]");
            }
            else
            {
                try
                {
                    GEMFCreator cr = new GEMFCreator();

                    // set options
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i].Equals("-i"))
                            cr.Writer.InputPath = args[++i];

                        else if (args[i].Equals("-o"))
                            cr.Writer.OutputFile = args[++i];

                        else if (args[i].Equals("-m"))
                            cr.Writer.MaxZoom = System.Convert.ToInt32(args[++i]);

                        else if (args[i].Equals("-s"))
                            cr.Writer.TilePixelSize = System.Convert.ToUInt32(args[++i]);

                        else if (args[i].Equals("-n"))
                            cr.Writer.Name = args[++i];
                    }

                    cr.Create();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
