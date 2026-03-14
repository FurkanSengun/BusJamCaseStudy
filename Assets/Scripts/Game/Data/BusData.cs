using UnityEngine;

namespace Game.Data
{
    [System.Serializable]
    public class BusData
    {
        public int order;
        public PassengerColorType colorType;
        public int capacity = 3;
    }
}