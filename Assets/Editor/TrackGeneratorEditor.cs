using Track;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(TrackGenerator<>), true)]
    public class TrackGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button(new GUIContent("Generate Mesh", "Generate the track mesh")))
            {

                GenerateMesh(target);
            }
        }
        
        private void GenerateMesh(dynamic trackGenerator)
        {
            trackGenerator.GenerateMesh();
        }
    }
}