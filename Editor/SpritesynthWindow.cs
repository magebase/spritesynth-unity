using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;

namespace Magebase.Spritesynth.Editor
{
    public class SpritesynthWindow : EditorWindow
    {
        private string _apiKey;
        private string _prompt = "a 16x16 pixel art knight";
        private string _imageSize = "128x128";
        private string _status;

        [MenuItem("Tools/SpriteSynth/Generator")]
        public static void ShowWindow()
        {
            GetWindow<SpritesynthWindow>("SpriteSynth Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("SpriteSynth AI Generator", EditorStyles.boldLabel);
            _apiKey = EditorGUILayout.TextField("API Key", _apiKey);
            _prompt = EditorGUILayout.TextField("Prompt", _prompt);
            _imageSize = EditorGUILayout.TextField("Size (WxH)", _imageSize);

            if (GUILayout.Button("Generate") && !string.IsNullOrEmpty(_apiKey))
            {
                _status = "Generating...";
                _ = GenerateAsync();
            }

            if (!string.IsNullOrEmpty(_status))
                GUILayout.Label(_status);
        }

        private async Task GenerateAsync()
        {
            var client = new SpritesynthClient(_apiKey);
            var request = new CreateImageRequest
            {
                description = _prompt,
                image_size = _imageSize
            };
            var response = await client.CreateImageAsync(request);
            var result = await client.PollGenerationAsync(response.job_id);
            if (result.status == "completed")
                _status = $"Done! Download from: {result.asset.url}";
            else
                _status = $"Failed: {result.status}";
        }
    }
}
