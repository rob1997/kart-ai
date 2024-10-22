using UnityEngine;

namespace Voronoi
{
    public struct Intersection
    {
        public Vector3 Point { get; private set; }

        public Segment Segment { get; private set; }

        public Intersection(Vector3 point, Segment segment)
        {
            Point = point;

            Segment = segment;
        }

        public override bool Equals(object obj)
        {
            if (obj is Intersection other)
            {
                return Equals(other);
            }

            return false;
        }

        public bool Equals(Intersection other)
        {
            return Point.Equals(other.Point);
        }

        public override int GetHashCode()
        {
            return Point.GetHashCode();
        }
        
        public static Intersection FromStartIntersection(Intersection intersection)
        {
            return new Intersection(intersection.Point, new Segment(intersection.Segment.Start, intersection.Point));
        }
        
        public static Intersection FromEndIntersection(Intersection intersection)
        {
            return new Intersection(intersection.Point, new Segment(intersection.Point, intersection.Segment.End));
        }
    }
}
