using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;
using Voronoi;

namespace Core
{
    [RequireComponent(typeof(Motor))]
    public class Driver : Agent
    {
        protected int Index { get; private set; }
        
        protected Simulation Simulation { get; private set; }

        protected Motor Motor { get; private set; }
        
        protected float3 Target { get; private set; }

        private float3 _right;
        
        private float3 _pointL;
        
        private float3 _pointR;
        
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
            
            Index = 0;
            
            transform.position = Simulation.EvaluatePosition(Index);
            
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
            if (ProximityToTarget() <= Simulation.ProximityRange)
            {
                Next();

                return true;
            }

            return false;
        }

        private float ProximityToTarget()
        {
            float3 position = transform.position;
            
            position.y = 0;
            
            float3 directionL = position - _pointL;
            
            float3 directionR = position - _pointR;
            
            float angleL = Voronoi.Utils.Angle(_right, directionL);
            
            float angleR = Voronoi.Utils.Angle(- _right, directionR);

            if (angleL < 90 && angleR < 90)
            {
                return math.length(directionL) * math.sin(angleL * math.TORADIANS);
            }

            if (angleL >= 90)
            {
                return math.distance(position, _pointL);
            }

            return math.distance(position, _pointR);
        }
        
        private void Next()
        {
            Index++;

            Target = Simulation.EvaluatePosition(Index);
            
            //Re-calculate proximity values
            float3 forward = Simulation.EvaluatePosition(Index + 1) - Target;

            forward.y = 0;

            forward = forward.Normalize();
            
            _right = math.cross(forward, Simulation.transform.up);

            _right = _right.Normalize() * Simulation.TrackWidth;
            
            _pointR = Target + _right;
            
            _pointL = Target - _right;
            
            _pointR.y = _pointL.y = 0;
            
            _right = _right.Normalize();
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