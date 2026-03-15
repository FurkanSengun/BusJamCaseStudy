using System.Collections.Generic;
using UnityEngine;

namespace Core.UI
{
    /// <summary>
    /// Canvasları kapatıp açmamıza yarar
    /// </summary>
    public class UIManager : MonoBehaviour, IUIManager
    {
        private readonly Dictionary<UIType, GameObject> _map = new();

        public void Register(UIType type, GameObject canvas)
        {
            if (canvas == null) return;
            _map[type] = canvas;
        }

        public void Unregister(UIType type, GameObject canvas)
        {
            if (!_map.TryGetValue(type, out var existing)) return;
            if (existing == canvas) _map.Remove(type);
        }

        public bool TryGet(UIType type, out GameObject canvas)
        {
            return _map.TryGetValue(type, out canvas);
        }

        private GameObject Get(UIType type)
        {
            return _map.GetValueOrDefault(type);
        }

        public void Show(UIType type, bool hideOthers = false)
        {
            var target = Get(type);

            if (target == null)
                return;

            if (hideOthers)
            {
                foreach (var kv in new List<KeyValuePair<UIType, GameObject>>(_map))
                {
                    if (kv.Key == type) continue;
                    if (kv.Value != null) kv.Value.SetActive(false);
                }
            }

            target.SetActive(true);
        }

        public void Hide(UIType type)
        {
            var target = Get(type);
            if (target == null) return;
            target.SetActive(false);
        }

        public void HideAllSceneUI()
        {
            foreach (var kv in _map)
            {
                if (kv.Value != null)
                    kv.Value.SetActive(false);
            }
        }
    }
}