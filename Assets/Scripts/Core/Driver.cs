using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(Motor))]
    public class Driver : Agent
    {
        protected int Index { get; private set; }

        protected float3 Target { get; private set; }

        protected float3 Position => Motor.RigidBody.position;

        protected Motor Motor { get; private set; }

        protected Simulation Simulation { get; private set; }
        
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
            Index = 0;
            
            Motor.transform.position = Simulation.EvaluatePosition(Index);
            
            Next();
            
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
        
        public override void OnActionReceived(ActionBuffers actions)
        {
            float acceleration = actions.ContinuousActions[0];
            
            float direction = actions.ContinuousActions[1];
            
            float brake = actions.DiscreteActions[0];

            Motor.Drive(acceleration, direction, brake);
        }
        
        private void LateUpdate()
        {
            float distance = math.distance(Position, Target);

            if (distance <= Simulation.Proximity)
            {
                Next();
            }
        }

        private void Next()
        {
            Index++;

            Target = Simulation.EvaluatePosition(Index);
        }
        
        private void Draw()
        {
            Gizmos.color = Color.red;
            
            Gizmos.DrawLine(Position, Target);
        }
    }
}