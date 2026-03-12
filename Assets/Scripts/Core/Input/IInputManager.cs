using UnityEngine;
using UnityEngine.Events;

namespace Core.Input
{
    public interface IInputManager
    {
        UnityAction<Vector3> OnInputDetected { get; set; }
    }
}