using System.Threading.Tasks;
using Core.SceneManagement;
using Core.UI;
using Game.Data;
using Game.GameManager;
using Game.Level;
using Game.Timer;
using Game.WinCondition;
using UnityEngine;
using Utils;
using Zenject;

namespace Game.States
{
    public class GameplayState : GameState
    {
        private readonly IUIManager _uiManager;
        private readonly ISceneManager _sceneManager;

        private ILevelManager _levelManager;
        private ITimeManager _timeManager;
        private IWinConditionTracker _winConditionTracker;

        public GameplayState( IGameManager gameManager, IUIManager uiManager, ISceneManager sceneManager) : base(gameManager)
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

            await _sceneManager.LoadSceneWithTransitionAsync(SceneNames.GameScene);
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
            var sceneContext = Object.FindFirstObjectByType<SceneContext>();

            if (sceneContext == null)
            {
                DevLog.LogError("SceneContext not found in GameScene.");
                return;
            }

            _levelManager = sceneContext.Container.Resolve<ILevelManager>();
            _timeManager = sceneContext.Container.Resolve<ITimeManager>();
            _winConditionTracker = sceneContext.Container.Resolve<IWinConditionTracker>();
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
        }

        private void HandleLevelLoaded(LevelData levelData)
        {
            int totalPassengerCount = levelData.passengers?.Count ?? 0;

            _winConditionTracker?.Initialize(totalPassengerCount);
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
    }
}