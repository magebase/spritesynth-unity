using NUnit.Framework;
using UnityEditor;

namespace Magebase.Spritesynth.Tests
{
    public class SpritesynthSettingsTests
    {
        [SetUp]
        public void SetUp()
        {
            EditorPrefs.DeleteKey("Spritesynth_API_Key");
            EditorPrefs.DeleteKey("Spritesynth_Base_URL");
        }

        [TearDown]
        public void TearDown()
        {
            EditorPrefs.DeleteKey("Spritesynth_API_Key");
            EditorPrefs.DeleteKey("Spritesynth_Base_URL");
        }

        [Test]
        public void ApiKey_DefaultIsEmpty()
        {
            string key = Magebase.Spritesynth.Editor.SpritesynthSettings.ApiKey;
            Assert.IsEmpty(key);
        }

        [Test]
        public void ApiKey_RoundTrips()
        {
            Magebase.Spritesynth.Editor.SpritesynthSettings.ApiKey = "sk-test-key-123";
            string retrieved = Magebase.Spritesynth.Editor.SpritesynthSettings.ApiKey;

            Assert.AreEqual("sk-test-key-123", retrieved);
        }

        [Test]
        public void ApiKey_NullClearsPref()
        {
            Magebase.Spritesynth.Editor.SpritesynthSettings.ApiKey = "sk-something";
            Magebase.Spritesynth.Editor.SpritesynthSettings.ApiKey = null;

            Assert.IsEmpty(Magebase.Spritesynth.Editor.SpritesynthSettings.ApiKey);
            Assert.IsFalse(EditorPrefs.HasKey("Spritesynth_API_Key"));
        }

        [Test]
        public void BaseUrl_DefaultIsProduction()
        {
            string url = Magebase.Spritesynth.Editor.SpritesynthSettings.BaseUrl;
            Assert.AreEqual("https://api.spritesynth.com/api", url);
        }

        [Test]
        public void BaseUrl_RoundTrips()
        {
            Magebase.Spritesynth.Editor.SpritesynthSettings.BaseUrl = "https://staging.api.spritesynth.com/api";
            string retrieved = Magebase.Spritesynth.Editor.SpritesynthSettings.BaseUrl;

            Assert.AreEqual("https://staging.api.spritesynth.com/api", retrieved);
        }

        [Test]
        public void ClearApiKey_RemovesKey()
        {
            Magebase.Spritesynth.Editor.SpritesynthSettings.ApiKey = "sk-test";
            Magebase.Spritesynth.Editor.SpritesynthSettings.ClearApiKey();

            Assert.IsFalse(EditorPrefs.HasKey("Spritesynth_API_Key"));
            Assert.IsEmpty(Magebase.Spritesynth.Editor.SpritesynthSettings.ApiKey);
        }

        [Test]
        public void ClearAll_RemovesAllPrefs()
        {
            Magebase.Spritesynth.Editor.SpritesynthSettings.ApiKey = "sk-test";
            Magebase.Spritesynth.Editor.SpritesynthSettings.BaseUrl = "https://custom.api.com";

            Magebase.Spritesynth.Editor.SpritesynthSettings.ClearAll();

            Assert.IsFalse(EditorPrefs.HasKey("Spritesynth_API_Key"));
            Assert.IsFalse(EditorPrefs.HasKey("Spritesynth_Base_URL"));
        }

        [Test]
        public void HasEnvVar_InitiallyFalse()
        {
            bool has = Magebase.Spritesynth.Editor.SpritesynthSettings.HasEnvVar;
            Assert.IsFalse(has);
        }

        [Test]
        public void IsUsingEnvVar_FalseWhenKeyIsSet()
        {
            Magebase.Spritesynth.Editor.SpritesynthSettings.ApiKey = "sk-manual";
            bool usingEnv = Magebase.Spritesynth.Editor.SpritesynthSettings.IsUsingEnvVar;

            Assert.IsFalse(usingEnv);
        }
    }
}
