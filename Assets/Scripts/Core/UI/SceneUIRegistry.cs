using UnityEngine;
using Zenject;

namespace Core.UI
{
    public class SceneUIRegistry : MonoBehaviour
    {
        [SerializeField] private UIEntry[] _sceneEntries;

        [Inject] private IUIManager _uiManager;

        private bool _isRegistered;

        private void Start()
        {
            RegisterAll();
        }

        private void OnDisable()
        {
            UnregisterAll();
        }

        private void RegisterAll()
        {
            if (_uiManager == null || _isRegistered) return;

            foreach (var e in _sceneEntries)
            {
                if (e.UICanvas == null) continue;
                _uiManager.Register(e.UIType, e.UICanvas);
            }

            _isRegistered = true;
        }

        private void UnregisterAll()
        {
            if (_uiManager == null || !_isRegistered) return;

            foreach (var e in _sceneEntries)
            {
                if (e.UICanvas == null) continue;
                _uiManager.Unregister(e.UIType, e.UICanvas);
            }

            _isRegistered = false;
        }
    }
}