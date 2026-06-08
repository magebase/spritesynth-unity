using NUnit.Framework;
using UnityEngine;

namespace Magebase.Spritesynth.Tests
{
    public class SpritesynthClientTests
    {
        [Test]
        public void CreateImageRequest_DefaultImageSize()
        {
            var req = new CreateImageRequest { description = "test" };
            Assert.AreEqual("128x128", req.image_size);
        }

        [Test]
        public void CreateImageRequest_SerializesCorrectly()
        {
            var req = new CreateImageRequest
            {
                description = "a pixel art wizard",
                image_size = "64x64",
                seed = 42,
                negative_prompt = "blurry",
            };

            string json = JsonUtility.ToJson(req);

            Assert.IsTrue(json.Contains("a pixel art wizard"));
            Assert.IsTrue(json.Contains("\"64x64\""));
            Assert.IsTrue(json.Contains("42"));
            Assert.IsTrue(json.Contains("blurry"));
        }

        [Test]
        public void CreateImageRequest_SerializesWithDefaults()
        {
            var req = new CreateImageRequest { description = "hello" };
            string json = JsonUtility.ToJson(req);

            Assert.IsTrue(json.Contains("hello"));
            Assert.IsTrue(json.Contains("128x128"));
        }

        [Test]
        public void CreateImageResponse_DeserializesCorrectly()
        {
            string json = "{\"job_id\":\"abc123\",\"status\":\"pending\"}";
            var resp = JsonUtility.FromJson<CreateImageResponse>(json);

            Assert.AreEqual("abc123", resp.job_id);
            Assert.AreEqual("pending", resp.status);
        }

        [Test]
        public void GenerationResult_DeserializesWithAsset()
        {
            string json = "{\"id\":\"gen_1\",\"status\":\"completed\",\"asset\":{\"url\":\"https://cdn.example.com/asset.png\",\"width\":128,\"height\":128},\"credits_cost\":1,\"duration_ms\":500}";
            var result = JsonUtility.FromJson<GenerationResult>(json);

            Assert.AreEqual("completed", result.status);
            Assert.IsNotNull(result.asset);
            Assert.AreEqual("https://cdn.example.com/asset.png", result.asset.url);
            Assert.AreEqual(128, result.asset.width);
            Assert.AreEqual(128, result.asset.height);
            Assert.AreEqual(1, result.credits_cost);
            Assert.AreEqual(500, result.duration_ms);
        }

        [Test]
        public void GenerationResult_DeserializesFailedStatus()
        {
            string json = "{\"id\":\"gen_fail\",\"status\":\"failed\",\"asset\":null,\"credits_cost\":0,\"duration_ms\":100}";
            var result = JsonUtility.FromJson<GenerationResult>(json);

            Assert.AreEqual("failed", result.status);
            Assert.IsNull(result.asset);
        }

        [Test]
        public void AssetInfo_DeserializesCorrectly()
        {
            string json = "{\"url\":\"https://cdn.example.com/sprite.png\",\"width\":64,\"height\":64}";
            var asset = JsonUtility.FromJson<AssetInfo>(json);

            Assert.AreEqual("https://cdn.example.com/sprite.png", asset.url);
            Assert.AreEqual(64, asset.width);
            Assert.AreEqual(64, asset.height);
        }

        [Test]
        public void Client_ConstructorThrowsOnNullKey()
        {
            Assert.That(() => new SpritesynthClient(null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Client_ConstructorTrimsBaseUrl()
        {
            var client = new SpritesynthClient("test-key", "https://api.example.com/api/");
            Assert.IsNotNull(client);
        }
    }
}
