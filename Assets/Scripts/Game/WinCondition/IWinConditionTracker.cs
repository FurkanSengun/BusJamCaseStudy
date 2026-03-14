using System;

namespace Game.WinCondition
{
    public interface IWinConditionTracker
    {
        event Action OnLevelCompleted;

        bool IsLevelCompleted { get; }
        int TotalPassengerCount { get; }
        int BoardedPassengerCount { get; }

        void Initialize(int totalPassengerCount);
        void NotifyPassengerBoarded();
        void ResetTracker();
    }
}