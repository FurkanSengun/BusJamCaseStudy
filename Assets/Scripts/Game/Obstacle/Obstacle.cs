using Game.Grid;
using UnityEngine;
using Zenject;

namespace Game.Obstacle
{
    public class Obstacle : MonoBehaviour, IObstacle
    {
        [Inject] private IGridManager _gridManager;

        public Vector2Int GridIndex { get; private set; }

        public void Initialize(Vector2Int gridIndex)
        {
            GridIndex = gridIndex;
            _gridManager?.SetBlocked(GridIndex, true);
        }
    }
}