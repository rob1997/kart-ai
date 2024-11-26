using System;
using Unity.Mathematics;

namespace Voronoi
{
    public struct Cell : IEquatable<Cell>
    {
        public float3 Center { get; private set; }

        public Segment[] Segments { get; private set; }
        
        public float3[] Vertices { get; private set; }

        public Cell(float3 center, Segment[] segments)
        {
            Center = center;
            
            Segments = segments;
            
            Vertices = new float3[segments.Length];
            
            for (int i = 0; i < segments.Length; i++)
            {
                Vertices[i] = segments[i].Start;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Cell other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Center.GetHashCode();
        }

        public bool Equals(Cell other)
        {
            return Center.Equals(other.Center);
        }

        // TODO: this shouldn't happen, it's a bug in Voronoi generation
        // where segments aren't properly calculated (center isn't within segments and segments are unusually smaller)
        public bool Verify()
        {
            float3 previous = Utils.Cross(Segments[0].Direction, Center - Segments[0].Start).Normalize();
            
            for (int i = 1; i < Segments.Length; i++)
            {
                Segment segment = Segments[i];
                
                float3 current = Utils.Cross(segment.Direction, Center - segment.Start).Normalize();
                
                if (!previous.Equals(current))
                {
                    return false;
                }
                
                previous = current;
            }

            return true;
        }
    }
}
