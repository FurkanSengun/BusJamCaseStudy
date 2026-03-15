using UnityEngine;

namespace Game.Obstacle
{
    public interface IObstacle
    {
        Vector2Int GridIndex { get; }
        void Initialize(Vector2Int gridIndex);
    }
}