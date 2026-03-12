using UnityEngine;
using Zenject;

namespace Core.UI
{
    public class SceneUIRegistry : MonoBehaviour
    {
        [SerializeField] private UIEntry[] _sceneEntries;

        [Inject] private UIManager _uiManager;

        private void OnEnable()
        {
            foreach (var e in _sceneEntries)
            {
                if (e.UICanvas == null) continue;
                _uiManager.Register(e.UIType, e.UICanvas);
            }
        }

        private void OnDisable()
        {
            foreach (var e in _sceneEntries)
            {
                if (e.UICanvas == null) continue;
                _uiManager.Unregister(e.UIType, e.UICanvas);
            }
        }
    }
}