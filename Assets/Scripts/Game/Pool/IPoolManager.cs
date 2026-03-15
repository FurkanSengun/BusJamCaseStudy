using UnityEngine;

namespace Game.Pool
{
    public interface IPoolManager
    {
        void Release(GameObject instance);
    }
}