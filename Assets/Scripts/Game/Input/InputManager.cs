using System;
using UnityEngine;

namespace Game.Input
{
    public class InputManager : MonoBehaviour, IInputManager
    {
        [Header("References")]
        [SerializeField] private LayerMask clickableLayer;
        [SerializeField] private Camera mainCamera;

        public event Action OnPrimaryInputStarted;
        public event Action<RaycastHit> OnWorldHitDetected;

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (!HasPrimaryInputDown())
            {
                return;
            }

            OnPrimaryInputStarted?.Invoke();

            Vector3 screenPosition = GetInputPosition();
            ProcessRaycast(screenPosition);
        }

        private bool HasPrimaryInputDown()
        {
            return UnityEngine.Input.GetMouseButtonDown(0) ||
                   (UnityEngine.Input.touchCount > 0 &&
                    UnityEngine.Input.GetTouch(0).phase == TouchPhase.Began);
        }

        private Vector3 GetInputPosition()
        {
            if (UnityEngine.Input.touchCount > 0)
            {
                return UnityEngine.Input.GetTouch(0).position;
            }

            return UnityEngine.Input.mousePosition;
        }

        private void ProcessRaycast(Vector3 screenPosition)
        {
            if (mainCamera == null)
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, clickableLayer))
            {
                OnWorldHitDetected?.Invoke(hit);
            }
        }
    }
}