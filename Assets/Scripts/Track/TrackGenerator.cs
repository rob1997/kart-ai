using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Voronoi;

namespace Track
{
    public class TrackGenerator : MonoBehaviour
    {
        public const float Tolerance = 0.05f;
        
        [field: SerializeField] public VoronoiPlane VoronoiPlane { get; private set; }

        private readonly List<float3> _vertices = new List<float3>();

        public int loop = 100;

        private List<List<Cell>> _groups;
        
        private List<Segment> _segments;
        
        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                float startTime = Time.realtimeSinceStartup;

                for (int j = 0; j < loop; j++)
                {
                    VoronoiPlane.Generate(transform);

                    List<Cell> interior = new List<Cell>();
                    
                    List<Cell> exterior = new List<Cell>();
                    
                    foreach (Cell cell in VoronoiPlane.Cells)
                    {
                        bool isExterior = false;
                        
                        foreach (var segment in cell.Segments)
                        {
                            if (VoronoiPlane.Cells.Any(c => !c.Equals(cell) && c.Segments.Any(s => s.SameAs(segment))))
                            {
                                continue;
                            }

                            isExterior = true;
                            
                            break;
                        }

                        if (isExterior)
                        {
                            exterior.Add(cell);
                        }

                        else
                        {
                            interior.Add(cell);
                        }
                    }

                    List<List<Cell>> groups = GetAdjacentGroups(interior);

                    // while (interior.Count != 0)
                    // {
                    //     groups.Add(FindAllAdjacent(ref interior));
                    // }

                    _groups = groups.ToList();
                    
                    while (groups.Count > 1)
                    {
                        groups = Merge(groups, ref exterior);
                    }

                    interior = groups.First();
                
                    List<Segment> segments = new List<Segment>();
                    
                    foreach (Cell cell in interior)
                    {
                        foreach (var segment in cell.Segments)
                        {
                            if (interior.Any(c => !c.Equals(cell) && c.Segments.Any(s => s.SameAs(segment))))
                            {
                                continue;
                            }
                        
                            segments.Add(segment);
                        }
                    }

                    _segments = segments.ToList();
                    
                    _vertices.Clear();
                
                    _vertices.AddRange(segments[0].Vertices());

                    segments.RemoveAt(0);

                    while (segments.Count != 0)
                    {
                        float3 vertex = _vertices.Last();

                        Segment closestSegment = default;

                        float distance = float.MaxValue;
                        
                        foreach (Segment segment in segments)
                        {
                            float distanceToStart = math.distance(segment.Start, vertex);
                            
                            float distanceToEnd = math.distance(segment.End, vertex);

                            if (math.min(distanceToStart, distanceToEnd) >= distance)
                            {
                                continue;
                            }
                            
                            distance = math.min(distanceToStart, distanceToEnd);

                            closestSegment = segment;
                        }
                        
                        segments.Remove(closestSegment);
                        
                        _vertices.AddRange(closestSegment.Vertices().OrderBy(s => math.distance(vertex, s)));
                    }
                }
                
