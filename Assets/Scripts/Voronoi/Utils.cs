using Unity.Mathematics;
using UnityEngine;

namespace Voronoi
{
    public static class Utils
    {
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