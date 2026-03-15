using System;
using Game.Passenger;
using UnityEngine;

namespace Game.Bus
{
    public interface IBusManager
    {
        event Action OnQueueFull;

        void Initialize(Transform busRoot);
        bool TryBoardSelectedPassenger(IPassenger passenger);
    }
}