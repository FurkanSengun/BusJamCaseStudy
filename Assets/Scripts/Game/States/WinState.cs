using Game.GameManager;

namespace Game.States
{
    public class WinState : GameState
    {
        protected readonly IGameManager _gameManager;

        protected WinState(IGameManager gameManager) : base(gameManager)
        {
            _gameManager = gameManager;
        }
        

        public virtual void Enter()
        {
            
        }
        public virtual void Tick()
        {
        }

        public virtual void Exit()
        {
            
        }
    }
}