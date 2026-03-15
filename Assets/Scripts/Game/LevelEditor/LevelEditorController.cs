using System.Collections.Generic;
using Game.Data;
using UnityEngine;
using UnityEditor;

namespace Game.LevelEditor
{
    [ExecuteAlways]
    public class LevelEditorController : MonoBehaviour
    {
        #region Constants
        
        private const float CellSizeConst = 1f;
        private const float GridGapConst = 1.2f;
        private const float QueueSlotSpacingConst = 2.5f;
        private const float BusSpacingConst = 10f;
        
        #endregion

        [Header("Data")] 
        [SerializeField] private LevelDatabase levelDatabase;
        [SerializeField] private LevelData targetLevelData;
        [SerializeField] private LevelPrefabRegistry prefabRegistry;

        [Header("Scene Roots")] 
        [SerializeField] private Transform gridRoot;
        [SerializeField] private Transform queueRoot;

        [Header("Grid")] 
        [SerializeField] private Vector2Int gridSize = new(6, 6);
        [SerializeField] private int queueSlotCount = 5;

        [Header("Spawn Offsets")] 
        [SerializeField] private Vector3 passengerOffset = Vector3.zero;
        [SerializeField] private Vector3 obstacleOffset = Vector3.zero;

        [Header("Level Settings")] 
        [SerializeField] private float timeLimit = 60f;

        [Header("Bus Preview")] 
        [SerializeField] private Transform busPreviewRoot;

        [SerializeField] private Transform busPreviewSpawnPoint;
        [SerializeField] private List<BusData> buses = new();

        [Header("Paint Tool")] 
        [SerializeField] private bool paintModeEnabled;

        [SerializeField] private CellPaintMode paintMode = CellPaintMode.Passenger;
        [SerializeField] private PassengerColorType paintPassengerColor = PassengerColorType.Red;

#if UNITY_EDITOR
        private bool _refreshQueued;
#endif

        #region Properties
        
        public LevelDatabase LevelDatabase => levelDatabase;
        public LevelData TargetLevelData => targetLevelData;
        public LevelPrefabRegistry PrefabRegistry => prefabRegistry;

        public Transform GridRoot => gridRoot;
        public Transform QueueRoot => queueRoot;
        public Transform BusPreviewRoot => busPreviewRoot;
        public Transform BusPreviewSpawnPoint => busPreviewSpawnPoint;

        public Vector2Int GridSize => gridSize;
        public int QueueSlotCount => queueSlotCount;
        public float TimeLimit => timeLimit;

        public Vector3 PassengerOffset => passengerOffset;
        public Vector3 ObstacleOffset => obstacleOffset;

        public List<BusData> Buses => buses;

        public bool PaintModeEnabled => paintModeEnabled;
        public CellPaintMode PaintMode => paintMode;
        public PassengerColorType PaintPassengerColor => paintPassengerColor;

        public float CellSize => CellSizeConst;
        public float GridGap => GridGapConst;
        public float GridStep => CellSizeConst + GridGapConst;
        public float QueueSlotSpacing => QueueSlotSpacingConst;
        public float BusSpacing => BusSpacingConst;
        
        #endregion

        public void SetTargetLevelData(LevelData levelData)
        {
            targetLevelData = levelData;
        }

        public void ApplyLoadedEditorState( Vector2Int loadedGridSize, int loadedQueueSlotCount, float loadedTimeLimit, List<BusData> loadedBuses)
        {
            gridSize = new Vector2Int( Mathf.Max(1, loadedGridSize.x), Mathf.Max(1, loadedGridSize.y));

            queueSlotCount = Mathf.Max(1, loadedQueueSlotCount);
            timeLimit = Mathf.Max(1f, loadedTimeLimit);

            buses = loadedBuses != null ? new List<BusData>(loadedBuses) : new List<BusData>();
        }

        private void OnEnable()
        {
            QueueRefresh();
        }

        private void OnValidate()
        {
            ClampValues();
            QueueRefresh();
        }

        private void ClampValues()
        {
            gridSize.x = Mathf.Max(1, gridSize.x);
            gridSize.y = Mathf.Max(1, gridSize.y);
            queueSlotCount = Mathf.Max(1, queueSlotCount);
            timeLimit = Mathf.Max(1f, timeLimit);

            buses ??= new List<BusData>();

            foreach (var t in buses)
            {
                t.capacity = Mathf.Max(1, t.capacity);
                t.order = Mathf.Max(0, t.order);
            }
        }

        public void SyncEditorVisuals()
        {
            if (Application.isPlaying)
            {
                return;
            }

#if UNITY_EDITOR
            LevelEditorSyncHandler.Sync(this);
#endif
        }

        public void LoadSelectedLevelToEditor()
        {
#if UNITY_EDITOR
            LevelEditorDataLoadHandler.LoadIntoController(this);
#endif
        }

        public void BakeSelectedLevel()
        {
#if UNITY_EDITOR
            LevelEditorBakeHandler.Bake(this);
#endif
        }

        public void ApplyBrushToCell(LevelCell cell)
        {
            if (cell == null)
            {
                return;
            }

            switch (paintMode)
            {
                case CellPaintMode.Empty:
                    cell.SetEmpty();
                    break;

                case CellPaintMode.Passenger:
                    cell.SetPassenger(paintPassengerColor);
                    break;

                case CellPaintMode.Obstacle:
                    cell.SetObstacle();
                    break;
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(cell);
#endif
        }

        public void ClearAllCells()
        {
            if (gridRoot == null)
            {
                return;
            }

            var cells = gridRoot.GetComponentsInChildren<LevelCell>(true);

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i].SetEmpty();

#if UNITY_EDITOR
                EditorUtility.SetDirty(cells[i]);
#endif
            }
        }

        private void QueueRefresh()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                return;
            }

            if (_refreshQueued)
            {
                return;
            }

            _refreshQueued = true;
            EditorApplication.delayCall += DelayedRefresh;
#endif
        }

#if UNITY_EDITOR
        private void DelayedRefresh()
        {
            EditorApplication.delayCall -= DelayedRefresh;
            _refreshQueued = false;

            if (this == null)
            {
                return;
            }

            SyncEditorVisuals();
        }
#endif
    }
    
    public enum CellPaintMode
    {
        Empty,
        Passenger,
        Obstacle
    }

}