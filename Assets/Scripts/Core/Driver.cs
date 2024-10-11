using System;
using UnityEngine;

[RequireComponent(typeof(Motor))]
public abstract class Driver : MonoBehaviour
{
    protected Motor Motor { get; private set; }
    
    private void Start()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        Motor = GetComponent<Motor>();
        
        Motor.Initialize();
    }
}
