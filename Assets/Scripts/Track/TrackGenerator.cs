using UnityEngine;
using UnityEngine.InputSystem;

namespace Track
{
    public class TrackGenerator : MonoBehaviour
    {
        public const float Tolerance = 0.05f;

        [SerializeField] private Path path;

        [SerializeField] private int loop;

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                float startTime = Time.realtimeSinceStartup;

                for (int i = 0; i < loop; i++)
                {
                    path.Generate(transform);
                }

                // Execution time in milliseconds
                Debug.Log($"{(Time.realtimeSinceStartup - startTime) * 1000f}ms");
            }
        }
    }
}