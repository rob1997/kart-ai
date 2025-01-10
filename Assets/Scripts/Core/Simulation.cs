using Track;
using Unity.Mathematics;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Serialization;
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

        [SerializeField] private Transform checkpointPrefab;
        
        [SerializeField] private Transform targetCheckpoint;
        
        [SerializeField] private Transform lapCheckpoint;
        
        [SerializeField] private Transform checkpointContainer;
        
        [field: Space]
        
        [field: SerializeField] public float ProximityPadding { get; private set; } = 3f;

        public float TrackWidth => _trackGenerator.Width;

        private TrackGenerator _trackGenerator;

        private bool _initialized;

        private int _steps = - 1;
        
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            
            _trackGenerator = GetComponent<TrackGenerator>();
            
            _initialized = true;
        }

        public void Setup()
        {
            // used to only setup once in case there's multiple agents per simulation
            // since it's being called OnEpisodeBegin of every Agent
            if (_steps == Academy.Instance.TotalStepCount)
            {
                return;
            }
            
            if (inference)
            {
                _trackGenerator.Generate();
                
                // Day/night cycle
                skyRenderer.sharedMaterial.mainTextureScale = new Vector2(Random.Range(0f, 2f), 1f);
                
                // random ground texture
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

            _steps = Academy.Instance.TotalStepCount;
            
#if UNITY_EDITOR
            _drawing = true;
#endif
        }
        
        public float3 EvaluatePosition(int index)
        {
            while (index < 0)
            {
                index += checkpoints;
            }
            
            index %= checkpoints;
            
            float t = (float) index / checkpoints;

            float3 position = _trackGenerator.Spline.EvaluatePosition(t);
            
            return transform.TransformPoint(position);
        }

        private void Checkpoint(int index)
        {
            float3 target = EvaluatePosition(index);
                
            float3 forward = EvaluatePosition(index - 1) - target;
            
            if (index % checkpoints != 0)
            {
                Instantiate(checkpointPrefab, target, Quaternion.LookRotation(forward), checkpointContainer);
            }

            else
            {
                lapCheckpoint.SetPositionAndRotation(target, Quaternion.LookRotation(forward));
            }
        }
        
        public void UpdateTargetCheckpoint(int index)
        {
            float3 target = EvaluatePosition(index);
                
            float3 forward = EvaluatePosition(index - 1) - target;
            
            if (index % checkpoints != 0)
            {
                targetCheckpoint.SetPositionAndRotation(target, Quaternion.LookRotation(forward));
            }
            
            targetCheckpoint.gameObject.SetActive(index % checkpoints != 0);
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