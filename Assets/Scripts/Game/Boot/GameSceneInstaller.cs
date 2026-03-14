using Game.Input;
using Game.Level;
using Game.Timer;
using Game.WinCondition;
using Zenject;

namespace Game.Boot
{
    public class GameSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IWinConditionTracker>().To<WinConditionTracker>().AsSingle();
            
            Container.Bind<ILevelManager>().To<LevelManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<IInputManager>().To<InputManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ITimeManager>().To<TimeManager>().FromComponentInHierarchy().AsSingle();

            

            // Sonraki adımlarda:
            // Container.Bind<IGridManager>().To<GridManager>().FromComponentInHierarchy().AsSingle();
            // Container.Bind<IBusManager>().To<BusManager>().FromComponentInHierarchy().AsSingle();
            // Container.Bind<IQueueManager>().To<QueueManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}