#if UNITY_EDITOR
using System.Linq;
using Game.Data;
using UnityEngine;
using Utils;

namespace Game.LevelEditor
{
    /// <summary>
    /// Level Editor için gereken dataları yükleme görevi üstlenir
    /// </summary>
    public static class LevelEditorDataLoadHandler
    {
        public static void LoadIntoController(LevelEditorController controller)
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

            var levelData = controller.TargetLevelData;

            int loadedQueueSlotCount = 5;

            if (levelData.queueSlots != null && levelData.queueSlots.Count > 0)
            {
                loadedQueueSlotCount = Mathf.Max(1, levelData.queueSlots[0].localPositions.Count);
            }

            var loadedBuses = levelData.buses.OrderBy(bus => bus.order).Select(bus => new BusData
                { order = bus.order, colorType = bus.colorType, capacity = Mathf.Max(1, bus.capacity) }).ToList();

            controller.ApplyLoadedEditorState(levelData.gridSize, loadedQueueSlotCount, levelData.timeLimit, loadedBuses);

            LevelEditorSyncHandler.Sync(controller);
            LevelEditorSyncHandler.ApplyCellDataToGrid(controller);
            LevelEditorSyncHandler.ApplyQueueData(controller);
            LevelEditorSyncHandler.RefreshAllCellPreviews(controller);
        }
    }
}
#endif