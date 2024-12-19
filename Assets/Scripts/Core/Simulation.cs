using Track;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Core
{
    [RequireComponent(typeof(TrackGenerator))]
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private bool inference;
        
        [Space]
        
        [SerializeField] private int checkpoints = 25;

        [SerializeField, Range(0f, 1f)] private float normalizedProximityRange = 0.25f;

        public float TrackWidth => _trackGenerator.Width;

        public float ProximityRange => normalizedProximityRange * TrackWidth;
        
        private TrackGenerator _trackGenerator;

        private bool _initialized;

        public float DistanceBetweenCheckpoints { get; private set; }
        
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
            if (inference)
            {
                _trackGenerator.Generate();
            }

            else
            {
                _trackGenerator.GenerateVertices();
            
                _trackGenerator.GenerateSpline();
            }

            DistanceBetweenCheckpoints = _trackGenerator.Spline.GetLength() / checkpoints;
            
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
        
        [SerializeField] private float targetRadius = 0.5f;
        
        private void OnDrawGizmosSelected()
        {
            if (_drawing)
            {
                for (int i = 0; i < checkpoints; i++)
                {
                    Gizmos.color = Color.green;
                
                    Gizmos.DrawSphere(EvaluatePosition(i), targetRadius);
                }
            }
        }
#endif
    }
}