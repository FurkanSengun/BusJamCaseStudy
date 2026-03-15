using Game.Data.PlayerData;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.UI
{
    public class SoundButtonController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image buttonBackground;
        [SerializeField] private Image soundIcon;

        [Header("Icons")]
        [SerializeField] private Sprite soundOnIcon;
        [SerializeField] private Sprite soundOffIcon;

        [Header("Colors")]
        [SerializeField] private Color soundOnColor = Color.green;
        [SerializeField] private Color soundOffColor = Color.gray;

        [Inject] private IPlayerDataManager _playerDataManager;

        
        private void Start()
        {
            RefreshVisual();
        }

        private void OnEnable()
        {
            if (_playerDataManager != null)
            {
                _playerDataManager.OnSoundStateChanged += HandleSoundStateChanged;
            }

            RefreshVisual();
        }
        
        private void OnDisable()
        {
            if (_playerDataManager != null)
            {
                _playerDataManager.OnSoundStateChanged -= HandleSoundStateChanged;
            }
        }

        public void OnButtonClick()
        {
            if (_playerDataManager == null)
            {
                return;
            }

            _playerDataManager.SetSound(!_playerDataManager.IsSoundOn);
            RefreshVisual();
        }

        private void HandleSoundStateChanged(bool _)
        {
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            if (_playerDataManager == null)
            {
                return;
            }

            bool isSoundOn = _playerDataManager.IsSoundOn;

            if (buttonBackground != null)
            {
                buttonBackground.color = isSoundOn ? soundOnColor : soundOffColor;
            }

            if (soundIcon != null)
            {
                soundIcon.sprite = isSoundOn ? soundOnIcon : soundOffIcon;
            }
        }
    }
}