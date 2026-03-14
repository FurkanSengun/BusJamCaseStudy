using System;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Data.PlayerData;
using UnityEngine;
using Utils;
using Zenject;

namespace Game.Level
{
    public class LevelManager : MonoBehaviour, ILevelManager
    {
        #region Constants
        private const float GridGap = 1.2f;
        private const float SurfaceLift = 0.01f;
        private const float BusSpacing = 8f;
        #endregion

        [Header("Data")] 
        [SerializeField] private LevelDatabase levelDatabase;
        [SerializeField] private LevelPrefabRegistry prefabRegistry;

        [Header("Scene Roots")] 
        [SerializeField] private Transform groundRoot;

        [SerializeField] private Transform queueRoot;
        [SerializeField] private Transform busRoot;

        [Header("Bus Spawn")] 
        [SerializeField] private Transform busSpawnPoint;

        [Header("Offsets")] 
        
        [SerializeField] private Vector3 passengerOffset = Vector3.zero;
        [SerializeField] private Vector3 obstacleOffset = Vector3.zero;
        [SerializeField] private Vector3 busOffset = Vector3.zero;

        [Header("Load")] [SerializeField] private bool loadOnStart = false;

        private readonly Dictionary<Vector2Int, GameObject> _groundLookup = new();

        [Inject] private IPlayerDataManager _playerDataManager;

        public event Action<LevelData> OnLevelLoadStarted;
        public event Action<LevelData> OnLevelLoaded;
        public event Action OnLevelCleared;
        public event Action<string> OnLevelLoadFailed;

        public LevelData LoadedLevelData { get; private set; }
        public bool IsLevelLoaded => LoadedLevelData != null;

        public Transform GroundRoot => groundRoot;
        public Transform QueueRoot => queueRoot;
        public Transform BusRoot => busRoot;
        

        private void Start()
        {
            if (!loadOnStart)
            {
                return;
            }

            Load();
        }

        public bool Load()
        {
            if (!ValidateDependencies())
            {
                return false;
            }

            if (!TryResolveLevelData(out var levelData))
            {
                NotifyLoadFailed("LevelData couldn't found");
                return false;
            }

            return BuildLevel(levelData);
        }

        public void ReloadLoadedLevel()
        {
            if (LoadedLevelData == null)
            {
                Load();
                return;
            }

            BuildLevel(LoadedLevelData);
        }

        public void ClearLevel()
        {
            ResetLevelContent();
            LoadedLevelData = null;
            OnLevelCleared?.Invoke();
        }

        public bool TryGetGroundTile(Vector2Int gridIndex, out GameObject groundTile)
        {
            return _groundLookup.TryGetValue(gridIndex, out groundTile);
        }

        public IReadOnlyDictionary<Vector2Int, GameObject> GetGroundLookup()
        {
            return _groundLookup;
        }

        private bool BuildLevel(LevelData levelData)
        {
            if (levelData == null)
            {
                NotifyLoadFailed("LevelData is null");
                return false;
            }

            OnLevelLoadStarted?.Invoke(levelData);

            ResetLevelContent();
            LoadedLevelData = levelData;

            SpawnGround(levelData);
            SpawnQueue(levelData);
            SpawnPassengers(levelData);
            SpawnObstacles(levelData);
            SpawnBuses(levelData);

            OnLevelLoaded?.Invoke(levelData);

            return true;
        }

        private bool ValidateDependencies()
        {
            if (levelDatabase == null)
            {
                NotifyLoadFailed("LevelDatabase is not assigned.");
                return false;
            }

            if (prefabRegistry == null)
            {
                NotifyLoadFailed("LevelPrefabRegistry is not assigned.");
                return false;
            }

            return true;
        }

        private bool TryResolveLevelData(out LevelData levelData)
        {
            levelData = null;
            
            int currentLevelNumber = _playerDataManager != null
                ? _playerDataManager.CurrentLevel
                : 0;

            levelData = levelDatabase.GetLevel(currentLevelNumber);

            if (levelData != null)
            {
                return true;
            }

            levelData = GetFirstAvailableLevel();
            return levelData != null;
        }

        #region Spawn Prefabs
        private void SpawnGround(LevelData levelData)
        {
            if (prefabRegistry.groundCellPrefab == null || groundRoot == null)
            {
                return;
            }

            for (int y = 0; y < levelData.gridSize.y; y++)
            {
                for (int x = 0; x < levelData.gridSize.x; x++)
                {
                    Vector2Int cellIndex = new(x, y);
                    var localPosition = GridToLocalPosition(cellIndex, levelData);

                    var instance = Instantiate(prefabRegistry.groundCellPrefab, groundRoot);
                    instance.transform.localPosition = localPosition;
                    instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;
                    instance.name = $"Ground_{x}_{y}";

                    _groundLookup[cellIndex] = instance;
                }
            }
        }

