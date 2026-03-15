using Game.Passenger;

namespace Game.Queue
{
    public readonly struct QueueBoardingCandidate
    {
        public IPassenger Passenger { get; }
        public QueueSlot Slot { get; }

        public QueueBoardingCandidate(IPassenger passenger, QueueSlot slot)
        {
            Passenger = passenger;
            Slot = slot;
        }
    }
}