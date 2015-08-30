using System;

namespace GEMF
{
    public class Range
    {
        public UInt32 Zoom;
        public UInt32 XMin;
        public UInt32 XMax;
        public UInt32 YMin;
        public UInt32 YMax;
        public UInt32 SourceIndex;
        public UInt64 Offset;

        public bool IsInRange(uint zoom, uint x, uint y)
        {
            if (Zoom != zoom)
                return false;
            if (XMin <= x && XMax >= x &&
                YMin <= y && YMax >= y)
                return true;
            return false;
        }
    }
}
