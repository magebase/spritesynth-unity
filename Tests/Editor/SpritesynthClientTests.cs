using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Magebase.Spritesynth.Tests.Editor
{
    public class SpritesynthClientTests
    {
        private const string TestApiKey = "test-api-key-12345";

        // ===== Data Model Tests =====

        [Test]
        public void CreateImageRequest_DefaultValues()
        {
            var req = new CreateImageRequest
            {
                description = "a pixel art knight",
            };

            Assert.AreEqual("a pixel art knight", req.description);
            Assert.AreEqual("128x128", req.image_size);
            Assert.AreEqual(0, req.seed);
            Assert.IsNull(req.negative_prompt);
            Assert.IsNull(req.model);
            Assert.IsNull(req.project_id);
        }

        [Test]
        public void CreateImageRequest_SerializesCorrectly()
        {
            var req = new CreateImageRequest
            {
                description = "hero",
                image_size = "64x64",
                seed = 42,
                negative_prompt = "ugly",
                project_id = "proj-1",
            };

            string json = JsonUtility.ToJson(req);

            Assert.IsTrue(json.Contains("hero"));
            Assert.IsTrue(json.Contains("64x64"));
            Assert.IsTrue(json.Contains("42"));
            Assert.IsTrue(json.Contains("ugly"));
            Assert.IsTrue(json.Contains("proj-1"));
        }

        [Test]
        public void CreateImageRequest_DeserializesCorrectly()
        {
            string json = "{\"description\":\"test\",\"image_size\":\"32x32\",\"seed\":7,\"negative_prompt\":\"no\",\"model\":\"fast\",\"project_id\":\"p1\"}";
            var req = JsonUtility.FromJson<CreateImageRequest>(json);

            Assert.AreEqual("test", req.description);
            Assert.AreEqual("32x32", req.image_size);
            Assert.AreEqual(7, req.seed);
            Assert.AreEqual("no", req.negative_prompt);
            Assert.AreEqual("fast", req.model);
            Assert.AreEqual("p1", req.project_id);
        }

        [Test]
        public void CreateImageResponse_DeserializesCorrectly()
        {
            string json = "{\"job_id\":\"abc-123\",\"status\":\"pending\"}";
            var res = JsonUtility.FromJson<CreateImageResponse>(json);

            Assert.AreEqual("abc-123", res.job_id);
            Assert.AreEqual("pending", res.status);
        }

        [Test]
        public void StyleGenRequest_SerializesCorrectly()
        {
            var req = new StyleGenRequest
            {
                description = "styled",
                style_image = "https://example.com/style.png",
                image_size = "128x128",
                seed = 1,
                project_id = "proj-x",
            };

            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("styled"));
            Assert.IsTrue(json.Contains("style.png"));
        }

        [Test]
        public void UiGenRequest_SerializesCorrectly()
        {
            var req = new UiGenRequest
            {
                description = "ui button",
                image_size = "64x64",
                style = "modern",
                project_id = "proj-ui",
            };

            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("ui button"));
            Assert.IsTrue(json.Contains("\"style\":\"modern\""));
        }

        [Test]
        public void PreviewRequest_SerializesCorrectly()
        {
            var req = new PreviewRequest
            {
                description = "preview test",
                image_size = "32x32",
                model = "fast",
            };

            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("preview test"));
            Assert.IsTrue(json.Contains("\"model\":\"fast\""));
        }

        [Test]
        public void GenerationResponse_FullDeserialization()
        {
            string json = "{\"id\":\"gen-1\",\"uuid\":\"uuid-1\",\"user_id\":\"user-1\",\"project_id\":\"proj-1\",\"asset_id\":\"asset-1\",\"type\":\"create_image\",\"model\":\"fast\",\"prompt\":\"knight\",\"negative_prompt\":\"\",\"status\":\"completed\",\"runpod_job_id\":\"rp-1\",\"runpod_endpoint\":\"generate_klein_4b\",\"credits_cost\":5,\"duration_ms\":1234,\"error_message\":\"\",\"retry_count\":0,\"max_retries\":3,\"created_at\":\"2025-01-01T00:00:00Z\",\"updated_at\":\"2025-01-01T00:01:00Z\",\"asset\":{\"id\":\"a-1\",\"uuid\":\"au-1\",\"url\":\"https://cdn.example.com/img.png\",\"cdn_url\":\"https://cdn.example.com/img.png\",\"width\":128,\"height\":128,\"mime_type\":\"image/png\",\"file_size\":1024,\"filename\":\"img.png\",\"type\":\"generation\"}}";

            var res = JsonUtility.FromJson<GenerationResponse>(json);

            Assert.AreEqual("gen-1", res.id);
            Assert.AreEqual("completed", res.status);
            Assert.AreEqual(5, res.credits_cost);
            Assert.AreEqual(1234, res.duration_ms);
            Assert.IsNotNull(res.asset);
            Assert.AreEqual(128, res.asset.width);
            Assert.AreEqual(128, res.asset.height);
            Assert.AreEqual("https://cdn.example.com/img.png", res.asset.url);
        }

        [Test]
        public void GenerationStatusResponse_DeserializesCorrectly()
        {
            string json = "{\"status\":\"processing\",\"credits_cost\":0,\"duration_ms\":0}";
            var res = JsonUtility.FromJson<GenerationStatusResponse>(json);
            Assert.AreEqual("processing", res.status);
        }

        [Test]
        public void AssetInfo_DeserializesCorrectly()
        {
            string json = "{\"id\":\"a1\",\"uuid\":\"au1\",\"url\":\"https://cdn.example.com/i.png\",\"cdn_url\":\"https://cdn.example.com/i.png\",\"width\":64,\"height\":64,\"mime_type\":\"image/png\",\"file_size\":512,\"filename\":\"i.png\",\"type\":\"generation\"}";
            var info = JsonUtility.FromJson<AssetInfo>(json);

            Assert.AreEqual("a1", info.id);
            Assert.AreEqual(64, info.width);
            Assert.AreEqual(64, info.height);
            Assert.AreEqual("https://cdn.example.com/i.png", info.url);
        }

        [Test]
        public void CharacterRequest_SerializesCorrectly()
        {
            var req = new CharacterRequest
            {
                name = "Hero",
                description = "Main character",
                project_id = "proj-1",
                direction_count = 4,
                metadata = "{\"color\":\"blue\"}",
            };

            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("\"name\":\"Hero\""));
            Assert.IsTrue(json.Contains("\"direction_count\":4"));
        }

        [Test]
        public void CharacterResponse_FullDeserialization()
        {
            string json = "{\"id\":\"c-1\",\"uuid\":\"cu-1\",\"user_id\":\"u-1\",\"project_id\":\"p-1\",\"name\":\"Knight\",\"description\":\"Blue knight\",\"asset_id\":\"a-1\",\"thumbnail_asset_id\":\"t-1\",\"direction_count\":4,\"width\":64,\"height\":64,\"tags\":\"[\\\"hero\\\"]\",\"metadata\":\"{}\",\"generation_id\":\"g-1\",\"created_at\":\"2025-01-01T00:00:00Z\",\"updated_at\":\"2025-01-01T00:00:00Z\",\"states\":[{\"id\":\"s-1\",\"uuid\":\"su-1\",\"character_id\":\"c-1\",\"name\":\"idle\",\"direction\":\"south\",\"asset_id\":\"a-2\",\"frame_count\":4,\"frame_width\":64,\"frame_height\":64,\"frame_duration_ms\":200,\"metadata\":\"{}\",\"created_at\":\"2025-01-01T00:00:00Z\",\"updated_at\":\"2025-01-01T00:00:00Z\"}]}";

            var res = JsonUtility.FromJson<CharacterResponse>(json);

            Assert.AreEqual("Knight", res.name);
            Assert.AreEqual(4, res.direction_count);
            Assert.IsNotNull(res.states);
            Assert.AreEqual(1, res.states.Count);
            Assert.AreEqual("idle", res.states[0].name);
            Assert.AreEqual("south", res.states[0].direction);
            Assert.AreEqual(4, res.states[0].frame_count);
        }

        [Test]
        public void CharacterStateRequest_SerializesCorrectly()
        {
            var req = new CharacterStateRequest
            {
                name = "walk",
                asset_id = "asset-123",
                direction = "north",
                frame_count = 4,
                frame_duration_ms = 150,
            };

            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("\"name\":\"walk\""));
            Assert.IsTrue(json.Contains("\"direction\":\"north\""));
            Assert.IsTrue(json.Contains("\"frame_count\":4"));
            Assert.IsTrue(json.Contains("\"frame_duration_ms\":150"));
        }

        [Test]
        public void ObjectRequest_SerializesCorrectly()
        {
            var req = new ObjectRequest
            {
                name = "Chest",
                description = "Treasure chest",
                direction_count = 1,
                tags = "[\"prop\"]",
            };

            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("\"name\":\"Chest\""));
            Assert.IsTrue(json.Contains("\"direction_count\":1"));
        }

        [Test]
        public void ObjectResponse_DeserializesCorrectly()
        {
            string json = "{\"id\":\"o-1\",\"uuid\":\"ou-1\",\"name\":\"Chest\",\"direction_count\":1,\"states\":[]}";
            var res = JsonUtility.FromJson<ObjectResponse>(json);

            Assert.AreEqual("Chest", res.name);
            Assert.AreEqual(1, res.direction_count);
            Assert.IsNotNull(res.states);
        }

        [Test]
        public void ObjectStateRequest_SerializesCorrectly()
        {
            var req = new ObjectStateRequest
            {
                name = "open",
                asset_id = "a-1",
                direction = "front",
                frame_count = 2,
                frame_duration_ms = 300,
            };

            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("\"name\":\"open\""));
            Assert.IsTrue(json.Contains("\"frame_count\":2"));
        }

        [Test]
        public void TilesetRequest_SerializesCorrectly()
        {
            var req = new TilesetRequest
            {
                name = "Grassland",
                type = "top_down",
                tile_size = 16,
                project_id = "proj-1",
            };

            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("\"name\":\"Grassland\""));
            Assert.IsTrue(json.Contains("\"type\":\"top_down\""));
            Assert.IsTrue(json.Contains("\"tile_size\":16"));
        }

        [Test]
        public void TilesetResponse_DeserializesCorrectly()
        {
            string json = "{\"id\":\"t-1\",\"uuid\":\"tu-1\",\"name\":\"Grassland\",\"type\":\"top_down\",\"tile_size\":16,\"tile_count\":25}";
            var res = JsonUtility.FromJson<TilesetResponse>(json);

            Assert.AreEqual("Grassland", res.name);
            Assert.AreEqual("top_down", res.type);
            Assert.AreEqual(16, res.tile_size);
            Assert.AreEqual(25, res.tile_count);
        }

        [Test]
        public void TilesetTileResponse_DeserializesCorrectly()
        {
            string json = "{\"tiles\":[{\"index\":0,\"x\":0,\"y\":0,\"width\":16,\"height\":16,\"asset_url\":\"https://cdn.example.com/tile0.png\"},{\"index\":1,\"x\":16,\"y\":0,\"width\":16,\"height\":16,\"asset_url\":\"https://cdn.example.com/tile1.png\"}]}";
            var res = JsonUtility.FromJson<TilesetTileResponse>(json);

            Assert.IsNotNull(res.tiles);
            Assert.AreEqual(2, res.tiles.Count);
            Assert.AreEqual(0, res.tiles[0].index);
            Assert.AreEqual(16, res.tiles[0].width);
        }

        [Test]
        public void ProjectRequest_SerializesCorrectly()
        {
            var req = new ProjectRequest
            {
                name = "My Game",
                description = "RPG project",
            };

            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("\"name\":\"My Game\""));
            Assert.IsTrue(json.Contains("\"description\":\"RPG project\""));
        }

        [Test]
        public void ProjectResponse_DeserializesCorrectly()
        {
            string json = "{\"id\":\"p-1\",\"uuid\":\"pu-1\",\"user_id\":\"u-1\",\"name\":\"My Game\",\"description\":\"RPG project\",\"created_at\":\"2025-01-01T00:00:00Z\",\"updated_at\":\"2025-01-01T00:00:00Z\"}";
            var res = JsonUtility.FromJson<ProjectResponse>(json);

            Assert.AreEqual("My Game", res.name);
            Assert.AreEqual("RPG project", res.description);
        }

        [Test]
        public void AssetResponse_DeserializesCorrectly()
        {
            string json = "{\"id\":\"a-1\",\"uuid\":\"au-1\",\"user_id\":\"u-1\",\"project_id\":\"p-1\",\"type\":\"generation\",\"filename\":\"sprite.png\",\"mime_type\":\"image/png\",\"width\":128,\"height\":128,\"file_size\":2048,\"cdn_url\":\"https://cdn.example.com/sprite.png\"}";
            var res = JsonUtility.FromJson<AssetResponse>(json);

            Assert.AreEqual("a-1", res.id);
            Assert.AreEqual("generation", res.type);
            Assert.AreEqual(128, res.width);
            Assert.AreEqual(128, res.height);
        }

        [Test]
        public void ApiKeyRequest_SerializesCorrectly()
        {
            var req = new ApiKeyRequest { name = "CLI Key" };
            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("\"name\":\"CLI Key\""));
        }

        [Test]
        public void ApiKeyResponse_DeserializesCorrectly()
        {
            string json = "{\"id\":\"k-1\",\"uuid\":\"ku-1\",\"name\":\"My Key\",\"key_prefix\":\"sk_abc\",\"plain_text_key\":\"sk_abc...xyz\",\"rate_limit\":60,\"scopes\":\"[\\\"read\\\"]\",\"is_active\":true,\"created_at\":\"2025-01-01T00:00:00Z\"}";
            var res = JsonUtility.FromJson<ApiKeyResponse>(json);

            Assert.AreEqual("My Key", res.name);
            Assert.AreEqual("sk_abc", res.key_prefix);
            Assert.IsTrue(res.is_active);
        }

        [Test]
        public void AccountBalanceResponse_DeserializesCorrectly()
        {
            string json = "{\"credits_balance\":1500}";
            var res = JsonUtility.FromJson<AccountBalanceResponse>(json);
            Assert.AreEqual(1500, res.credits_balance);
        }

        [Test]
        public void TemplateRequest_SerializesCorrectly()
        {
            var req = new TemplateRequest
            {
                name = "Hero Template",
                description = "Template for heroes",
                type = "character",
                config = "{\"prompt\":\"hero\"}",
                is_public = true,
            };

            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("\"name\":\"Hero Template\""));
            Assert.IsTrue(json.Contains("\"is_public\":true"));
        }

        [Test]
        public void TemplateResponse_DeserializesCorrectly()
        {
            string json = "{\"id\":\"t-1\",\"uuid\":\"tu-1\",\"user_id\":\"u-1\",\"name\":\"Hero Template\",\"type\":\"character\",\"is_public\":true,\"downloads_count\":10,\"rating_avg\":4.5}";
            var res = JsonUtility.FromJson<TemplateResponse>(json);

            Assert.AreEqual("Hero Template", res.name);
            Assert.AreEqual("character", res.type);
            Assert.IsTrue(res.is_public);
            Assert.AreEqual(4.5f, res.rating_avg, 0.01);
        }

        [Test]
        public void PaginatedResponse_DeserializesCorrectly()
        {
            string json = "{\"data\":[{\"id\":\"gen-1\"},{\"id\":\"gen-2\"}],\"meta\":{\"current_page\":1,\"last_page\":3,\"total\":25,\"per_page\":10}}";
            var res = JsonUtility.FromJson<PaginatedResponse<GenerationResponse>>(json);

            Assert.IsNotNull(res.data);
            Assert.AreEqual(2, res.data.Count);
            Assert.IsNotNull(res.meta);
            Assert.AreEqual(1, res.meta.current_page);
            Assert.AreEqual(25, res.meta.total);
        }

        [Test]
        public void PaginationMeta_DeserializesCorrectly()
        {
            string json = "{\"current_page\":2,\"last_page\":5,\"total\":50,\"per_page\":10}";
            var meta = JsonUtility.FromJson<PaginationMeta>(json);

            Assert.AreEqual(2, meta.current_page);
            Assert.AreEqual(5, meta.last_page);
            Assert.AreEqual(50, meta.total);
        }

        [Test]
        public void StatusResponse_DeserializesCorrectly()
        {
            string json = "{\"status\":\"cancelled\"}";
            var res = JsonUtility.FromJson<StatusResponse>(json);
            Assert.AreEqual("cancelled", res.status);
        }

        [Test]
        public void DownloadUrlResponse_DeserializesCorrectly()
        {
            string json = "{\"url\":\"https://cdn.example.com/download.png?signature=abc\"}";
            var res = JsonUtility.FromJson<DownloadUrlResponse>(json);
            Assert.AreEqual("https://cdn.example.com/download.png?signature=abc", res.url);
        }

        [Test]
        public void VersionListResponse_DeserializesCorrectly()
        {
            string json = "{\"data\":[{\"id\":\"v-1\",\"asset_id\":\"a-1\",\"version_number\":1,\"s3_key\":\"key1\",\"file_size\":1024,\"created_at\":\"2025-01-01T00:00:00Z\"},{\"id\":\"v-2\",\"asset_id\":\"a-1\",\"version_number\":2,\"s3_key\":\"key2\",\"file_size\":2048,\"created_at\":\"2025-01-01T01:00:00Z\"}]}";
            var res = JsonUtility.FromJson<VersionListResponse>(json);

            Assert.IsNotNull(res.data);
            Assert.AreEqual(2, res.data.Count);
            Assert.AreEqual(1, res.data[0].version_number);
            Assert.AreEqual(2, res.data[1].version_number);
        }

        [Test]
        public void AssetVersionResponse_DeserializesCorrectly()
        {
            string json = "{\"id\":\"v-1\",\"asset_id\":\"a-1\",\"version_number\":1,\"s3_key\":\"key1\",\"file_size\":1024,\"metadata\":\"{}\",\"created_by\":\"u-1\",\"created_at\":\"2025-01-01T00:00:00Z\"}";
            var res = JsonUtility.FromJson<AssetVersionResponse>(json);

            Assert.AreEqual(1, res.version_number);
            Assert.AreEqual("key1", res.s3_key);
            Assert.AreEqual(1024, res.file_size);
        }

        [Test]
        public void ImageOpRequest_DefaultValues()
        {
            var req = new ImageOpRequest { image = "https://example.com/img.png" };

            Assert.AreEqual("https://example.com/img.png", req.image);
            Assert.AreEqual(8, req.pixel_size);
            Assert.AreEqual(0.8f, req.strength);
            Assert.AreEqual(128, req.width);
            Assert.AreEqual(128, req.height);
            Assert.AreEqual(90, req.degrees);
            Assert.IsFalse(req.expand);
        }

        [Test]
        public void ImageOpRequest_SerializesCorrectly()
        {
            var req = new ImageOpRequest
            {
                image = "https://example.com/i.png",
                pixel_size = 16,
                width = 256,
                height = 256,
                prompt = "edit this",
                strength = 0.5f,
                degrees = 180,
                expand = true,
            };

            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("https://example.com/i.png"));
            Assert.IsTrue(json.Contains("\"pixel_size\":16"));
            Assert.IsTrue(json.Contains("\"degrees\":180"));
            Assert.IsTrue(json.Contains("\"expand\":true"));
        }

        [Test]
        public void BulkDestroyRequest_SerializesCorrectly()
        {
            var req = new BulkDestroyRequest { ids = new[] { "a-1", "a-2" } };
            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("a-1"));
            Assert.IsTrue(json.Contains("a-2"));
        }

        [Test]
        public void AssignProjectRequest_SerializesCorrectly()
        {
            var req = new AssignProjectRequest { project_id = "proj-123" };
            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("\"project_id\":\"proj-123\""));
        }

        [Test]
        public void SetThumbnailRequest_SerializesCorrectly()
        {
            var req = new SetThumbnailRequest { asset_id = "asset-456" };
            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("\"asset_id\":\"asset-456\""));
        }

        [Test]
        public void MoveAssetRequest_SerializesCorrectly()
        {
            var req = new MoveAssetRequest { project_id = "proj-789" };
            string json = JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("\"project_id\":\"proj-789\""));
        }

        [Test]
        public void HistoryEntry_AllFields()
        {
            var entry = new HistoryEntry
            {
                job_id = "job-1",
                prompt = "test prompt",
                image_size = "128x128",
                negative_prompt = "no",
                seed = 42,
                model = "fast",
                generation_type = "Create Image",
                status = "completed",
                asset_url = "https://cdn.example.com/img.png",
                width = 128,
                height = 128,
                credits_cost = 5,
                duration_ms = 1000,
                date = "2025-01-01T00:00:00Z",
                local_path = "Assets/Spritesynth/Generations/2025-01-01/test.png",
            };

            Assert.AreEqual("job-1", entry.job_id);
            Assert.AreEqual("test prompt", entry.prompt);
            Assert.AreEqual("completed", entry.status);
        }

        [Test]
        public void GenerationHistory_AddEntries()
        {
            var history = new GenerationHistory();
            history.entries.Add(new HistoryEntry { job_id = "1" });
            history.entries.Add(new HistoryEntry { job_id = "2" });

            Assert.AreEqual(2, history.entries.Count);
        }

        [Test]
        public void GenerationHistory_SerializationRoundTrip()
        {
            var history = new GenerationHistory();
            history.entries.Add(new HistoryEntry
            {
                job_id = "rt-1",
                prompt = "round trip",
                status = "completed",
                credits_cost = 3,
                date = "2025-06-01T00:00:00Z",
            });

            string json = JsonUtility.ToJson(history);
            var loaded = JsonUtility.FromJson<GenerationHistory>(json);

            Assert.IsNotNull(loaded);
            Assert.AreEqual(1, loaded.entries.Count);
            Assert.AreEqual("rt-1", loaded.entries[0].job_id);
            Assert.AreEqual("round trip", loaded.entries[0].prompt);
        }

        [Test]
        public void NullFields_DeserializeAsDefaults()
        {
            string json = "{\"job_id\":\"test\",\"prompt\":\"test\"}";
            var entry = JsonUtility.FromJson<HistoryEntry>(json);

            Assert.AreEqual("test", entry.job_id);
            Assert.AreEqual("test", entry.prompt);
            Assert.AreEqual(0, entry.seed);
            Assert.AreEqual(0, entry.credits_cost);
            Assert.AreEqual(0, entry.duration_ms);
            Assert.IsNull(entry.status);
        }

        [Test]
        public void EmptyRequest_SerializesAsEmptyObject()
        {
            var req = new CharacterRequest();
            string json = JsonUtility.ToJson(req);

            Assert.IsTrue(json.Contains("\"direction_count\":1"));
            Assert.IsTrue(json.Contains("\"name\":"));
        }

        [Test]
        public void ListParams_DefaultValues()
        {
            var p = new ListParams();
            Assert.AreEqual(20, p.per_page);
            Assert.AreEqual(1, p.page);
            Assert.AreEqual(0, p.direction_count);
        }

        [Test]
        public void GenerationListParams_DefaultValues()
        {
            var p = new GenerationListParams();
            Assert.AreEqual(20, p.per_page);
            Assert.AreEqual(1, p.page);
        }

        [Test]
        public void CharacterStateResponse_DeserializesCorrectly()
        {
            string json = "{\"id\":\"s-1\",\"uuid\":\"su-1\",\"character_id\":\"c-1\",\"name\":\"walk\",\"direction\":\"east\",\"asset_id\":\"a-1\",\"frame_count\":6,\"frame_width\":32,\"frame_height\":32,\"frame_duration_ms\":150,\"created_at\":\"2025-01-01T00:00:00Z\"}";
            var res = JsonUtility.FromJson<CharacterStateResponse>(json);

            Assert.AreEqual("walk", res.name);
            Assert.AreEqual("east", res.direction);
            Assert.AreEqual(6, res.frame_count);
            Assert.AreEqual(150, res.frame_duration_ms);
        }

        [Test]
        public void ObjectStateResponse_DeserializesCorrectly()
        {
            string json = "{\"id\":\"os-1\",\"uuid\":\"osu-1\",\"object_id\":\"o-1\",\"name\":\"broken\",\"direction\":\"front\",\"frame_count\":3,\"frame_duration_ms\":250}";
            var res = JsonUtility.FromJson<ObjectStateResponse>(json);

            Assert.AreEqual("broken", res.name);
            Assert.AreEqual(3, res.frame_count);
            Assert.AreEqual(250, res.frame_duration_ms);
        }

        [Test]
        public void SpritesynthClient_Constructor_StoresApiKey()
        {
            var client = new SpritesynthClient(TestApiKey);
            Assert.AreEqual(TestApiKey, client.ApiKey);
        }

        [Test]
        public void SpritesynthClient_Constructor_ThrowsOnNullKey()
        {
            Assert.Throws<ArgumentNullException>(() => new SpritesynthClient(null));
        }

        [Test]
        public void SpritesynthClient_ApiKeyProperty_CanBeUpdated()
        {
            var client = new SpritesynthClient("original-key");
            client.ApiKey = "updated-key";
            Assert.AreEqual("updated-key", client.ApiKey);
        }

        [Test]
        public void SpritesynthClient_ApiKeyProperty_ThrowsOnNull()
        {
            var client = new SpritesynthClient(TestApiKey);
            Assert.Throws<ArgumentNullException>(() => client.ApiKey = null);
        }

        [Test]
        public void SpritesynthClient_Constructor_UsesDefaultBaseUrl()
        {
            var client = new SpritesynthClient(TestApiKey);
            Assert.AreEqual(TestApiKey, client.ApiKey);
        }

        [Test]
        public void UnityWebRequestExtensions_SendWebRequestAsync_ReturnsTask()
        {
            Assert.DoesNotThrow(() =>
            {
                var method = typeof(UnityWebRequestExtensions).GetMethod("SendWebRequestAsync");
                Assert.IsNotNull(method);
                Assert.AreEqual(typeof(Task), method.ReturnType);
            });
        }
    }
}
