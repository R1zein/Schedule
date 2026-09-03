using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Core.Saving
{
    /// <summary>
    /// Содержимое файла сохранения: по строке JSON на каждую сущность под её ключом.
    /// Плоский список вместо словаря — JsonUtility словари не сериализует.
    /// </summary>
    [Serializable]
    public class SaveFile
    {
        /// <summary>Версия схемы. Пишется в файл, чтобы старый сейв можно было мигрировать, а не терять.</summary>
        public const int CurrentVersion = 1;

        [Serializable]
        public class Entry
        {
            public string Key;
            public string Value;
        }

        public int Version = CurrentVersion;
        public string SavedAt;
        public List<Entry> Entries = new();

        public static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

        public void SetState(object state, string key)
        {
            string json = JsonUtility.ToJson(state);
            Entry entry = Entries.Find(e => e.Key == key);

            if (entry == null)
            {
                Entries.Add(new Entry { Key = key, Value = json });
            }
            else
            {
                entry.Value = json;
            }
        }

        public T GetState<T>(string key) where T : class
        {
            Entry entry = Entries.Find(e => e.Key == key);
            return entry == null ? null : JsonUtility.FromJson<T>(entry.Value);
        }

        /// <summary>Пишем через временный файл: обрыв посреди записи не оставит битый сейв.</summary>
        public void Write()
        {
            SavedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string path = FilePath;
            string temp = path + ".tmp";

            try
            {
                File.WriteAllText(temp, JsonUtility.ToJson(this, true));

                if (File.Exists(path))
                {
                    File.Replace(temp, path, null);
                }
                else
                {
                    File.Move(temp, path);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoad] Не записался сейв {path}: {e.Message}");
            }
        }

        /// <summary>null — файла нет, он битый или из более новой версии игры. Во всех случаях стартуем с дефолтов.</summary>
        public static SaveFile Read()
        {
            string path = FilePath;
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                SaveFile file = JsonUtility.FromJson<SaveFile>(File.ReadAllText(path));

                if (file != null && file.Version > CurrentVersion)
                {
                    Debug.LogWarning($"[SaveLoad] Сейв версии {file.Version}, приложение понимает {CurrentVersion}. Файл не читаем.");
                    return null;
                }

                return file;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoad] Не прочитался сейв {path}: {e.Message}");
                return null;
            }
        }
    }
}
