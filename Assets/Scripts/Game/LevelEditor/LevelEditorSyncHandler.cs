#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>
    /// Level Editor'de Level ile alakalı verilerin senkronize olmasını sağlar
    /// </summary>
    public static class LevelEditorSyncHandler
    {
         public static void Sync(LevelEditorController controller)
        {
            if (controller == null || Application.isPlaying)
            {
                return;
            }

            SyncGrid(controller);
            SyncQueueSlots(controller);
            SyncBusPreview(controller);
            RefreshAllCellPreviews(controller);
        }

        public static void ApplyCellDataToGrid(LevelEditorController controller)
        {
            if (controller == null || controller.TargetLevelData == null || controller.GridRoot == null)
            {
                return;
            }

            Dictionary<Vector2Int, LevelCell> cellLookup = new();
            LevelCell[] cells = controller.GridRoot.GetComponentsInChildren<LevelCell>(true);

            for (int i = 0; i < cells.Length; i++)
            {
                if (!cellLookup.ContainsKey(cells[i].GridIndex))
                {
                    cellLookup.Add(cells[i].GridIndex, cells[i]);
                }
            }

            foreach (CellData cellData in controller.TargetLevelData.cells)
            {
                if (cellLookup.TryGetValue(cellData.gridIndex, out LevelCell levelCell))
                {
                    levelCell.SetContent(cellData.cellType, cellData.passengerColor);
                }
            }
        }

        public static void ApplyQueueData(LevelEditorController controller)
        {
            if (controller == null || controller.TargetLevelData == null || controller.QueueRoot == null)
            {
                return;
            }

            if (controller.TargetLevelData.queueSlots == null || controller.TargetLevelData.queueSlots.Count == 0)
            {
                SyncQueueSlots(controller);
                return;
            }

            QueueData queueData = controller.TargetLevelData.queueSlots[0];
            controller.QueueRoot.localPosition = queueData.rootLocalPosition;

            ClearChildren(controller.QueueRoot);

            for (int i = 0; i < queueData.localPositions.Count; i++)
            {
                GameObject slotObject = CreateSceneObject(
                    controller.PrefabRegistry != null ? controller.PrefabRegistry.queueSlotPrefab : null,
                    controller.QueueRoot,
                    "Create Queue Slot",
                    false);

                slotObject.name = $"QueueSlot_{i}";
                slotObject.transform.localPosition = queueData.localPositions[i];
                slotObject.transform.localRotation = Quaternion.identity;
                slotObject.transform.localScale = Vector3.one;
            }
        }

        public static void RefreshAllCellPreviews(LevelEditorController controller)
        {
            if (controller == null || controller.GridRoot == null)
            {
                return;
            }

            LevelCell[] cells = controller.GridRoot.GetComponentsInChildren<LevelCell>(true);

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i].SetOwner(controller);
                cells[i].RefreshPreview();
            }
        }

        private static void SyncGrid(LevelEditorController controller)
        {
            if (controller.GridRoot == null)
            {
                return;
            }

            Dictionary<Vector2Int, CellSnapshot> snapshots = CaptureCellSnapshots(controller);
            ClearChildren(controller.GridRoot);

            for (int y = 0; y < controller.GridSize.y; y++)
            {
                for (int x = 0; x < controller.GridSize.x; x++)
                {
                    Vector2Int gridIndex = new(x, y);

                    GameObject cellObject = CreateSceneObject(
                        controller.PrefabRegistry != null ? controller.PrefabRegistry.groundCellPrefab : null,
                        controller.GridRoot,
                        "Create Level Cell",
                        true);

                    cellObject.transform.localPosition = GetGridLocalPosition(controller, gridIndex);
                    cellObject.transform.localRotation = Quaternion.identity;
                    cellObject.transform.localScale = Vector3.one;

                    LevelCell levelCell = cellObject.GetComponent<LevelCell>();

                    if (levelCell == null)
                    {
                        levelCell = cellObject.AddComponent<LevelCell>();
                    }

                    levelCell.SetOwner(controller);
                    levelCell.SetGridIndex(gridIndex);

                    if (snapshots.TryGetValue(gridIndex, out CellSnapshot snapshot))
                    {
                        levelCell.SetContent(snapshot.cellType, snapshot.passengerColor);
                    }
                    else
                    {
                        levelCell.SetEmpty();
                    }
                }
            }
        }

        private static void SyncQueueSlots(LevelEditorController controller)
        {
            if (controller.QueueRoot == null)
            {
                return;
            }

            ClearChildren(controller.QueueRoot);

            float totalWidth = (controller.QueueSlotCount - 1) * controller.QueueSlotSpacing;
            float startX = -totalWidth * 0.5f;

            for (int i = 0; i < controller.QueueSlotCount; i++)
            {
                GameObject slotObject = CreateSceneObject(
                    controller.PrefabRegistry != null ? controller.PrefabRegistry.queueSlotPrefab : null,
                    controller.QueueRoot,
                    "Create Queue Slot",
                    false);

                slotObject.name = $"QueueSlot_{i}";
                slotObject.transform.localPosition = new Vector3(startX + i * controller.QueueSlotSpacing, 0f, 0f);
                slotObject.transform.localRotation = Quaternion.identity;
                slotObject.transform.localScale = Vector3.one;
            }
        }

        private static void SyncBusPreview(LevelEditorController controller)
        {
            if (controller.BusPreviewRoot == null ||
                controller.BusPreviewSpawnPoint == null ||
                controller.PrefabRegistry == null ||
                controller.PrefabRegistry.busPrefab == null)
            {
                return;
            }

            ClearChildren(controller.BusPreviewRoot);

            List<BusData> orderedBuses = controller.Buses
                .OrderBy(bus => bus.order)
                .ToList();

            for (int i = 0; i < orderedBuses.Count; i++)
            {
                BusData busData = orderedBuses[i];

                Vector3 worldPosition =
                    controller.BusPreviewSpawnPoint.position -
                    (controller.BusPreviewSpawnPoint.right * (i * controller.BusSpacing));

                Quaternion worldRotation = controller.BusPreviewSpawnPoint.rotation;

                GameObject instance = CreateSceneObject(
                    controller.PrefabRegistry.busPrefab,
                    controller.BusPreviewRoot,
                    "Create Bus Preview",
                    false);

                instance.transform.position = worldPosition;
                instance.transform.rotation = worldRotation;
                instance.transform.localScale = Vector3.one;
                instance.name = $"BusPreview_{busData.order}_{busData.colorType}";

                LevelEditorColorHandler.ApplyPreviewColor(instance, busData.colorType);
            }
        }

        private static Dictionary<Vector2Int, CellSnapshot> CaptureCellSnapshots(LevelEditorController controller)
        {
            Dictionary<Vector2Int, CellSnapshot> result = new();

            if (controller.GridRoot == null)
            {
                return result;
            }

            LevelCell[] cells = controller.GridRoot.GetComponentsInChildren<LevelCell>(true);

            for (int i = 0; i < cells.Length; i++)
            {
                result[cells[i].GridIndex] = new CellSnapshot
                {
                    cellType = cells[i].CellType,
                    passengerColor = cells[i].PassengerColor
                };
            }

            return result;
        }

        private static Vector3 GetGridLocalPosition(LevelEditorController controller, Vector2Int index)
        {
            float width = (controller.GridSize.x - 1) * controller.GridStep;
            float startX = -width * 0.5f;

            return new Vector3(
                startX + index.x * controller.GridStep,
                0f,
                -(index.y * controller.GridStep));
        }

        private static GameObject CreateSceneObject(GameObject prefab, Transform parent, string undoLabel, bool isGridCell)
        {
            if (!Application.isPlaying && prefab != null && PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;

                if (instance != null)
                {
                    Undo.RegisterCreatedObjectUndo(instance, undoLabel);
                    return instance;
                }
            }

            if (prefab != null)
            {
                GameObject instance = Object.Instantiate(prefab, parent);
                Undo.RegisterCreatedObjectUndo(instance, undoLabel);
                return instance;
            }

            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallback.transform.SetParent(parent, false);

            fallback.transform.localScale = isGridCell
                ? new Vector3(1f, 0.1f, 1f)
                : new Vector3(0.8f, 0.08f, 0.8f);

            Undo.RegisterCreatedObjectUndo(fallback, undoLabel);
            return fallback;
        }

        private static void ClearChildren(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                GameObject child = root.GetChild(i).gameObject;
                Undo.DestroyObjectImmediate(child);
            }
        }

        private struct CellSnapshot
        {
            public CellType cellType;
            public PassengerColorType passengerColor;
        }
    }
}
#endif