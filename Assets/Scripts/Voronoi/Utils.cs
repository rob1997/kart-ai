using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Voronoi
{
    public static class Utils
    {
        public static Cell FromRect(this Cell cell, Rect rect)
        {
            Segment[] segments = new Segment[4];
        
            Segment segment = segments[0] = new Segment(rect.Min, new Vector3(rect.MinX, rect.MaxY));
        
            segment = segments[1] = new Segment(segment.End, rect.Max);
        
            segment = segments[2] = new Segment(segment.End, new Vector3(rect.MaxX, rect.MinY));
        
            segments[3] = new Segment(segment.End, rect.Min);
        
            return new Cell(cell.Center, segments);
        }
        
        public static Cell GetSegmentsFromIntersections(this Cell cell, NativeArray<Intersection> intersections)
        {
            BisectorSegment bisector = cell.GetBisectorSegment(intersections[0], intersections[1]);

            intersections.Dispose();
            
            List<Segment> segments = new List<Segment>
            {
                bisector.Segment,
                
                bisector.End.Segment
            };
        
            Segment segment = segments.Last();
            
            while (!segment.End.Equals(bisector.Start.Point))
            {
                segment = segment.Next(cell.Segments);

                // If last segment
                if (bisector.Start.Segment.Start.Equals(segment.Start))
                {
                    segment = bisector.Start.Segment;
                }
            
                segments.Add(segment);
            }

            return new Cell(cell.Center, segments.ToArray());
        }

        public static Segment Next(this Segment segment, Segment[] segments)
        {
            return segments.Single(s => segment.End.Equals(s.Start));
        }
        
        // math.cross isn't consistent with Vector3.Cross
        public static float3 Cross(float3 lhs, float3 rhs)
        {
            return new Vector3(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
        }
        
        // math.normalize isn't consistent with Vector3.Normalize
        public static float3 Normalize(this float3 value)
        {
            float magnitude = math.length(value);
            
            return magnitude == 0 ? float3.zero : value / magnitude;
        }
    }
}