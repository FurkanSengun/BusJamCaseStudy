using System;

namespace Game.Interaction
{
    public interface IInteractionManager
    {
        event Action<IInteractable> OnInteractionDispatched;

        bool IsInteractionEnabled { get; set; }
    }
}