using System;
using Unity.Mathematics;

namespace Voronoi
{
    public struct Cell
    {
        public float3 Center { get; private set; }

        public Segment[] Segments { get; private set; }

        public Cell(float3 center, Segment[] segments)
        {
            Center = center;
            
            Segments = segments;
        }
    }
}
