using Core.SceneManagement;
using Game.GameManager;
using Game.States;
using UnityEngine;
using Zenject;

namespace Game.UI
{

    public class PlayButtonController : MonoBehaviour
    {
        [Inject] private ISceneManager _sceneManager;
        [Inject] private IGameManager _gameManager;
        
        public void OnButtonClick()
        {
            _gameManager.ChangeState(new GameplayState(_gameManager, 0));
            _sceneManager.LoadSceneWithTransitionAsync(SceneNames.GameScene);

        }

    }
}
