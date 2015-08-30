using System;

namespace GEMFExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("GEMFExporter input.gemf output-path image-extension(png/jpg)");
            }
            else
            {
                try
                {
                    GEMFExporter exp = new GEMFExporter();
                    exp.Export(args[0], args[1], args[2]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
