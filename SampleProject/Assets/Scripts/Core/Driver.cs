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

        public int Score { get; private set; } = - 1;
        
        protected Simulation Simulation { get; private set; }

        protected Motor Motor { get; private set; }
        
        protected float3 Target { get; private set; }

        // right vector of the track at the current target
        private float3 _right;
        
        // point on the right side of the track's current boundary
        private float3 _pointL;
        
        // point on the left side of the track's current boundary
        private float3 _pointR;

        // is the agent inside the track or not
        private bool _inTrack;
        
        // which side of the boundary is the agent on
        private int _side;
        
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
            Simulation.Setup();
            
            Index = transform.GetSiblingIndex();
            
            Next();
            
            float3 position = Simulation.EvaluatePosition(Index - 1);
            
            // look towards target
            Quaternion rotation = Quaternion.LookRotation(Target - position);
            
            Motor.Setup(position, rotation);
            
#if UNITY_EDITOR
            _drawing = true;
#endif
        }

        
        public override void OnActionReceived(ActionBuffers actions)
        {
            float gas = actions.ContinuousActions[0];
            
            float steer = actions.ContinuousActions[1];
            
            float brake = actions.DiscreteActions[0];

            Motor.Drive(gas, steer, brake);
        }
        
        protected bool CheckAndUpdateTarget()
        {
            float3 position = transform.position;
            
            float3 target = Target;
            
            position.y = target.y = 0;

            float3 direction = position - target;

            // which side of the boundary is the agent on
            int side = (int) math.cross(direction, _right).Normalize().y;
            
            if (_inTrack && side != _side)
            {
                Next();
                
                return true;
            }

            GetAngles(out float angleL, out float angleR);

            _inTrack = angleL < 90 && angleR < 90;

            _side = side;
            
            return false;
        }

        // Get angles between the agent and the left and right boundaries
        private void GetAngles(out float angleL, out float angleR)
        {
            float3 position = transform.position;

            position.y = 0;
            
            float3 directionL = position - _pointL;
            
            float3 directionR = position - _pointR;
            
            angleL = Voronoi.Utils.Angle(_right, directionL);
            
            angleR = Voronoi.Utils.Angle(- _right, directionR);
        }
        
        protected virtual void Next()
        {
            Index++;
            
            Score++;

            Target = Simulation.EvaluatePosition(Index);
            
            //Re-calculate cached values
            float3 forward = Simulation.EvaluatePosition(Index - 1) - Target;

            forward.y = 0;

            forward = forward.Normalize();
            
            _right = math.cross(forward, Simulation.transform.up);

            _right = _right.Normalize() * Simulation.TrackWidth;
            
            _pointR = Target + _right;
            
            _pointL = Target - _right;
            
            _right.y = _pointR.y = _pointL.y = 0;

            _side = - 1;
        }

        protected float Proximity()
        {
            GetAngles(out float angleL, out float angleR);

            float3 position = transform.position;

            position.y = 0;
            
            // both angles are acute, it means it's inside the bounds
            if (angleL < 90 && angleR < 90)
            {
                return math.length(position - _pointL) * math.sin(angleL * math.TORADIANS);
            }

            // else if one of the angles is obtuse then proximity is calculated as the distance from edge of the obtuse side
            return math.distance(angleL >= 90 ? _pointL : _pointR, position);
        }
        
#if UNITY_EDITOR
        private bool _drawing;
        
        private void OnDrawGizmosSelected()
        {
            if (_drawing)
            {
                Gizmos.color = Color.red;
            
                Gizmos.DrawLine(transform.position, Target);

                Gizmos.color = Color.blue;

                float3 pointL = Simulation.transform.TransformPoint(_pointL);
                
                float3 pointR = Simulation.transform.TransformPoint(_pointR);
                
                Gizmos.DrawLine(pointL, pointR);
            }
        }
#endif
    }
}