using UnityEngine;

namespace Track
{
    public class RandomTrackGenerator : TrackGenerator<RandomPath>
    {
        [SerializeField] private int cellsCount = 10;

        [SerializeField] private float scale = 5;
        
        [SerializeField, Range(0f, 1f)] private float complexity = .5f;
        
        protected override RandomPath GetPath()
        {
            return new RandomPath(cellsCount, scale, complexity);
        }
    }
}