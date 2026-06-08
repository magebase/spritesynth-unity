using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Magebase.Spritesynth.Editor
{
    public class SpritesynthWindow : EditorWindow
    {
        private int _selectedTab;
        private readonly string[] _tabNames = { "Generate", "History", "Settings" };

        [MenuItem("Tools/SpriteSynth/Generator")]
        public static void ShowWindow()
        {
            SpritesynthWindow window = GetWindow<SpritesynthWindow>("SpriteSynth");
            window.minSize = new Vector2(420, 500);
            window.Show();
        }

        private void OnGUI()
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);

            switch (_selectedTab)
            {
                case 0: DrawGenerateTab(); break;
                case 1: DrawHistoryTab(); break;
                case 2: DrawSettingsTab(); break;
            }
        }

        private void OnEnable()
        {
            LoadGenerateFields();
            LoadSettingsFields();
        }

        private void OnDisable()
        {
            SaveGenerateFields();
        }

        private string _prompt = "";
        private string _imageSize = "128x128";
        private string _negativePrompt = "";
        private int _seed;
        private bool _isGenerating;
        private float _progress;
        private string _statusMessage = "";

        private Vector2 _generateScrollPos;
        private Vector2 _historyScrollPos;
        private Vector2 _settingsScrollPos;

        private string _apiKeyField = "";
        private string _baseUrlField = "";
        private string _connectionStatus = "";

        private const string PromptPref = "Spritesynth_Generate_Prompt";
        private const string ImageSizePref = "Spritesynth_Generate_ImageSize";
        private const string NegativePromptPref = "Spritesynth_Generate_NegativePrompt";
        private const string SeedPref = "Spritesynth_Generate_Seed";

        private void LoadGenerateFields()
        {
            _prompt = EditorPrefs.GetString(PromptPref, "a 16x16 pixel art knight with blue armor");
            _imageSize = EditorPrefs.GetString(ImageSizePref, "128x128");
            _negativePrompt = EditorPrefs.GetString(NegativePromptPref, "");
            _seed = EditorPrefs.GetInt(SeedPref, 0);
        }

        private void SaveGenerateFields()
        {
            EditorPrefs.SetString(PromptPref, _prompt);
            EditorPrefs.SetString(ImageSizePref, _imageSize);
            EditorPrefs.SetString(NegativePromptPref, _negativePrompt);
            EditorPrefs.SetInt(SeedPref, _seed);
        }

        private void LoadSettingsFields()
        {
            _apiKeyField = SpritesynthSettings.ApiKey;
            _baseUrlField = SpritesynthSettings.BaseUrl;
        }

        private void DrawGenerateTab()
        {
            _generateScrollPos = EditorGUILayout.BeginScrollView(_generateScrollPos);

            GUILayout.Label("Prompt", EditorStyles.boldLabel);
            _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.Height(60));

            GUILayout.Space(4);

            GUILayout.Label("Image Size (e.g. 128x128)", EditorStyles.boldLabel);
            _imageSize = EditorGUILayout.TextField(_imageSize);

            GUILayout.Space(4);

            GUILayout.Label("Negative Prompt (optional)", EditorStyles.boldLabel);
            _negativePrompt = EditorGUILayout.TextArea(_negativePrompt, GUILayout.Height(40));

            GUILayout.Space(4);

            _seed = EditorGUILayout.IntField("Seed (0 = random)", _seed);

            GUILayout.Space(8);

            string apiKey = SpritesynthSettings.ApiKey;
            bool hasKey = !string.IsNullOrEmpty(apiKey);

            using (new EditorGUI.DisabledGroupScope(_isGenerating || !hasKey))
            {
                if (!hasKey)
                {
                    EditorGUILayout.HelpBox(
                        "Set your API key in the Settings tab first.",
                        MessageType.Warning);
                }

                if (GUILayout.Button("Generate", GUILayout.Height(36)))
                {
                    _ = GenerateAsync();
                }
            }

            if (_isGenerating)
            {
                GUILayout.Space(8);
                EditorGUILayout.ProgressBar(_progress, _statusMessage);
                GUILayout.Label(_statusMessage, EditorStyles.centeredGreyMiniLabel);
            }
            else if (!string.IsNullOrEmpty(_statusMessage))
            {
                GUILayout.Space(8);
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        private async Task GenerateAsync()
        {
            if (_isGenerating)
                return;

            string apiKey = SpritesynthSettings.ApiKey;
            if (string.IsNullOrEmpty(apiKey))
            {
                _statusMessage = "API key is not set. Go to Settings tab.";
                return;
            }

            _isGenerating = true;
            _progress = 0f;
            _statusMessage = "Preparing request...";
            Repaint();

            try
            {
                var client = new SpritesynthClient(apiKey, SpritesynthSettings.BaseUrl);

                var request = new CreateImageRequest
                {
                    description = _prompt,
                    image_size = string.IsNullOrEmpty(_imageSize) ? "128x128" : _imageSize,
                    seed = _seed,
                    negative_prompt = _negativePrompt,
                };

                _statusMessage = "Sending to SpriteSynth API...";
                _progress = 0.1f;
                Repaint();

                CreateImageResponse createResponse = await client.CreateImageAsync(request);

                _statusMessage = $"Queued (job: {createResponse.job_id})...";
                _progress = 0.2f;
                Repaint();

                GenerationResult result = await client.PollGenerationAsync(createResponse.job_id);

                if (result.status != "completed")
                {
                    _statusMessage = $"Generation failed with status: {result.status}";
                    _progress = 0f;
                    return;
                }

                _statusMessage = "Downloading asset...";
                _progress = 0.7f;
                Repaint();

                byte[] pngData = await client.DownloadAssetAsync(result.asset.url);

                _statusMessage = "Importing to project...";
                _progress = 0.9f;
                Repaint();

                string localPath = SpritesynthImporter.Import(pngData, _prompt, result);

                var entry = new HistoryEntry
                {
                    job_id = createResponse.job_id,
                    prompt = _prompt,
                    image_size = request.image_size,
                    negative_prompt = _negativePrompt,
                    seed = _seed,
                    model = "fast",
                    status = "completed",
                    asset_url = result.asset.url,
                    width = result.asset.width,
                    height = result.asset.height,
                    credits_cost = result.credits_cost,
                    duration_ms = result.duration_ms,
                    date = DateTime.UtcNow.ToString("o"),
                    local_path = localPath,
                };

                SpritesynthHistory.AddEntry(entry);

                _statusMessage = "Done!";
                _progress = 1f;

                EditorApplication.delayCall += Repaint;
                SaveGenerateFields();
            }
            catch (Exception ex)
            {
                _statusMessage = $"Error: {ex.Message}";
                _progress = 0f;
                Debug.LogError($"[SpriteSynth] Generation failed: {ex}");
            }
            finally
            {
                _isGenerating = false;
                EditorApplication.delayCall += Repaint;
            }
        }

        private void DrawHistoryTab()
        {
            _historyScrollPos = EditorGUILayout.BeginScrollView(_historyScrollPos);
            var entries = SpritesynthHistory.Entries;

            if (entries.Count == 0)
            {
                EditorGUILayout.HelpBox("No generations yet. Switch to the Generate tab to create one.", MessageType.Info);
            }
            else
            {
                int totalCredits = 0;
                foreach (var entry in entries)
                {
                    totalCredits += entry.credits_cost;
                    DrawHistoryEntry(entry);
                }

                GUILayout.Space(4);
                GUILayout.Label(
                    $"{entries.Count} generation(s) — {totalCredits} total credits used",
                    EditorStyles.miniLabel);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHistoryEntry(HistoryEntry entry)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            string dateStr = "Unknown";
            if (DateTime.TryParse(entry.date, out DateTime dt))
                dateStr = dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

            GUILayout.Label(dateStr, EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();

            GUIStyle statusStyle = entry.status == "completed"
                ? EditorStyles.boldLabel
                : EditorStyles.label;
            GUILayout.Label(entry.status, statusStyle);

            EditorGUILayout.EndHorizontal();

            GUILayout.Label(entry.prompt, EditorStyles.wordWrappedLabel);

            GUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();

            string localPath = entry.local_path;
            bool hasLocalAsset = !string.IsNullOrEmpty(localPath)
                && File.Exists(localPath);

            if (hasLocalAsset)
            {
                Texture2D preview = AssetDatabase.LoadAssetAtPath<Texture2D>(localPath);
                if (preview != null)
                {
                    GUILayout.Box(preview, GUILayout.Width(64), GUILayout.Height(64));
                }
            }
            else
            {
                Rect previewRect = GUILayoutUtility.GetRect(64, 64);
                EditorGUI.DrawRect(previewRect, new Color(0.15f, 0.15f, 0.15f));
                EditorGUI.LabelField(previewRect, "no\npreview", EditorStyles.centeredGreyMiniLabel);
            }

            GUILayout.Space(8);

            EditorGUILayout.BeginVertical();

            GUILayout.Label($"Size: {entry.width}x{entry.height}", EditorStyles.miniLabel);
            GUILayout.Label($"Credits: {entry.credits_cost}", EditorStyles.miniLabel);
            GUILayout.Label($"Model: {entry.model}", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical();

            if (entry.status == "completed")
            {
                if (hasLocalAsset)
                {
                    if (GUILayout.Button("Select", GUILayout.Width(64), GUILayout.Height(22)))
                    {
                        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(localPath);
                        if (tex != null)
                        {
                            Selection.activeObject = tex;
                            EditorGUIUtility.PingObject(tex);
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("Import", GUILayout.Width(64), GUILayout.Height(22)))
                    {
                        _ = ImportHistoryEntryAsync(entry);
                    }
                }
            }

            if (GUILayout.Button("Delete", GUILayout.Width(64), GUILayout.Height(22)))
            {
                SpritesynthHistory.RemoveEntry(entry.job_id);
                EditorApplication.delayCall += Repaint;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }

        private async Task ImportHistoryEntryAsync(HistoryEntry entry)
        {
            try
            {
                string apiKey = SpritesynthSettings.ApiKey;
                if (string.IsNullOrEmpty(apiKey))
                {
                    EditorUtility.DisplayDialog("Error", "API key is not set.", "OK");
                    return;
                }

                var client = new SpritesynthClient(apiKey, SpritesynthSettings.BaseUrl);
                byte[] pngData = await client.DownloadAssetAsync(entry.asset_url);

                var result = new GenerationResult
                {
                    id = entry.job_id,
                    status = entry.status,
                    asset = new AssetInfo
                    {
                        url = entry.asset_url,
                        width = entry.width,
                        height = entry.height,
                    },
                    credits_cost = entry.credits_cost,
                    duration_ms = entry.duration_ms,
                };

                string localPath = SpritesynthImporter.Import(pngData, entry.prompt, result);

                entry.local_path = localPath;
                SpritesynthHistory.RemoveEntry(entry.job_id);
                SpritesynthHistory.AddEntry(entry);

                EditorApplication.delayCall += Repaint;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SpriteSynth] Import failed: {ex}");
                EditorUtility.DisplayDialog("Import Failed", ex.Message, "OK");
            }
        }

        private void DrawSettingsTab()
        {
            _settingsScrollPos = EditorGUILayout.BeginScrollView(_settingsScrollPos);

            GUILayout.Label("API Key", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _apiKeyField = EditorGUILayout.PasswordField(_apiKeyField);

            if (GUILayout.Button("Save", GUILayout.Width(60), GUILayout.Height(18)))
            {
                SpritesynthSettings.ApiKey = _apiKeyField;
                EditorUtility.DisplayDialog("Saved", "API key saved to EditorPrefs.", "OK");
            }
            EditorGUILayout.EndHorizontal();

            if (SpritesynthSettings.HasEnvVar)
            {
                EditorGUILayout.HelpBox(
                    "Using SPIRESYNTH_API_KEY environment variable",
                    MessageType.Info);
            }

            if (SpritesynthSettings.IsUsingEnvVar)
            {
                EditorGUILayout.HelpBox(
                    "No saved key — falling back to environment variable.",
                    MessageType.Warning);
            }

            GUILayout.Space(12);

            GUILayout.Label("API Base URL", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _baseUrlField = EditorGUILayout.TextField(_baseUrlField);
            if (GUILayout.Button("Save", GUILayout.Width(60), GUILayout.Height(18)))
            {
                SpritesynthSettings.BaseUrl = _baseUrlField;
                EditorUtility.DisplayDialog("Saved", "Base URL saved.", "OK");
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(12);

            if (GUILayout.Button("Test Connection", GUILayout.Height(24)))
            {
                _ = TestConnectionAsync();
            }

            if (!string.IsNullOrEmpty(_connectionStatus))
            {
                bool isSuccess = _connectionStatus.Contains("Success");
                EditorGUILayout.HelpBox(
                    _connectionStatus,
                    isSuccess ? MessageType.Info : MessageType.Error);
            }

            GUILayout.Space(16);

            if (GUILayout.Button("Clear All History", GUILayout.Height(24)))
            {
                if (EditorUtility.DisplayDialog(
                    "Clear History",
                    "Delete all generation history entries? This cannot be undone.",
                    "Clear", "Cancel"))
                {
                    SpritesynthHistory.Clear();
                    EditorApplication.delayCall += Repaint;
                }
            }

            GUILayout.Space(24);

            GUILayout.Label("About", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Package", "com.magebase.spritesynth");
            EditorGUILayout.LabelField("Version", "1.0.0");
            EditorGUILayout.LabelField("Unity", "2022.3+");

            EditorGUILayout.SelectableLabel(
                "https://spritesynth.com",
                EditorStyles.linkLabel,
                GUILayout.Height(18));

            EditorGUILayout.EndScrollView();
        }

        private async Task TestConnectionAsync()
        {
            string apiKey = SpritesynthSettings.ApiKey;
            if (string.IsNullOrEmpty(apiKey))
            {
                _connectionStatus = "Error: API key is not set.";
                return;
            }

            try
            {
                var client = new SpritesynthClient(apiKey, SpritesynthSettings.BaseUrl);

                var request = new CreateImageRequest
                {
                    description = "test connection pixel art 16x16",
                    image_size = "16x16",
                };

                CreateImageResponse response = await client.CreateImageAsync(request);

                _connectionStatus = $"Success! API is reachable (job: {response.job_id}, status: {response.status})";
            }
            catch (Exception ex)
            {
                _connectionStatus = $"Connection failed: {ex.Message}";
            }
        }
    }
}
