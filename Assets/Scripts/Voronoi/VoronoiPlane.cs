using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Voronoi
{
    [Serializable]

    public class VoronoiPlane
    {
        [SerializeField] private int planeWidth = 5;

        [SerializeField] private int planeHeight = 5;

        [SerializeField] private float cellSize = 5f;

        [SerializeField] private float padding = 1f;

        private Cell[] _cells;

        private Rect _boundingRect;
        
        private float _diagonalDistance;

        private Vector3 _normal;
        
        private Vector3 _origin;
        
        public void Generate(Vector3 origin, Vector3 normal)
        {
            _normal = normal;
            
            _origin = origin;
            
            int arrayLength = planeWidth * planeHeight;
            
            _cells = new Cell[arrayLength];
            
            NativeArray<float3> centers = new NativeArray<float3>(arrayLength, Allocator.TempJob);
            
            GetRandomCenterJob getRandomCenterJob = new GetRandomCenterJob
            {
                Centers = centers,
                
                Width = planeWidth,
                
                Height = planeHeight,
                
                CellSize = cellSize,
                
                Seed = Random.Range(0, int.MaxValue)
            };
            
            JobHandle jobHandle = getRandomCenterJob.Schedule(arrayLength, arrayLength / 6);
            
            jobHandle.Complete();

            for (int i = 0; i < arrayLength; i++)
            {
                _cells[i] = new Cell(centers[i]);
            }
            
            centers.Dispose();

            _boundingRect = GetBoundingRect();

            _diagonalDistance = Vector3.Distance(_boundingRect.min, _boundingRect.max);
            
            for (int i = 0; i < _cells.Length; i++)
            {
                CalculateSegments(i);
            }
            
            for (int i = 0; i < _cells.Length; i++)
            {
                Cell cell = _cells[i];
                
                _cells[i] = ProjectAndTranslate(cell);
            }

            
            
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

        private void CalculateSegments(int index)
        {
            Cell cell = _cells[index];

            cell = cell.FromRect(_boundingRect);

            for (int i = 0; i < _cells.Length; i++)
            {
                if (i == index)
                {
                    continue;
                }

                Cell other = _cells[i];

                Segment connectingSegment = new Segment(cell.Center, other.Center);

                Vector3 bisectorDirection = Vector3.Cross(connectingSegment.Direction, Vector3.forward).normalized * _diagonalDistance;

                Vector3 bisectorEnd = bisectorDirection + connectingSegment.Center;

                Segment bisector = new Segment(connectingSegment.Center - bisectorDirection, bisectorEnd);

                if (cell.GetIntersections(bisector, out HashSet<Intersection> intersections))
                {
                    cell = cell.FromIntersections(intersections);
                }
            }

            _cells[index] = cell;
        }

        private Rect GetBoundingRect()
        {
            float minX = _cells.Min(c => c.Center.x) - padding;
            float minY = _cells.Min(c => c.Center.y) - padding;

            float maxX = _cells.Max(c => c.Center.x) + padding;
            float maxY = _cells.Max(c => c.Center.y) + padding;

            float sizeX = maxX - minX;
            float sizeY = maxY - minY;

            return new Rect(minX, minY, sizeX, sizeY);
        }

        private Vector3 ProjectAndTranslate(Vector3 value)
        {
            value = Quaternion.LookRotation(_normal) * value;
            
            return value + _origin;
        }
        
        private Cell ProjectAndTranslate(Cell cell)
        {
            Vector3 center = ProjectAndTranslate(cell.Center);

            Segment[] segments = new Segment[cell.Segments.Length];
            
            for (int i = 0; i < segments.Length; i++)
            {
                Segment segment = cell.Segments[i];
                
                segments[i] = new Segment(ProjectAndTranslate(segment.Start), ProjectAndTranslate(segment.End));
            }
            
            return new Cell(center, segments);
        }
        
        #region Gizmo

        [Space] [SerializeField] private bool drawGizmos = true;

        [SerializeField] private float centerRadius = .25f;

        public void Draw()
        {
            if (!drawGizmos)
            {
                return;
            }

            if (_cells != null)
            {
                // Draw centers
                foreach (Cell cell in _cells)
                {
                    Gizmos.color = Color.green;

                    Gizmos.DrawSphere(cell.Center, centerRadius);
                }

                // Draw bounding box
                Gizmos.color = Color.white;

                Gizmos.DrawLine(ProjectAndTranslate(_boundingRect.min), ProjectAndTranslate(new Vector3(_boundingRect.xMin, _boundingRect.yMax)));

                Gizmos.DrawLine(ProjectAndTranslate(_boundingRect.min), ProjectAndTranslate(new Vector3(_boundingRect.xMax, _boundingRect.yMin)));

                Gizmos.DrawLine(ProjectAndTranslate(_boundingRect.max), ProjectAndTranslate(new Vector3(_boundingRect.xMax, _boundingRect.yMin)));

                Gizmos.DrawLine(ProjectAndTranslate(_boundingRect.max), ProjectAndTranslate(new Vector3(_boundingRect.xMin, _boundingRect.yMax)));

                foreach (var cell in _cells)
                {
                    foreach (var segment in cell.Segments)
                    {
                        Gizmos.DrawLine(segment.Start, segment.End);
                    }
                }
            }
        }

        #endregion
    }
}
