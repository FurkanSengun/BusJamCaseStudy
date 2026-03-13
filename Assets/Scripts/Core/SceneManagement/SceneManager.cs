using System;
using System.Threading;
using System.Threading.Tasks;
using Core.UI;
using Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Core.SceneManagement
{
    public class SceneManager : MonoBehaviour, ISceneManager
    {
        public event Action<string, float> OnSceneLoadProgress;
        public event Action<string> OnSceneLoaded;
        public event Action<string> OnSceneUnloaded;

        [Inject] private IUIManager _uiManager;

        private bool _isTransitioning;

        public async Task LoadSceneWithTransitionAsync(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            CancellationToken cancellationToken = default)
        {
            if (_isTransitioning) return;

            _isTransitioning = true;

            LoadingView currentLoadingView = null;

            if (_uiManager.TryGet(UIType.Loading, out var currentLoadingCanvas) && currentLoadingCanvas != null)
            {
                _uiManager.Show(UIType.Loading, true);

                currentLoadingView = currentLoadingCanvas.GetComponent<LoadingView>();
                if (currentLoadingView != null)
                {
                    currentLoadingView.SetProgress(0f);
                    await currentLoadingView.FadeToBlackAsync(cancellationToken);
                }
            }

            await LoadSceneAsync(
                sceneName,
                mode,
                new Progress<float>(progress =>
                {
                    currentLoadingView?.SetProgress(progress);
                }),
                cancellationToken,
                true);

            if (_uiManager.TryGet(UIType.Loading, out var nextLoadingCanvas) && nextLoadingCanvas != null)
            {
                _uiManager.Show(UIType.Loading);

                var nextLoadingView = nextLoadingCanvas.GetComponent<LoadingView>();
                if (nextLoadingView != null)
                {
                    nextLoadingView.SetProgress(1f);
                    await nextLoadingView.FadeOutAndHideAsync(cancellationToken);
                }
                else
                {
                    _uiManager.Hide(UIType.Loading);
                }
            }

            _isTransitioning = false;
        }

        public async Task LoadSceneAsync(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default,
            bool allowSceneActivation = true)
        {
            var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);

            if (asyncOperation == null)
                return;

            asyncOperation.allowSceneActivation = allowSceneActivation;

            while (!asyncOperation.isDone)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    asyncOperation.allowSceneActivation = false;
                    _isTransitioning = false;
                    return;
                }

                float normalizedProgress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

                progress?.Report(normalizedProgress);
                OnSceneLoadProgress?.Invoke(sceneName, normalizedProgress);

                if (!allowSceneActivation && asyncOperation.progress >= 0.9f)
                {
                    progress?.Report(1f);
                    OnSceneLoadProgress?.Invoke(sceneName, 1f);
                    break;
                }

                await Task.Yield();
            }

            if (allowSceneActivation || asyncOperation.isDone)
            {
                // SceneUIRegistry Start içinde register ettiği için bir frame bekliyoruz
                await Task.Yield();
                OnSceneLoaded?.Invoke(sceneName);
            }
        }

        public async Task UnloadSceneAsync(string sceneName)
        {
            var asyncOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);

            if (asyncOperation == null)
                return;

            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            OnSceneUnloaded?.Invoke(sceneName);
        }

        public async Task ReloadActiveSceneAsync()
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            await LoadSceneWithTransitionAsync(currentSceneName);
        }
    }
}