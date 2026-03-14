using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Level/Level Data", order = 1)]
    public class LevelData : ScriptableObject
    {
        public int levelNumber;
        public Vector2Int gridSize;
        public float cellSize = 1f;
        public float timeLimit = 60f;

        public List<CellData> cells = new();
        public List<QueueData> queueSlots = new();
        public List<PassengerData> passengers = new();
        public List<BusData> buses = new();
        public List<ObstacleData> obstacles = new();
    }
}