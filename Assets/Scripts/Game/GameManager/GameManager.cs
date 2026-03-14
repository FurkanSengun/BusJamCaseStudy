using Core.FSM;
using Core.SceneManagement;
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

        public IState CurrentState => _stateMachine.CurrentState;

        [Inject]
        public void Construct(
            IUIManager uiManager,
            ISceneManager sceneManager,
            IPlayerDataManager playerDataManager)
        {
            _uiManager = uiManager;
            _sceneManager = sceneManager;
            _playerDataManager = playerDataManager;
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
            ChangeState(new GameplayState(this, _uiManager, _sceneManager));
        }

        public void EnterLose()
        {
            ChangeState(new LoseState(this, _uiManager));
        }
    }
}