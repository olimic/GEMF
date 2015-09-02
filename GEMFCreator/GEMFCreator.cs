using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GEMF;

namespace GEMFCreator
{
    public class GEMFCreator
    {
        public Writer Writer { get; set; }        

        public GEMFCreator()
        {
            Writer = new Writer();
        }

        public void Create()
        {
            try
            {
                Writer.Create();
            }
            catch(Exception ex)
            {
                Console.WriteLine("exception: " + ex.Message);
            }
        }
    }
}
