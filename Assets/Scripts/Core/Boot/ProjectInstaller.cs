using Core.SceneManagement;
using Core.UI;
using Game.GameManager;
using Zenject;

namespace Core.Boot
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IUIManager>().To<UIManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ISceneManager>().To<SceneManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<IGameManager>().To<GameManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}