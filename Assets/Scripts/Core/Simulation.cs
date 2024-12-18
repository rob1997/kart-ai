using Track;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Core
{
    [RequireComponent(typeof(TrackGenerator))]
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private int checkpoints = 25;

        public float ProximityThreshold => _trackGenerator.Width;
        
        private TrackGenerator _trackGenerator;

        private bool _initialized;
        
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            
            _trackGenerator = GetComponent<TrackGenerator>();
            
            _initialized = true;
        }

        public void Restart()
        {
            _trackGenerator.GenerateVertices();
            
            _trackGenerator.GenerateSpline();

#if UNITY_EDITOR
            _drawing = true;
#endif
        }
        
        public float3 EvaluatePosition(int index)
        {
            index %= checkpoints;
            
            float t = (float) index / checkpoints;

            float3 position = _trackGenerator.Spline.EvaluatePosition(t);
            
            return transform.TransformPoint(position);
        }

#if UNITY_EDITOR
        private bool _drawing;
        
        private void OnDrawGizmosSelected()
        {
            if (_drawing)
            {
                for (int i = 0; i < checkpoints; i++)
                {
                    Gizmos.color = Color.green;
                
                    Gizmos.DrawWireSphere(EvaluatePosition(i), ProximityThreshold);
                }
            }
        }
#endif
    }
}