using System.Collections.Generic;
using Game.Grid;
using UnityEngine;
using Zenject;

namespace Game.Navigation
{
    public class PathManager : MonoBehaviour, IPathManager
    {
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        [Inject] private IGridManager _gridManager;

        public bool CanReachFrontRow(Vector2Int startGridIndex)
        {
            return TryGetPathToFrontRow(startGridIndex, out _);
        }

        public bool TryGetPathToFrontRow(Vector2Int startGridIndex, out List<Vector2Int> path)
        {
            path = null;

            if (_gridManager == null || !_gridManager.IsInsideBounds(startGridIndex))
            {
                return false;
            }

            Queue<Vector2Int> openSet = new();
            Dictionary<Vector2Int, Vector2Int> cameFrom = new();
            HashSet<Vector2Int> visited = new();

            openSet.Enqueue(startGridIndex);
            visited.Add(startGridIndex);

            while (openSet.Count > 0)
            {
                Vector2Int current = openSet.Dequeue();

                if (current.y == 0)
                {
                    path = ReconstructPath(current, cameFrom);
                    return true;
                }

                for (int i = 0; i < Directions.Length; i++)
                {
                    Vector2Int next = current + Directions[i];

                    if (visited.Contains(next))
                    {
                        continue;
                    }

                    if (!_gridManager.IsInsideBounds(next))
                    {
                        continue;
                    }

                    bool canStep = next == startGridIndex || _gridManager.IsWalkable(next);

                    if (!canStep)
                    {
                        continue;
                    }

                    visited.Add(next);
                    cameFrom[next] = current;
                    openSet.Enqueue(next);
                }
            }

            return false;
        }

        private List<Vector2Int> ReconstructPath(Vector2Int goal, Dictionary<Vector2Int, Vector2Int> cameFrom)
        {
            List<Vector2Int> result = new() { goal };
            Vector2Int current = goal;

            while (cameFrom.TryGetValue(current, out Vector2Int previous))
            {
                result.Add(previous);
                current = previous;
            }

            result.Reverse();
            return result;
        }
    }
}