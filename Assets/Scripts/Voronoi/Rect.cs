using Unity.Mathematics;

namespace Voronoi
{
    public struct Rect
    {
        public float3 Min;
        public float3 Max;

        public float MinX => Min.x;
        public float MinY => Min.y;
        
        public float MaxX => Max.x;
        public float MaxY => Max.y;
        
        public Rect(float2 min, float2 max)
        {
            Min = new float3(min.xy, 0);
            Max = new float3(max.xy, 0);
        }
    }
}
