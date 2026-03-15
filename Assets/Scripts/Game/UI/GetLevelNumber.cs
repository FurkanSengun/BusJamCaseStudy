using Game.Data.PlayerData;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.UI
{
    public class GetLevelNumber : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelNumberText;
        
        [Inject] private IPlayerDataManager _playerDataManager;

        private void Start()
        {
            int levelNumber = _playerDataManager.CurrentLevel + 1;
            _levelNumberText.text = levelNumber.ToString();
        }
    }
}
