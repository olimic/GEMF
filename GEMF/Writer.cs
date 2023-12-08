using System;
using System.Collections.Generic;
using System.IO;

namespace GEMF
{
    #region COPYING
    /* 
    Copyright (c) 2015, Oliver Michel
    All rights reserved.

    Redistribution and use in source and binary forms, with or without
    modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this
      list of conditions and the following disclaimer.

    * Redistributions in binary form must reproduce the above copyright notice,
      this list of conditions and the following disclaimer in the documentation
      and/or other materials provided with the distribution.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
    AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
    DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
    FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
    SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
    CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
    OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
    OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
     */

    #endregion

    public class Writer
    {
        public string OutputFile { get; set; }
        public string InputPath { get; set; }
        public uint TilePixelSize { get; set; }
        public int MaxZoom { get; set; }
        public string Name { get; set; }

        protected Dictionary<uint, Range> Ranges;
        protected List<Dictionary<string, long>> ListOfFilesWithFileSize = null;

        private ulong FileSeek = 0;
        

        public Writer()
        {
            OutputFile = string.Empty;
            InputPath = string.Empty;
            TilePixelSize = 256;
            MaxZoom = 20;
            Name = "map";
            Ranges = new Dictionary<uint, Range>();
            ListOfFilesWithFileSize = new List<Dictionary<string,long>>();
        }

        public void Create()
        {
            try
            {
                if (Directory.Exists(InputPath))
                {
                    char[] spl1 = new char[]{'.'};
                    char[] spl2 = new char[] { '\\' };

                    // collect ranges
                    string[] zoom_dirs = Directory.GetDirectories(InputPath);
                    foreach (string zoom_dir in zoom_dirs)
                    {
                        uint zoom = 0;
                        if (uint.TryParse(zoom_dir.Substring(zoom_dir.LastIndexOf("\\")+1), out zoom))
                        {
                            if (zoom <= MaxZoom)
                            {
                                Range rg = new Range();
                                rg.Zoom = zoom;
                                rg.Offset = 0;
                                Dictionary<string,long> files_sizes = new Dictionary<string,long>();
                                ListOfFilesWithFileSize.Add(files_sizes);

                                string[] x_dirs = Directory.GetDirectories(zoom_dir);
                                foreach (string x_dir in x_dirs)
                                {
                                    uint x = 0;
                                    if (uint.TryParse(x_dir.Substring(x_dir.LastIndexOf('\\')+ 1), out x))
                                    {
                                        string[] y_files = Directory.GetFiles(x_dir);
                                        foreach (string y_file in y_files)
                                        {
                                            string[] y_spl = y_file.Substring(y_file.LastIndexOf('\\')+ 1).Split(spl1);

                                            uint y = 0;
                                            if (uint.TryParse(y_spl[0], out y))
                                            {
                                                rg.Expand(x, y);
                                                FileInfo fi = new FileInfo(y_file);
                                                files_sizes.Add(y_file, fi.Length);
                                                rg.Offset += (ulong)fi.Length;
                                            }
                                        }
                                    }
                                }
                                Ranges.Add(zoom, rg);
                            }
                        }
                    }

                    if (Ranges.Count == 0)
                    {
                        throw new Exception("nothing to do. wrong directory structure. must be zoom\\x\\y_file");
                    }

                    FileStream out_file = File.Create(OutputFile);
                    if (out_file == null)
                    {
                        throw new Exception("error creation output file.");
                    }

                    // write header
                    WriteUInt32(out_file, 4);                   // version
                    WriteUInt32(out_file, TilePixelSize);       
                    WriteUInt32(out_file, 1);                   // num sources
                    WriteUInt32(out_file, 0);                   // first and only source
                    WriteUInt32(out_file, (uint)Name.Length);   
                    out_file.Write(System.Text.ASCIIEncoding.ASCII.GetBytes(Name.ToCharArray()), 0, Name.Length);
                    FileSeek += (ulong)Name.Length;             // just for debuging 

                    WriteUInt32(out_file, (uint)Ranges.Count);

                    ulong offset = 5 * 4 + (ulong)Name.Length + 4 + (ulong)Ranges.Count * (4 * 6 + 8);
                    foreach(KeyValuePair<uint, Range> kv in Ranges)
                    {
                        Range rg = kv.Value;
                        WriteUInt32(out_file, rg.Zoom);
                        WriteUInt32(out_file, rg.XMin);
                        WriteUInt32(out_file, rg.XMax);
                        WriteUInt32(out_file, rg.YMin);
                        WriteUInt32(out_file, rg.YMax);
                        WriteUInt32(out_file, 0);
                        WriteUInt64(out_file, offset);
                        offset += (rg.XMax - rg.XMin + 1) * (rg.YMax - rg.YMin + 1) * 12;
                        rg.Offset = offset;
                    }

                    // write address + offset Index
                    for(int i = 0; i < ListOfFilesWithFileSize.Count; i++)
                    {
                        Dictionary<string,long> files_sizes = ListOfFilesWithFileSize[i];
                        foreach(KeyValuePair<string, long> kv in files_sizes)
                        {
                            WriteUInt64(out_file, offset);
                            WriteUInt32(out_file, (uint)kv.Value);
                            offset += (ulong)kv.Value;
                        }
                    }                    

                    // write content
                    byte[] image = new byte[100000];
                    for(int i = 0; i < ListOfFilesWithFileSize.Count; i++)
                    {
                        Dictionary<string,long> files_sizes = ListOfFilesWithFileSize[i];
                        foreach(KeyValuePair<string, long> kv in files_sizes)
                        {
                            if (image.Length < kv.Value)
                                image = new byte[kv.Value + 50000];

                            FileStream fs = File.OpenRead(kv.Key);
                            fs.Read(image, 0, (int)kv.Value);
                            out_file.Write(image, 0, (int)kv.Value);
                        }
                    }

                    out_file.Flush();
                    out_file.Close();
                }
                else
                    throw new Exception("input-path does not exist.");
            }
            catch(Exception ex)
            {
                throw new Exception("exception: " + ex.Message);
            }
        }

        protected void WriteUInt32(Stream st, uint value)
        {
            byte[] buf = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buf);
            st.Write(buf, 0, 4);
            FileSeek += 4;
        }

        protected void WriteUInt64(Stream st, ulong value)
        {
            byte[] buf = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buf);
           st.Write(buf, 0, 8);
           FileSeek += 8;
        }
    }
}
