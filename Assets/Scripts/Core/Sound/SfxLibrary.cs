using System.Collections.Generic;
using UnityEngine;

namespace Core.Sound
{
    [CreateAssetMenu(fileName = "SfxLibrary", menuName = "Audio/Sfx Library")]
    public class SfxLibrary : ScriptableObject
    {
        [SerializeField] private List<SfxEntry> entries = new();
        
        public List<SfxEntry> Entries => entries;
    }
}