using Game.Data;
using UnityEngine;
using UnityEditor;

namespace Game.LevelEditor
{
    [ExecuteAlways]
    public class LevelCell : MonoBehaviour
    {
        [SerializeField] private Vector2Int gridIndex;
        [SerializeField] private CellType cellType = CellType.Empty;
        [SerializeField] private PassengerColorType passengerColor = PassengerColorType.Red;

        [Header("Editor Preview")]
        [SerializeField] private Transform contentRoot;
        [SerializeField] private LevelEditorController owner;

        private const float SurfaceLift = 0.01f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public Vector2Int GridIndex => gridIndex;
        public CellType CellType => cellType;
        public PassengerColorType PassengerColor => passengerColor;

        public void Initialize(LevelEditorController controller, Vector2Int index)
        {
            owner = controller;
            gridIndex = index;
            gameObject.name = $"Cell_{gridIndex.x}_{gridIndex.y}";
            EnsureContentRoot();
            RefreshPreview();
        }

        public void SetGridIndex(Vector2Int index)
        {
            gridIndex = index;
            gameObject.name = $"Cell_{gridIndex.x}_{gridIndex.y}";
        }

        public void SetOwner(LevelEditorController controller)
        {
            owner = controller;
            EnsureContentRoot();
        }

        public void SetEmpty()
        {
            cellType = CellType.Empty;
            RefreshPreview();
        }

        public void SetObstacle()
        {
            cellType = CellType.Obstacle;
            RefreshPreview();
        }

        public void SetPassenger(PassengerColorType color)
        {
            cellType = CellType.Passenger;
            passengerColor = color;
            RefreshPreview();
        }

        public void SetContent(CellType type, PassengerColorType color = PassengerColorType.Red)
        {
            cellType = type;

            if (type == CellType.Passenger)
            {
                passengerColor = color;
            }

            RefreshPreview();
        }

        public void RefreshPreview()
        {
            EnsureContentRoot();
            ClearContentRoot();

            if (owner == null || owner.PrefabRegistry == null)
            {
                return;
            }

            if (cellType == CellType.Empty)
            {
                return;
            }

            GameObject prefab = cellType == CellType.Passenger
                ? owner.PrefabRegistry.passengerPrefab
                : owner.PrefabRegistry.obstaclePrefab;

            if (prefab == null)
            {
                return;
            }

            GameObject previewInstance = CreatePreviewInstance(prefab, contentRoot);
            previewInstance.name = $"{cellType}_Preview";

            previewInstance.transform.localPosition = Vector3.zero;
            previewInstance.transform.localRotation = Quaternion.identity;
            previewInstance.transform.localScale = Vector3.one;

            if (cellType == CellType.Passenger)
            {
                ApplyColor(previewInstance, passengerColor);
            }

            SnapPreviewToSurface(previewInstance.transform);
            ApplyPreviewOffset(previewInstance.transform);
        }

        private void OnValidate()
        {
            EnsureContentRoot();
            RefreshPreview();
        }

        private void Reset()
        {
            EnsureContentRoot();
            RefreshPreview();
        }

        private void EnsureContentRoot()
        {
            if (contentRoot != null)
            {
                return;
            }

            Transform existing = transform.Find("_EditorContentRoot");
            if (existing != null)
            {
                contentRoot = existing;
                return;
            }

            GameObject rootObject = new("_EditorContentRoot");
            rootObject.transform.SetParent(transform, false);
            rootObject.transform.localPosition = Vector3.zero;
            rootObject.transform.localRotation = Quaternion.identity;
            rootObject.transform.localScale = Vector3.one;
            contentRoot = rootObject.transform;
        }

        private void ClearContentRoot()
        {
            if (contentRoot == null)
            {
                return;
            }

            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                GameObject child = contentRoot.GetChild(i).gameObject;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(child);
                }
                else
                {
                    Destroy(child);
                }
#else
                Destroy(child);
#endif
            }
        }

        private GameObject CreatePreviewInstance(GameObject prefab, Transform parent)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
                if (instance != null)
                {
                    return instance;
                }
            }
