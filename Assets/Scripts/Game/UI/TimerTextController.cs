using Game.Timer;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.UI
{
    public class GameTimerTextController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;

        [Inject] private ITimeManager _timeManager;

        private void OnEnable()
        {
            if (_timeManager == null) return;
            
            _timeManager.OnTimeChanged += HandleTimeChanged;
            HandleTimeChanged(_timeManager.RemainingTime);
        }

        private void OnDisable()
        {
            if (_timeManager != null)
            {
                _timeManager.OnTimeChanged -= HandleTimeChanged;
            }
        }

        private void HandleTimeChanged(float remainingTime)
        {
            if (timerText == null)
            {
                return;
            }

            int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, remainingTime));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}