        private void SpawnQueue(LevelData levelData)
        {
            if (prefabRegistry.queueSlotPrefab == null || queueRoot == null)
            {
                return;
            }

            if (levelData.queueSlots == null || levelData.queueSlots.Count == 0)
            {
                return;
            }

            var queueData = levelData.queueSlots[0];

            GameObject queueContainer = new("Queue_0");
            queueContainer.transform.SetParent(queueRoot, false);
            queueContainer.transform.localPosition = queueData.rootLocalPosition;
            queueContainer.transform.localRotation = Quaternion.identity;
            queueContainer.transform.localScale = Vector3.one;

            for (int i = 0; i < queueData.localPositions.Count; i++)
            {
                var slot = Instantiate(prefabRegistry.queueSlotPrefab, queueContainer.transform);
                slot.transform.localPosition = queueData.localPositions[i];
                slot.transform.localRotation = Quaternion.identity;
                slot.transform.localScale = Vector3.one;
                slot.name = $"QueueSlot_{i}";
            }
        }

        private void SpawnPassengers(LevelData levelData)
        {
            if (prefabRegistry.passengerPrefab == null || levelData.passengers == null)
            {
                return;
            }

            foreach (var passengerData in levelData.passengers)
            {
                if (!_groundLookup.TryGetValue(passengerData.gridIndex, out var groundTile))
                {
                    continue;
                }

                var instance = Instantiate(prefabRegistry.passengerPrefab, groundTile.transform);
                instance.transform.localPosition = passengerOffset;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                instance.name = $"Passenger_{passengerData.color}_{passengerData.gridIndex.x}_{passengerData.gridIndex.y}";

                LevelColorHandler.ApplyColor(instance, passengerData.color);
                LevelSnapHandler.SnapToCellSurface(instance.transform, groundTile.transform, SurfaceLift);
            }
        }

        private void SpawnObstacles(LevelData levelData)
        {
            if (prefabRegistry.obstaclePrefab == null || levelData.obstacles == null)
            {
                return;
            }

            foreach (var obstacleData in levelData.obstacles)
            {
                if (!_groundLookup.TryGetValue(obstacleData.gridIndex, out var groundTile))
                {
                    continue;
                }

                var instance = Instantiate(prefabRegistry.obstaclePrefab, groundTile.transform);
                instance.transform.localPosition = obstacleOffset;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                instance.name = $"Obstacle_{obstacleData.gridIndex.x}_{obstacleData.gridIndex.y}";

                LevelSnapHandler.SnapToCellSurface(instance.transform, groundTile.transform, SurfaceLift);
            }
        }

        private void SpawnBuses(LevelData levelData)
        {
            if (prefabRegistry.busPrefab == null || busRoot == null || busSpawnPoint == null || levelData.buses == null)
            {
                return;
            }

            var orderedBuses = levelData.buses
                .OrderBy(bus => bus.order)
                .ToList();

            for (int i = 0; i < orderedBuses.Count; i++)
            {
                var busData = orderedBuses[i];

                var worldPosition =
                    busSpawnPoint.position
                    - busSpawnPoint.right * (i * BusSpacing)
                    + busOffset;

                var worldRotation = busSpawnPoint.rotation;

                var instance = Instantiate(prefabRegistry.busPrefab, worldPosition, worldRotation, busRoot);
                instance.name = $"Bus_{busData.order}_{busData.colorType}";

                LevelColorHandler.ApplyColor(instance, busData.colorType);
            }
        }
        
        #endregion

        private Vector3 GridToLocalPosition(Vector2Int gridIndex, LevelData levelData)
        {
            float step = levelData.cellSize + GridGap;
            float width = (levelData.gridSize.x - 1) * step;
            float startX = -width * 0.5f;

            return new Vector3(
                startX + gridIndex.x * step,
                0f,
                -(gridIndex.y * step));
        }

        private LevelData GetFirstAvailableLevel()
        {
            if (levelDatabase.levels == null || levelDatabase.levels.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < levelDatabase.levels.Count; i++)
            {
                if (levelDatabase.levels[i] != null)
                {
                    return levelDatabase.levels[i];
                }
            }

            return null;
        }

        private void ResetLevelContent()
        {
            ClearChildren(groundRoot);
            ClearChildren(queueRoot);
            ClearChildren(busRoot);
            _groundLookup.Clear();
        }

        private void ClearChildren(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }

        private void NotifyLoadFailed(string message)
        {
            DevLog.LogWarning(message);
            OnLevelLoadFailed?.Invoke(message);
        }
    }
}