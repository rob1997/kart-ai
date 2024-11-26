using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Voronoi;

namespace Track
{
    [Serializable]
    public class Path : IDisposable
    {
        [SerializeField] private int width = 3;

        [SerializeField] private int height = 3;

        [SerializeField] private float size = 5f;

        public List<float3> Vertices { get; private set; } = new List<float3>();

        public void Generate(Transform transform)
        {
            List<Cell> interior = new List<Cell>();

            List<Cell> exterior = new List<Cell>();
            
            using (VoronoiPlane plane = new VoronoiPlane(width + 2, height + 2, size))
            {
                plane.Generate(transform);

                Cell[] cells = plane.Cells;
                
                foreach (Cell cell in cells)
                {
                    if (cell.Verify())
                    {
                        if (FilterCell(cell.Segments, cells))
                        {
                            interior.Add(cell);
                        }
                        else
                        {
                            exterior.Add(cell);
                        }
                    }
                }
            }
            
            if (interior.Count == 0)
            {
                interior.Add(exterior[0]);
                    
                exterior.RemoveAt(0);
            }

            List<List<Cell>> clusters = GetClusters(interior);
            
            while (clusters.Count > 1)
            {
                clusters = MergeClusters(clusters, exterior);
            }

            interior = clusters[0];
            
            List<Segment> segments = GetEdgeSegments(interior);
            
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
        
        private bool FilterCell(Segment[] segments, Cell[] cells)
        {
            foreach (Segment segment in segments)
            {
                bool interior = false;

                float3 previous = Voronoi.Utils.Cross(cells[0].Center - segment.Start, segment.Direction).Normalize();
                
                for (int i = 1; i < cells.Length; i++)
                {
                    float3 next = Voronoi.Utils.Cross(cells[i].Center - segment.Start, segment.Direction)
                        .Normalize();

                    if (!previous.Equals(next))
                    {
                        interior = true;

                        break;
                    }

                    previous = next;
                }
                
                if (!interior)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private List<List<Cell>> GetClusters(List<Cell> interior)
        {
            List<List<Cell>> groups = new List<List<Cell>>();
            
            HashSet<int> visited = new HashSet<int>();

            void DepthFirstSearch(int index, List<Cell> group)
            {
                Stack<int> stack = new Stack<int>();
                
                stack.Push(index);

                while (stack.Count > 0)
                {
                    int current = stack.Pop();

                    if (!visited.Add(current))
                    {
                        continue;
                    }

                    group.Add(interior[current]);

                    for (int i = 0; i < interior.Count; i++)
                    {
                        if (!visited.Contains(i) && interior[current].IsAdjacentTo(interior[i]))
                        {
                            stack.Push(i);
                        }
                    }
                }
            }

            for (int i = 0; i < interior.Count; i++)
            {
                if (!visited.Contains(i))
                {
                    List<Cell> group = new List<Cell>();
                    
                    DepthFirstSearch(i, group);
                    
                    groups.Add(group);
                }
            }

            return groups;
        }
        
        private List<List<Cell>> MergeClusters(List<List<Cell>> clusters, List<Cell> exterior)
        {
            List<Cell> clusterA = new List<Cell>(clusters[0]);

            List<Cell> clusterB;
            
            float minimumDistance = float.MaxValue;
            
            int index = 0;
            
            for (var i = 1; i < clusters.Count; i++)
            {
                clusterB = clusters[i];
                
                foreach (Cell cellA in clusterA)
                {
                    foreach (Cell cellB in clusterB)
                    {
                        foreach (float3 vertexA in cellA.Vertices)
                        {
                            foreach (float3 vertexB in cellB.Vertices)
                            {
                                float distance = math.distance(vertexA, vertexB);
                            
                                if (distance < minimumDistance)
                                {
                                    minimumDistance = distance;
                                
                                    index = i;
                                }
                            }
                        }
                    }
                }
            }

            clusterB = clusters[index];
            
            clusters.RemoveAt(index);
            
            while (true)
            {
                minimumDistance = float.MaxValue;

                index = - 1;

                for (int i = 0; i < exterior.Count; i++)
                {
                    Cell cell = exterior[i];
                    
                    foreach (Cell cellA in clusterA)
                    {
                        if (cell.IsAdjacentTo(cellA))
                        {
                            foreach (Cell cellB in clusterB)
                            {
                                if (cell.IsAdjacentTo(cellB))
                                {
                                    clusterA.AddRange(clusterB);
                                    
                                    clusterA.Add(cell);
                                    
                                    clusters[0] = clusterA;
                                    
                                    return clusters;
                                }

                                foreach (float3 vertex in cell.Vertices)
                                {
                                    foreach (float3 vertexB in cellB.Vertices)
                                    {
                                        float distance = math.distance(vertex, vertexB);

                                        if (distance < minimumDistance)
                                        {
                                            minimumDistance = distance;
                                            
                                            index = i;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                clusterA.Add(exterior[index]);

                exterior.RemoveAt(index);
            }
        }

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
            
            Vertices.AddRange(segments[0].Vertices());

            List<int> visited = new List<int>{ 0 };
            
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