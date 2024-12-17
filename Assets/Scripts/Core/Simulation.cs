using Track;
using Unity.Mathematics;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Splines;

namespace Core
{
    [RequireComponent(typeof(TrackGenerator))]
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private int checkpoints = 25;

        public float Proximity => _trackGenerator.Width;
        
        private TrackGenerator _trackGenerator;

        private bool _initialized;
        
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            
            _trackGenerator = GetComponent<TrackGenerator>();
            
            Academy.Instance.OnEnvironmentReset += Restart;
            
            _initialized = true;
        }

        private void Restart()
        {
            _trackGenerator.GenerateVertices();
            
            _trackGenerator.GenerateSpline();

#if UNITY_EDITOR
            if (!_drawing)
            {
                Drawer.Instance.OnDraw += Draw;

                _drawing = true;
            }
        }

        private bool _drawing;
#else
        }
#endif
        
        public float3 EvaluatePosition(int index)
        {
            index %= checkpoints;
            
            float t = (float) index / checkpoints;

            return _trackGenerator.Spline.EvaluatePosition(t);
        }

        private void Draw()
        {
            for (int i = 0; i < checkpoints; i++)
            {
                float3 position = _trackGenerator.Spline.EvaluatePosition((float) i / checkpoints);
                
                Gizmos.color = Color.green;
                
                Gizmos.DrawWireSphere(position, Proximity);
            }
        }
    }
}