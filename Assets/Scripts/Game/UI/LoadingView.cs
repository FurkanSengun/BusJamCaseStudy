using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class LoadingView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider slider;

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private float fullBarHoldDuration = 0.2f;

        private void Awake()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            SetProgress(0f);
        }

        public void SetProgress(float normalizedValue)
        {
            if (slider == null)
            {
                return;
            }

            float clampedValue = Mathf.Clamp01(normalizedValue);
            slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, clampedValue);
        }

        public async UniTask FadeToBlackAsync(CancellationToken cancellationToken = default)
        {
            if (canvasGroup == null)
            {
                return;
            }

            gameObject.SetActive(true);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            await FadeAsync(canvasGroup.alpha, 1f, cancellationToken);
        }

        public async UniTask ShowCompletedAndHideAsync(CancellationToken cancellationToken = default)
        {
            SetProgress(1f);

            if (fullBarHoldDuration > 0f)
            {
                await UniTask.Delay((int)(fullBarHoldDuration * 1000f), cancellationToken: cancellationToken);
            }

            await FadeOutAndHideAsync(cancellationToken);
        }

        public async UniTask FadeOutAndHideAsync(CancellationToken cancellationToken = default)
        {
            if (canvasGroup == null)
            {
                return;
            }

            await FadeAsync(canvasGroup.alpha, 0f, cancellationToken);

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            gameObject.SetActive(false);
        }

        private async UniTask FadeAsync(float from, float to, CancellationToken cancellationToken)
        {
            float elapsed = 0f;
            canvasGroup.alpha = from;

            while (elapsed < fadeDuration)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(from, to, t);

                await UniTask.Yield(cancellationToken);
            }

            canvasGroup.alpha = to;
        }
    }
}
