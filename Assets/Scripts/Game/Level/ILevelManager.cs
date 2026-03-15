using System;
using System.Collections.Generic;
using Game.Data;
using UnityEngine;

namespace Game.Level
{
    public interface ILevelManager
    {
        int LoadedLevelIndex { get; }
        bool IsLastPlayableLevel { get; }
        
        event Action<LevelData> OnLevelLoadStarted;
        event Action<LevelData> OnLevelLoaded;
        event Action OnLevelCleared;
        event Action<string> OnLevelLoadFailed;

        LevelData LoadedLevelData { get; }
        bool IsLevelLoaded { get; }

        Transform GroundRoot { get; }
        Transform QueueRoot { get; }
        Transform BusRoot { get; }

        bool Load();
        void ReloadLoadedLevel();
        void ClearLevel();

        bool TryGetGroundTile(Vector2Int gridIndex, out GameObject groundTile);
        IReadOnlyDictionary<Vector2Int, GameObject> GetGroundLookup();
    }
}