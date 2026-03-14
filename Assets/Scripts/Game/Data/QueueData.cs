using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// Yolcuların otobüsü beklediği, genelde 5 kişilik olan kuyruğu temsil eder
    /// </summary>
    [System.Serializable]
    public class QueueData
    {
        public int queueIndex;
        public Vector3 rootLocalPosition;
        public List<Vector3> localPositions = new();
    }
}