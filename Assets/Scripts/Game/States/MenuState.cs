using Cysharp.Threading.Tasks;
using Core.SceneManagement;
using Core.UI;
using Game.GameManager;
using UnityEngine;

namespace Game.States
{
    public class MenuState : GameState
    {
        private readonly IUIManager _uiManager;
        private readonly ISceneManager _sceneManager;

        public MenuState(IGameManager gameManager, IUIManager uiManager, ISceneManager sceneManager)
            : base(gameManager)
        {
            _uiManager = uiManager;
            _sceneManager = sceneManager;
        }

        public override void Enter()
        {
            Time.timeScale = 1f;

            string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (activeSceneName == SceneNames.MenuScene)
            {
                _uiManager.Show(UIType.Menu, true);
                return;
            }

            _sceneManager.OnSceneLoaded -= HandleSceneLoaded;
            _sceneManager.OnSceneLoaded += HandleSceneLoaded;

            EnterAsync().Forget();
        }

        private async UniTaskVoid EnterAsync()
        {
            _uiManager.HideAllSceneUI();
            await _sceneManager.LoadSceneWithTransitionAsync(SceneNames.MenuScene, useLoadingScreen: true);
        }

        public override void Exit()
        {
            _sceneManager.OnSceneLoaded -= HandleSceneLoaded;
            _uiManager.Hide(UIType.Menu);
        }

        private void HandleSceneLoaded(string sceneName)
        {
            if (sceneName != SceneNames.MenuScene)
            {
                return;
            }

            _sceneManager.OnSceneLoaded -= HandleSceneLoaded;
            _uiManager.Show(UIType.Menu, true);
        }
    }
}
