using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cell
{
    public Vector2 Center { get; private set; }

    public Segment[] Segments { get; private set; }
    
    public Cell(Vector2 center)
    {
        Center = center;
        
        Segments = Array.Empty<Segment>();
    }
    
    public void CalculateSegments(Rect rect)
    {
        Segments = new Segment[4];
        
        Segment segment = Segments[0] = new Segment(rect.min, new Vector2(rect.xMin, rect.yMax));
        
        segment = Segments[1] = new Segment(segment.End, rect.max);
        
        segment = Segments[2] = new Segment(segment.End, new Vector2(rect.xMax, rect.yMin));
        
        Segments[3] = new Segment(segment.End, rect.min);
    }
    
    public IEnumerator CalculateSegments(Intersection[] intersections)
    {
        Segment bisector = GetBisectorSegment(intersections);

        Draw.Instance.DrawList.Add(delegate
        {
            Gizmos.color = Color.red;
            
            Gizmos.DrawLine(bisector.Start, bisector.End);
            
            Gizmos.color = Color.yellow;
            
            Gizmos.DrawSphere(bisector.End, 0.5f);
        });

        //yield return new WaitForSeconds(.025f);
        yield return 0;
        
        List<Segment> segments = new List<Segment>
        {
            bisector
        };
        
        var intersection = intersections.Single(i => i.Point == bisector.End);

        Segment segment = intersection.Segment;
        
        if (intersection.Point == bisector.End)
        {
            segment = new Segment(intersection.Point, segment.End);
        }
        
        else
        {
            segment = new Segment(segment.Start, intersection.Point);
        }
        
        
        segments.Add(segment);

        intersection = intersections.Single(i => i.Point == bisector.Start);
        
        while (segment.End != bisector.Start)
        {
            segment = Segments.Single(s => segment.End == s.Start);

            if (intersection.Segment.Equals(segment))
            {
                segment = new Segment(segment.Start, bisector.Start);
            }
            
            segments.Add(segment);
        }
        
        Segments = segments.ToArray();
    }
    
    public bool GetIntersections(Segment bisector, out Intersection[] intersections)
    {
        intersections = new Intersection[2];

        int index = 0;
        
        foreach (var segment in Segments)
        {
            if (GetIntersection(bisector, segment, out Intersection intersection))
            {
                // sometimes this happens
                if (intersections.Any(i => i != null && i.Point == intersection.Point))
                {
                    continue;
                }
                
                intersections[index] = intersection;
                
                index++;

                if (index == intersections.Length)
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    private bool GetIntersection(Segment bisector, Segment segment, out Intersection intersection)
    {
        intersection = default;

        // Segment AB
        Vector2 a = bisector.Start;
        Vector2 b = bisector.End;
        
        // Segment CD
        Vector2 c = segment.Start;
        Vector2 d = segment.End;

        float t = ((a.x - c.x) * (c.y - d.y) - (a.y - c.y) * (c.x - d.x)) / ((a.x - b.x) * (c.y - d.y) - (a.y - b.y) * (c.x - d.x));
        
        float u = - ((a.x - b.x) * (a.y - c.y) - (a.y - b.y) * (a.x - c.x)) / ((a.x - b.x) * (c.y - d.y) - (a.y - b.y) * (c.x - d.x));

        // Both t & u are between 0 and 1
        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            Vector2 point;
            
            point.x = a.x + t * (b.x - a.x);
        
            point.y = a.y + t * (b.y - a.y);

            intersection = new Intersection(point, segment);
            
            return true;
        }
        
        return false;
    }

    // Get the correct bisector segment from two intersections.
    public Segment GetBisectorSegment(Intersection[] intersections)
    {
        Segment segment = new Segment(intersections[0].Point, intersections[1].Point);

        return VerifyBisectorSegment(segment, intersections[1]) ? segment : new Segment(intersections[1].Point, intersections[0].Point);
    }
    
    // Check if bisector segment is correct.
    // Checks If Center is on the same side of the bisector.
    private bool VerifyBisectorSegment(Segment bisector, Intersection intersection)
    {
        Vector2 directionToCenter = Center - bisector.Start;

        var segment = new Segment(bisector.End, intersection.Segment.End);
        
        return Vector3.Cross(bisector.AsVector3, directionToCenter).normalized == Vector3.Cross(segment.AsVector3, Center - segment.Start).normalized;
    }
}
