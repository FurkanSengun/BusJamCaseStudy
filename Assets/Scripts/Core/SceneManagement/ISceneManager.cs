using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Core.SceneManagement
{
    public interface ISceneManager
    {
        event Action<string, float> OnSceneLoadProgress;

        event Action<string> OnSceneLoaded;

        event Action<string> OnSceneUnloaded;

        Task LoadSceneAsync(string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default,
            bool allowSceneActivation = true);

        Task UnloadSceneAsync(string sceneName);

        Task ReloadActiveSceneAsync();

        void Test();
    }
}