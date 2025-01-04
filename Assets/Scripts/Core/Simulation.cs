using Track;
using Unity.Mathematics;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

namespace Core
{
    [RequireComponent(typeof(TrackGenerator))]
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private bool inference;
        
        [SerializeField] private MeshRenderer skyRenderer;
        
        [SerializeField] private Material[] groundMats;
        
        [SerializeField] private MeshRenderer groundRenderer;
        
        [Space]
        
        [SerializeField] private int checkpoints = 25;

        [SerializeField] private Transform checkpoint;
        
        [SerializeField] private Transform next;
        
        [SerializeField] private Transform entry;
        
        [SerializeField] private Transform container;
        
        [field: SerializeField] public float ProximityPadding { get; private set; } = 3f;

        public float TrackWidth => _trackGenerator.Width;

        private TrackGenerator _trackGenerator;

        private bool _initialized;

        private int _steps = - 1;
        
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
            if (_steps == Academy.Instance.TotalStepCount)
            {
                return;
            }
            
            if (inference)
            {
                _trackGenerator.Generate();
                
                // Day/night cycle
                skyRenderer.sharedMaterial.mainTextureScale = new Vector2(Random.Range(0f, 2f), 1f);
                
                groundRenderer.material = groundMats[Random.Range(0, groundMats.Length)];

                for (int i = 0; i < checkpoints; i++)
                {
                    Checkpoint(i);
                }
            }

            else
            {
                _trackGenerator.GenerateVertices();
            
                _trackGenerator.GenerateSpline();
            }

            DistanceBetweenCheckpoints = _trackGenerator.Spline.GetLength() / checkpoints;
            
            _steps = Academy.Instance.TotalStepCount;
            
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

        public void Checkpoint(int index, bool isTarget = false)
        {
            float3 target = EvaluatePosition(index);
                
            float3 forward = EvaluatePosition(index - 1) - target;

            if (isTarget)
            {
                if (index % checkpoints != 0)
                {
                    next.SetPositionAndRotation(target, Quaternion.LookRotation(forward));
                }
            }
            else
            {
                if (index == 0)
                {
                    entry.SetPositionAndRotation(target, Quaternion.LookRotation(forward));
                }

                else
                {
                    Instantiate(checkpoint, target, Quaternion.LookRotation(forward), container);
                }
            }
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