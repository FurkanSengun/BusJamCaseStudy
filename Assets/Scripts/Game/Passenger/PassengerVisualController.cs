using Game.Data;
using Game.Level;
using UnityEngine;

namespace Game.Passenger
{
    public class PassengerVisualController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SkinnedMeshRenderer rendererTarget;
        [SerializeField] private Animator animator;

        private static readonly int OutlineEnabledId = Shader.PropertyToID("_OutlineEnabled");

        private static readonly int IsIdleId = Animator.StringToHash("isIdle");
        private static readonly int IsRunningId = Animator.StringToHash("isRunning");
        private static readonly int IsSittingId = Animator.StringToHash("isSitting");

        private MaterialPropertyBlock _block;

        private void Awake()
        {
            if (rendererTarget == null)
            {
                rendererTarget = GetComponentInChildren<SkinnedMeshRenderer>(true);
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            _block = new MaterialPropertyBlock();
        }

        public void ApplyColor(PassengerColorType colorType)
        {
            LevelColorHandler.ApplyColor(gameObject, colorType);
        }

        public void SetOutlineEnabled(bool isEnabled)
        {
            if (rendererTarget == null)
            {
                return;
            }

            float value = isEnabled ? 1f : 0f;

            rendererTarget.GetPropertyBlock(_block);
            _block.SetFloat(OutlineEnabledId, value);
            rendererTarget.SetPropertyBlock(_block);
        }

        public void PlayIdle()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(IsIdleId, true);
            animator.SetBool(IsRunningId, false);
            animator.SetBool(IsSittingId, false);
        }

        public void PlayRunning()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(IsIdleId, false);
            animator.SetBool(IsRunningId, true);
            animator.SetBool(IsSittingId, false);
        }

        public void PlaySitting()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(IsIdleId, false);
            animator.SetBool(IsRunningId, false);
            animator.SetBool(IsSittingId, true);
        }
    }
}