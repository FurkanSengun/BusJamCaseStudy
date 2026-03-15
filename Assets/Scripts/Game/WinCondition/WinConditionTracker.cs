using System;

namespace Game.WinCondition
{
    public class WinConditionTracker : IWinConditionTracker
    {
        public event Action OnLevelCompleted;

        public bool IsLevelCompleted { get; private set; }
        public int TotalBusCount { get; private set; }
        public int CompletedBusCount { get; private set; }

        public void Initialize(int totalBusCount)
        {
            TotalBusCount = Math.Max(0, totalBusCount);
            CompletedBusCount = 0;
            IsLevelCompleted = false;

            CheckCompletion();
        }

        public void NotifyBusCompleted()
        {
            if (IsLevelCompleted)
            {
                return;
            }

            if (CompletedBusCount >= TotalBusCount)
            {
                return;
            }

            CompletedBusCount++;
            CheckCompletion();
        }

        public void ResetTracker()
        {
            TotalBusCount = 0;
            CompletedBusCount = 0;
            IsLevelCompleted = false;
        }

        private void CheckCompletion()
        {
            if (CompletedBusCount < TotalBusCount)
            {
                return;
            }

            if (IsLevelCompleted)
            {
                return;
            }

            IsLevelCompleted = true;
            OnLevelCompleted?.Invoke();
        }
    }
}