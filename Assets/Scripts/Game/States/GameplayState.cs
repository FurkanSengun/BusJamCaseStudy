using Game.GameManager;

namespace Game.States
{
    public class GameplayState : GameState
    {
        
        private readonly int _levelIndex;

        public GameplayState(IGameManager gameManager, int levelIndex) : base(gameManager)
        {
            _levelIndex = levelIndex;
        }


        public override void Enter()
        {
        }

        public override void Tick()
        {
        }

        public override void Exit()
        {}
    }
}