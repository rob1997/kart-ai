using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Voronoi;

namespace Track
{
    [Serializable]
    public abstract class Path : IDisposable
    {
        public List<float3> Vertices { get; private set; } = new List<float3>();

        private Transform _transform;
        
        public void Generate(Transform transform)
        {
            _transform = transform;
            
            List<Cell> cells = GetCells(transform);
            
            List<Segment> segments = GetEdgeSegments(cells);
            
            GetVertices(segments);
            
#if UNITY_EDITOR
            if (!_drawing)
            {
                Drawer.Instance.OnDraw += Draw;

                _drawing = true;
            }
        }

        private bool _drawing;
        
#else
        }
#endif

        public abstract List<Cell> GetCells(Transform transform);
        
        private List<Segment> GetEdgeSegments(List<Cell> interior)
        {
            Dictionary<Segment, int> countDictionary = new Dictionary<Segment, int>();
            
            foreach (Cell cell in interior)
            {
                foreach (Segment segment in cell.Segments)
                {
                    bool isUnique = true;
                            
                    foreach (Segment key in countDictionary.Keys)
                    {
                        if (key.SameAs(segment))
                        {
                            countDictionary[key]++;
            
                            isUnique = false;
                                        
                            break;
                        }
                    }
            
                    if (isUnique)
                    {
                        countDictionary.Add(segment, 1);
                    }
                }
            }
            
            List<Segment> segments = new List<Segment>();
            
            foreach (var pair in countDictionary)
            {
                if (pair.Value == 1)
                {
                    segments.Add(pair.Key);
                }
            }
            
            return segments;
        }

        private void GetVertices(List<Segment> segments)
        {
            Vertices.Clear();
            
            int first = GetLeftMostSegment(segments);
            
            Vertices.AddRange(segments[first].Vertices());

            List<int> visited = new List<int>{ first };
            
            int last = GetClosestVertex(false, false);
            
            while (true)
            {
                int index = GetClosestVertex();

                if (index == last)
                {
                    break;
                }
                
                Vertices.Add(segments[index].End);
                
                visited.Add(index);
            }
            
            int GetClosestVertex(bool clockwise = true, bool visit = true)
            {
                float3 vertex = clockwise ? Vertices[^1] : Vertices[0];
                
                float minimumDistance = float.MaxValue;
                
                int closest = - 1;
                
                for (int i = 0; i < segments.Count; i++)
                {
                    if (visited.Contains(i))
                    {
                        continue;
                    }
                    
                    Segment segment = segments[i];
                    
                    float distance = math.distance(vertex, clockwise ? segment.Start : segment.End);
                    
                    if (distance < minimumDistance)
                    {
                        minimumDistance = distance;
                        
                        closest = i;
                    }
                }

                if (visit)
                {
                    visited.Add(closest);
                }
                
                return closest;
            }
        }

        private int GetLeftMostSegment(List<Segment> segments)
        {
            float minimum = float.MaxValue;
            
            int leftMost = - 1;
            
            float3 left = - _transform.right;
            
            for (int i = 0; i < segments.Count; i++)
            {
                Segment segment = segments[i];
                
                foreach (float3 vertex in segment.Vertices())
                {
                    float3 direction = vertex - (float3) _transform.position;
                    
                    float dot = math.dot(left, direction);
                    
                    if (dot < minimum)
                    {
                        minimum = dot;
                        
                        leftMost = i;
                    }
                }
            }

            return leftMost;
        }

        #region Gizmo

        [Space] [SerializeField] private bool drawGizmos = true;

        [SerializeField] private float centerRadius = .125f;
        
        private void Draw()
        {
            if (!drawGizmos)
            {
                return;
            }
            
            if (Vertices != null)
            {
                for (int i = 0; i < Vertices.Count; i++)
                {
                    Gizmos.color = Color.green;
                    
                    Gizmos.DrawSphere(Vertices[i], centerRadius);

                    Gizmos.color = Color.red;
                    
                    Gizmos.DrawLine(Vertices[i], Vertices[(i + 1) % Vertices.Count]);
                }
            }
        }

        #endregion
        
        public void Dispose()
        {
            Drawer.Instance.OnDraw -= Draw;
        }
    }
}