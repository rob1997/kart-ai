using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Voronoi;

namespace Track
{
    public class TrackGenerator : MonoBehaviour
    {
        [field: SerializeField] public VoronoiPlane VoronoiPlane { get; private set; }

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                float startTime = Time.realtimeSinceStartup;
                
                VoronoiPlane.Generate(transform);
                
                // Execution time in milliseconds
                Debug.Log($"{(Time.realtimeSinceStartup - startTime) * 1000f}ms");
            }
        }
    }
}