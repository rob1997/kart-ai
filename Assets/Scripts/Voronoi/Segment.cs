using System;
using UnityEngine;

public class Segment
{
    public Vector2 Start { get; private set; }
    public Vector2 End { get; private set; }

    public Vector2 AsVector2 => End - Start;
    
    public Vector2 AsVector3 => new Vector3(AsVector2.x, AsVector2.y, 0f);

    public Vector2 Center { get; private set; }
    
    public Segment(Vector2 start, Vector2 end)
    {
        Start = start;
        
        End = end;
        
        Center = (start + end) / 2f;
    }

    public bool Equals(Segment other)
    {
        return Start == other.Start && End == other.End;
    }
}
