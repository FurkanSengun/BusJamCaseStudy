namespace Game.Interaction
{
    public interface IInteractable
    {
        bool CanInteract { get; }
        void Interact();
    }
}