using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Voronoi
{
    public static class Utils
    {
        public static Cell FromRect(this Cell cell, Rect rect)
        {
            Segment[] segments = new Segment[4];
        
            Segment segment = segments[0] = new Segment(rect.min, new Vector3(rect.xMin, rect.yMax));
        
            segment = segments[1] = new Segment(segment.End, rect.max);
        
            segment = segments[2] = new Segment(segment.End, new Vector3(rect.xMax, rect.yMin));
        
            segments[3] = new Segment(segment.End, rect.min);
        
            return new Cell(cell.Center, segments);
        }
        
        public static Cell FromIntersections(this Cell cell, HashSet<Intersection> intersections)
        {
            BisectorSegment bisector = cell.GetBisectorSegment(intersections.First(), intersections.Last());

            List<Segment> segments = new List<Segment>
            {
                bisector.Segment,
                
                bisector.End.Segment
            };
        
            Segment segment = segments.Last();
            
            while (segment.End != bisector.Start.Point)
            {
                segment = segment.Next(cell.Segments);

                // If last segment
                if (bisector.Start.Segment.Start == segment.Start)
                {
                    segment = bisector.Start.Segment;
                }
            
                segments.Add(segment);
            }

            return new Cell(cell.Center, segments.ToArray());
        }

        public static Segment Next(this Segment segment, Segment[] segments)
        {
            return segments.Single(s => segment.End == s.Start);
        }
    }
}