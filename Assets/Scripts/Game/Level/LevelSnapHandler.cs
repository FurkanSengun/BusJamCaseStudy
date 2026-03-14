using UnityEngine;

namespace Game.Level
{
    /// <summary>
    /// Yolcu ve engel gibi cell üzerinde spawnlanan objeleri görsel istikrar için cell üzerine snapleyebilmeye yarar
    /// </summary>
    public static class LevelSnapHandler
    {
        public static void SnapToCellSurface(Transform contentTransform, Transform groundTransform, float surfaceLift)
        {
            if (contentTransform == null || groundTransform == null)
            {
                return;
            }

            float groundTopY = GetTopWorldYExcludingTransform(groundTransform, contentTransform);
            float contentBottomY = GetBottomWorldY(contentTransform);

            var worldPosition = contentTransform.position;
            worldPosition.y += groundTopY - contentBottomY + surfaceLift;
            contentTransform.position = worldPosition;
        }

        private static float GetTopWorldYExcludingTransform(Transform root, Transform excludedSubtree)
        {
            bool foundAny = false;
            float highestY = float.MinValue;

            var renderers = root.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (excludedSubtree != null && renderers[i].transform.IsChildOf(excludedSubtree))
                {
                    continue;
                }

                highestY = Mathf.Max(highestY, renderers[i].bounds.max.y);
                foundAny = true;
            }

            if (foundAny)
            {
                return highestY;
            }

            var colliders = root.GetComponentsInChildren<Collider>(true);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (excludedSubtree != null && colliders[i].transform.IsChildOf(excludedSubtree))
                {
                    continue;
                }

                highestY = Mathf.Max(highestY, colliders[i].bounds.max.y);
                foundAny = true;
            }

            return foundAny ? highestY : root.position.y;
        }

        private static float GetBottomWorldY(Transform root)
        {
            bool foundAny = false;
            float lowestY = float.MaxValue;

            var renderers = root.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                lowestY = Mathf.Min(lowestY, renderers[i].bounds.min.y);
                foundAny = true;
            }

            if (foundAny)
            {
                return lowestY;
            }

            var colliders = root.GetComponentsInChildren<Collider>(true);

            for (int i = 0; i < colliders.Length; i++)
            {
                lowestY = Mathf.Min(lowestY, colliders[i].bounds.min.y);
                foundAny = true;
            }

            return foundAny ? lowestY : root.position.y;
        }
    }
}