using System;
using Core.Sound;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButtonSfxHandler : MonoBehaviour
    {
        private Button _button;
        
        [Inject] private ISoundManager _soundManager;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            _soundManager?.PlayOneShot(SfxIds.UiClick);
        }
    }
}