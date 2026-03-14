using System;

namespace Game.WinCondition
{
    public class WinConditionTracker : IWinConditionTracker
    {
        public event Action OnLevelCompleted;

        public bool IsLevelCompleted { get; private set; }
        public int TotalPassengerCount { get; private set; }
        public int BoardedPassengerCount { get; private set; }

        public void Initialize(int totalPassengerCount)
        {
            TotalPassengerCount = Math.Max(0, totalPassengerCount);
            BoardedPassengerCount = 0;
            IsLevelCompleted = false;

            CheckCompletion();
        }

        public void NotifyPassengerBoarded()
        {
            if (IsLevelCompleted)
            {
                return;
            }

            if (BoardedPassengerCount >= TotalPassengerCount)
            {
                return;
            }

            BoardedPassengerCount++;
            CheckCompletion();
        }

        public void ResetTracker()
        {
            TotalPassengerCount = 0;
            BoardedPassengerCount = 0;
            IsLevelCompleted = false;
        }

        private void CheckCompletion()
        {
            if (BoardedPassengerCount < TotalPassengerCount)
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