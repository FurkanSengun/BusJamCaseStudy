using System;
using Core.SaveSystem;
using Game.Data.PlayerData;
using Game.GameManager;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.UI
{
    public class PlayButtonController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelText;
        
        [Inject] private IGameManager _gameManager;
        [Inject] private IPlayerDataManager _playerDataManager;

        private void Start()
        {
            int levelNumber = _playerDataManager.CurrentLevel + 1;
            _levelText.text = "Level " + levelNumber;
        }

        public void OnButtonClick()
        {
            _gameManager.StartGameplay();
        }
    }
}