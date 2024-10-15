using UnityEngine;

public class Intersection
{
    public Vector2 Point { get; private set; }
    
    public Segment Segment { get; private set; }

    public Intersection(Vector2 point, Segment segment)
    {
        Point = point;
        
        Segment = segment;
    }
}
