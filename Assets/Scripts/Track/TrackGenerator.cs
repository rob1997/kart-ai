using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

namespace Track
{
    [RequireComponent(typeof(SplineContainer), typeof(MeshFilter), typeof(MeshRenderer))]
    public abstract class TrackGenerator<T> : MonoBehaviour where T : Path
    {
        [SerializeField] private int smoothness = 100;
        
        [SerializeField] private float width = 5f;
        
        [SerializeField, Range(0f, 1f)] private float tiling = 1f;
        
        private SplineContainer _splineContainer;
        
        private MeshFilter _meshFilter;
        
        private MeshRenderer _meshRenderer;
        
        private Spline Spline
        {
            get => _splineContainer.Spline;

            set => _splineContainer.Spline = value;
        }
        
        private List<float3> _vertices = new List<float3>();
        
        private void Awake()
        {
            _splineContainer = GetComponent<SplineContainer>();
            
            _meshFilter = GetComponent<MeshFilter>();
            
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                float startTime = Time.realtimeSinceStartup;

                Generate();
                
                // Execution time in milliseconds
                Debug.Log($"{(Time.realtimeSinceStartup - startTime) * 1000f}ms");
            }
        }

        public void Generate()
        {
            GenerateVertices();
            
            GenerateSpline();

            GenerateMesh();
        }
        
        protected abstract T GetPath();
        
        public void GenerateVertices()
        {
            using (Path path = GetPath())
            {
                path.Generate(transform);

                _vertices = path.Vertices;
            }
        }

        public void GenerateSpline()
        {
            Spline.Clear();
                    
            foreach (float3 vertex in _vertices)
            {
                Spline.Add(transform.InverseTransformPoint(vertex));
            }
            
            Spline.Closed = true;
        }

        public void GenerateMesh()
        {
            int vLength = (smoothness + 2) * 2;
            
            NativeArray<float3> vertices = new NativeArray<float3>(vLength, Allocator.TempJob);
            
            NativeArray<float2> uvs = new NativeArray<float2>(vLength, Allocator.TempJob);
            
            NativeArray<int> triangles = new NativeArray<int>(vLength * 3, Allocator.TempJob);
            
            NativeSpline spline = new NativeSpline(Spline, Allocator.TempJob);
            
            new CalculateUVsAndVerticesJob
            {
                Vertices = vertices,
                
                UVs = uvs,
                
                Length = vLength,
                
                Spline = spline,
                
                Width = width
            }.Schedule(vLength, 8).Complete();

            new CalculateTrianglesJob()
            {
                Triangles = triangles,
                
                VerticesLength = vLength
            }.Schedule(triangles.Length, 8).Complete();
            
            Spline = new Spline(spline, true);

            // Set Mesh
            _meshFilter.mesh.Clear();
            
            _meshFilter.mesh.vertices = Array.ConvertAll(vertices.ToArray(), v => (Vector3) v);
            
            _meshFilter.mesh.uv = Array.ConvertAll(uvs.ToArray(), v => (Vector2) v);
            
            _meshFilter.mesh.triangles = triangles.ToArray();
            
            // Tiling
            _meshRenderer.sharedMaterial.mainTextureScale = new Vector2(1, tiling * Spline.GetLength());
            
            _meshFilter.mesh.RecalculateNormals();

            // Dispose
            vertices.Dispose();

            uvs.Dispose();

            triangles.Dispose();
            
            spline.Dispose();
        }
    }
}