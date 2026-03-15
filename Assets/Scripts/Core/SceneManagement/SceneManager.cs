using System;
using System.Threading;
using System.Threading.Tasks;
using Core.UI;
using Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
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

        public async Task<bool> LoadSceneWithTransitionAsync( string sceneName, LoadSceneMode mode = LoadSceneMode.Single, CancellationToken cancellationToken = default, bool useLoadingScreen = true)
        {
            if (_isTransitioning)
            {
                DevLog.LogWarning($"Scene transition blocked. Already transitioning while trying to load: {sceneName}");
                return false;
            }

            _isTransitioning = true;

            try
            {
                LoadingView currentLoadingView = null;

                if (useLoadingScreen && TryGetLoadingView(out _, out currentLoadingView))
                {
                    currentLoadingView.gameObject.SetActive(true);
                    currentLoadingView.SetProgress(0f);
                    await currentLoadingView.FadeToBlackAsync(cancellationToken);
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

                if (useLoadingScreen && TryGetLoadingView(out GameObject nextLoadingCanvas, out LoadingView nextLoadingView))
                {
                    nextLoadingCanvas.SetActive(true);
                    nextLoadingView.SetProgress(1f);
                    await nextLoadingView.ShowCompletedAndHideAsync(cancellationToken);
                }

                return true;
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public async Task LoadSceneAsync(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default,
            bool allowSceneActivation = true)
        {
            AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);

            if (asyncOperation == null)
            {
                return;
            }

            asyncOperation.allowSceneActivation = allowSceneActivation;

            while (!asyncOperation.isDone)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    asyncOperation.allowSceneActivation = false;
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
                await Task.Yield();
                OnSceneLoaded?.Invoke(sceneName);
            }
        }

        public async Task UnloadSceneAsync(string sceneName)
        {
            AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);

            if (asyncOperation == null)
            {
                return;
            }

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

        private bool TryGetLoadingView(out GameObject loadingCanvas, out LoadingView loadingView)
        {
            loadingCanvas = null;
            loadingView = null;

            if (_uiManager == null)
            {
                return false;
            }

            if (!_uiManager.TryGet(UIType.Loading, out loadingCanvas) || loadingCanvas == null)
            {
                return false;
            }

            loadingView = loadingCanvas.GetComponent<LoadingView>();
            return loadingView != null;
        }
    }
}