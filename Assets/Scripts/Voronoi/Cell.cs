using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace Voronoi
{
    public struct Cell
    {
        public float3 Center { get; private set; }

        public Segment[] Segments { get; private set; }

        public Cell(float3 center)
        {
            Center = center;

            Segments = Array.Empty<Segment>();
        }

        public Cell(float3 center, Segment[] segments) : this(center)
        {
            Segments = segments;
        }

        public bool GetIntersections(Segment bisector, out NativeHashSet<Intersection> intersections)
        {
            intersections = new NativeHashSet<Intersection>(0, Allocator.Temp);

            foreach (var segment in Segments)
            {
                if (GetIntersection(bisector, segment, out Intersection intersection))
                {
                    intersections.Add(intersection);
                }
            }

            return intersections.Count == 2;
        }

        private bool GetIntersection(Segment bisector, Segment segment, out Intersection intersection)
        {
            intersection = default;

            // Segment AB
            float3 a = bisector.Start;
            float3 b = bisector.End;

            // Segment CD
            float3 c = segment.Start;
            float3 d = segment.End;

            float t = ((a.x - c.x) * (c.y - d.y) - (a.y - c.y) * (c.x - d.x)) /
                      ((a.x - b.x) * (c.y - d.y) - (a.y - b.y) * (c.x - d.x));

            float u = -((a.x - b.x) * (a.y - c.y) - (a.y - b.y) * (a.x - c.x)) /
                      ((a.x - b.x) * (c.y - d.y) - (a.y - b.y) * (c.x - d.x));

            // Both t & u are between 0 and 1
            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                var point = a + t * (b - a);

                intersection = new Intersection(point, segment);

                return true;
            }

            return false;
        }

        // Get the correct bisector segment from two intersections.
        public BisectorSegment GetBisectorSegment(Intersection first, Intersection second)
        {
            BisectorSegment segment = new BisectorSegment(first, second);
            
            return segment.Verify(Center) ? segment : new BisectorSegment(second, first);
        }
    }
}
