using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(Motor))]
    public class Driver : Agent
    {
        // Distance to Target
        protected float ProximityToTarget
        {
            get
            {
                float3 position = transform.position;
                
                float3 target = Target;
                
                position.y = 0;
                
                target.y = 0;
                
                return math.distance(position, target);
            }
        }

        private int _index;

        protected Simulation Simulation { get; private set; }

        protected Motor Motor { get; private set; }
        
        protected float3 Target { get; private set; }
        
        public override void Initialize()
        {
            base.Initialize();
            
            Simulation = GetComponentInParent<Simulation>();
            
            Simulation.Initialize();
            
            Motor = GetComponent<Motor>();
            
            Motor.Initialize();
        }
        
        public override void OnEpisodeBegin()
        {
            Simulation.Restart();
            
            Motor.RigidBody.linearVelocity = Vector3.zero;
            
            Motor.RigidBody.angularVelocity = Vector3.zero;
            
            _index = 0;
            
            transform.position = Simulation.EvaluatePosition(_index);
            
            Next();
            
#if UNITY_EDITOR
            _drawing = true;
#endif
        }

        
        public override void OnActionReceived(ActionBuffers actions)
        {
            float acceleration = actions.ContinuousActions[0];
            
            float direction = actions.ContinuousActions[1];
            
            float brake = actions.DiscreteActions[0];

            Motor.Drive(acceleration, direction, brake);
        }
        
        protected bool CheckProximityAndUpdateTarget()
        {
            if (ProximityToTarget <= Simulation.ProximityThreshold)
            {
                Next();

                return true;
            }

            return false;
        }

        private void Next()
        {
            _index++;

            Target = Simulation.EvaluatePosition(_index);
        }

#if UNITY_EDITOR
        private bool _drawing;
        
        private void OnDrawGizmosSelected()
        {
            if (_drawing)
            {
                Gizmos.color = Color.red;
            
                Gizmos.DrawLine(transform.position, Target);
            }
        }
#endif
    }
}