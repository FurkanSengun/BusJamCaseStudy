using UnityEngine;

namespace Core.SaveSystem
{
    public class SaveManager : ISaveManager
    {
        public void Save<T>(string key, T data)
        {
            if (data is int intValue)
            {
                PlayerPrefs.SetInt(key, intValue);
            }
            else if (data is float floatValue)
            {
                PlayerPrefs.SetFloat(key, floatValue);
            }
            else if (data is string stringValue)
            {
                PlayerPrefs.SetString(key, stringValue);
            }
            else
            {
                string json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString(key, json);
            }

            PlayerPrefs.Save();
        }

        public T Load<T>(string key, T defaultValue = default)
        {
            if (!Exists(key))
            {
                return defaultValue;
            }

            if (typeof(T) == typeof(int))
            {
                return (T)(object)PlayerPrefs.GetInt(key, (int)(object)defaultValue);
            }

            if (typeof(T) == typeof(float))
            {
                return (T)(object)PlayerPrefs.GetFloat(key, (float)(object)defaultValue);
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)PlayerPrefs.GetString(key, (string)(object)defaultValue);
            }

            string json = PlayerPrefs.GetString(key);
            return JsonUtility.FromJson<T>(json);
        }

        public bool Exists(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}