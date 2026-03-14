using Core.UI;
using Game.GameManager;

namespace Game.States
{
    public class LoseState : GameState
    {
        private readonly IUIManager _uiManager;

        public LoseState(IGameManager gameManager, IUIManager uiManager) : base(gameManager)
        {
            _uiManager = uiManager;
        }

        public override void Enter()
        {
            _uiManager.Show(UIType.Fail, true);
        }

        public override void Exit()
        {
            _uiManager.Hide(UIType.Fail);
        }
    }
}