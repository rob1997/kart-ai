using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class VoronoiPlane
{
    [SerializeField] private int planeWidth = 10;
    
    [SerializeField] private int planeHeight = 10;
    
    [SerializeField] private float cellSize = 5f;
    
    [SerializeField] private float padding = 1f;
    
    public Cell[,] Cells { get; private set; }

    private Cell[] _cells;
    
    private Rect _boundingRect;
    
    public IEnumerator Generate()
    {
        Cells = new Cell[planeWidth, planeHeight];

        _cells = new Cell[planeWidth * planeHeight];
        
        for (int i = 0; i < planeWidth; i++)
        {
            for (int j = 0; j < planeHeight; j++)
            {
                Vector2 center = GetRandomCenter(i, j);

                while (_cells.Any(c => c != null && c.Center == center))
                {
                    center = GetRandomCenter(i, j);
                }
                
                Cells[i, j] = new Cell(center);

                _cells[GetIndex(i, j)] = Cells[i, j];
            }
        }

        _boundingRect = GetBoundingRect();

        for (int i = 0; i < Cells.Length; i++)
        {
            yield return CalculateSegments(i);
        }
    }

    private IEnumerator CalculateSegments(int index)
    {
        Cell cell = _cells[index];
        
        cell.CalculateSegments(_boundingRect);

        for (int i = 0; i < Cells.Length; i++)
        {
            Draw.Instance.DrawList.Clear();
            
            if (i == index)
            {
                continue;
            }
            
            Cell other = _cells[i];
            
            Segment connectingSegment = new Segment(cell.Center, other.Center);

            Vector2 bisectorDirection = Vector3.Cross(connectingSegment.AsVector3, Vector3.forward);

            Vector2 bisectorEnd = (bisectorDirection * 1000) + connectingSegment.Center;
            
            Segment bisector = new Segment(connectingSegment.Center - (bisectorDirection * 1000), bisectorEnd);

            Draw.Instance.DrawList.Add(delegate
            {
                Gizmos.color = Color.blue;
                
                Gizmos.DrawSphere(cell.Center, .5f);
                
                Debug.DrawLine(connectingSegment.Start, connectingSegment.End, Color.blue);
            });

            if (cell.GetIntersections(bisector, out Intersection[] intersections))
            {
                foreach (var intersection in intersections)
                {
                    Draw.Instance.DrawList.Add(delegate
                    {
                        Gizmos.color = Color.yellow;
                        
                        Gizmos.DrawSphere(intersection.Point, .25f);
                    });
                }

                yield return cell.CalculateSegments(intersections);
            }
        }
        
        _cells[index] = cell;
    }

    private int GetIndex(int i, int j)
    {
        if (planeWidth > planeHeight)
        {
            return (i * planeHeight) + j;
        }
        else
        {
            return i + (j * planeWidth);
        }
    }
    
    private Vector2 GetRandomCenter(int i, int j)
    {
        float x = i * cellSize;

        x += UnityEngine.Random.Range(0f, cellSize);
                
        float y = j * cellSize;
                
        y += UnityEngine.Random.Range(0f, cellSize);
        
        return new Vector2(x, y);
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
    
    #region Gizmo

    [Space]
    
    [SerializeField] private bool drawGizmos = true;
    
    [SerializeField] private float centerRadius = .25f;

    public void DrawPlane()
    {
        if (!drawGizmos)
        {
            return;
        }

        if (Cells != null)
        {
            // Draw centers
            foreach (Cell cell in Cells)
            {
                Gizmos.color = Color.green;
            
                Gizmos.DrawSphere(cell.Center, centerRadius);
            }
            
            // Draw bounding box
            Gizmos.color = Color.white;
            
            Gizmos.DrawLine(_boundingRect.min, new Vector2(_boundingRect.xMin, _boundingRect.yMax));
            
            Gizmos.DrawLine(_boundingRect.min, new Vector2(_boundingRect.xMax, _boundingRect.yMin));
            
            Gizmos.DrawLine(_boundingRect.max, new Vector2(_boundingRect.xMax, _boundingRect.yMin));
            
            Gizmos.DrawLine(_boundingRect.max, new Vector2(_boundingRect.xMin, _boundingRect.yMax));
            
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
