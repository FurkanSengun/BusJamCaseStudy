using System;
using System.Collections.Generic;
using Game.Data;
using Game.Passenger;
using UnityEngine;

namespace Game.Bus
{
    /// <summary>
    /// Otobüs yapay zekasını barındırır. Init olma, hareket etme ve yolcu bindirme mantığını barındırır
    /// </summary>
    public class Bus : MonoBehaviour, IBus
    {
        [Header("References")] 
        [SerializeField] private BusMovementController movementController;
        [SerializeField] private BusVisualController visualController;
        [SerializeField] private Transform boardingPoint;
        [SerializeField] private List<Transform> seatPoints = new();

        private readonly List<IPassenger> _seatedPassengers = new();
        private int _reservedSeatCount;

        public event Action<IBus> OnReachedStopPoint;
        public event Action<IBus> OnBecameFull;
        public event Action<IBus> OnExited;

        public int Order { get; private set; }
        public PassengerColorType ColorType { get; private set; }
        public int Capacity { get; private set; }
        public int OccupiedSeatCount => _seatedPassengers.Count;
        public int AvailableSeatCount => Mathf.Max(0, Capacity - (_seatedPassengers.Count + _reservedSeatCount));

        public bool IsAtStopPoint { get; private set; }
        public bool IsFull => OccupiedSeatCount >= Capacity;

        public GameObject BusObject => gameObject;

        private void Awake()
        {
            if (movementController == null)
            {
                movementController = GetComponent<BusMovementController>();
            }

            if (visualController == null)
            {
                visualController = GetComponent<BusVisualController>();
            }
        }

        public void Initialize(int order, PassengerColorType colorType, int capacity)
        {
            Order = Mathf.Max(0, order);
            ColorType = colorType;

            int seatCount = seatPoints != null ? seatPoints.Count : 0;
            Capacity = Mathf.Clamp(capacity, 1, Mathf.Max(1, seatCount));

            _seatedPassengers.Clear();
            _reservedSeatCount = 0;
            IsAtStopPoint = false;

            visualController?.ApplyBusColor(colorType);
            visualController?.StartEngineIdle();
        }

        public void MoveToPosition(Vector3 worldPosition, float duration, Action onArrived = null)
        {
            if (movementController == null)
            {
                transform.position = worldPosition;
                onArrived?.Invoke();
                return;
            }

            movementController.MoveTo(worldPosition, duration, onArrived);
        }


        public void MoveToStopPoint(Vector3 stopPosition, float duration)
        {
            IsAtStopPoint = false;

            MoveToPosition(stopPosition, duration, () =>
            {
                IsAtStopPoint = true;
                OnReachedStopPoint?.Invoke(this);
            });
        }

        public bool TryBoardPassenger(IPassenger passenger, Action onBoarded = null)
        {
            if (passenger == null || !IsAtStopPoint || IsFull)
            {
                return false;
            }

            if (passenger.ColorType != ColorType)
            {
                return false;
            }

            if (AvailableSeatCount <= 0)
            {
                return false;
            }

            int seatIndex = _seatedPassengers.Count + _reservedSeatCount;

            if (seatIndex < 0 || seatIndex >= seatPoints.Count || seatIndex >= Capacity)
            {
                return false;
            }

            var seat = seatPoints[seatIndex];
            _reservedSeatCount++;


            passenger.BoardBus(boardingPoint, seat, () =>
            {
                _reservedSeatCount = Mathf.Max(0, _reservedSeatCount - 1);
                _seatedPassengers.Add(passenger);

                onBoarded?.Invoke();

                if (IsFull)
                {
                    OnBecameFull?.Invoke(this);
                }
            });

            return true;
        }

        public void MoveToExitPoint(Vector3 exitPosition, float duration)
        {
            IsAtStopPoint = false;

            if (movementController == null)
            {
                transform.position = exitPosition;
                OnExited?.Invoke(this);
                return;
            }

            movementController.MoveTo(exitPosition, duration, () => { OnExited?.Invoke(this); });
        }
    }
}