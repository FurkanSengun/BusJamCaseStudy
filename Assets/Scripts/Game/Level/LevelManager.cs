using System;
using System.Collections.Generic;
using System.Linq;
using Game.Bus;
using Game.Data;
using Game.Data.PlayerData;
using Game.Grid;
using Game.Queue;
using UnityEngine;
using Utils;
using Zenject;
using PassengerEntity = Game.Passenger.Passenger;
using ObstacleEntity = Game.Obstacle.Obstacle;

namespace Game.Level
{
    public class LevelManager : MonoBehaviour, ILevelManager
    {
        #region Constants
        private const float GridGap = 1.2f;
        private const float SurfaceLift = 0.01f;
        private const float BusSpacing = 10f;
        #endregion

        #region Inspector
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

        [Header("Load")]
        [SerializeField] private bool loadOnStart = false;
        #endregion

        #region Fields
        private readonly Dictionary<Vector2Int, GameObject> _groundLookup = new();

        [Inject] private IPlayerDataManager _playerDataManager;
        [Inject] private DiContainer _container;
        [Inject] private IQueueManager _queueManager;
        [Inject] private IGridManager _gridManager;
        [Inject] private IBusManager _busManager;
        #endregion

        #region Events
        public event Action<LevelData> OnLevelLoadStarted;
        public event Action<LevelData> OnLevelLoaded;
        public event Action OnLevelCleared;
        public event Action<string> OnLevelLoadFailed;
        #endregion

        #region Properties
        public LevelData LoadedLevelData { get; private set; }
        public bool IsLevelLoaded => LoadedLevelData != null;
        
        public int LoadedLevelIndex { get; private set; }
        public bool IsLastPlayableLevel => LoadedLevelIndex >= GetLastPlayableLevelIndex();

        public Transform GroundRoot => groundRoot;
        public Transform QueueRoot => queueRoot;
        public Transform BusRoot => busRoot;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (!loadOnStart)
            {
                return;
            }

            Load();
        }
        #endregion
        
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

            _gridManager?.Initialize(levelData.gridSize);

            SpawnGround(levelData);
            SpawnQueue(levelData);
            SpawnPassengers(levelData);
            SpawnObstacles(levelData);
            SpawnBuses(levelData);

            InitializeRuntimeManagers();

