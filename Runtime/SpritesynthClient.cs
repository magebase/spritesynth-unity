using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Magebase.Spritesynth
{
    public class SpritesynthClient
    {
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public SpritesynthClient(string apiKey, string baseUrl = "https://api.spritesynth.com/api")
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _baseUrl = (baseUrl ?? "https://api.spritesynth.com/api").TrimEnd('/');
        }

        public async Task<CreateImageResponse> CreateImageAsync(CreateImageRequest request)
        {
            string json = JsonUtility.ToJson(request);
            byte[] body = Encoding.UTF8.GetBytes(json);

            using var web = new UnityWebRequest(_baseUrl + "/generations/image", "POST");
            web.uploadHandler = new UploadHandlerRaw(body);
            web.downloadHandler = new DownloadHandlerBuffer();
            web.SetRequestHeader("Content-Type", "application/json");
            web.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
            web.SetRequestHeader("Accept", "application/json");

            await web.SendWebRequestAsync();

            if (web.result != UnityWebRequest.Result.Success)
            {
                string detail = string.IsNullOrEmpty(web.downloadHandler.text)
                    ? web.error
                    : web.downloadHandler.text;
                throw new Exception($"Create image failed ({web.responseCode}): {detail}");
            }

            return JsonUtility.FromJson<CreateImageResponse>(web.downloadHandler.text);
        }

        public async Task<GenerationResult> PollGenerationAsync(string jobId, int maxRetries = 60, int delayMs = 2000)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                using var web = UnityWebRequest.Get(_baseUrl + "/generations/" + jobId);
                web.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
                web.SetRequestHeader("Accept", "application/json");

                await web.SendWebRequestAsync();

                if (web.result != UnityWebRequest.Result.Success)
                    throw new Exception($"Poll failed ({web.responseCode}): {web.error}");

                GenerationResult result;
                try
                {
                    result = JsonUtility.FromJson<GenerationResult>(web.downloadHandler.text);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to parse poll response: {ex.Message}");
                }

                if (result == null)
                    throw new Exception("Poll returned null result");

                if (result.status == "completed" || result.status == "failed")
                    return result;

                await Task.Delay(delayMs);
            }

            throw new TimeoutException("Generation did not complete within polling limit.");
        }

        public async Task<byte[]> DownloadAssetAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            using var web = UnityWebRequest.Get(url);
            web.downloadHandler = new DownloadHandlerBuffer();

            await web.SendWebRequestAsync();

            if (web.result != UnityWebRequest.Result.Success)
                throw new Exception($"Download failed ({web.responseCode}): {web.error}");

            return web.downloadHandler.data;
        }
    }

    public static class UnityWebRequestExtensions
    {
        public static Task SendWebRequestAsync(this UnityWebRequest request)
        {
            var tcs = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();
            operation.completed += _ => tcs.TrySetResult(true);
            return tcs.Task;
        }
    }
}
