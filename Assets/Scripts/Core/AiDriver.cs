using Unity.Mathematics;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Voronoi;

namespace Core
{
    public class AiDriver : Driver
    {
        private float _lastProximity;
        
        private float _proximity;
        
        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();

            EvaluateProximity(true);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            EvaluateProximity();
            
            // 1 Observation
            sensor.AddObservation(_proximity);

            // 3 Observations
            sensor.AddObservation(transform.InverseTransformPoint(Target));

            // 3 Observations
            sensor.AddObservation(Velocity);

            // 1 Observation
            sensor.AddObservation(MovementDirection());
            
            // 1 Observation
            sensor.AddObservation(LookDirection());
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            
            float proximityDelta = _lastProximity - _proximity;
            
            AddReward(math.max(0, proximityDelta));
            
            CacheProximity();
        }

        private void EvaluateProximity(bool @override = false)
        {
            _proximity = ProximityToTarget;
            
            if (@override || CheckProximityAndUpdateTarget())
            {
                CacheProximity();
            }
        }

        private void CacheProximity()
        {
            _lastProximity = _proximity;
        }
        
        private float LookDirection()
        {
            float3 forward = transform.forward;
            
            float3 velocity = Velocity.Normalize();
            
            forward.y = 0;
            
            velocity.y = 0;
            
            return math.dot(velocity, forward);
        }
        
        private float MovementDirection()
        {
            float3 direction = (Target - (float3) transform.position).Normalize();
            
            float3 velocity = Velocity.Normalize();
            
            direction.y = 0;
            
            velocity.y = 0;
            
            return math.dot(velocity, direction);
        }
    }
}