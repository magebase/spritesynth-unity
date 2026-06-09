using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Magebase.Spritesynth.Tests.Editor
{
    public class SpritesynthHistoryTests
    {
        private string _testFilePath;

        [SetUp]
        public void Setup()
        {
            string projectPath = Path.GetDirectoryName(Application.dataPath);
            string dir = Path.Combine(projectPath, "ProjectSettings", "Spritesynth");
            _testFilePath = Path.Combine(dir, "history.json");

            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);

            SpritesynthHistory.Clear();
        }

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }

        [Test]
        public void Entries_WhenEmpty_ReturnsEmptyList()
        {
            Assert.AreEqual(0, SpritesynthHistory.Entries.Count);
        }

        [Test]
        public void AddEntry_AddsToBeginning()
        {
            var entry1 = new HistoryEntry { job_id = "1", prompt = "first" };
            var entry2 = new HistoryEntry { job_id = "2", prompt = "second" };

            SpritesynthHistory.AddEntry(entry1);
            SpritesynthHistory.AddEntry(entry2);

            Assert.AreEqual(2, SpritesynthHistory.Entries.Count);
            Assert.AreEqual("second", SpritesynthHistory.Entries[0].prompt);
        }

        [Test]
        public void RemoveEntry_ByJobId_RemovesCorrectEntry()
        {
            SpritesynthHistory.AddEntry(new HistoryEntry { job_id = "a", prompt = "alpha" });
            SpritesynthHistory.AddEntry(new HistoryEntry { job_id = "b", prompt = "beta" });
            SpritesynthHistory.AddEntry(new HistoryEntry { job_id = "c", prompt = "gamma" });

            SpritesynthHistory.RemoveEntry("b");

            Assert.AreEqual(2, SpritesynthHistory.Entries.Count);
            Assert.IsFalse(SpritesynthHistory.Entries.Exists(e => e.job_id == "b"));
        }

        [Test]
        public void RemoveEntry_NonExistentId_DoesNothing()
        {
            SpritesynthHistory.AddEntry(new HistoryEntry { job_id = "x", prompt = "test" });
            SpritesynthHistory.RemoveEntry("nonexistent");
            Assert.AreEqual(1, SpritesynthHistory.Entries.Count);
        }

        [Test]
        public void Clear_RemovesAllEntries()
        {
            SpritesynthHistory.AddEntry(new HistoryEntry { job_id = "1" });
            SpritesynthHistory.AddEntry(new HistoryEntry { job_id = "2" });
            SpritesynthHistory.Clear();
            Assert.AreEqual(0, SpritesynthHistory.Entries.Count);
        }

        [Test]
        public void Persistence_SavesAndLoads()
        {
            SpritesynthHistory.AddEntry(new HistoryEntry
            {
                job_id = "persist-test",
                prompt = "persistence check",
                status = "completed",
                credits_cost = 5,
                date = DateTime.UtcNow.ToString("o"),
            });

            Assert.IsTrue(File.Exists(_testFilePath));

            SpritesynthHistory.Clear();
            Assert.AreEqual(0, SpritesynthHistory.Entries.Count);

            string json = File.ReadAllText(_testFilePath);
            var loaded = JsonUtility.FromJson<GenerationHistory>(json);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(1, loaded.entries.Count);
            Assert.AreEqual("persist-test", loaded.entries[0].job_id);
        }
    }
}
