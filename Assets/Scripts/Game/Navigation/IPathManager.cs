using System.Collections.Generic;
using UnityEngine;

namespace Game.Navigation
{
    public interface IPathManager
    {
        bool CanReachFrontRow(Vector2Int startIndex);
        bool TryGetPathToFrontRow(Vector2Int startGridIndex, out List<Vector2Int> path);

    }
}