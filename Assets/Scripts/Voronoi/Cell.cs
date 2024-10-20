using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voronoi
{
    public struct Cell
    {
        public Vector2 Center { get; private set; }

        public Segment[] Segments { get; private set; }

        public Cell(Vector2 center)
        {
            Center = center;

            Segments = Array.Empty<Segment>();
        }

        public Cell(Vector2 center, Segment[] segments) : this(center)
        {
            Segments = segments;
        }

        public bool GetIntersections(Segment bisector, out HashSet<Intersection> intersections)
        {
            intersections = new HashSet<Intersection>();

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
            Vector2 a = bisector.Start;
            Vector2 b = bisector.End;

            // Segment CD
            Vector2 c = segment.Start;
            Vector2 d = segment.End;

            float t = ((a.x - c.x) * (c.y - d.y) - (a.y - c.y) * (c.x - d.x)) /
                      ((a.x - b.x) * (c.y - d.y) - (a.y - b.y) * (c.x - d.x));

            float u = -((a.x - b.x) * (a.y - c.y) - (a.y - b.y) * (a.x - c.x)) /
                      ((a.x - b.x) * (c.y - d.y) - (a.y - b.y) * (c.x - d.x));

            // Both t & u are between 0 and 1
            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                Vector2 point;

                point.x = a.x + t * (b.x - a.x);

                point.y = a.y + t * (b.y - a.y);

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
