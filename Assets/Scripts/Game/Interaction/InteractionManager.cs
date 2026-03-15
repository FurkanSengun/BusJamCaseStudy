using System;
using Game.Input;
using UnityEngine;
using Zenject;

namespace Game.Interaction
{
    public class InteractionManager : MonoBehaviour, IInteractionManager
    {
        private IInputManager _inputManager;

        public event Action<IInteractable> OnInteractionDispatched;

        public bool IsInteractionEnabled { get; set; } = true;

        [Inject]
        public void Construct(IInputManager inputManager)
        {
            _inputManager = inputManager;
        }

        private void OnEnable()
        {
            if (_inputManager != null)
            {
                _inputManager.OnWorldHitDetected += HandleWorldHitDetected;
            }
        }

        private void OnDisable()
        {
            if (_inputManager != null)
            {
                _inputManager.OnWorldHitDetected -= HandleWorldHitDetected;
            }
        }

        private void HandleWorldHitDetected(RaycastHit hit)
        {
            if (!IsInteractionEnabled)
            {
                return;
            }

            IInteractable interactable = FindInteractable(hit.collider);

            if (interactable == null || !interactable.CanInteract)
            {
                return;
            }

            interactable.Interact();
            OnInteractionDispatched?.Invoke(interactable);
        }

        private IInteractable FindInteractable(Collider hitCollider)
        {
            if (hitCollider == null)
            {
                return null;
            }

            MonoBehaviour[] behaviours = hitCollider.GetComponentsInParent<MonoBehaviour>(true);

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IInteractable interactable)
                {
                    return interactable;
                }
            }

            return null;
        }
    }
}