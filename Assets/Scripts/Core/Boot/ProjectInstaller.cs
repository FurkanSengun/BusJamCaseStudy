using Core.PlayerData;
using Core.SaveSystem;
using Core.SceneManagement;
using Core.Sound;
using Core.UI;
using Game.Data;
using Game.GameManager;
using Zenject;

namespace Core.Boot
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ISaveManager>().To<SaveManager>().AsSingle();
            Container.Bind<IPlayerDataManager>().To<PlayerDataManager>().AsSingle();
            
            Container.Bind<IUIManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ISceneManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<IGameManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ISoundManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}