#endif
            return Instantiate(prefab, parent);
        }

        private void ApplyColor(GameObject target, PassengerColorType colorType)
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            Color color = GetPassengerColor(colorType);

            for (int i = 0; i < renderers.Length; i++)
            {
                MaterialPropertyBlock block = new();
                renderers[i].GetPropertyBlock(block);
                block.SetColor(BaseColorId, color);
                block.SetColor(ColorId, color);
                renderers[i].SetPropertyBlock(block);
            }
        }

        private void ApplyPreviewOffset(Transform preview)
        {
            if (owner == null)
            {
                return;
            }

            Vector3 offset = cellType switch
            {
                CellType.Passenger => owner.PassengerOffset,
                CellType.Obstacle => owner.ObstacleOffset,
                _ => Vector3.zero
            };

            preview.localPosition += offset;
        }

        private void SnapPreviewToSurface(Transform preview)
        {
            float tileTopY = GetTopLocalYExcludingPreview();
            float previewBottomY = GetBottomLocalY(preview);

            Vector3 localPosition = preview.localPosition;
            localPosition.y += (tileTopY - previewBottomY) + SurfaceLift;
            preview.localPosition = localPosition;
        }

        private float GetTopLocalYExcludingPreview()
        {
            bool foundAny = false;
            float highestY = float.MinValue;

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (contentRoot != null && renderers[i].transform.IsChildOf(contentRoot))
                {
                    continue;
                }

                Bounds bounds = renderers[i].bounds;
                float localY = transform.InverseTransformPoint(
                    new Vector3(bounds.center.x, bounds.max.y, bounds.center.z)).y;

                highestY = Mathf.Max(highestY, localY);
                foundAny = true;
            }

            if (foundAny)
            {
                return highestY;
            }

            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (contentRoot != null && colliders[i].transform.IsChildOf(contentRoot))
                {
                    continue;
                }

                Bounds bounds = colliders[i].bounds;
                float localY = transform.InverseTransformPoint(
                    new Vector3(bounds.center.x, bounds.max.y, bounds.center.z)).y;

                highestY = Mathf.Max(highestY, localY);
                foundAny = true;
            }

            return foundAny ? highestY : 0f;
        }

        private float GetBottomLocalY(Transform targetRoot)
        {
            bool foundAny = false;
            float lowestY = float.MaxValue;

            Renderer[] renderers = targetRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Bounds bounds = renderers[i].bounds;
                float localY = transform.InverseTransformPoint(
                    new Vector3(bounds.center.x, bounds.min.y, bounds.center.z)).y;

                lowestY = Mathf.Min(lowestY, localY);
                foundAny = true;
            }

            if (foundAny)
            {
                return lowestY;
            }

            Collider[] colliders = targetRoot.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Bounds bounds = colliders[i].bounds;
                float localY = transform.InverseTransformPoint(
                    new Vector3(bounds.center.x, bounds.min.y, bounds.center.z)).y;

                lowestY = Mathf.Min(lowestY, localY);
                foundAny = true;
            }

            return foundAny ? lowestY : 0f;
        }

        private Color GetPassengerColor(PassengerColorType colorType)
        {
            return colorType switch
            {
                PassengerColorType.Red => new Color(0.9f, 0.25f, 0.25f, 1f),
                PassengerColorType.Green => new Color(0.25f, 0.75f, 0.3f, 1f),
                PassengerColorType.Blue => new Color(0.25f, 0.45f, 0.9f, 1f),
                PassengerColorType.Yellow => new Color(0.95f, 0.8f, 0.2f, 1f),
                PassengerColorType.Purple => new Color(0.65f, 0.35f, 0.85f, 1f),
                _ => Color.white
            };
        }
    }
}