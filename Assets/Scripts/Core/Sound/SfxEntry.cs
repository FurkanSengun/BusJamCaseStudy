using System;
using UnityEngine;

namespace Core.Sound
{
    [Serializable]
    public class SfxEntry
    {
        public string id;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
    }
}