using System;
using System.Collections.Generic;
using Game.Data;
using Game.Passenger;
using UnityEngine;

namespace Game.Queue
{
    public class QueueManager : MonoBehaviour, IQueueManager
    {
        private readonly List<QueueSlot> _slots = new();

        public event Action OnQueueChanged;

        public bool IsFull
        {
            get
            {
                for (int i = 0; i < _slots.Count; i++)
                {
                    if (!_slots[i].IsOccupied)
                    {
                        return false;
                    }
                }

                return _slots.Count > 0;
            }
        }

        public void Initialize(Transform queueRoot)
        {
            _slots.Clear();

            if (queueRoot == null)
            {
                return;
            }

            QueueSlot[] slots = queueRoot.GetComponentsInChildren<QueueSlot>(true);
            _slots.AddRange(slots);
            _slots.Sort((a, b) => a.Order.CompareTo(b.Order));

            NotifyQueueChanged();
        }

        public bool TryReserveNextSlot(out QueueSlot slot)
        {
            slot = null;

            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsOccupied)
                {
                    continue;
                }

                slot = _slots[i];
                return true;
            }

            return false;
        }

        public bool TryGetFrontPassenger(out IPassenger passenger, out QueueSlot slot)
        {
            passenger = null;
            slot = null;

            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].IsOccupied)
                {
                    continue;
                }

                passenger = _slots[i].Passenger;
                slot = _slots[i];
                return passenger != null;
            }

            return false;
        }

        public bool TryGetBoardablePassengers(
            PassengerColorType colorType,
            int maxCount,
            out List<QueueBoardingCandidate> candidates)
        {
            candidates = new List<QueueBoardingCandidate>();

            if (maxCount <= 0)
            {
                return false;
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].IsOccupied)
                {
                    continue;
                }

                IPassenger passenger = _slots[i].Passenger;

                if (passenger == null)
                {
                    continue;
                }

                if (!passenger.IsReadyForBoarding)
                {
                    continue;
                }

                if (passenger.ColorType != colorType)
                {
                    continue;
                }

                candidates.Add(new QueueBoardingCandidate(passenger, _slots[i]));

                if (candidates.Count >= maxCount)
                {
                    break;
                }
            }

            return candidates.Count > 0;
        }

        public bool HasAnyBoardablePassenger(PassengerColorType colorType)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].IsOccupied)
                {
                    continue;
                }

                IPassenger passenger = _slots[i].Passenger;

                if (passenger == null)
                {
                    continue;
                }

                if (!passenger.IsReadyForBoarding)
                {
                    continue;
                }

                if (passenger.ColorType == colorType)
                {
                    return true;
                }
            }

            return false;
        }

        public void RemovePassengerFromQueue(QueueSlot slot)
        {
            if (slot == null)
            {
                return;
            }

            RemovePassengersFromQueue(new List<QueueSlot> { slot });
        }

        public void RemovePassengersFromQueue(IReadOnlyList<QueueSlot> slots)
        {
            if (slots == null || slots.Count == 0)
            {
                return;
            }

            HashSet<QueueSlot> removedSlots = new(slots);

            for (int i = 0; i < _slots.Count; i++)
            {
                if (removedSlots.Contains(_slots[i]))
                {
                    _slots[i].Clear();
                }
            }

            List<IPassenger> remainingPassengers = new();

            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].IsOccupied)
                {
                    continue;
                }

                remainingPassengers.Add(_slots[i].Passenger);
                _slots[i].Clear();
            }

            for (int i = 0; i < remainingPassengers.Count; i++)
            {
                QueueSlot targetSlot = _slots[i];
                targetSlot.TryOccupy(remainingPassengers[i]);
                remainingPassengers[i].MoveToQueueSlot(targetSlot);
            }

            NotifyQueueChanged();
        }

        public void NotifyQueueChanged()
        {
            OnQueueChanged?.Invoke();
        }
    }
}