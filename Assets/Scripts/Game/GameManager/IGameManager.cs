using Core.FSM;

namespace Game.GameManager
{
    public interface IGameManager
    {
        public void Init();
        public void ChangeState(IState newState);
    }
}