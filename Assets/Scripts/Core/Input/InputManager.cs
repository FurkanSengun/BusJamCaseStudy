using UnityEngine;
using UnityEngine.Events;

namespace Core.Input
{
    public class InputManager : MonoBehaviour, IInputManager
    {
        public UnityAction<Vector3> OnInputDetected { get; set; }

        [SerializeField] private LayerMask clickableLayer;
        [SerializeField]private Camera _mainCamera;

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0) || (UnityEngine.Input.touchCount > 0 && UnityEngine.Input.GetTouch(0).phase == TouchPhase.Began))
            {
                Vector3 screenPosition = GetInputPosition();
                ProcessRaycast(screenPosition);
            }
        }

        private Vector3 GetInputPosition()
        {
            if (UnityEngine.Input.touchCount > 0)
                return UnityEngine.Input.GetTouch(0).position;
        
            return UnityEngine.Input.mousePosition;
        }

        private void ProcessRaycast(Vector3 screenPos)
        {
            Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, clickableLayer))
            {
                OnInputDetected?.Invoke(hit.point);
            }
        }
    }
}