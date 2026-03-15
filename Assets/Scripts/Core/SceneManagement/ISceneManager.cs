using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Core.SceneManagement
{
    public interface ISceneManager
    {
        event Action<string> OnSceneLoaded;

        Task<bool> LoadSceneWithTransitionAsync(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            CancellationToken cancellationToken = default,
            bool useLoadingScreen = true);
        
    }
}