using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Game.Passenger
{
    public class PassengerMovementController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float seatPopDuration = 0.15f;
        [SerializeField] private float seatPopStartScaleMultiplier = 0.75f;

        private Tween _moveTween;
        private Tween _scaleTween;
        private Vector3 _defaultScale;

        private void Awake()
        {
            _defaultScale = transform.localScale;
        }

        public void MoveTo(Vector3 worldPosition, Action onComplete = null)
        {
            _moveTween?.Kill();

            float distance = Vector3.Distance(transform.position, worldPosition);
            float duration = moveSpeed > 0f ? distance / moveSpeed : 0f;

            if (duration <= 0f)
            {
                transform.position = worldPosition;
                onComplete?.Invoke();
                return;
            }

            _moveTween = transform.DOMove(worldPosition, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void MoveAlongPath(IReadOnlyList<Vector3> worldPoints, Action onComplete = null)
        {
            _moveTween?.Kill();

            if (worldPoints == null || worldPoints.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            Sequence sequence = DOTween.Sequence();
            Vector3 current = transform.position;

            for (int i = 0; i < worldPoints.Count; i++)
            {
                float distance = Vector3.Distance(current, worldPoints[i]);
                float duration = moveSpeed > 0f ? distance / moveSpeed : 0f;

                if (duration > 0f)
                {
                    sequence.Append(transform.DOMove(worldPoints[i], duration).SetEase(Ease.Linear));
                }
                else
                {
                    transform.position = worldPoints[i];
                }

                current = worldPoints[i];
            }

            _moveTween = sequence.OnComplete(() => onComplete?.Invoke());
        }

        public void MoveToBoardingPointThenSeat(
            Vector3 boardingWorldPosition,
            Transform seatTransform,
            Action onComplete = null)
        {
            if (seatTransform == null)
            {
                onComplete?.Invoke();
                return;
            }

            MoveTo(boardingWorldPosition, () =>
            {
                transform.position = seatTransform.position;
                transform.SetParent(seatTransform, true);
                transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

                PlaySeatPopEffect(onComplete);
            });
        }

        private void PlaySeatPopEffect(Action onComplete)
        {
            _scaleTween?.Kill();

            Vector3 startScale = _defaultScale * seatPopStartScaleMultiplier;
            transform.localScale = startScale;

            _scaleTween = transform.DOScale(_defaultScale, seatPopDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        private void OnDestroy()
        {
            _moveTween?.Kill();
            _scaleTween?.Kill();
        }
    }
}