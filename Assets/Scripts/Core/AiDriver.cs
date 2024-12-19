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

        private float _distanceBetweenCheckpoints;
        
        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();

            EvaluateProximity(true);
            
            _distanceBetweenCheckpoints = Simulation.DistanceBetweenCheckpoints;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            EvaluateProximity();
            
            // 1 Observation
            sensor.AddObservation(_proximity);

            // 3 Observations
            sensor.AddObservation(transform.InverseTransformPoint(Target));

            // 3 Observations
            sensor.AddObservation(Velocity());

            // 1 Observation
            sensor.AddObservation(MovementDirection());
            
            // 1 Observation
            sensor.AddObservation(LookDirection());
            
            // 1 Observation
            sensor.AddObservation(NextTurn());
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);
            
            float proximityDelta = _lastProximity - _proximity;

            // 0 - 1 value, increases as you get closer to the target
            float proximityFactor = (_distanceBetweenCheckpoints - math.clamp(_proximity, 0f, _distanceBetweenCheckpoints)) / _distanceBetweenCheckpoints;

            // Higher reward as you get closer to the target
            float reward = math.max(0f, proximityDelta * proximityFactor);
            
            AddReward(reward);
            
            CacheProximity();
        }

        private void EvaluateProximity(bool @override = false)
        {
            float3 target = Target;

            float3 position = transform.position;
            
            target.y = position.y = 0;
            
            _proximity = math.distance(target, position);
            
            if (@override || CheckProximityAndUpdateTarget())
            {
                CacheProximity();
            }
        }

        private void CacheProximity()
        {
            _lastProximity = _proximity;
        }
        
        private float3 Velocity()
        {
            return Motor.RigidBody.linearVelocity;
        }
        
        private float LookDirection()
        {
            float3 forward = transform.forward;
            
            float3 velocity = Velocity().Normalize();
            
            forward.y = 0;
            
            velocity.y = 0;
            
            return math.dot(velocity, forward);
        }
        
        private float MovementDirection()
        {
            float3 direction = (Target - (float3) transform.position).Normalize();
            
            float3 velocity = Velocity().Normalize();
            
            direction.y = 0;
            
            velocity.y = 0;
            
            return math.dot(velocity, direction);
        }

        private float NextTurn()
        {
            float3 direction = Target - (float3) transform.position;

            float3 nextDirection = Simulation.EvaluatePosition(Index + 1) - Target;
            
            return Voronoi.Utils.SignedAngle(direction, nextDirection, Simulation.transform.up) / 180f;
        }
    }
}