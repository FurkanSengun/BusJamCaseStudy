using Game.GameManager;
using UnityEngine;
using Zenject;

namespace Game.UI
{
    public class PlayButtonController : MonoBehaviour
    {
        [Inject] private IGameManager _gameManager;

        public void OnButtonClick()
        {
            _gameManager.StartGameplay();
        }
    }
}