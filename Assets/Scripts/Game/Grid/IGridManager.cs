using System;
using UnityEngine;

namespace Game.Grid
{
    public interface IGridManager
    {
        event Action OnGridStateChanged;

        Vector2Int GridSize { get; }

        void Initialize(Vector2Int gridSize);
        void Clear();

        bool IsInsideBounds(Vector2Int gridIndex);
        bool IsBlocked(Vector2Int gridIndex);
        bool IsOccupied(Vector2Int gridIndex);
        bool IsWalkable(Vector2Int gridIndex);

        void SetBlocked(Vector2Int gridIndex, bool isBlocked);
        void Occupy(Vector2Int gridIndex);
        void Release(Vector2Int gridIndex);
    }
}