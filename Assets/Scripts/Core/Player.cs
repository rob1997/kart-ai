using Unity.MLAgents.Actuators;
using UnityEngine;

namespace Core
{
    public class Player : Driver
    {
        private InputMaster _inputMaster;

        private bool _brake;

        public override void Initialize()
        {
            base.Initialize();

            _inputMaster = new InputMaster();

            _inputMaster.Enable();
            
            _inputMaster.Player.Brake.started += delegate { _brake = true; };
            
            _inputMaster.Player.Brake.canceled += delegate { _brake = false; };
        }
        
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            Vector2 moveVector = _inputMaster.Player.Move.ReadValue<Vector2>();

            //get inputs
            float acceleration = moveVector.y;
            
            float direction = moveVector.x;

            actionsOut.ContinuousActions.Array[0] = acceleration;
            
            actionsOut.ContinuousActions.Array[1] = direction;

            actionsOut.DiscreteActions.Array[0] = _brake ? 1 : 0;
        }
    }
}
