using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Magebase.Spritesynth
{
    public static class SpritesynthHistory
    {
        private static GenerationHistory _cache;
        private static string _filePath;

        private static string FilePath
        {
            get
            {
                if (_filePath == null)
                {
                    string projectPath = Path.GetDirectoryName(Application.dataPath);
                    string dir = Path.Combine(projectPath, "ProjectSettings", "Spritesynth");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    _filePath = Path.Combine(dir, "history.json");
                }
                return _filePath;
            }
        }

        public static List<HistoryEntry> Entries
        {
            get
            {
                EnsureLoaded();
                return _cache.entries;
            }
        }

        private static void EnsureLoaded()
        {
            if (_cache != null)
                return;

            if (File.Exists(FilePath))
            {
                try
                {
                    string json = File.ReadAllText(FilePath);
                    _cache = JsonUtility.FromJson<GenerationHistory>(json);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"SpriteSynth: Failed to load history: {ex.Message}");
                    _cache = new GenerationHistory();
                }
            }
            else
            {
                _cache = new GenerationHistory();
            }
        }

        public static void AddEntry(HistoryEntry entry)
        {
            EnsureLoaded();
            _cache.entries.Insert(0, entry);
            Persist();
        }

        public static void RemoveEntry(string jobId)
        {
            EnsureLoaded();
            _cache.entries.RemoveAll(e => e.job_id == jobId);
            Persist();
        }

        public static void Clear()
        {
            _cache = new GenerationHistory();
            Persist();
        }

        private static void Persist()
        {
            try
            {
                string json = JsonUtility.ToJson(_cache, true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"SpriteSynth: Failed to save history: {ex.Message}");
            }
        }
    }
}
