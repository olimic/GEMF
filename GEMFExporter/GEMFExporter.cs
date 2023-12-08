using System;
using System.IO;
using GEMF;

namespace GEMFExporter
{
    public class GEMFExporter
    {
        public void Export(string filepath, string outputpath, string fileextension)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open);
            Reader rd = new Reader();

            rd.ReadStream = fs;
            rd.ReadHeader(fs);
            Console.WriteLine(string.Format("Provider: {0}", rd.Name));
            Console.WriteLine(string.Format("Ranges: {0}", rd.Ranges.Count));

            foreach (Range r in rd.Ranges)
            {
                Console.WriteLine(string.Format("export Range Zoom={0}, XMin={1}, XMax={2}, YMin={3}, YMax={4}", 
                    r.Zoom, r.XMin, r.XMax, r.YMin, r.YMax));

                for(uint x = r.XMin; x <= r.XMax; x++)
                {
                    string outpath = string.Format("{0}\\{1}\\{2}", outputpath, r.Zoom, x);
                    if (!Directory.Exists(outpath))
                        Directory.CreateDirectory(outpath);

                    for (uint y = r.YMin; y <= r.YMax; y++)
                    {
                        if (rd.GetMapTile(r.Zoom, x, y))
                        {
                            string outfile = string.Format("{0}\\{1}.{2}", outpath, y, fileextension);
                            Console.WriteLine(string.Format("write {0}", outfile));

                            FileStream fo = new FileStream(outfile, FileMode.CreateNew);
                            fo.Write(rd.ImageByteArray, 0, (int)rd.ImageLength);
                            fo.Close();
                        }
                    }
                }
            }
        }
    }
}
