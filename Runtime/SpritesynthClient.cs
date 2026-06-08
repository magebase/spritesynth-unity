using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Magebase.Spritesynth
{
    [Serializable]
    public class CreateImageRequest
    {
        public string description;
        public string image_size = "128x128";
        public int seed;
        public string negative_prompt;
        public string project_id;
    }

    [Serializable]
    public class CreateImageResponse
    {
        public string job_id;
        public string status;
    }

    [Serializable]
    public class GenerationResult
    {
        public string id;
        public string status;
        public AssetInfo asset;
        public int credits_cost;
        public long duration_ms;
    }

    [Serializable]
    public class AssetInfo
    {
        public string url;
        public int width;
        public int height;
    }

    public class SpritesynthClient
    {
        private readonly HttpClient _httpClient;
        private string _apiKey;
        private const string BaseUrl = "https://api.spritesynth.com/api";

        public SpritesynthClient(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<CreateImageResponse> CreateImageAsync(CreateImageRequest request)
        {
            var json = JsonUtility.ToJson(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/generations/image", content);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonUtility.FromJson<CreateImageResponse>(body);
        }

        public async Task<GenerationResult> PollGenerationAsync(string jobId, int maxRetries = 30, int delayMs = 1000)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/generations/{jobId}");
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonUtility.FromJson<GenerationResult>(body);
                if (result.status == "completed" || result.status == "failed")
                    return result;
                await Task.Delay(delayMs);
            }
            throw new TimeoutException("Generation did not complete within the polling limit.");
        }

        public async Task<byte[]> DownloadAssetAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
