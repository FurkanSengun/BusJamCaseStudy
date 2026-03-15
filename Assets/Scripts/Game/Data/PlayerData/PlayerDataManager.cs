using System;
using Core.SaveSystem;
using UnityEngine;

namespace Game.Data.PlayerData
{
    /// <summary>
    /// Oyuncunun verilerini set eder ve yükler
    /// </summary>
    public class PlayerDataManager : IPlayerDataManager
    {
        private const string CurrentLevelKey = "player_current_level";
        private const string SoundKey = "player_sound_state";

        private readonly ISaveManager _saveManager;

        public int CurrentLevel { get; private set; }
        public bool IsSoundOn { get; private set; }

        public event Action<bool> OnSoundStateChanged;

        public PlayerDataManager(ISaveManager saveManager)
        {
            _saveManager = saveManager;
        }

        public void Load()
        {
            CurrentLevel = _saveManager.Load(CurrentLevelKey, 0);
            IsSoundOn = _saveManager.Load(SoundKey, 1) == 1;

            OnSoundStateChanged?.Invoke(IsSoundOn);
        }

        public void SetCurrentLevel(int level)
        {
            CurrentLevel = Mathf.Max(0, level);
            _saveManager.Save(CurrentLevelKey, CurrentLevel);
        }

        public void SetSound(bool isOn)
        {
            if (IsSoundOn == isOn)
            {
                return;
            }

            IsSoundOn = isOn;
            _saveManager.Save(SoundKey, isOn ? 1 : 0);
            OnSoundStateChanged?.Invoke(IsSoundOn);
        }
    }
}