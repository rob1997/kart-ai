using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Draw : MonoBehaviour
{
    public static Draw Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public List<Action> DrawList = new List<Action>();
    
    public VoronoiPlane voronoiPlane;

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(DrawPlane(100));
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var action in DrawList)
        {
            action.Invoke();
        }
        
        voronoiPlane.DrawPlane();
    }

    public IEnumerator DrawPlane(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return voronoiPlane.Generate();
        }
    }
}
