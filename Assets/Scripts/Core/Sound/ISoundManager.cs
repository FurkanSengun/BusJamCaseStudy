using UnityEngine;

namespace Core.Sound
{
    public interface ISoundManager
    {
        void PlayOneShot(string soundId);
        void PlayOneShot(string soundId, float volumeMultiplier);
        
    }
}