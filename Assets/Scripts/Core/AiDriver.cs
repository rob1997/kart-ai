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

            EvaluateProximity();
            
            CacheProximity();
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
             sensor.AddObservation(TrackDirection());
            
            // 1 Observation
            sensor.AddObservation(NextTurn());

            // 1 Observation
            sensor.AddObservation(NextDistance());
            
            // 1 Observation
            sensor.AddObservation(Speed());
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            base.OnActionReceived(actions);

            float reward = 0;

            float proximityDelta = _lastProximity - _proximity;

            // Distance between checkpoints
            float distance = Distance();
            
            // 0 - 1 value, increases as you get closer to the target
            float proximityFactor = (distance - math.clamp(_proximity, 0f, distance)) / distance;

            // Higher reward as you get closer to the target
            reward += math.max(0f, proximityDelta * proximityFactor);
            
            float trackDirection = TrackDirection();
            
            reward += reward * math.clamp(trackDirection, 0, 1);
            
            float lookDirection = LookDirection();

            reward += reward * math.clamp(lookDirection, 0, 1);

            // higher reward for faster velocity towards target
            float speedFactor = (math.max(0, MovementDirection()) * Speed()) / Motor.MaxSpeed;
            
            reward += reward * math.clamp(speedFactor, 0, 1);
            
            if (CheckAndUpdateTarget())
            {
                reward += 1;
                
                EvaluateProximity();
            }
            
            AddReward(reward);
            
            CacheProximity();
        }

        private void EvaluateProximity()
        {
            _proximity = Proximity();
        }

        private void CacheProximity()
        {
            _lastProximity = _proximity;
        }
        
        private float3 Velocity()
        {
            float3 velocity = Motor.RigidBody.linearVelocity;
            
            velocity.y = 0;
            
            return velocity;
        }
        
        private float Speed()
        {
            return math.length(Velocity());
        }
        
        private float TrackDirection()
        {
            float3 direction = (float3) transform.position - Target;
            
            float3 roadDirection = Simulation.EvaluatePosition(Index - 1) - Target;
            
            direction.y = roadDirection.y = 0;

            return math.dot(direction.Normalize(), roadDirection.Normalize());
        }
        
        private float LookDirection()
        {
            float3 forward = transform.forward;
            
            float3 velocity = Velocity();
            
            forward.y = 0;
            
            return math.dot(velocity.Normalize(), forward.Normalize());
        }
        
        private float MovementDirection()
        {
            float3 direction = Target - (float3) transform.position;
            
            float3 velocity = Velocity();
            
            direction.y = 0;

            return math.dot(velocity.Normalize(), direction.Normalize());
        }

        private float NextTurn()
        {
            float3 direction = Velocity();

            float3 nextDirection = Simulation.EvaluatePosition(Index + 1) - Target;

            nextDirection.y = 0;
            
            return Voronoi.Utils.SignedAngle(direction, nextDirection, Simulation.transform.up) / 180f;
        }
        
        private float NextDistance()
        {
            float3 current = Target;
            
            float3 next = Simulation.EvaluatePosition(Index + 1);

            current.y = next.y = 0;
            
            return math.distance(current, next);
        }
        
        private float Distance()
        {
            float3 current = Target;
            
            float3 previous = Simulation.EvaluatePosition(Index - 1);

            current.y = previous.y = 0;
            
            return math.distance(previous, current);
        }
    }
}