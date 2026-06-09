using System;
using NUnit.Framework;
using UnityEditor;
using Magebase.Spritesynth.Editor;

namespace Magebase.Spritesynth.Tests.Editor
{
    public class SpritesynthSettingsTests
    {
        private const string TestKeyPref = "Spritesynth_API_Key";
        private const string TestUrlPref = "Spritesynth_Base_URL";
        private const string TestEnvVar = "SPIRESYNTH_API_KEY";

        [SetUp]
        public void Setup()
        {
            EditorPrefs.DeleteKey(TestKeyPref);
            EditorPrefs.DeleteKey(TestUrlPref);
            Environment.SetEnvironmentVariable(TestEnvVar, null);
        }

        [Test]
        public void ApiKey_Default_ReturnsEmpty()
        {
            Assert.AreEqual("", SpritesynthSettings.ApiKey);
        }

        [Test]
        public void ApiKey_SetAndGet_ReturnsSavedValue()
        {
            SpritesynthSettings.ApiKey = "test-key-123";
            Assert.AreEqual("test-key-123", SpritesynthSettings.ApiKey);
        }

        [Test]
        public void ApiKey_SetNull_DeletesKey()
        {
            SpritesynthSettings.ApiKey = "some-key";
            SpritesynthSettings.ApiKey = null;
            Assert.AreEqual("", SpritesynthSettings.ApiKey);
        }

        [Test]
        public void ApiKey_EnvVarFallback_ReturnsEnvValue()
        {
            Environment.SetEnvironmentVariable(TestEnvVar, "env-key");
            Assert.AreEqual("env-key", SpritesynthSettings.ApiKey);
        }

        [Test]
        public void HasEnvVar_WhenSet_ReturnsTrue()
        {
            Environment.SetEnvironmentVariable(TestEnvVar, "present");
            Assert.IsTrue(SpritesynthSettings.HasEnvVar);
        }

        [Test]
        public void HasEnvVar_WhenNotSet_ReturnsFalse()
        {
            Assert.IsFalse(SpritesynthSettings.HasEnvVar);
        }

        [Test]
        public void BaseUrl_Default_ReturnsDefaultUrl()
        {
            Assert.AreEqual("https://api.spritesynth.com/api", SpritesynthSettings.BaseUrl);
        }

        [Test]
        public void BaseUrl_SetAndGet_ReturnsSavedValue()
        {
            SpritesynthSettings.BaseUrl = "https://custom.url/api";
            Assert.AreEqual("https://custom.url/api", SpritesynthSettings.BaseUrl);
        }

        [Test]
        public void ClearAll_RemovesAllPrefs()
        {
            SpritesynthSettings.ApiKey = "key";
            SpritesynthSettings.BaseUrl = "https://url";
            SpritesynthSettings.ClearAll();
            Assert.AreEqual("", SpritesynthSettings.ApiKey);
            Assert.AreEqual("https://api.spritesynth.com/api", SpritesynthSettings.BaseUrl);
        }

        [Test]
        public void ClearApiKey_RemovesKey()
        {
            SpritesynthSettings.ApiKey = "key-to-clear";
            SpritesynthSettings.ClearApiKey();
            Assert.AreEqual("", SpritesynthSettings.ApiKey);
        }

        [Test]
        public void IsUsingEnvVar_NoSavedKeyAndEnvSet_ReturnsTrue()
        {
            Environment.SetEnvironmentVariable(TestEnvVar, "env-key");
            Assert.IsTrue(SpritesynthSettings.IsUsingEnvVar);
        }

        [Test]
        public void IsUsingEnvVar_SavedKeyPresent_ReturnsFalse()
        {
            SpritesynthSettings.ApiKey = "saved-key";
            Environment.SetEnvironmentVariable(TestEnvVar, "env-key");
            Assert.IsFalse(SpritesynthSettings.IsUsingEnvVar);
        }
    }
}
