using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Magebase.Spritesynth.Tests
{
    public class SpritesynthClientTests
    {
        [Test]
        public void CreateImageRequest_DefaultValues()
        {
            var req = new CreateImageRequest { description = "test" };
            Assert.AreEqual("128x128", req.image_size);
            Assert.AreEqual("test", req.description);
        }

        [Test]
        public void CreateImageRequest_Serialization()
        {
            var req = new CreateImageRequest
            {
                description = "a wizard",
                image_size = "64x64",
                seed = 42,
                negative_prompt = "blurry"
            };
            var json = UnityEngine.JsonUtility.ToJson(req);
            Assert.IsTrue(json.Contains("a wizard"));
            Assert.IsTrue(json.Contains("\"64x64\""));
            Assert.IsTrue(json.Contains("42"));
        }

        [Test]
        public void CreateImageResponse_Deserialization()
        {
            var json = "{\"job_id\":\"abc123\",\"status\":\"pending\"}";
            var resp = UnityEngine.JsonUtility.FromJson<CreateImageResponse>(json);
            Assert.AreEqual("abc123", resp.job_id);
            Assert.AreEqual("pending", resp.status);
        }

        [Test]
        public void GenerationResult_Deserialization()
        {
            var json = "{\"id\":\"gen_1\",\"status\":\"completed\",\"asset\":{\"url\":\"https://cdn.example.com/asset.png\",\"width\":128,\"height\":128},\"credits_cost\":1,\"duration_ms\":500}";
            var result = UnityEngine.JsonUtility.FromJson<GenerationResult>(json);
            Assert.AreEqual("completed", result.status);
            Assert.AreEqual(128, result.asset.width);
        }
    }
}
