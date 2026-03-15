using System;
using System.Collections.Generic;
using Game.Data;
using Game.Passenger;
using UnityEngine;

namespace Game.Queue
{
    public interface IQueueManager
    {
        event Action OnQueueChanged;

        bool IsFull { get; }

        void Initialize(Transform queueRoot);
        bool TryReserveNextSlot(out QueueSlot slot);
        bool TryGetFrontPassenger(out IPassenger passenger, out QueueSlot slot);

        bool TryGetBoardablePassengers(
            PassengerColorType colorType,
            int maxCount,
            out List<QueueBoardingCandidate> candidates);

        bool HasAnyBoardablePassenger(PassengerColorType colorType);

        void RemovePassengerFromQueue(QueueSlot slot);
        void RemovePassengersFromQueue(IReadOnlyList<QueueSlot> slots);

        void NotifyQueueChanged();
    }
}