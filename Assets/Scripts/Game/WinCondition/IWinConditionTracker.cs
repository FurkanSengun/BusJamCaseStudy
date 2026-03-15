using System;

namespace Game.WinCondition
{
    public interface IWinConditionTracker
    {
        event Action OnLevelCompleted;

        bool IsLevelCompleted { get; }
        int TotalBusCount { get; }
        int CompletedBusCount { get; }

        void Initialize(int totalBusCount);
        void NotifyBusCompleted();
        void ResetTracker();
    }
}