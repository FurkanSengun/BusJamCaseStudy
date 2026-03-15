using Core.FSM;
using Core.SceneManagement;
using Core.Sound;
using Core.UI;
using Game.Data.PlayerData;
using Game.States;
using UnityEngine;
using Zenject;

namespace Game.GameManager
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        private readonly StateMachine _stateMachine = new();

        private IUIManager _uiManager;
        private ISceneManager _sceneManager;
        private IPlayerDataManager _playerDataManager;
        private ISoundManager _soundManager;

        [Inject]
        public void Construct( IUIManager uiManager, ISceneManager sceneManager, IPlayerDataManager playerDataManager, ISoundManager soundManager)
        {
            _uiManager = uiManager;
            _sceneManager = sceneManager;
            _playerDataManager = playerDataManager;
            _soundManager = soundManager;
        }

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        public void Init()
        {
            _playerDataManager.Load();
            EnterMenu();
        }

        public void ChangeState(IState newState)
        {
            _stateMachine.ChangeState(newState);
        }

        public void EnterMenu()
        {
            ChangeState(new MenuState(this, _uiManager, _sceneManager));
        }

        public void StartGameplay()
        {
            ChangeState(new GameplayState(this, _uiManager, _sceneManager,  _soundManager));
        }

        public void EnterLose()
        {
            ChangeState(new LoseState(this, _uiManager, _soundManager));
        }

        public void CompleteCurrentLevel()
        {
            _playerDataManager.SetCurrentLevel(_playerDataManager.CurrentLevel + 1);
            ChangeState(new WinState(this, _uiManager, _soundManager));
        }
    }
}