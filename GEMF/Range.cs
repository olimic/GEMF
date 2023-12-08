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
        public bool IsInitialized = false;

        public bool IsInRange(uint zoom, uint x, uint y)
        {
            if (Zoom != zoom)
                return false;
            if (XMin <= x && XMax >= x &&
                YMin <= y && YMax >= y)
                return true;
            return false;
        }

        public void Expand(uint x, uint y)
        {
            if (!IsInitialized)
            {
                XMin = XMax = x;
                YMin = YMax = y;
                IsInitialized = true;
            }
            else
            {
                if (x < XMin)
                    XMin = x;
                else if (x > XMax)
                    XMax = x;

                if (y < YMin)
                    YMin = y;
                else if (y > YMax)
                    YMax = y;
            }
        }
    }
}
