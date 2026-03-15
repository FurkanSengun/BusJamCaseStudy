using System;
using Game.Data;
using Game.Passenger;
using UnityEngine;

public interface IBus
{
    event Action<IBus> OnReachedStopPoint;
    event Action<IBus> OnBecameFull;
    event Action<IBus> OnExited;

    int Order { get; }
    PassengerColorType ColorType { get; }
    int Capacity { get; }
    int OccupiedSeatCount { get; }
    int AvailableSeatCount { get; }

    bool IsAtStopPoint { get; }
    bool IsFull { get; }

    GameObject BusObject { get; }

    void Initialize(int order, PassengerColorType colorType, int capacity);
    void MoveToPosition(Vector3 worldPosition, float duration, Action onArrived = null);
    void MoveToStopPoint(Vector3 stopPosition, float duration);
    bool TryBoardPassenger(IPassenger passenger, Action onBoarded = null);
    void MoveToExitPoint(Vector3 exitPosition, float duration);
}