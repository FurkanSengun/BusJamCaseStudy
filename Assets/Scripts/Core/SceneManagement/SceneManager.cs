using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.SceneManagement
{
    public class SceneManager : MonoBehaviour, ISceneManager
    {
        public event Action<string, float> OnSceneLoadProgress;
        public event Action<string> OnSceneLoaded;
        public event Action<string> OnSceneUnloaded;

        public async Task LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single,
            IProgress<float> progress = null, CancellationToken cancellationToken = default, bool allowSceneActivation = true)
        {
            var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);

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
                OnSceneLoaded?.Invoke(sceneName);
            }
        }

        public async Task UnloadSceneAsync(string sceneName)
        {
            var asyncOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);

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
            await LoadSceneAsync(currentSceneName);
        }

        public void Test()
        {
            Debug.Log("Test");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }
}