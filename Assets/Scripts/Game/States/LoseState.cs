using Core.Sound;
using Core.UI;
using Game.GameManager;
using UnityEngine;

namespace Game.States
{
    public class LoseState : GameState
    {
        private readonly IUIManager _uiManager;
        private readonly ISoundManager _soundManager;

        public LoseState(IGameManager gameManager, IUIManager uiManager, ISoundManager soundManager) : base(gameManager)
        {
            _uiManager = uiManager;
            _soundManager = soundManager;
        }

        public override void Enter()
        {
            Time.timeScale = 1;
            _soundManager?.PlayOneShot(SfxIds.Lose);
            _uiManager.Show(UIType.Fail, true);
        }

        public override void Exit()
        {
            _uiManager.Hide(UIType.Fail);
        }
    }
}