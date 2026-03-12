using Core.FSM;
using Game.GameManager;

namespace Game.States
{
    public abstract class GameState : IState
    {
        protected readonly IGameManager _gameManager;

        protected GameState(IGameManager gameManager)
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