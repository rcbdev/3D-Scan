using System.Collections.Generic;

namespace Scanner3D.Library
{
    public class Slice
    {
        public double Angle { get; set; }
        public List<DepthPoint> Depths { get; set; }
    }
}