            OnLevelLoaded?.Invoke(levelData);
            return true;
        }

        private void InitializeRuntimeManagers()
        {
            _queueManager?.Initialize(queueRoot);
            _busManager?.Initialize(busRoot);
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

            int savedLevelIndex = _playerDataManager != null
                ? _playerDataManager.CurrentLevel
                : 0;

            int lastPlayableLevelIndex = GetLastPlayableLevelIndex();
            int clampedLevelIndex = Mathf.Clamp(savedLevelIndex, 0, lastPlayableLevelIndex);

            if (_playerDataManager != null && clampedLevelIndex != savedLevelIndex)
            {
                _playerDataManager.SetCurrentLevel(clampedLevelIndex);
            }

            LoadedLevelIndex = clampedLevelIndex;
            levelData = levelDatabase.GetLevel(clampedLevelIndex);

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
                    Vector3 localPosition = GridToLocalPosition(cellIndex, levelData);

                    GameObject instance = InstantiateInjectedPrefab(prefabRegistry.groundCellPrefab, groundRoot);
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

            QueueData queueData = levelData.queueSlots[0];

            GameObject queueContainer = new("Queue_0");
            queueContainer.transform.SetParent(queueRoot, false);
            queueContainer.transform.localPosition = queueData.rootLocalPosition;
            queueContainer.transform.localRotation = Quaternion.identity;
            queueContainer.transform.localScale = Vector3.one;

            for (int i = 0; i < queueData.localPositions.Count; i++)
            {
                GameObject slotObject = InstantiateInjectedPrefab(prefabRegistry.queueSlotPrefab, queueContainer.transform);
                slotObject.transform.localPosition = queueData.localPositions[i];
                slotObject.transform.localRotation = Quaternion.identity;
                slotObject.transform.localScale = Vector3.one;
                slotObject.name = $"QueueSlot_{i}";

                if (slotObject.TryGetComponent(out QueueSlot queueSlot))
                {
                    queueSlot.Initialize(i);
                }
            }
        }

        private void SpawnPassengers(LevelData levelData)
        {
            if (prefabRegistry.passengerPrefab == null || levelData.passengers == null)
            {
                return;
            }

            foreach (PassengerData passengerData in levelData.passengers)
            {
                if (!_groundLookup.TryGetValue(passengerData.gridIndex, out GameObject groundTile))
                {
                    continue;
                }

                GameObject instance = InstantiateInjectedPrefab(prefabRegistry.passengerPrefab, groundTile.transform);
                instance.transform.localPosition = passengerOffset;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                instance.name = $"Passenger_{passengerData.color}_{passengerData.gridIndex.x}_{passengerData.gridIndex.y}";

                LevelColorHandler.ApplyColor(instance, passengerData.color);
                LevelSnapHandler.SnapToCellSurface(instance.transform, groundTile.transform, SurfaceLift);

                if (instance.TryGetComponent(out PassengerEntity passenger))
                {
                    passenger.Initialize(passengerData.gridIndex, passengerData.color);
                }
            }
        }

        private void SpawnObstacles(LevelData levelData)
        {
            if (prefabRegistry.obstaclePrefab == null || levelData.obstacles == null)
            {
                return;
            }

            foreach (ObstacleData obstacleData in levelData.obstacles)
            {
                if (!_groundLookup.TryGetValue(obstacleData.gridIndex, out GameObject groundTile))
                {
                    continue;
                }

                GameObject instance = InstantiateInjectedPrefab(prefabRegistry.obstaclePrefab, groundTile.transform);
                instance.transform.localPosition = obstacleOffset;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                instance.name = $"Obstacle_{obstacleData.gridIndex.x}_{obstacleData.gridIndex.y}";

                LevelSnapHandler.SnapToCellSurface(instance.transform, groundTile.transform, SurfaceLift);

                if (instance.TryGetComponent(out ObstacleEntity obstacle))
                {
                    obstacle.Initialize(obstacleData.gridIndex);
                }
            }
        }

        private void SpawnBuses(LevelData levelData)
        {
            if (prefabRegistry.busPrefab == null || busRoot == null || busSpawnPoint == null || levelData.buses == null)
            {
                return;
            }

            List<BusData> orderedBuses = levelData.buses
                .OrderBy(bus => bus.order)
                .ToList();

            for (int i = 0; i < orderedBuses.Count; i++)
            {
                BusData busData = orderedBuses[i];

                Vector3 worldPosition =
                    busSpawnPoint.position
                    - busSpawnPoint.right * (i * BusSpacing)
                    + busOffset;

                Quaternion worldRotation = busSpawnPoint.rotation;

                GameObject instance = InstantiateInjectedPrefab(prefabRegistry.busPrefab, worldPosition, worldRotation, busRoot);
                instance.name = $"Bus_{busData.order}_{busData.colorType}";
                
                if (instance.TryGetComponent(out Game.Bus.Bus bus))
                {
                    bus.Initialize(busData.order, busData.colorType, busData.capacity);
                }
            }
        }
        #endregion

        #region Helpers
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
            _gridManager?.Clear();
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

        private GameObject InstantiateInjectedPrefab(GameObject prefab, Transform parent)
        {
            return _container.InstantiatePrefab(prefab, parent);
        }

        private GameObject InstantiateInjectedPrefab(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            return _container.InstantiatePrefab(prefab, position, rotation, parent);
        }
        
        private int GetLastPlayableLevelIndex()
        {
            if (levelDatabase == null || levelDatabase.levels == null || levelDatabase.levels.Count == 0)
            {
                return 0;
            }

            for (int i = levelDatabase.levels.Count - 1; i >= 0; i--)
            {
                if (levelDatabase.levels[i] != null)
                {
                    return i;
                }
            }

            return 0;
        }

        private void NotifyLoadFailed(string message)
        {
            DevLog.LogWarning(message);
            OnLevelLoadFailed?.Invoke(message);
        }
        #endregion
    }
}