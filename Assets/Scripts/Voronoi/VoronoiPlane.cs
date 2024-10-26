using System;
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
        
        private float3[] _centers;

        private Rect _boundingRect;
        
        private float _diagonalDistance;

        private Vector3 _normal;
        
        private Vector3 _origin;
        
        public void Generate(Vector3 origin, Vector3 normal)
        {
            _normal = normal;
            
            _origin = origin;
            
            int arrayLength = planeWidth * planeHeight;
            
            NativeArray<float3> centers = new NativeArray<float3>(arrayLength, Allocator.TempJob);
            
            GetRandomCenterJob getRandomCenterJob = new GetRandomCenterJob
            {
                Centers = centers,
                
                Width = planeWidth,
                
                Height = planeHeight,
                
                CellSize = cellSize,
                
                Seed = Random.Range(0, int.MaxValue)
            };

            getRandomCenterJob.Schedule(arrayLength, 16).Complete();

            _boundingRect = GetBoundingRect();

            _diagonalDistance = math.distance(_boundingRect.Min, _boundingRect.Max);
            
            NativeList<Segment>[] segmentsArray = new NativeList<Segment>[arrayLength];
            
            NativeArray<JobHandle> allCellJobs = new NativeArray<JobHandle>(arrayLength, Allocator.Temp);
            
            for (int i = 0; i < arrayLength; i++)
            {
                segmentsArray[i] = new NativeList<Segment>(0, Allocator.TempJob);
                
                CellJob cellJob = new CellJob
                {
                    Centers = centers,
                    
                    Segments = segmentsArray[i],
                    
                    Index = i,
                    
                    BoundingRect = _boundingRect,
                    
                    DiagonalDistance = _diagonalDistance
                };
                
                allCellJobs[i] = cellJob.Schedule();
            }
            
            JobHandle.CompleteAll(allCellJobs);

            allCellJobs.Dispose();

            _cells = new Cell[arrayLength];
            
            for (int i = 0; i < arrayLength; i++)
            {
                _cells[i] = new Cell(centers[i], segmentsArray[i].AsArray().ToArray());

                segmentsArray[i].Dispose();
                
                Cell cell = _cells[i];
                
                _cells[i] = ProjectAndTranslate(cell);
            }

            centers.Dispose();
            
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

        private Rect GetBoundingRect()
        {
            float minX = - padding;
            float minY = - padding;

            float maxX = (planeWidth * cellSize) + padding;
            float maxY = (planeHeight * cellSize) + padding;

            return new Rect(new float2(minX, minY), new float2(maxX, maxY));
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

                Gizmos.DrawLine(ProjectAndTranslate(_boundingRect.Min), ProjectAndTranslate(new Vector3(_boundingRect.MinX, _boundingRect.MaxY)));

                Gizmos.DrawLine(ProjectAndTranslate(_boundingRect.Min), ProjectAndTranslate(new Vector3(_boundingRect.MaxX, _boundingRect.MinY)));

                Gizmos.DrawLine(ProjectAndTranslate(_boundingRect.Max), ProjectAndTranslate(new Vector3(_boundingRect.MaxX, _boundingRect.MinY)));

                Gizmos.DrawLine(ProjectAndTranslate(_boundingRect.Max), ProjectAndTranslate(new Vector3(_boundingRect.MinX, _boundingRect.MaxY)));

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
