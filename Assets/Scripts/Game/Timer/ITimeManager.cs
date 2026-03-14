using System;

namespace Game.Timer
{
    public interface ITimeManager
    {
        event Action OnTimerStarted;
        event Action<float> OnTimeChanged;
        event Action OnTimeExpired;

        float RemainingTime { get; }
        bool IsRunning { get; }

        void Initialize(float duration);
        void StartTimer();
        void StopTimer();
        void ResetTimer();
    }
}