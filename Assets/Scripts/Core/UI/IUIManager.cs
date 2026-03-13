using UnityEngine;

namespace Core.UI
{
    public interface IUIManager
    {
        void Register(UIType type, GameObject canvas);
        void Unregister(UIType type, GameObject canvas);
        void Show(UIType type, bool hideOthers = false);
        void Hide(UIType type);
        void HideAllSceneUI();
        bool TryGet(UIType type, out GameObject canvas);
    }
}