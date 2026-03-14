using UnityEngine;

namespace Game.Data
{
    public enum CellType
    {
        Empty,
        Passenger,
        Obstacle
    }
    
    [System.Serializable]
    public class CellData
    {
        public Vector2Int gridIndex;
        public CellType cellType;
        public PassengerColorType passengerColor;
    }
}