using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "Level Prefab Registry", menuName = "Level/Level Prefab Registry", order = 2)]
    public class LevelPrefabRegistry : ScriptableObject
    {
        [Header("Editor")]
        public GameObject editorCellPrefab;
        public GameObject editorQueueSlotPrefab;

        [Header("Runtime")]
        public GameObject groundCellPrefab;
        public GameObject queueSlotPrefab;
        public GameObject obstaclePrefab;
        public GameObject passengerPrefab;
        public GameObject busPrefab;
    }
}