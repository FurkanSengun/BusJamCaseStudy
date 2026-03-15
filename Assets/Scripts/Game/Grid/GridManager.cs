using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    ///     Init edilen gridleri kontrol eder. Dolu olup olmadığını, yürüyebilir olduğunu vs. kontrol eder
    /// </summary>
    public class GridManager : MonoBehaviour, IGridManager
    {
        private readonly HashSet<Vector2Int> _blockedTiles = new();
        private readonly HashSet<Vector2Int> _occupiedTiles = new();

        public event Action OnGridStateChanged;

        public Vector2Int GridSize { get; private set; }

        public void Initialize(Vector2Int gridSize)
        {
            GridSize = new Vector2Int( Mathf.Max(1, gridSize.x), Mathf.Max(1, gridSize.y));

            _blockedTiles.Clear();
            _occupiedTiles.Clear();

            OnGridStateChanged?.Invoke();
        }

        public void Clear()
        {
            GridSize = Vector2Int.zero;
            _blockedTiles.Clear();
            _occupiedTiles.Clear();

            OnGridStateChanged?.Invoke();
        }

        public bool IsInsideBounds(Vector2Int gridIndex)
        {
            return gridIndex.x >= 0 &&
                   gridIndex.y >= 0 &&
                   gridIndex.x < GridSize.x &&
                   gridIndex.y < GridSize.y;
        }

        public bool IsBlocked(Vector2Int gridIndex)
        {
            return _blockedTiles.Contains(gridIndex);
        }

        public bool IsOccupied(Vector2Int gridIndex)
        {
            return _occupiedTiles.Contains(gridIndex);
        }

        public bool IsWalkable(Vector2Int gridIndex)
        {
            return IsInsideBounds(gridIndex) &&
                   !IsBlocked(gridIndex) &&
                   !IsOccupied(gridIndex);
        }

        public void SetBlocked(Vector2Int gridIndex, bool isBlocked)
        {
            if (!IsInsideBounds(gridIndex))
            {
                return;
            }

            if (isBlocked)
            {
                _blockedTiles.Add(gridIndex);
            }
            else
            {
                _blockedTiles.Remove(gridIndex);
            }

            OnGridStateChanged?.Invoke();
        }

        public void Occupy(Vector2Int gridIndex)
        {
            if (!IsInsideBounds(gridIndex))
            {
                return;
            }

            _occupiedTiles.Add(gridIndex);
            OnGridStateChanged?.Invoke();
        }

        public void Release(Vector2Int gridIndex)
        {
            _occupiedTiles.Remove(gridIndex);
            OnGridStateChanged?.Invoke();
        }
    }
}