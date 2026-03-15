using System;
using Game.Data;
using Game.Queue;
using UnityEngine;

namespace Game.Passenger
{
    public interface IPassenger
    {
        Vector2Int GridIndex { get; }
        PassengerColorType ColorType { get; }
        bool IsInQueue { get; }
        bool IsReadyForBoarding { get; }

        void Initialize(Vector2Int gridIndex, PassengerColorType colorType);
        void MoveToQueueSlot(QueueSlot slot, Action onComplete = null);
        void BoardBus(Transform boardingPoint, Transform seatTransform, Action onComplete = null);
        void RelocateToQueueSlot(QueueSlot slot, Action onComplete = null);
    }
}