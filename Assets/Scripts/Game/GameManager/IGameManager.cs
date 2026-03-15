using Core.FSM;

namespace Game.GameManager
{
    public interface IGameManager
    {
        void Init();
        void ChangeState(IState newState);

        void EnterMenu();
        void StartGameplay();
        void EnterLose();
        void CompleteCurrentLevel();
    }
}