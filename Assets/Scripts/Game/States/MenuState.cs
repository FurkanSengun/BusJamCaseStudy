using Core.SceneManagement;
using Core.UI;
using Game.GameManager;
using UnityEngine;
using Zenject;

namespace Game.States
{
    public class MenuState : GameState
    {
        [Inject] private IUIManager _uiManager;
        [Inject] private ISceneManager _sceneManager;
        
        public MenuState(IGameManager gm) : base(gm) { }

        public override void Enter()
        {
            Debug.Log(_uiManager.GetType());
            //_uiManager.HideAllSceneUI();
            //_sceneManager.LoadSceneAsync(SceneNames.MenuScene);
            
        }

        public override void Tick()
        {
        }

        public override void Exit()
        {
            _uiManager.Hide(UIType.Menu);
        }

        private void OnMenuSceneLoaded()
        {
            _uiManager.Show(UIType.Menu, true);
        }
    }
}