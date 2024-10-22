using UnityEngine;

namespace Voronoi
{
    public struct BisectorSegment
    {
        public Intersection Start { get; private set; }

        public Intersection End { get; private set; }

        public Segment Segment { get; private set; }

        public BisectorSegment(Intersection start, Intersection end)
        {
            Start = Intersection.FromStartIntersection(start);

            End = Intersection.FromEndIntersection(end);

            Segment = new Segment(Start.Point, End.Point);
        }

        // Check if bisector segment is oriented correct.
        // Checks If Center is on the same side of the bisector.
        public bool Verify(Vector3 center)
        {
            Segment segment = new Segment(End.Point, End.Segment.End);

            return Vector3.Cross(Segment.Direction, center - Start.Point).normalized ==
                   Vector3.Cross(segment.Direction, center - segment.Start).normalized;
        }
    }
}
