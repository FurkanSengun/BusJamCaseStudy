using System.Threading.Tasks;
using Core.SceneManagement;
using Core.UI;
using Game.GameManager;

namespace Game.States
{
    public class MenuState : GameState
    {
        private readonly IUIManager _uiManager;
        private readonly ISceneManager _sceneManager;

        public MenuState(IGameManager gm, IUIManager uiManager, ISceneManager sceneManager) : base(gm)
        {
            _uiManager = uiManager;
            _sceneManager = sceneManager;
        }

        public override void Enter()
        {
            _sceneManager.OnSceneLoaded -= HandleSceneLoaded;
            _sceneManager.OnSceneLoaded += HandleSceneLoaded;

            _ = EnterAsync();
        }

        private async Task EnterAsync()
        {
            _uiManager.HideAllSceneUI();
            _uiManager.Show(UIType.Loading, true);

            await _sceneManager.LoadSceneWithTransitionAsync(SceneNames.MenuScene);
        }

        public override void Tick()
        {
        }

        public override void Exit()
        {
            _sceneManager.OnSceneLoaded -= HandleSceneLoaded;

            _uiManager.Hide(UIType.Menu);
        }

        private void HandleSceneLoaded(string sceneName)
        {
            if (sceneName != SceneNames.MenuScene) return;

            _sceneManager.OnSceneLoaded -= HandleSceneLoaded;
            OnMenuSceneLoaded();
        }

        private void OnMenuSceneLoaded()
        {
            _uiManager.Show(UIType.Menu);
        }
    }
}