                // Execution time in milliseconds
                Debug.Log($"{(Time.realtimeSinceStartup - startTime) * 1000f}ms");
            }
        }

        private List<List<Cell>> Merge(List<List<Cell>> groups, ref List<Cell> exterior)
        {
            (float3 vertex, List<Cell> cluster) groupA = (default, groups[0]);
            
            (float3 vertex, List<Cell> cluster) groupB = (default, new List<Cell>());

            HashSet<float3> aVertices = new HashSet<float3>();
            
            groupA.cluster.Select(c => c.Vertices).ToList().ForEach(v => aVertices = aVertices.Concat(v).ToHashSet());

            float minDistance = float.MaxValue;

            int index = 0;
            
            for (int i = 1; i < groups.Count; i++)
            {
                HashSet<float3> bVertices = new HashSet<float3>();

                groups[i].Select(c => c.Vertices).ToList().ForEach(v => bVertices = bVertices.Concat(v).ToHashSet());
                
                foreach (float3 aVertex in aVertices)
                {
                    foreach (float3 bVertex in bVertices)
                    {
                        float distance = math.distance(aVertex, bVertex);
                        
                        if (distance < minDistance)
                        {
                            minDistance = distance;

                            groupA.vertex = aVertex;
                            
                            groupB = (bVertex, groups[i]);

                            index = i;
                        }
                    }
                }
            }

            List<Cell> merged = Merge(groupA, groupB, ref exterior);

            groups[0] = merged;
            
            groups.RemoveAt(index);

            return groups;
        }

        private List<Cell> Merge((float3 vertex, List<Cell> cluster) groupA, (float3 vertex ,List<Cell> cluster) groupB, ref List<Cell> exterior)
        {
            Cell cellA = groupA.cluster.First(c => c.Vertices.Contains(groupA.vertex));
            
            Cell cellB = groupB.cluster.First(c => c.Vertices.Contains(groupB.vertex));

            List<Cell> allCellsAdjacentToA = exterior.Where(c => c.IsAdjacentTo(cellA)).ToList();

            Cell mergeCell;
            
            try
            {
                mergeCell = allCellsAdjacentToA.OrderBy(c => c.Vertices.Min(v => math.distance(v, groupB.vertex)))
                    .First();
            }
            catch (Exception)
            {
                throw;
            }

            exterior.Remove(mergeCell);
            
            groupA.cluster = groupA.cluster.Append(mergeCell).ToList();
            
            if (mergeCell.IsAdjacentTo(cellB))
            {
                return groupA.cluster.Concat(groupB.cluster).ToList();
            }

            groupA.vertex = mergeCell.Vertices.OrderBy(v => math.distance(v, groupB.vertex)).First();
            
            return Merge(groupA, groupB, ref exterior);
        }
        
        private List<Cell> FindAllAdjacent(ref List<Cell> interior)
        {
            List<Cell> group = new List<Cell>()
            {
                interior[0]
            };
            
            interior.RemoveAt(0);
            
            while (IsAdjacent(group, interior))
            {
                List<Cell> adjacent = new List<Cell>();

                foreach (Cell groupCell in group)
                {
                    foreach (Cell interiorCell in interior)
                    {
                        if (groupCell.IsAdjacentTo(interiorCell))
                        {
                            adjacent.Add(interiorCell);
                        }
                    }
                }
                
                group.AddRange(adjacent);

                interior = interior.Except(adjacent).ToList();
            }
            
            return group;
        }
        
        private List<List<Cell>> GetAdjacentGroups(List<Cell> interior)
        {
            List<List<Cell>> map = new List<List<Cell>>();

            foreach (Cell cell in interior)
            {
                map.Add(interior.Where(c => c.IsAdjacentTo(cell)).ToList());
            }

            List<List<Cell>> groups = new List<List<Cell>>();
            
            while (map.Count != 0)
            {
                groups.Add(map[0]);
                
                map.RemoveAt(0);
                
                foreach (List<Cell> cells in map.ToList())
                {
                    foreach (List<Cell> group in groups)
                    {
                        if (group.Any(c => cells.Contains(c)))
                        {
                            group.AddRange(cells);

                            map.Remove(cells);
                            
                            break;
                        }
                    }
                }
            }
            
            return groups.ConvertAll(g => g.ToHashSet().ToList());
        }

        private bool IsAdjacent(List<Cell> groupA, List<Cell> groupB)
        {
            foreach (Cell cellA in groupA)
            {
                foreach (Cell cellB in groupB)
                {
                    if (cellA.IsAdjacentTo(cellB))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public Vector2Int GetCellCoordinateFromIndex(int index)
        {
            Vector2Int size = VoronoiPlane.Size;
            
            int width = size.x;
            
            int height = size.y;
            
            //Get row and column
            int column = width > height ? (index - (index % height)) / height : index % width;

            int row = width > height ? index % height : (index - (index % width)) / width;

            return new Vector2Int(column, row);
        }
        
        private void OnDrawGizmos()
        {
            Color[] colors = new[] { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan, Color.black};
            
            int colorIndex = 0;
            
            if (_groups != null)
            {
                foreach (List<Cell> group in _groups)
                {
                    Gizmos.color = colors[colorIndex % colors.Length];
                    
                    colorIndex++;
                    
                    foreach (Cell cell in group)
                    {
                        foreach (Segment segment in cell.Segments)
                        {
                            Gizmos.DrawLine(segment.Start, segment.End);
                        }
                    }
                }
            }

            if (_segments != null)
            {
                Gizmos.color = colors[colorIndex % colors.Length];
                
                colorIndex++;
                
                foreach (Segment segment in _segments)
                {
                    Gizmos.DrawLine(segment.Start, segment.End);
                }
            }

            if (_vertices != null)
            {
                Gizmos.color = colors[colorIndex % colors.Length];
                
                for (int i = 0; i < _vertices.Count; i++)
                {
                    Gizmos.DrawSphere(_vertices[i], .125f);
                }
            }
        }
    }
}