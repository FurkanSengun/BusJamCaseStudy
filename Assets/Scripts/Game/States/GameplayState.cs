using System.Threading.Tasks;
using Core.SceneManagement;
using Core.Sound;
using Core.UI;
using Game.Bus;
using Game.GameManager;
using Game.Level;
using Game.Timer;
using Game.WinCondition;
using UnityEngine;
using Zenject;

namespace Game.States
{
    public class GameplayState : GameState
    {
        private readonly IUIManager _uiManager;
        private readonly ISceneManager _sceneManager;
        private readonly ISoundManager _soundManager;

        private ILevelManager _levelManager;
        private ITimeManager _timeManager;
        private IWinConditionTracker _winConditionTracker;
        private IBusManager _busManager;

        public GameplayState(
            IGameManager gameManager,
            IUIManager uiManager,
            ISceneManager sceneManager, ISoundManager soundManager) : base(gameManager)
        {
            _uiManager = uiManager;
            _sceneManager = sceneManager;
            _soundManager = soundManager;
        }

        public override void Enter()
        {
            Time.timeScale = 1;
            
            _sceneManager.OnSceneLoaded -= HandleSceneLoaded;
            _sceneManager.OnSceneLoaded += HandleSceneLoaded;

            _ = EnterAsync();
        }

        private async Task EnterAsync()
        {
            _uiManager.HideAllSceneUI();
            _uiManager.Show(UIType.Loading, true);

            bool started = await _sceneManager.LoadSceneWithTransitionAsync(SceneNames.GameScene, useLoadingScreen: true);

            if (!started)
            {
                _uiManager.Hide(UIType.Loading);
                _uiManager.Show(UIType.Menu, true);
            }
        }

        public override void Exit()
        {
            _sceneManager.OnSceneLoaded -= HandleSceneLoaded;
            UnsubscribeGameplayEvents();
        }

        private void HandleSceneLoaded(string sceneName)
        {
            if (sceneName != SceneNames.GameScene)
            {
                return;
            }

            _sceneManager.OnSceneLoaded -= HandleSceneLoaded;
            _ = InitializeGameplaySceneAsync();
        }

        private async Task InitializeGameplaySceneAsync()
        {
            await Task.Yield();

            ResolveSceneDependencies();
            SubscribeGameplayEvents();

            _uiManager.Show(UIType.Game, true);
            _levelManager?.Load();
        }

        private void ResolveSceneDependencies()
        {
            SceneContext sceneContext = Object.FindFirstObjectByType<SceneContext>();

            if (sceneContext == null)
            {
                Debug.LogError("SceneContext not found in GameScene.");
                return;
            }

            _levelManager = sceneContext.Container.Resolve<ILevelManager>();
            _timeManager = sceneContext.Container.Resolve<ITimeManager>();
            _winConditionTracker = sceneContext.Container.Resolve<IWinConditionTracker>();
            _busManager = sceneContext.Container.Resolve<IBusManager>();
        }

        private void SubscribeGameplayEvents()
        {
            if (_levelManager != null)
            {
                _levelManager.OnLevelLoaded -= HandleLevelLoaded;
                _levelManager.OnLevelLoaded += HandleLevelLoaded;
            }

            if (_timeManager != null)
            {
                _timeManager.OnTimeExpired -= HandleTimeExpired;
                _timeManager.OnTimeExpired += HandleTimeExpired;
            }

            if (_winConditionTracker != null)
            {
                _winConditionTracker.OnLevelCompleted -= HandleLevelCompleted;
                _winConditionTracker.OnLevelCompleted += HandleLevelCompleted;
            }
            
            if (_busManager != null)
            {
                _busManager.OnQueueFull -= HandleQueueFull;
                _busManager.OnQueueFull += HandleQueueFull;
            }
            
            
        }

        private void UnsubscribeGameplayEvents()
        {
            if (_levelManager != null)
            {
                _levelManager.OnLevelLoaded -= HandleLevelLoaded;
            }

            if (_timeManager != null)
            {
                _timeManager.OnTimeExpired -= HandleTimeExpired;
            }

            if (_winConditionTracker != null)
            {
                _winConditionTracker.OnLevelCompleted -= HandleLevelCompleted;
            }
            
            if (_busManager != null)
            {
                _busManager.OnQueueFull -= HandleQueueFull;
            }
        }

        private void HandleLevelLoaded(Game.Data.LevelData levelData)
        {
            int totalBusCount = levelData.buses != null
                ? levelData.buses.Count
                : 0;

            _winConditionTracker?.Initialize(totalBusCount);
            _timeManager?.Initialize(levelData.timeLimit);
        }

        private void HandleTimeExpired()
        {
            if (_winConditionTracker != null && _winConditionTracker.IsLevelCompleted)
            {
                return;
            }

            _gameManager.EnterLose();
        }

        private void HandleLevelCompleted()
        {
            _timeManager?.StopTimer();

            if (_levelManager != null && _levelManager.IsLastPlayableLevel)
            {
                _gameManager.ChangeState(new WinState(_gameManager, _uiManager, _soundManager));
                return;
            }
            _gameManager.CompleteCurrentLevel();
        }
        
        private void HandleQueueFull()
        {
            if (_winConditionTracker != null && _winConditionTracker.IsLevelCompleted)
            {
                return;
            }

            _gameManager.EnterLose();
        }
    }
}