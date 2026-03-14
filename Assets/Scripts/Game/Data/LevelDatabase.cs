using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Level/Level Database", order = 0)]
    public class LevelDatabase : ScriptableObject
    {
        public List<LevelData> levels = new();

        public LevelData GetLevel(int levelNumber)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i] != null && levels[i].levelNumber == levelNumber)
                {
                    return levels[i];
                }
            }

            return null;
        }

        public List<LevelData> GetOrderedLevels()
        {
            return levels
                .Where(level => level != null)
                .OrderBy(level => level.levelNumber)
                .ToList();
        }

        public int GetNextLevelNumber()
        {
            var orderedLevels = GetOrderedLevels();

            if (orderedLevels.Count == 0)
            {
                return 0;
            }

            return orderedLevels[orderedLevels.Count - 1].levelNumber + 1;
        }

        public void AddLevel(LevelData levelData)
        {
            if (levelData == null)
            {
                return;
            }

            if (levels.Contains(levelData))
            {
                return;
            }

            levels.Add(levelData);

            levels = levels
                .Where(level => level != null)
                .OrderBy(level => level.levelNumber)
                .ToList();
        }
    }
}