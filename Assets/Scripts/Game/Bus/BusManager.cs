using System;
using System.Collections.Generic;
using System.Linq;
using Core.Sound;
using Game.Passenger;
using Game.Pool;
using Game.Queue;
using Game.WinCondition;
using UnityEngine;
using Zenject;

namespace Game.Bus
{
    public class BusManager : MonoBehaviour, IBusManager
    {
        [Header("Points")]
        [SerializeField] private Transform stopPoint;
        [SerializeField] private Transform exitPoint;

        [Header("Motion")]
        [SerializeField] private float moveToStopDuration = 0.5f;
        [SerializeField] private float moveToExitDuration = 0.75f;
        
        [Header("Queue Layout")]
        [SerializeField] private float busGap = 10f;
        [SerializeField] private float queueShiftDuration = 0.35f;

        private readonly List<IBus> _buses = new();

        private IQueueManager _queueManager;
        private IWinConditionTracker _winConditionTracker;
        private IPoolManager _poolManager;
        private ISoundManager _soundManager;

        private int _currentBusIndex;

        public event Action OnQueueFull;

        [Inject]
        public void Construct( IQueueManager queueManager, IWinConditionTracker winConditionTracker, IPoolManager poolManager, ISoundManager soundManager)
        {
            _queueManager = queueManager;
            _winConditionTracker = winConditionTracker;
            _poolManager = poolManager;
            _soundManager = soundManager;
        }

        public void Initialize(Transform busRoot)
        {
            UnsubscribeAll();

            _buses.Clear();
            _currentBusIndex = 0;

            if (busRoot == null)
            {
                return;
            }

            Bus[] sceneBuses = busRoot.GetComponentsInChildren<Bus>(true);
            _buses.AddRange(sceneBuses.OrderBy(x => x.Order));

            for (int i = 0; i < _buses.Count; i++)
            {
                _buses[i].OnReachedStopPoint += HandleBusReachedStopPoint;
                _buses[i].OnBecameFull += HandleBusBecameFull;
                _buses[i].OnExited += HandleBusExited;
            }

            if (_queueManager != null)
            {
                _queueManager.OnQueueChanged -= HandleQueueChanged;
                _queueManager.OnQueueChanged += HandleQueueChanged;
            }

            MoveCurrentBusToStopPoint();
            RefreshQueuedBusPositions(true);
        }

        public bool TryBoardSelectedPassenger(IPassenger passenger)
        {
            IBus currentBus = GetCurrentBus();

            if (currentBus == null || !currentBus.IsAtStopPoint)
            {
                return false;
            }

            return currentBus.TryBoardPassenger(passenger, TryBoardFromQueue);
        }

        private void OnDestroy()
        {
            if (_queueManager != null)
            {
                _queueManager.OnQueueChanged -= HandleQueueChanged;
            }

            UnsubscribeAll();
        }

        private void HandleBusReachedStopPoint(IBus bus)
        {
            if (bus != GetCurrentBus())
            {
                return;
            }
            
            _soundManager?.PlayOneShot(SfxIds.BusHorn);
            TryBoardFromQueue();
            
        }

        private void HandleBusBecameFull(IBus bus)
        {
            if (bus != GetCurrentBus() || exitPoint == null)
            {
                return;
            }

            bus.MoveToExitPoint(exitPoint.position, moveToExitDuration);
        }

        private void HandleBusExited(IBus bus)
        {
            if (bus == null)
            {
                return;
            }

            _poolManager?.Release(bus.BusObject);
            _winConditionTracker?.NotifyBusCompleted();

            _currentBusIndex++;

            if (_currentBusIndex < _buses.Count)
            {
                RefreshQueuedBusPositions(true);
            }
        }

        private void HandleQueueChanged()
        {
            TryBoardFromQueue();
            CheckQueueLoseCondition();
        }

        private void MoveCurrentBusToStopPoint()
        {
            IBus currentBus = GetCurrentBus();

            if (currentBus == null || stopPoint == null)
            {
                return;
            }

            currentBus.MoveToStopPoint(stopPoint.position, moveToStopDuration);
        }

        private void TryBoardFromQueue()
        {
            IBus currentBus = GetCurrentBus();

            if (currentBus == null || !currentBus.IsAtStopPoint || currentBus.IsFull)
            {
                return;
            }

            if (_queueManager == null)
            {
                return;
            }

            int availableSeatCount = currentBus.AvailableSeatCount;

            if (availableSeatCount <= 0)
            {
                return;
            }

            if (!_queueManager.TryGetBoardablePassengers(
                    currentBus.ColorType,
                    availableSeatCount,
                    out List<QueueBoardingCandidate> candidates))
            {
                return;
            }

            List<QueueSlot> slotsToRemove = new();

            for (int i = 0; i < candidates.Count; i++)
            {
                slotsToRemove.Add(candidates[i].Slot);
            }

            _queueManager.RemovePassengersFromQueue(slotsToRemove);

            for (int i = 0; i < candidates.Count; i++)
            {
                currentBus.TryBoardPassenger(candidates[i].Passenger, TryBoardFromQueue);
            }
        }

        private void CheckQueueLoseCondition()
        {
            IBus currentBus = GetCurrentBus();

            if (currentBus == null || !currentBus.IsAtStopPoint || currentBus.IsFull)
            {
                return;
            }

            if (_queueManager == null || !_queueManager.IsFull)
            {
                return;
            }

            if (_queueManager.HasAnyBoardablePassenger(currentBus.ColorType))
            {
                return;
            }

            OnQueueFull?.Invoke();
        }

        private IBus GetCurrentBus()
        {
            if (_currentBusIndex < 0 || _currentBusIndex >= _buses.Count)
            {
                return null;
            }

            return _buses[_currentBusIndex];
        }

        private Vector3 GetQueuedBusPosition(int busIndex)
        {
            if (stopPoint == null)
            {
                return Vector3.zero;
            }
            
            int offsetFromCurrent = busIndex - _currentBusIndex;

            return stopPoint.position - stopPoint.right * (offsetFromCurrent * busGap);
        }
        private void RefreshQueuedBusPositions(bool includeCurrentBus)
        {
            for (int i = _currentBusIndex; i < _buses.Count; i++)
            {
                IBus bus = _buses[i];

                if (bus == null)
                {
                    continue;
                }

                if (!includeCurrentBus && i == _currentBusIndex)
                {
                    continue;
                }

                Vector3 targetPosition = GetQueuedBusPosition(i);

                if (i == _currentBusIndex)
                {
                    bus.MoveToStopPoint(targetPosition, moveToStopDuration);
                }
                else
                {
                    bus.MoveToPosition(targetPosition, queueShiftDuration);
                }
            }
        }

        private void UnsubscribeAll()
        {
            for (int i = 0; i < _buses.Count; i++)
            {
                _buses[i].OnReachedStopPoint -= HandleBusReachedStopPoint;
                _buses[i].OnBecameFull -= HandleBusBecameFull;
                _buses[i].OnExited -= HandleBusExited;
            }
        }
    }
}