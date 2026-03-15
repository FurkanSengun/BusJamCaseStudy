using System;
using System.Collections;
using Game.Input;
using UnityEngine;
using Utils;
using Zenject;

namespace Game.Timer
{
    public class TimeManager : MonoBehaviour, ITimeManager
    {
        private IInputManager _inputManager;
        private Coroutine _timerCoroutine;

        private bool _isArmed;
        private float _initialDuration;

        public event Action OnTimerStarted;
        public event Action<float> OnTimeChanged;
        public event Action OnTimeExpired;

        public float RemainingTime { get; private set; }
        public bool IsRunning { get; private set; }

        [Inject]
        public void Construct(IInputManager inputManager)
        {
            if (_inputManager != null)
            {
                _inputManager.OnPrimaryInputStarted -= HandlePrimaryInputStarted;
            }

            _inputManager = inputManager;

            if (_inputManager != null)
            {
                _inputManager.OnPrimaryInputStarted += HandlePrimaryInputStarted;
            }
        }

        private void OnDestroy()
        {
            if (_inputManager != null)
            {
                _inputManager.OnPrimaryInputStarted -= HandlePrimaryInputStarted;
            }
        }

        public void Initialize(float duration)
        {
            StopTimerInternal();

            _initialDuration = Mathf.Max(1f, duration);
            RemainingTime = _initialDuration;
            IsRunning = false;
            _isArmed = true;

            OnTimeChanged?.Invoke(RemainingTime);
        }

        public void StartTimer()
        {
            if (IsRunning || !_isArmed || RemainingTime <= 0f)
            {
                return;
            }

            _isArmed = false;
            IsRunning = true;
            
            DevLog.Log("Starting timer");
            OnTimerStarted?.Invoke();
            _timerCoroutine = StartCoroutine(TimerRoutine());
        }

        public void StopTimer()
        {
            StopTimerInternal();
        }

        public void ResetTimer()
        {
            StopTimerInternal();

            if (_initialDuration <= 0f)
            {
                return;
            }

            RemainingTime = _initialDuration;
            IsRunning = false;
            _isArmed = true;

            OnTimeChanged?.Invoke(RemainingTime);
        }

        private void HandlePrimaryInputStarted()
        {
            if (!IsRunning && _isArmed)
            {
                StartTimer();
            }
        }

        private IEnumerator TimerRoutine()
        {
            while (RemainingTime > 0f)
            {
                RemainingTime -= Time.deltaTime;
                RemainingTime = Mathf.Max(0f, RemainingTime);

                OnTimeChanged?.Invoke(RemainingTime);
                yield return null;
            }

            _timerCoroutine = null;
            IsRunning = false;
            OnTimeExpired?.Invoke();
            DevLog.Log("Stopping timer");
        }

        private void StopTimerInternal()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }

            IsRunning = false;
        }
    }
}