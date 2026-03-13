using System.Collections.Generic;
using Game.Data;
using UnityEngine;
using Zenject;

namespace Core.Sound
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour, ISoundManager
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private SfxLibrary sfxLibrary;
        private readonly Dictionary<string, SfxEntry> _soundMap = new();

        [Inject] private IPlayerDataManager _playerDataManager;


        private void Awake()
        {
            BuildLibraryMap();

            ApplySoundState(_playerDataManager.IsSoundOn);
            _playerDataManager.OnSoundStateChanged += ApplySoundState;
        }

        private void OnDestroy()
        {
            if (_playerDataManager != null)
            {
                _playerDataManager.OnSoundStateChanged -= ApplySoundState;
            }
        }

        public void PlayOneShot(string soundId, float volume)
        {
            if (!TryGetClip(soundId, out var clip, out _))
            {
                return;
            }

            audioSource.PlayOneShot(clip, volume);
        }

        private bool TryGetClip(string soundId, out AudioClip clip, out float defaultVolume)
        {
            clip = null;
            defaultVolume = 1f;

            if (!_playerDataManager.IsSoundOn)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(soundId))
            {
                return false;
            }

            if (!_soundMap.TryGetValue(soundId, out var entry))
            {
                return false;
            }

            if (entry.clips == null || entry.clips.Length == 0)
            {
                return false;
            }

            int randomIndex = Random.Range(0, entry.clips.Length);
            clip = entry.clips[randomIndex];
            defaultVolume = entry.volume;

            return clip != null;
        }

        private void BuildLibraryMap()
        {
            _soundMap.Clear();

            if (sfxLibrary == null)
            {
                return;
            }

            foreach (var entry in sfxLibrary.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id))
                {
                    continue;
                }

                if (_soundMap.ContainsKey(entry.id))
                {
                    continue;
                }

                _soundMap.Add(entry.id, entry);
            }
        }

        private void ApplySoundState(bool isOn)
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.enabled = isOn;
        }
    }
}