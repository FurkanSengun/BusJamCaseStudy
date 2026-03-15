using Game.GameManager;
using UnityEngine;
using Zenject;

namespace Game.UI
{
    public class MainMenuButtonController : MonoBehaviour
    {
        [Inject] private IGameManager _gameManager;

        public void OnButtonClick()
        {
            Time.timeScale = 1f;
            _gameManager.EnterMenu();
        }
    }
}