using System;
using DG.Tweening;
using UnityEngine;

namespace Game.Bus
{
    public class BusMovementController : MonoBehaviour
    {
        private Tween _moveTween;

        public void MoveTo(Vector3 worldPosition, float duration, Action onComplete = null)
        {
            _moveTween?.Kill();

            _moveTween = transform.DOMove(worldPosition, duration) .SetEase(Ease.Linear) .OnComplete(() => onComplete?.Invoke());
        }

        private void OnDestroy()
        {
            _moveTween?.Kill();
        }
    }
}