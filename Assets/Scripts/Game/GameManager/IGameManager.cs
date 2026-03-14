using Core.FSM;

namespace Game.GameManager
{
    public interface IGameManager
    {
        IState CurrentState { get; }

        void Init();
        void ChangeState(IState newState);

        void EnterMenu();
        void StartGameplay();
        void EnterLose();
    }
}