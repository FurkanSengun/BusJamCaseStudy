using UnityEngine;

namespace Core.Sound
{
    public interface ISoundManager
    {
        void PlayOneShot(string soundId, float volume);
    }
}