using UnityEngine;

namespace Game.Data
{
    public enum PassengerColorType
    {
        Red,
        Green,
        Blue,
        Yellow,
        Purple,
    }
    [System.Serializable]
    public class PassengerData
    {
        public Vector2Int gridIndex;
        public PassengerColorType color;
    }
}