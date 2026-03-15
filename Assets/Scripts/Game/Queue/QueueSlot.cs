using Game.Passenger;
using UnityEngine;

namespace Game.Queue
{
    public class QueueSlot : MonoBehaviour
    {
        public int Order { get; private set; }
        public IPassenger Passenger { get; private set; }
        public bool IsOccupied => Passenger != null;

        public void Initialize(int order)
        {
            Order = Mathf.Max(0, order);
            Passenger = null;
        }

        public bool TryOccupy(IPassenger passenger)
        {
            if (IsOccupied || passenger == null)
            {
                return false;
            }

            Passenger = passenger;
            return true;
        }

        public void Clear()
        {
            Passenger = null;
        }
    }
}