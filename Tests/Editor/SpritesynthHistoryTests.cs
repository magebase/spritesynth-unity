using NUnit.Framework;
using System;
using System.IO;
using UnityEngine;

namespace Magebase.Spritesynth.Tests
{
    public class SpritesynthHistoryTests
    {
        private string _originalDataPath;

        [SetUp]
        public void SetUp()
        {
            _originalDataPath = Application.dataPath;
            Application.dataPath = Path.Combine(Path.GetTempPath(), "SpritesynthTests", "Assets");
        }

        [TearDown]
        public void TearDown()
        {
            string testDir = Path.GetDirectoryName(Application.dataPath);
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, true);

            string projectSettings = Path.Combine(
                Path.GetDirectoryName(_originalDataPath),
                "ProjectSettings", "Spritesynth");
            if (Directory.Exists(projectSettings))
            {
                string historyFile = Path.Combine(projectSettings, "history.json");
                if (File.Exists(historyFile))
                    File.Delete(historyFile);
            }

            Application.dataPath = _originalDataPath;
        }

        [Test]
        public void History_StartsWithEmptyEntries()
        {
            var entries = SpritesynthHistory.Entries;
            Assert.IsNotNull(entries);
            Assert.AreEqual(0, entries.Count);
        }

        [Test]
        public void History_AddEntry_IncreasesCount()
        {
            SpritesynthHistory.AddEntry(new HistoryEntry
            {
                job_id = "job_1",
                prompt = "test sprite",
                status = "completed",
                date = DateTime.UtcNow.ToString("o"),
            });

            Assert.AreEqual(1, SpritesynthHistory.Entries.Count);
        }

        [Test]
        public void History_AddEntry_InsertsAtFront()
        {
            SpritesynthHistory.AddEntry(new HistoryEntry
            {
                job_id = "first",
                prompt = "first entry",
                status = "completed",
                date = DateTime.UtcNow.ToString("o"),
            });

            SpritesynthHistory.AddEntry(new HistoryEntry
            {
                job_id = "second",
                prompt = "second entry",
                status = "completed",
                date = DateTime.UtcNow.ToString("o"),
            });

            Assert.AreEqual(2, SpritesynthHistory.Entries.Count);
            Assert.AreEqual("second", SpritesynthHistory.Entries[0].job_id);
            Assert.AreEqual("first", SpritesynthHistory.Entries[1].job_id);
        }

        [Test]
        public void History_AddEntry_StoresFields()
        {
            var now = DateTime.UtcNow.ToString("o");
            SpritesynthHistory.AddEntry(new HistoryEntry
            {
                job_id = "job_42",
                prompt = "knight sprite",
                image_size = "64x64",
                seed = 7,
                model = "fast",
                status = "completed",
                width = 64,
                height = 64,
                credits_cost = 2,
                duration_ms = 1500,
                date = now,
                local_path = "Assets/Spritesynth/Generations/test.png",
            });

            var entry = SpritesynthHistory.Entries[0];
            Assert.AreEqual("job_42", entry.job_id);
            Assert.AreEqual("knight sprite", entry.prompt);
            Assert.AreEqual("64x64", entry.image_size);
            Assert.AreEqual(7, entry.seed);
            Assert.AreEqual("fast", entry.model);
            Assert.AreEqual("completed", entry.status);
            Assert.AreEqual(64, entry.width);
            Assert.AreEqual(64, entry.height);
            Assert.AreEqual(2, entry.credits_cost);
            Assert.AreEqual(1500, entry.duration_ms);
            Assert.AreEqual(now, entry.date);
            Assert.AreEqual("Assets/Spritesynth/Generations/test.png", entry.local_path);
        }

        [Test]
        public void History_RemoveEntry_DecreasesCount()
        {
            SpritesynthHistory.AddEntry(new HistoryEntry
            {
                job_id = "job_1",
                prompt = "test",
                status = "completed",
                date = DateTime.UtcNow.ToString("o"),
            });

            SpritesynthHistory.AddEntry(new HistoryEntry
            {
                job_id = "job_2",
                prompt = "test 2",
                status = "completed",
                date = DateTime.UtcNow.ToString("o"),
            });

            SpritesynthHistory.RemoveEntry("job_1");

            Assert.AreEqual(1, SpritesynthHistory.Entries.Count);
            Assert.AreEqual("job_2", SpritesynthHistory.Entries[0].job_id);
        }

        [Test]
        public void History_RemoveEntry_UnknownIdDoesNothing()
        {
            SpritesynthHistory.AddEntry(new HistoryEntry
            {
                job_id = "job_1",
                prompt = "test",
                status = "completed",
                date = DateTime.UtcNow.ToString("o"),
            });

            SpritesynthHistory.RemoveEntry("nonexistent");

            Assert.AreEqual(1, SpritesynthHistory.Entries.Count);
        }

        [Test]
        public void History_Clear_RemovesAll()
        {
            SpritesynthHistory.AddEntry(new HistoryEntry
            {
                job_id = "job_1",
                prompt = "test",
                status = "completed",
                date = DateTime.UtcNow.ToString("o"),
            });

            SpritesynthHistory.Clear();

            Assert.AreEqual(0, SpritesynthHistory.Entries.Count);
        }
    }
}
