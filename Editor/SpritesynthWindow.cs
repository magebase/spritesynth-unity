using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Magebase.Spritesynth.Editor
{
    public class SpritesynthWindow : EditorWindow
    {
        private int _selectedTab;
        private readonly string[] _tabNames = { "Generate", "Manage", "Tools & Settings" };

        [MenuItem("Tools/SpriteSynth/Generator")]
        public static void ShowWindow()
        {
            SpritesynthWindow window = GetWindow<SpritesynthWindow>("SpriteSynth");
            window.minSize = new Vector2(520, 600);
            window.Show();
        }

        private void OnGUI()
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            switch (_selectedTab)
            {
                case 0: DrawGenerateTab(); break;
                case 1: DrawManageTab(); break;
                case 2: DrawToolsSettingsTab(); break;
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

        // ===== Tab 1: Generate =====

        private string _prompt = "";
        private string _imageSize = "128x128";
        private string _negativePrompt = "";
        private int _seed;
        private string _styleImageUrl = "";
        private string _uiStyle = "modern";
        private string _projectId = "";
        private int _genModeIndex;

        private readonly string[] _genModes = {
            "Create Image", "Style Reference", "UI Elements", "Preview"
        };

        private bool _isGenerating;
        private float _progress;
        private string _statusMessage = "";
        private Texture2D _previewTexture;
        private Vector2 _generateScrollPos;

        private const string PromptPref = "Spritesynth_Generate_Prompt";
        private const string ImageSizePref = "Spritesynth_Generate_ImageSize";
        private const string NegativePromptPref = "Spritesynth_Generate_NegativePrompt";
        private const string SeedPref = "Spritesynth_Generate_Seed";
        private const string GenModePref = "Spritesynth_Generate_Mode";
        private const string ProjectIdPref = "Spritesynth_Generate_ProjectId";

        private void LoadGenerateFields()
        {
            _prompt = EditorPrefs.GetString(PromptPref, "a 16x16 pixel art knight with blue armor");
            _imageSize = EditorPrefs.GetString(ImageSizePref, "128x128");
            _negativePrompt = EditorPrefs.GetString(NegativePromptPref, "");
            _seed = EditorPrefs.GetInt(SeedPref, 0);
            _genModeIndex = EditorPrefs.GetInt(GenModePref, 0);
            _projectId = EditorPrefs.GetString(ProjectIdPref, "");
        }

        private void SaveGenerateFields()
        {
            EditorPrefs.SetString(PromptPref, _prompt);
            EditorPrefs.SetString(ImageSizePref, _imageSize);
            EditorPrefs.SetString(NegativePromptPref, _negativePrompt);
            EditorPrefs.SetInt(SeedPref, _seed);
            EditorPrefs.SetInt(GenModePref, _genModeIndex);
            EditorPrefs.SetString(ProjectIdPref, _projectId);
        }

        private void DrawGenerateTab()
        {
            _generateScrollPos = EditorGUILayout.BeginScrollView(_generateScrollPos);

            GUILayout.Label("Generation Mode", EditorStyles.boldLabel);
            _genModeIndex = GUILayout.Toolbar(_genModeIndex, _genModes);
            GUILayout.Space(8);

            GUILayout.Label("Prompt", EditorStyles.boldLabel);
            _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.Height(60));
            GUILayout.Space(4);

            if (_genModeIndex == 1)
            {
                GUILayout.Label("Style Image URL", EditorStyles.boldLabel);
                _styleImageUrl = EditorGUILayout.TextField(_styleImageUrl);
                GUILayout.Space(4);
            }

            if (_genModeIndex == 2)
            {
                GUILayout.Label("UI Style (optional)", EditorStyles.boldLabel);
                _uiStyle = EditorGUILayout.TextField(_uiStyle);
                GUILayout.Space(4);
            }

            GUILayout.Label("Image Size (e.g. 128x128)", EditorStyles.boldLabel);
            _imageSize = EditorGUILayout.TextField(_imageSize);
            GUILayout.Space(4);

            GUILayout.Label("Negative Prompt (optional)", EditorStyles.boldLabel);
            _negativePrompt = EditorGUILayout.TextArea(_negativePrompt, GUILayout.Height(40));
            GUILayout.Space(4);

            _seed = EditorGUILayout.IntField("Seed (0 = random)", _seed);
            GUILayout.Space(4);

            GUILayout.Label("Project ID (optional)", EditorStyles.boldLabel);
            _projectId = EditorGUILayout.TextField(_projectId);
            GUILayout.Space(8);

            string apiKey = SpritesynthSettings.ApiKey;
            bool hasKey = !string.IsNullOrEmpty(apiKey);

            using (new EditorGUI.DisabledGroupScope(_isGenerating || !hasKey))
            {
                if (!hasKey)
                    EditorGUILayout.HelpBox("Set your API key in the Tools & Settings tab first.", MessageType.Warning);

                if (GUILayout.Button("Generate", GUILayout.Height(36)))
                    _ = GenerateAsync();
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

            if (_previewTexture != null)
            {
                GUILayout.Space(8);
                GUILayout.Label("Preview:", EditorStyles.boldLabel);
                float maxW = Mathf.Min(_previewTexture.width, 256);
                float maxH = Mathf.Min(_previewTexture.height, 256);
                GUILayout.Box(_previewTexture, GUILayout.Width(maxW), GUILayout.Height(maxH));
            }

            EditorGUILayout.EndScrollView();
        }

        private async Task GenerateAsync()
        {
            if (_isGenerating) return;

            string apiKey = SpritesynthSettings.ApiKey;
            if (string.IsNullOrEmpty(apiKey))
            {
                _statusMessage = "API key is not set. Go to Tools & Settings tab.";
                return;
            }

            _isGenerating = true;
            _progress = 0f;
            _statusMessage = "Preparing request...";
            _previewTexture = null;
            Repaint();

            try
            {
                var client = new SpritesynthClient(apiKey, SpritesynthSettings.BaseUrl);
                CreateImageResponse createResponse;

                _statusMessage = "Sending to SpriteSynth API...";
                _progress = 0.1f;
                Repaint();

                switch (_genModeIndex)
                {
                    case 0:
                        createResponse = await client.CreateImageAsync(new CreateImageRequest
                        {
                            description = _prompt,
                            image_size = string.IsNullOrEmpty(_imageSize) ? "128x128" : _imageSize,
                            seed = _seed,
                            negative_prompt = _negativePrompt,
                            project_id = string.IsNullOrEmpty(_projectId) ? null : _projectId,
                        });
                        break;
                    case 1:
                        createResponse = await client.CreateWithStyleAsync(new StyleGenRequest
                        {
                            description = _prompt,
                            style_image = _styleImageUrl,
                            image_size = string.IsNullOrEmpty(_imageSize) ? "128x128" : _imageSize,
                            seed = _seed,
                            project_id = string.IsNullOrEmpty(_projectId) ? null : _projectId,
                        });
                        break;
                    case 2:
                        createResponse = await client.CreateUiAsync(new UiGenRequest
                        {
                            description = _prompt,
                            image_size = string.IsNullOrEmpty(_imageSize) ? "128x128" : _imageSize,
                            style = _uiStyle,
                            seed = _seed,
                            project_id = string.IsNullOrEmpty(_projectId) ? null : _projectId,
                        });
                        break;
                    default:
                        createResponse = await client.PreviewAsync(new PreviewRequest
                        {
                            description = _prompt,
                            image_size = string.IsNullOrEmpty(_imageSize) ? "128x128" : _imageSize,
                        });
                        break;
                }

                _statusMessage = $"Queued (job: {createResponse.job_id})...";
                _progress = 0.2f;
                Repaint();

                GenerationResponse result = await client.PollGenerationAsync(createResponse.job_id);

                if (result.status != "completed")
                {
                    _statusMessage = $"Generation failed: {result.status}";
                    _progress = 0f;
                    return;
                }

                string assetUrl = result.asset?.cdn_url ?? result.asset?.url;
                if (string.IsNullOrEmpty(assetUrl))
                {
                    _statusMessage = "Generation completed but no asset URL returned.";
                    _progress = 0f;
                    return;
                }

                _statusMessage = "Downloading asset...";
                _progress = 0.7f;
                Repaint();

                byte[] pngData = await client.DownloadAssetAsync(assetUrl);

                _statusMessage = "Importing to project...";
                _progress = 0.9f;
                Repaint();

                string localPath = SpritesynthImporter.Import(pngData, _prompt, result);

                var entry = new HistoryEntry
                {
                    job_id = createResponse.job_id,
                    prompt = _prompt,
                    image_size = _imageSize,
                    negative_prompt = _negativePrompt,
                    seed = _seed,
                    model = result.model ?? "fast",
                    generation_type = _genModes[_genModeIndex],
                    status = "completed",
                    asset_url = assetUrl,
                    width = result.asset?.width ?? 0,
                    height = result.asset?.height ?? 0,
                    credits_cost = result.credits_cost,
                    duration_ms = result.duration_ms,
                    date = DateTime.UtcNow.ToString("o"),
                    local_path = localPath,
                };

                SpritesynthHistory.AddEntry(entry);

                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(localPath);
                _previewTexture = tex;

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

        // ===== Tab 2: Manage =====

        private int _manageCategoryIndex;
        private readonly string[] _manageCategories = { "Characters", "Objects", "Tilesets", "Projects" };
        private Vector2 _manageScrollPos;

        private PaginatedResponse<CharacterResponse> _characters;
        private PaginatedResponse<ObjectResponse> _objects;
        private PaginatedResponse<TilesetResponse> _tilesets;
        private PaginatedResponse<ProjectResponse> _projects;

        private int _managePage = 1;
        private string _manageSearch = "";
        private bool _manageLoading;
        private string _manageStatus = "";

        private CharacterResponse _selectedCharacter;
        private ObjectResponse _selectedObject;
        private TilesetResponse _selectedTileset;
        private ProjectResponse _selectedProject;
        private int _selectedStateIndex;

        private bool _showCreateForm;
        private string _createName = "";
        private string _createDescription = "";
        private string _createType = "top_down";
        private int _createTileSize = 16;
        private int _createDirCount = 1;

        private void DrawManageTab()
        {
            _manageScrollPos = EditorGUILayout.BeginScrollView(_manageScrollPos);

            GUILayout.Label("Asset Management", EditorStyles.boldLabel);
            _manageCategoryIndex = GUILayout.Toolbar(_manageCategoryIndex, _manageCategories);
            GUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            _manageSearch = EditorGUILayout.TextField("Search", _manageSearch);
            if (GUILayout.Button("Refresh", GUILayout.Width(70)))
                _ = RefreshManageListAsync();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);

            if (_manageLoading)
            {
                EditorGUILayout.HelpBox("Loading...", MessageType.Info);
            }
            else if (!string.IsNullOrEmpty(_manageStatus))
            {
                EditorGUILayout.HelpBox(_manageStatus, _manageStatus.Contains("Error") ? MessageType.Error : MessageType.Info);
            }

            EditorGUILayout.BeginHorizontal();
            if (_managePage > 1 && GUILayout.Button("Prev", GUILayout.Width(60)))
            {
                _managePage--;
                _ = RefreshManageListAsync();
            }
            GUILayout.Label($"Page {_managePage}", EditorStyles.miniLabel);
            if (GUILayout.Button("Next", GUILayout.Width(60)))
            {
                _managePage++;
                _ = RefreshManageListAsync();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ Create New", GUILayout.Width(100)))
                _showCreateForm = !_showCreateForm;
            EditorGUILayout.EndHorizontal();

            if (_showCreateForm)
                DrawCreateForm();

            GUILayout.Space(6);

            switch (_manageCategoryIndex)
            {
                case 0: DrawCharacterList(); break;
                case 1: DrawObjectList(); break;
                case 2: DrawTilesetList(); break;
                case 3: DrawProjectList(); break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawCreateForm()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Create New", EditorStyles.boldLabel);
            _createName = EditorGUILayout.TextField("Name", _createName);
            _createDescription = EditorGUILayout.TextField("Description", _createDescription);

            if (_manageCategoryIndex == 0)
            {
                _createDirCount = EditorGUILayout.IntField("Direction Count", _createDirCount);
            }
            else if (_manageCategoryIndex == 2)
            {
                _createType = EditorGUILayout.TextField("Type (top_down/sidescroller/isometric)", _createType);
                _createTileSize = EditorGUILayout.IntField("Tile Size", _createTileSize);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create"))
                _ = CreateAssetAsync();
            if (GUILayout.Button("Cancel"))
                _showCreateForm = false;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.Space(6);
        }

        private async Task CreateAssetAsync()
        {
            string apiKey = SpritesynthSettings.ApiKey;
            if (string.IsNullOrEmpty(apiKey)) return;

            try
            {
                _manageLoading = true;
                var client = new SpritesynthClient(apiKey, SpritesynthSettings.BaseUrl);

                switch (_manageCategoryIndex)
                {
                    case 0:
                        await client.CreateCharacterAsync(new CharacterRequest
                        {
                            name = _createName,
                            description = _createDescription,
                            direction_count = _createDirCount,
                        });
                        break;
                    case 1:
                        await client.CreateObjectAsync(new ObjectRequest
                        {
                            name = _createName,
                            description = _createDescription,
                        });
                        break;
                    case 2:
                        await client.CreateTilesetAsync(new TilesetRequest
                        {
                            name = _createName,
                            description = _createDescription,
                            type = _createType,
                            tile_size = _createTileSize,
                        });
                        break;
                    case 3:
                        await client.CreateProjectAsync(new ProjectRequest
                        {
                            name = _createName,
                            description = _createDescription,
                        });
                        break;
                }

                _showCreateForm = false;
                _createName = "";
                _createDescription = "";
                await RefreshManageListAsync();
            }
            catch (Exception ex)
            {
                _manageStatus = $"Error: {ex.Message}";
            }
            finally
            {
                _manageLoading = false;
            }
        }

        private async Task RefreshManageListAsync()
        {
            string apiKey = SpritesynthSettings.ApiKey;
            if (string.IsNullOrEmpty(apiKey)) return;

            _manageLoading = true;
            _manageStatus = "Loading...";
            _selectedCharacter = null;
            _selectedObject = null;
            _selectedTileset = null;
            _selectedProject = null;
            Repaint();

            try
            {
                var client = new SpritesynthClient(apiKey, SpritesynthSettings.BaseUrl);
                var p = new ListParams
                {
                    per_page = 20,
                    page = _managePage,
                    search = _manageSearch,
                };

                switch (_manageCategoryIndex)
                {
                    case 0:
                        _characters = await client.ListCharactersAsync(p);
                        _manageStatus = $"Characters: {_characters?.meta?.total ?? 0} total";
                        break;
                    case 1:
                        _objects = await client.ListObjectsAsync(p);
                        _manageStatus = $"Objects: {_objects?.meta?.total ?? 0} total";
                        break;
                    case 2:
                        _tilesets = await client.ListTilesetsAsync(p);
                        _manageStatus = $"Tilesets: {_tilesets?.meta?.total ?? 0} total";
                        break;
                    case 3:
                        _projects = await client.ListProjectsAsync(p);
                        _manageStatus = $"Projects: {_projects?.meta?.total ?? 0} total";
                        break;
                }
            }
            catch (Exception ex)
            {
                _manageStatus = $"Error: {ex.Message}";
            }
            finally
            {
                _manageLoading = false;
                Repaint();
            }
        }

        private void DrawCharacterList()
        {
            if (_selectedCharacter != null)
            {
                DrawCharacterDetail();
                return;
            }

            if (_characters?.data == null || _characters.data.Count == 0)
            {
                EditorGUILayout.HelpBox("No characters found. Create one or adjust filters.", MessageType.Info);
                return;
            }

            foreach (var c in _characters.data)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(c.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Directions: {c.direction_count}  |  States: {c.states?.Count ?? 0}");
                if (!string.IsNullOrEmpty(c.description))
                    EditorGUILayout.LabelField(c.description, EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Open", GUILayout.Width(60)))
                    _selectedCharacter = c;
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    _ = DeleteAssetAsync(c.uuid, "characters");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.Space(2);
            }
        }

        private void DrawCharacterDetail()
        {
            var c = _selectedCharacter;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(c.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"UUID: {c.uuid}");
            EditorGUILayout.LabelField($"Directions: {c.direction_count}");
            EditorGUILayout.LabelField($"Width: {c.width}  Height: {c.height}");

            if (c.states != null && c.states.Count > 0)
            {
                GUILayout.Space(6);
                GUILayout.Label("States:", EditorStyles.boldLabel);
                var stateNames = c.states.Select(s => $"{s.name} ({s.direction})").ToArray();
                _selectedStateIndex = GUILayout.SelectionGrid(_selectedStateIndex, stateNames, 2);

                if (_selectedStateIndex >= 0 && _selectedStateIndex < c.states.Count)
                {
                    var s = c.states[_selectedStateIndex];
                    EditorGUILayout.LabelField($"State: {s.name}");
                    EditorGUILayout.LabelField($"Direction: {s.direction}");
                    EditorGUILayout.LabelField($"Frames: {s.frame_count}  Duration: {s.frame_duration_ms}ms");
                }
            }

            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export ZIP"))
                _ = ExportAssetAsync(c.uuid, "characters");
            if (GUILayout.Button("Duplicate"))
                _ = DuplicateAssetAsync(c.uuid, "characters");
            if (GUILayout.Button("Back"))
                _selectedCharacter = null;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawObjectList()
        {
            if (_selectedObject != null)
            {
                DrawObjectDetail();
                return;
            }

            if (_objects?.data == null || _objects.data.Count == 0)
            {
                EditorGUILayout.HelpBox("No objects found. Create one or adjust filters.", MessageType.Info);
                return;
            }

            foreach (var o in _objects.data)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(o.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Directions: {o.direction_count}  |  States: {o.states?.Count ?? 0}");
                if (!string.IsNullOrEmpty(o.description))
                    EditorGUILayout.LabelField(o.description, EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Open", GUILayout.Width(60)))
                    _selectedObject = o;
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    _ = DeleteAssetAsync(o.uuid, "objects");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.Space(2);
            }
        }

        private void DrawObjectDetail()
        {
            var o = _selectedObject;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(o.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"UUID: {o.uuid}");
            EditorGUILayout.LabelField($"Directions: {o.direction_count}");

            if (o.states != null && o.states.Count > 0)
            {
                GUILayout.Space(6);
                GUILayout.Label("States:", EditorStyles.boldLabel);
                var stateNames = o.states.Select(s => $"{s.name} ({s.direction})").ToArray();
                _selectedStateIndex = GUILayout.SelectionGrid(_selectedStateIndex, stateNames, 2);
            }

            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export ZIP"))
                _ = ExportAssetAsync(o.uuid, "objects");
            if (GUILayout.Button("Duplicate"))
                _ = DuplicateAssetAsync(o.uuid, "objects");
            if (GUILayout.Button("Back"))
                _selectedObject = null;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawTilesetList()
        {
            if (_selectedTileset != null)
            {
                DrawTilesetDetail();
                return;
            }

            if (_tilesets?.data == null || _tilesets.data.Count == 0)
            {
                EditorGUILayout.HelpBox("No tilesets found. Create one or adjust filters.", MessageType.Info);
                return;
            }

            foreach (var t in _tilesets.data)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(t.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Type: {t.type}  |  Tiles: {t.tile_count}  |  Size: {t.tile_size}px");
                if (!string.IsNullOrEmpty(t.description))
                    EditorGUILayout.LabelField(t.description, EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Open", GUILayout.Width(60)))
                    _selectedTileset = t;
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    _ = DeleteAssetAsync(t.uuid, "tilesets");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.Space(2);
            }
        }

        private void DrawTilesetDetail()
        {
            var t = _selectedTileset;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(t.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"UUID: {t.uuid}");
            EditorGUILayout.LabelField($"Type: {t.type}  Size: {t.tile_size}px  Tiles: {t.tile_count}");

            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export ZIP"))
                _ = ExportAssetAsync(t.uuid, "tilesets");
            if (GUILayout.Button("Duplicate"))
                _ = DuplicateAssetAsync(t.uuid, "tilesets");
            if (GUILayout.Button("Back"))
                _selectedTileset = null;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawProjectList()
        {
            if (_selectedProject != null)
            {
                DrawProjectDetail();
                return;
            }

            if (_projects?.data == null || _projects.data.Count == 0)
            {
                EditorGUILayout.HelpBox("No projects found. Create one or adjust filters.", MessageType.Info);
                return;
            }

            foreach (var p in _projects.data)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(p.name, EditorStyles.boldLabel);
                if (!string.IsNullOrEmpty(p.description))
                    EditorGUILayout.LabelField(p.description, EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Open", GUILayout.Width(60)))
                    _selectedProject = p;
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    _ = DeleteAssetAsync(p.uuid, "projects");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.Space(2);
            }
        }

        private void DrawProjectDetail()
        {
            var p = _selectedProject;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(p.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"UUID: {p.uuid}");
            if (!string.IsNullOrEmpty(p.description))
                EditorGUILayout.LabelField(p.description, EditorStyles.wordWrappedLabel);

            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Duplicate"))
                _ = DuplicateAssetAsync(p.uuid, "projects");
            if (GUILayout.Button("Archive"))
                _ = ArchiveProjectAsync(p.uuid);
            if (GUILayout.Button("Back"))
                _selectedProject = null;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private async Task DeleteAssetAsync(string uuid, string type)
        {
            if (!EditorUtility.DisplayDialog("Confirm Delete", $"Delete this {type}? This cannot be undone.", "Delete", "Cancel"))
                return;

            string apiKey = SpritesynthSettings.ApiKey;
            if (string.IsNullOrEmpty(apiKey)) return;

            try
            {
                var client = new SpritesynthClient(apiKey, SpritesynthSettings.BaseUrl);
                switch (type)
                {
                    case "characters": await client.DeleteCharacterAsync(uuid); break;
                    case "objects": await client.DeleteObjectAsync(uuid); break;
                    case "tilesets": await client.DeleteTilesetAsync(uuid); break;
                    case "projects": await client.DeleteProjectAsync(uuid); break;
                }
                await RefreshManageListAsync();
            }
            catch (Exception ex)
            {
                _manageStatus = $"Error: {ex.Message}";
            }
        }

        private async Task ExportAssetAsync(string uuid, string type)
        {
            string apiKey = SpritesynthSettings.ApiKey;
            if (string.IsNullOrEmpty(apiKey)) return;

            try
            {
                var client = new SpritesynthClient(apiKey, SpritesynthSettings.BaseUrl);
                byte[] data;
                string ext = ".zip";

                switch (type)
                {
                    case "characters": data = await client.ExportCharacterZipAsync(uuid); break;
                    case "objects": data = await client.ExportObjectZipAsync(uuid); break;
                    case "tilesets": data = await client.ExportTilesetZipAsync(uuid); break;
                    default: return;
                }

                string path = EditorUtility.SaveFilePanel($"Export {type}", "", $"{type}_{uuid.Substring(0, 8)}{ext}", "zip");
                if (!string.IsNullOrEmpty(path))
                {
                    File.WriteAllBytes(path, data);
                    EditorUtility.RevealInFinder(path);
                }
            }
            catch (Exception ex)
            {
                _manageStatus = $"Export error: {ex.Message}";
            }
        }

        private async Task DuplicateAssetAsync(string uuid, string type)
        {
            string apiKey = SpritesynthSettings.ApiKey;
            if (string.IsNullOrEmpty(apiKey)) return;

            try
            {
                var client = new SpritesynthClient(apiKey, SpritesynthSettings.BaseUrl);
                switch (type)
                {
                    case "characters": await client.DuplicateCharacterAsync(uuid); break;
                    case "objects": await client.DuplicateObjectAsync(uuid); break;
                    case "tilesets": await client.DuplicateTilesetAsync(uuid); break;
                    case "projects": await client.DuplicateProjectAsync(uuid); break;
                }
                _manageStatus = "Duplicated successfully!";
                await RefreshManageListAsync();
            }
            catch (Exception ex)
            {
                _manageStatus = $"Duplicate error: {ex.Message}";
            }
        }

        private async Task ArchiveProjectAsync(string uuid)
        {
            string apiKey = SpritesynthSettings.ApiKey;
            if (string.IsNullOrEmpty(apiKey)) return;

            try
            {
                var client = new SpritesynthClient(apiKey, SpritesynthSettings.BaseUrl);
                await client.ArchiveProjectAsync(uuid);
                _manageStatus = "Project archived!";
                await RefreshManageListAsync();
            }
            catch (Exception ex)
            {
                _manageStatus = $"Archive error: {ex.Message}";
            }
        }

        // ===== Tab 3: Tools & Settings =====

        private int _toolsTab;
        private readonly string[] _toolsTabNames = { "Image Ops", "History", "Settings" };
        private Vector2 _toolsScrollPos;

        // Image Ops fields
        private string _opImageUrl = "";
        private string _opPrompt = "";
        private string _opMaskUrl = "";
        private int _opPixelSize = 8;
        private int _opWidth = 128;
        private int _opHeight = 128;
        private int _opDegrees = 90;
        private bool _opExpand;
        private float _opStrength = 0.8f;
        private int _opSelectedIndex;
        private readonly string[] _opNames = {
            "To Pixel Art", "Resize", "Remove BG", "Inpaint", "Edit", "Rotate"
        };

        private bool _isOpsRunning;
        private string _opsStatus = "";

        // Settings fields
        private string _apiKeyField = "";
        private string _baseUrlField = "";
        private string _connectionStatus = "";
        private string _balanceStatus = "";

        private void DrawToolsSettingsTab()
        {
            _toolsScrollPos = EditorGUILayout.BeginScrollView(_toolsScrollPos);
            _toolsTab = GUILayout.Toolbar(_toolsTab, _toolsTabNames);
            GUILayout.Space(6);

            switch (_toolsTab)
            {
                case 0: DrawImageOpsTab(); break;
                case 1: DrawHistoryTab(); break;
                case 2: DrawSettingsTab(); break;
            }

            EditorGUILayout.EndScrollView();
        }

        // ---- Image Ops ----

        private void DrawImageOpsTab()
        {
            GUILayout.Label("Image Operations", EditorStyles.boldLabel);
            _opSelectedIndex = GUILayout.Toolbar(_opSelectedIndex, _opNames);
            GUILayout.Space(8);

            GUILayout.Label("Image URL", EditorStyles.boldLabel);
            _opImageUrl = EditorGUILayout.TextField(_opImageUrl);
            GUILayout.Space(4);

            if (_opSelectedIndex == 0)
            {
                _opPixelSize = EditorGUILayout.IntField("Pixel Size", _opPixelSize);
            }
            else if (_opSelectedIndex == 1)
            {
                _opWidth = EditorGUILayout.IntField("Width", _opWidth);
                _opHeight = EditorGUILayout.IntField("Height", _opHeight);
            }
            else if (_opSelectedIndex == 3)
            {
                GUILayout.Label("Mask URL (optional)", EditorStyles.boldLabel);
                _opMaskUrl = EditorGUILayout.TextField(_opMaskUrl);
                GUILayout.Label("Prompt", EditorStyles.boldLabel);
                _opPrompt = EditorGUILayout.TextArea(_opPrompt, GUILayout.Height(40));
            }
            else if (_opSelectedIndex == 4)
            {
                GUILayout.Label("Prompt", EditorStyles.boldLabel);
                _opPrompt = EditorGUILayout.TextArea(_opPrompt, GUILayout.Height(40));
                _opStrength = EditorGUILayout.Slider("Strength", _opStrength, 0.1f, 1f);
            }
            else if (_opSelectedIndex == 5)
            {
                _opDegrees = EditorGUILayout.IntField("Degrees", _opDegrees);
                _opExpand = EditorGUILayout.Toggle("Expand Canvas", _opExpand);
            }

            GUILayout.Space(8);

            string apiKey = SpritesynthSettings.ApiKey;
            using (new EditorGUI.DisabledGroupScope(_isOpsRunning || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(_opImageUrl)))
            {
                if (GUILayout.Button("Run", GUILayout.Height(30)))
                    _ = RunImageOpAsync();
            }

            if (_isOpsRunning)
                EditorGUILayout.HelpBox("Processing...", MessageType.Info);
            else if (!string.IsNullOrEmpty(_opsStatus))
                EditorGUILayout.HelpBox(_opsStatus, _opsStatus.Contains("Error") ? MessageType.Error : MessageType.Info);
        }

        private async Task RunImageOpAsync()
        {
            if (_isOpsRunning) return;

            string apiKey = SpritesynthSettings.ApiKey;
            if (string.IsNullOrEmpty(apiKey))
            {
                _opsStatus = "Error: API key not set.";
                return;
            }

            _isOpsRunning = true;
            _opsStatus = "Running...";
            Repaint();

            try
            {
                var client = new SpritesynthClient(apiKey, SpritesynthSettings.BaseUrl);
                var req = new ImageOpRequest
                {
                    image = _opImageUrl,
                    pixel_size = _opPixelSize,
                    width = _opWidth,
                    height = _opHeight,
                    mask = _opMaskUrl,
                    prompt = _opPrompt,
                    strength = _opStrength,
                    degrees = _opDegrees,
                    expand = _opExpand,
                };

                CreateImageResponse createResponse;

                switch (_opSelectedIndex)
                {
                    case 0: createResponse = await client.ToPixelArtAsync(req); break;
                    case 1: createResponse = await client.ResizeAsync(req); break;
                    case 2: createResponse = await client.RemoveBackgroundAsync(req); break;
                    case 3: createResponse = await client.InpaintAsync(req); break;
                    case 4: createResponse = await client.EditAsync(req); break;
                    case 5: createResponse = await client.RotateAsync(req); break;
                    default: return;
                }

                GenerationResponse result = await client.PollGenerationAsync(createResponse.job_id);

                if (result.status == "completed")
                {
                    string assetUrl = result.asset?.cdn_url ?? result.asset?.url;
                    if (!string.IsNullOrEmpty(assetUrl))
                    {
                        byte[] pngData = await client.DownloadAssetAsync(assetUrl);
                        string opName = _opNames[_opSelectedIndex].Replace(" ", "_").ToLower();
                        string localPath = SpritesynthImporter.Import(pngData, opName, result);
                        _opsStatus = $"Success! Imported to {localPath}";
                    }
                    else
                    {
                        _opsStatus = "Completed but no asset URL.";
                    }
                }
                else
                {
                    _opsStatus = $"Failed: {result.status}";
                }
            }
            catch (Exception ex)
            {
                _opsStatus = $"Error: {ex.Message}";
            }
            finally
            {
                _isOpsRunning = false;
                Repaint();
            }
        }

        // ---- History ----

        private Vector2 _historyScrollPos;

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
                    GUILayout.Box(preview, GUILayout.Width(64), GUILayout.Height(64));
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
                        _ = ImportHistoryEntryAsync(entry);
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

                var result = new GenerationResponse
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

        // ---- Settings ----

        private void DrawSettingsTab()
        {
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
                EditorGUILayout.HelpBox("Using SPIRESYNTH_API_KEY environment variable", MessageType.Info);

            if (SpritesynthSettings.IsUsingEnvVar)
                EditorGUILayout.HelpBox("No saved key — falling back to environment variable.", MessageType.Warning);

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

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Connection", GUILayout.Height(24)))
                _ = TestConnectionAsync();
            if (GUILayout.Button("Get Balance", GUILayout.Height(24)))
                _ = GetBalanceAsync();
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_connectionStatus))
                EditorGUILayout.HelpBox(_connectionStatus,
                    _connectionStatus.Contains("Success") ? MessageType.Info : MessageType.Error);

            if (!string.IsNullOrEmpty(_balanceStatus))
                EditorGUILayout.HelpBox(_balanceStatus, MessageType.Info);

            GUILayout.Space(16);

            if (GUILayout.Button("Clear All History", GUILayout.Height(24)))
            {
                if (EditorUtility.DisplayDialog("Clear History",
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
                var response = await client.CreateImageAsync(new CreateImageRequest
                {
                    description = "test connection pixel art 16x16",
                    image_size = "16x16",
                });
                _connectionStatus = $"Success! API reachable (job: {response.job_id}, status: {response.status})";
            }
            catch (Exception ex)
            {
                _connectionStatus = $"Connection failed: {ex.Message}";
            }
        }

        private async Task GetBalanceAsync()
        {
            string apiKey = SpritesynthSettings.ApiKey;
            if (string.IsNullOrEmpty(apiKey))
            {
                _balanceStatus = "Error: API key not set.";
                return;
            }

            try
            {
                var client = new SpritesynthClient(apiKey, SpritesynthSettings.BaseUrl);
                var balance = await client.GetAccountBalanceAsync();
                _balanceStatus = $"Credits: {balance.credits_balance}";
            }
            catch (Exception ex)
            {
                _balanceStatus = $"Error: {ex.Message}";
            }
        }

        private void LoadSettingsFields()
        {
            _apiKeyField = SpritesynthSettings.ApiKey;
            _baseUrlField = SpritesynthSettings.BaseUrl;
        }
    }
}
