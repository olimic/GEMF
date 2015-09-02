using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace GEMF
{
    // GEMF-format description: http://www.cgtk.co.uk/gemf

    public class Reader
    {
        public string Name { get; set; }
        public List<Range> Ranges { get; set; }
        public List<List<Range>> RangesByZoomIndex { get; set; }
        public Stream ReadStream { get; set; }
        public string FileFormat { get; set; }

        public byte[] ImageByteArray { get; set; }
        public long ImageLength { get; set; }

        protected byte[] mReadBuffer = new byte[1024];

        private byte[] mBuf32 = new byte[4];
        private byte[] mBuf64 = new byte[8];

        private long ByteBufferLength = 100000;

        public Reader()
        {
            Name = string.Empty;
            Ranges = new List<Range>();
            RangesByZoomIndex= null;
            ReadStream = null;
            ImageByteArray = new byte[ByteBufferLength];
            ImageLength = 0;
            FileFormat = string.Empty;
        }

        public void ReadHeader(Stream in_stream)
        {
            try
            {
                // global header
                UInt32 ver = ReadUInt32(in_stream);
                if (ver != 4)
                    throw new Exception("GEMF.ReadHeader: wrong version of file");

                UInt32 tile_size = ReadUInt32(in_stream);
                //if (tile_size != 256)
                //    throw new Exception("GEMF.ReadHeader: only tile size 256 is supported");

                // Sources
                UInt32 num_sources = ReadUInt32(in_stream);
                if (num_sources != 1)
                    throw new Exception("GEMF.ReadHeader: only 1 source is supported");

                for (int i = 0; i < num_sources; i++)
                {
                    UInt32 source = ReadUInt32(in_stream);
                    if (source != i)
                        throw new Exception("GEMF.ReadHeader: wrong source index");

                    UInt32 num_bytes = ReadUInt32(in_stream);
                    in_stream.Read(mReadBuffer, 0, (int)num_bytes);
                    mReadBuffer[num_bytes] = 0;
                    Name = System.Text.ASCIIEncoding.ASCII.GetString(mReadBuffer, 0, (int)num_bytes);
                }

                // Ranges
                UInt32 num_ranges = ReadUInt32(in_stream);

                Debug.WriteLine("GEMF.ReadHeader: Provider:{0}, Ranges={1}", Name, num_ranges);

                for (int i = 0; i < num_ranges; i++)
                {
                    Range r = new Range();
                    r.Zoom = ReadUInt32(in_stream);
                    r.XMin = ReadUInt32(in_stream);
                    r.XMax = ReadUInt32(in_stream);
                    r.YMin = ReadUInt32(in_stream);
                    r.YMax = ReadUInt32(in_stream);
                    r.SourceIndex = ReadUInt32(in_stream);
                    r.Offset = ReadUInt64(in_stream);

                    Ranges.Add(r);

                    Debug.WriteLine("GEMF.ReadHeader: Zoom={0}, XMin={1}, XMax={2}, YMin={3}, YMax={4}, SourceIndex={5}, Offset={6}",
                        r.Zoom, r.XMin, r.XMax, r.YMin, r.YMax, r.SourceIndex, r.Offset);
                }

                RangesByZoomIndex = new List<List<Range>>();
                for (int i = 0; i <= 20; i++)
                {
                    List<Range> rz = new List<Range>();
                    foreach (Range r in Ranges)
                    {
                        if (r.Zoom == i)
                            rz.Add(r);
                    }
                    RangesByZoomIndex.Add(rz);                    
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GEMF.ReadHeader: " + ex.Message);
            }
        }

        public bool GetMapTile(uint zoom, uint x, uint y)
        {
            if (zoom > 20 || ReadStream == null || ImageByteArray == null)
                return false;

            List<Range> rz = RangesByZoomIndex[(int)zoom];
            foreach (Range r in rz)
            {
                if (r.IsInRange(zoom, x, y))
                {
                    // Load Image in ImageByteArray
                    UInt32 num_y = r.YMax + 1 - r.YMin;
                    x = x - r.XMin;
                    y = y - r.YMin;
                    UInt64 offset = (UInt64)((x * num_y) + y);
                    offset *= 12;
                    offset += r.Offset;

                    ReadStream.Seek((long)offset, SeekOrigin.Begin);

                    UInt64 address = ReadUInt64(ReadStream);
                    ImageLength = ReadUInt32(ReadStream);

                    if (address == 0 || ImageLength == 0)
                        return false;

                    ReadStream.Seek((long)address, SeekOrigin.Begin);

                    if (ImageByteArray.Length < ImageLength)
                        ImageByteArray = new byte[ImageLength + ByteBufferLength];

                    ReadStream.Read(ImageByteArray, 0, (int)ImageLength);

                    if (FileFormat.Length == 0 &&   // auto detect file format if empty
                        ImageLength > 1)     
                    {   
                        if (ImageByteArray[0] == 0xFF && ImageByteArray[1] == 0xD8)
                            FileFormat = "JPG";
                        if (ImageByteArray[0] == 0x89 && ImageByteArray[1] == 0x50)
                            FileFormat = "PNG";
                    }

                    return true;
                }
            }
            return false;
        }

        public int GetMaxZoom()
        {
            for (int i = RangesByZoomIndex.Count-1; i > 0; i--)
            {
                if (RangesByZoomIndex[i].Count > 0) return i;
            }
            return 0;
        }

        public int GetMinZoom()
        {
            for (int i = 0; i <= RangesByZoomIndex.Count-1; i++)
            {
                if (RangesByZoomIndex[i].Count > 0) return i;
            }
            return 0;
        }

        protected UInt32 ReadUInt32(Stream st, int offset = 0)
        {
            st.Read(mBuf32, offset, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(mBuf32);
            return BitConverter.ToUInt32(mBuf32, 0); 
        }

        protected UInt64 ReadUInt64(Stream st, int offset = 0)
        {
            st.Read(mBuf64, offset, 8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(mBuf64);
            return BitConverter.ToUInt64(mBuf64, 0);
        }
    }
}
