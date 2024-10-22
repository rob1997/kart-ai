using UnityEngine;

namespace Voronoi
{
    public struct Segment
    {
        public Vector3 Start { get; private set; }
        public Vector3 End { get; private set; }

        public Vector3 Direction => End - Start;

        public Vector3 Center { get; private set; }

        public Segment(Vector3 start, Vector3 end)
        {
            Start = start;

            End = end;

            Center = (start + end) / 2f;
        }

        public bool Equals(Segment other)
        {
            return Start == other.Start && End == other.End;
        }
    }
}
