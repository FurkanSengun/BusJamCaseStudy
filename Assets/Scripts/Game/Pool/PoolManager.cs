using UnityEngine;

namespace Game.Pool
{
    public class PoolManager : MonoBehaviour, IPoolManager
    {
        public void Release(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            instance.SetActive(false);
        }
    }
}