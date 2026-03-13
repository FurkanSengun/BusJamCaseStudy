using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class LoadingView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Slider _slider;
        [SerializeField] private float _fadeDuration = 0.25f;

        private void Awake()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            SetProgress(0f);
        }

        public void SetProgress(float value)
        {
            if (_slider == null) return;
            _slider.value = Mathf.Clamp01(value);
        }

        public async Task FadeToBlackAsync(CancellationToken cancellationToken = default)
        {
            if (_canvasGroup == null) return;

            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;

            await FadeAsync(_canvasGroup.alpha, 1f, cancellationToken);
        }

        public async Task FadeOutAndHideAsync(CancellationToken cancellationToken = default)
        {
            if (_canvasGroup == null) return;

            await FadeAsync(_canvasGroup.alpha, 0f, cancellationToken);

            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            gameObject.SetActive(false);
        }

        private async Task FadeAsync(float from, float to, CancellationToken cancellationToken)
        {
            float elapsed = 0f;
            _canvasGroup.alpha = from;

            while (elapsed < _fadeDuration)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _fadeDuration);
                _canvasGroup.alpha = Mathf.Lerp(from, to, t);

                await Task.Yield();
            }

            _canvasGroup.alpha = to;
        }
    }
}