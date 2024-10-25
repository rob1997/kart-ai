using Unity.Mathematics;

namespace Voronoi
{
    public struct Rect
    {
        public float2 Min;
        public float2 Max;

        public float MinX => Min.x;
        public float MinY => Min.y;
        
        public float MaxX => Max.x;
        public float MaxY => Max.y;
        
        public Rect(float2 min, float2 max)
        {
            Min = min;
            Max = max;
        }
    }
}
