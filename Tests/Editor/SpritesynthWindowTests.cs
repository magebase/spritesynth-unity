using NUnit.Framework;
using UnityEditor;

namespace Magebase.Spritesynth.Editor.Tests
{
    public class SpritesynthWindowTests
    {
        [Test]
        public void Window_CanBeOpened()
        {
            var window = EditorWindow.GetWindow<SpritesynthWindow>("SpriteSynth Generator");
            Assert.IsNotNull(window);
            window.Close();
        }

        [Test]
        public void Window_HasDefaultPrompt()
        {
            var window = EditorWindow.GetWindow<SpritesynthWindow>("SpriteSynth Generator");
            window.Close();
        }
    }
}
