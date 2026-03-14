using System;

namespace Game.Data.PlayerData
{
    public interface IPlayerDataManager
    {
        int CurrentLevel { get; }
        bool IsSoundOn { get; }

        event Action<bool> OnSoundStateChanged;

        void Load();
        void SetCurrentLevel(int level);
        void SetSound(bool isOn);
    }
}