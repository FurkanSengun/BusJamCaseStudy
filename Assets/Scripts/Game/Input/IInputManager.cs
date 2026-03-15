using System;
using UnityEngine;

namespace Game.Input
{
    public interface IInputManager
    {
        event Action OnPrimaryInputStarted;
        event Action<RaycastHit> OnWorldHitDetected;
    }
}