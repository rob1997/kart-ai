using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Driver
{
    private InputMaster _inputMaster;

    private bool _brake = false;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        _inputMaster = new InputMaster();
        
        _inputMaster.Enable();
        
        _inputMaster.Player.Brake.started += delegate { _brake = true; };
        
        _inputMaster.Player.Brake.canceled += delegate { _brake = false; };
    }
    
    private void Update()
    {
        Vector2 moveVector = _inputMaster.Player.Move.ReadValue<Vector2>();
        
        //get inputs
        float acceleration = moveVector.y;
        float direction = moveVector.x;

        Motor.Drive(acceleration, direction, _brake ? 1f : 0f);
    }
}
