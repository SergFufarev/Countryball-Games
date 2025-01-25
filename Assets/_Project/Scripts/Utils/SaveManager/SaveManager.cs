using System;
using UnityEngine;
using Newtonsoft.Json;

namespace FunnyBlox
{
    public static class SaveManager
    {
#if UNITY_EDITOR
        private static bool lockSaves = false;
#endif
        private static bool LockSaves
        {
            get
            {
#if UNITY_EDITOR
                return lockSaves;
#else
                return false;
#endif
            }
        }

        public static void Save<T>(string key, T data)
        {
            if (LockSaves) return;

            string savedData = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(key, savedData);
            PlayerPrefs.Save();
        }

        public static T Load<T>(string key)
        {
            T data;

            if (PlayerPrefs.HasKey(key))
            {
                string loadedJson = PlayerPrefs.GetString(key);
                data = JsonConvert.DeserializeObject<T>(loadedJson);
            }
            else
            {
                data = default;
                Save(key, data);
            }

            return data;
        }

        public static void ClearData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}