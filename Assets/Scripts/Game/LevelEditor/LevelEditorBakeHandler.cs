#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Game.LevelEditor
{
    /// <summary>
    /// Level Editor'de seviyeyi içerisindeki bütün veriyle kaydedebilmeye yarayan metotları içerir
    /// </summary>
    public static class LevelEditorBakeHandler
    {
        public static void Bake(LevelEditorController controller)
        {
            if (controller == null)
            {
                DevLog.LogError("LevelEditorController is null.");
                return;
            }

            if (controller.TargetLevelData == null)
            {
                DevLog.LogError("Target LevelData is null.");
                return;
            }

            LevelData targetLevelData = controller.TargetLevelData;

            EnsurePrefabLists(targetLevelData);
            ClearPrefabLists(targetLevelData);

            targetLevelData.gridSize = controller.GridSize;
            targetLevelData.cellSize = controller.CellSize;
            targetLevelData.timeLimit = controller.TimeLimit;

            BakeCells(controller, targetLevelData);
            BakeQueue(controller, targetLevelData);
            BakeBuses(controller, targetLevelData);

            EditorUtility.SetDirty(targetLevelData);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            DevLog.Log($"Level baked successfully: {targetLevelData.name}");
        }
        
        private static void EnsurePrefabLists(LevelData levelData)
        {
            levelData.cells ??= new List<CellData>();
            levelData.passengers ??= new List<PassengerData>();
            levelData.obstacles ??= new List<ObstacleData>();
            levelData.queueSlots ??= new List<QueueData>();
            levelData.buses ??= new List<BusData>();
        }
        
        private static void ClearPrefabLists(LevelData levelData)
        {
            levelData.cells.Clear();
            levelData.passengers.Clear();
            levelData.obstacles.Clear();
            levelData.queueSlots.Clear();
            levelData.buses.Clear();
        }
        
        #region Bake Lists
        
        private static void BakeCells(LevelEditorController controller, LevelData targetLevelData)
        {
            if (controller.GridRoot == null)
            {
                return;
            }

            LevelCell[] cells = controller.GridRoot.GetComponentsInChildren<LevelCell>(true);

            foreach (LevelCell cell in cells.OrderBy(cell => cell.GridIndex.y).ThenBy(cell => cell.GridIndex.x))
            {
                CellData cellData = new()
                {
                    gridIndex = cell.GridIndex,
                    cellType = cell.CellType,
                    passengerColor = cell.PassengerColor
                };

                targetLevelData.cells.Add(cellData);

                if (cell.CellType == CellType.Passenger)
                {
                    targetLevelData.passengers.Add(new PassengerData
                    {
                        gridIndex = cell.GridIndex,
                        color = cell.PassengerColor
                    });
                }
                else if (cell.CellType == CellType.Obstacle)
                {
                    targetLevelData.obstacles.Add(new ObstacleData
                    {
                        gridIndex = cell.GridIndex
                    });
                }
            }
        }
        
        private static void BakeQueue(LevelEditorController controller, LevelData targetLevelData)
        {
            if (controller.QueueRoot == null)
            {
                return;
            }

            QueueData queueData = new()
            {
                queueIndex = 0,
                rootLocalPosition = controller.QueueRoot.localPosition
            };

            List<Transform> orderedSlots = new();

            for (int i = 0; i < controller.QueueRoot.childCount; i++)
            {
                orderedSlots.Add(controller.QueueRoot.GetChild(i));
            }

            foreach (Transform slot in orderedSlots.OrderBy(slot => slot.localPosition.x))
            {
                queueData.localPositions.Add(slot.localPosition);
            }

            targetLevelData.queueSlots.Add(queueData);
        }
        
        private static void BakeBuses(LevelEditorController controller, LevelData targetLevelData)
        {
            List<BusData> sourceBuses = controller.Buses ?? new List<BusData>();

            for (int i = 0; i < sourceBuses.Count; i++)
            {
                BusData bus = sourceBuses[i];

                targetLevelData.buses.Add(new BusData
                {
                    order = i,
                    colorType = bus.colorType,
                    capacity = Mathf.Max(1, bus.capacity)
                });
            }
        }
        #endregion
    }
}
#endif
