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
            if (args.Length < 2)
            {
                Console.WriteLine("GEMFCreator output.gemf input-path [max-zoom]");
            }
            else
            {
                try
                {
                    GEMFCreator cr = new GEMFCreator();
                    cr.Create(args[0], args[1], args.Length > 2 ? Convert.ToInt32(args[2]) : 20);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
