using DG.Tweening;
using Game.Data;
using Game.Level;
using UnityEngine;

namespace Game.Bus
{
    public class BusVisualController : MonoBehaviour
    {
        [Header("Color")]
        [SerializeField] private Renderer[] colorRenderers;

        [Header("Engine Idle")]
        [SerializeField] private Transform engineIdleTarget;
        [SerializeField] private float idleMoveAmount = 0.03f;
        [SerializeField] private float idleMoveDuration = 0.35f;

        private Tween _idleTween;
        private Vector3 _initialLocalPosition;

        private void Awake()
        {
            if (engineIdleTarget == null)
            {
                engineIdleTarget = transform;
            }

            _initialLocalPosition = engineIdleTarget.localPosition;
        }

        private void OnDisable()
        {
            StopEngineIdle();
        }

        public void ApplyBusColor(PassengerColorType colorType)
        {
            if (colorRenderers == null || colorRenderers.Length == 0)
            {
                return;
            }

            Color color = LevelColorHandler.GetColor(colorType);

            for (int i = 0; i < colorRenderers.Length; i++)
            {
                Renderer renderer = colorRenderers[i];

                if (renderer == null)
                {
                    continue;
                }

                MaterialPropertyBlock block = new();
                renderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor", color);
                block.SetColor("_Color", color);
                renderer.SetPropertyBlock(block);
            }
        }

        public void StartEngineIdle()
        {
            if (engineIdleTarget == null)
            {
                return;
            }

            StopEngineIdle();

            engineIdleTarget.localPosition = _initialLocalPosition;

            _idleTween = engineIdleTarget.DOLocalMoveY(
                    _initialLocalPosition.y + idleMoveAmount,
                    idleMoveDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void StopEngineIdle()
        {
            _idleTween?.Kill();
            _idleTween = null;

            if (engineIdleTarget != null)
            {
                engineIdleTarget.localPosition = _initialLocalPosition;
            }
        }
    }
}