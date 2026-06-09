using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Magebase.Spritesynth
{
    public class SpritesynthClient
    {
        private string _apiKey;
        private readonly string _baseUrl;

        public string ApiKey
        {
            get => _apiKey;
            set => _apiKey = value ?? throw new ArgumentNullException(nameof(value));
        }

        public SpritesynthClient(string apiKey, string baseUrl = "https://api.spritesynth.com/api")
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _baseUrl = (baseUrl ?? "https://api.spritesynth.com/api").TrimEnd('/');
        }

        private UnityWebRequest CreateRequest(string path, string method = "GET")
        {
            var req = new UnityWebRequest(_baseUrl + path, method);
            req.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
            req.SetRequestHeader("Accept", "application/json");
            req.downloadHandler = new DownloadHandlerBuffer();
            return req;
        }

        private UnityWebRequest CreateJsonRequest(string path, string method, string json)
        {
            var req = CreateRequest(path, method);
            byte[] body = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.SetRequestHeader("Content-Type", "application/json");
            return req;
        }

        private async Task<T> SendAsync<T>(UnityWebRequest req)
        {
            await req.SendWebRequestAsync();
            if (req.result != UnityWebRequest.Result.Success)
            {
                string detail = string.IsNullOrEmpty(req.downloadHandler.text)
                    ? req.error
                    : req.downloadHandler.text;
                throw new Exception($"Request failed ({req.responseCode}): {detail}");
            }
            return JsonUtility.FromJson<T>(req.downloadHandler.text);
        }

        private async Task SendNoResponseAsync(UnityWebRequest req)
        {
            await req.SendWebRequestAsync();
            if (req.result != UnityWebRequest.Result.Success)
            {
                string detail = string.IsNullOrEmpty(req.downloadHandler.text)
                    ? req.error
                    : req.downloadHandler.text;
                throw new Exception($"Request failed ({req.responseCode}): {detail}");
            }
        }

        // ---- Generations ----

        public async Task<CreateImageResponse> CreateImageAsync(CreateImageRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/generations/image", "POST", json);
            return await SendAsync<CreateImageResponse>(web);
        }

        public async Task<CreateImageResponse> CreateWithStyleAsync(StyleGenRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/generations/style", "POST", json);
            return await SendAsync<CreateImageResponse>(web);
        }

        public async Task<CreateImageResponse> CreateUiAsync(UiGenRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/generations/ui", "POST", json);
            return await SendAsync<CreateImageResponse>(web);
        }

        public async Task<CreateImageResponse> PreviewAsync(PreviewRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/generations/preview", "POST", json);
            return await SendAsync<CreateImageResponse>(web);
        }

        public async Task<GenerationResponse> GetGenerationAsync(string uuid)
        {
            using var web = CreateRequest($"/generations/{uuid}");
            return await SendAsync<GenerationResponse>(web);
        }

        public async Task<GenerationStatusResponse> GetGenerationStatusAsync(string uuid)
        {
            using var web = CreateRequest($"/generations/{uuid}/status");
            return await SendAsync<GenerationStatusResponse>(web);
        }

        public async Task<PaginatedResponse<GenerationResponse>> ListGenerationsAsync(GenerationListParams p = null)
        {
            p ??= new GenerationListParams();
            var q = new List<string>();
            if (!string.IsNullOrEmpty(p.type)) q.Add($"type={Uri.EscapeDataString(p.type)}");
            if (!string.IsNullOrEmpty(p.model)) q.Add($"model={Uri.EscapeDataString(p.model)}");
            if (!string.IsNullOrEmpty(p.status)) q.Add($"status={Uri.EscapeDataString(p.status)}");
            if (!string.IsNullOrEmpty(p.date_from)) q.Add($"date_from={Uri.EscapeDataString(p.date_from)}");
            if (!string.IsNullOrEmpty(p.date_to)) q.Add($"date_to={Uri.EscapeDataString(p.date_to)}");
            q.Add($"per_page={p.per_page}");
            q.Add($"page={p.page}");
            string qs = string.Join("&", q);
            using var web = CreateRequest($"/generations?{qs}");
            return await SendAsync<PaginatedResponse<GenerationResponse>>(web);
        }

        public async Task<StatusResponse> CancelGenerationAsync(string uuid)
        {
            using var web = CreateJsonRequest($"/generations/{uuid}/cancel", "POST", "{}");
            return await SendAsync<StatusResponse>(web);
        }

        public async Task<CreateImageResponse> RetryGenerationAsync(string uuid)
        {
            using var web = CreateJsonRequest($"/generations/{uuid}/retry", "POST", "{}");
            return await SendAsync<CreateImageResponse>(web);
        }

        public async Task<CreateImageResponse> CreateVariationAsync(string uuid)
        {
            using var web = CreateJsonRequest($"/generations/{uuid}/variation", "POST", "{}");
            return await SendAsync<CreateImageResponse>(web);
        }

        public async Task<GenerationResponse> PollGenerationAsync(string jobId, int maxRetries = 60, int delayMs = 2000)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                GenerationResponse result = await GetGenerationAsync(jobId);
                if (result.status == "completed" || result.status == "failed")
                    return result;
                await Task.Delay(delayMs);
            }
            throw new TimeoutException("Generation did not complete within polling limit.");
        }

        // ---- Image Ops ----

        public async Task<CreateImageResponse> ToPixelArtAsync(ImageOpRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/image-ops/to-pixel", "POST", json);
            return await SendAsync<CreateImageResponse>(web);
        }

        public async Task<CreateImageResponse> ResizeAsync(ImageOpRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/image-ops/resize", "POST", json);
            return await SendAsync<CreateImageResponse>(web);
        }

        public async Task<CreateImageResponse> RemoveBackgroundAsync(ImageOpRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/image-ops/remove-bg", "POST", json);
            return await SendAsync<CreateImageResponse>(web);
        }

        public async Task<CreateImageResponse> InpaintAsync(ImageOpRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/image-ops/inpaint", "POST", json);
            return await SendAsync<CreateImageResponse>(web);
        }

        public async Task<CreateImageResponse> EditAsync(ImageOpRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/image-ops/edit", "POST", json);
            return await SendAsync<CreateImageResponse>(web);
        }

        public async Task<CreateImageResponse> RotateAsync(ImageOpRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/image-ops/rotate", "POST", json);
            return await SendAsync<CreateImageResponse>(web);
        }

        // ---- Characters ----

        public async Task<PaginatedResponse<CharacterResponse>> ListCharactersAsync(ListParams p = null)
        {
            p ??= new ListParams();
            var q = BuildQuery(p);
            using var web = CreateRequest($"/characters?{q}");
            return await SendAsync<PaginatedResponse<CharacterResponse>>(web);
        }

        public async Task<CharacterResponse> CreateCharacterAsync(CharacterRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/characters", "POST", json);
            return await SendAsync<CharacterResponse>(web);
        }

        public async Task<CharacterResponse> GetCharacterAsync(string uuid)
        {
            using var web = CreateRequest($"/characters/{uuid}");
            return await SendAsync<CharacterResponse>(web);
        }

        public async Task<CharacterResponse> UpdateCharacterAsync(string uuid, CharacterRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/characters/{uuid}", "PUT", json);
            return await SendAsync<CharacterResponse>(web);
        }

        public async Task DeleteCharacterAsync(string uuid)
        {
            using var web = CreateRequest($"/characters/{uuid}", "DELETE");
            await SendNoResponseAsync(web);
        }

        public async Task<byte[]> ExportCharacterZipAsync(string uuid)
        {
            using var web = CreateRequest($"/characters/{uuid}/export-zip");
            await web.SendWebRequestAsync();
            if (web.result != UnityWebRequest.Result.Success)
                throw new Exception($"Export failed ({web.responseCode}): {web.error}");
            return web.downloadHandler.data;
        }

        public async Task<CharacterResponse> DuplicateCharacterAsync(string uuid)
        {
            using var web = CreateJsonRequest($"/characters/{uuid}/duplicate", "POST", "{}");
            return await SendAsync<CharacterResponse>(web);
        }

        public async Task<CharacterResponse> AssignCharacterToProjectAsync(string uuid, string projectId)
        {
            var req = new AssignProjectRequest { project_id = projectId };
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/characters/{uuid}/assign-project", "POST", json);
            return await SendAsync<CharacterResponse>(web);
        }

        public async Task<CharacterResponse> SetCharacterThumbnailAsync(string uuid, string assetId)
        {
            var req = new SetThumbnailRequest { asset_id = assetId };
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/characters/{uuid}/thumbnail", "POST", json);
            return await SendAsync<CharacterResponse>(web);
        }

        // ---- Character States ----

        public async Task<CharacterStateResponse> AddCharacterStateAsync(string charUuid, CharacterStateRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/characters/{charUuid}/states", "POST", json);
            return await SendAsync<CharacterStateResponse>(web);
        }

        public async Task<CharacterStateResponse> UpdateCharacterStateAsync(string charUuid, string stateUuid, CharacterStateRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/characters/{charUuid}/states/{stateUuid}", "PATCH", json);
            return await SendAsync<CharacterStateResponse>(web);
        }

        public async Task<AssetResponse> GetCharacterStateSpritesheetAsync(string charUuid, string stateUuid)
        {
            using var web = CreateRequest($"/characters/{charUuid}/states/{stateUuid}/spritesheet");
            return await SendAsync<AssetResponse>(web);
        }

        // ---- Objects ----

        public async Task<PaginatedResponse<ObjectResponse>> ListObjectsAsync(ListParams p = null)
        {
            p ??= new ListParams();
            var q = BuildQuery(p);
            using var web = CreateRequest($"/objects?{q}");
            return await SendAsync<PaginatedResponse<ObjectResponse>>(web);
        }

        public async Task<ObjectResponse> CreateObjectAsync(ObjectRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/objects", "POST", json);
            return await SendAsync<ObjectResponse>(web);
        }

        public async Task<ObjectResponse> GetObjectAsync(string uuid)
        {
            using var web = CreateRequest($"/objects/{uuid}");
            return await SendAsync<ObjectResponse>(web);
        }

        public async Task<ObjectResponse> UpdateObjectAsync(string uuid, ObjectRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/objects/{uuid}", "PUT", json);
            return await SendAsync<ObjectResponse>(web);
        }

        public async Task DeleteObjectAsync(string uuid)
        {
            using var web = CreateRequest($"/objects/{uuid}", "DELETE");
            await SendNoResponseAsync(web);
        }

        public async Task<byte[]> ExportObjectZipAsync(string uuid)
        {
            using var web = CreateRequest($"/objects/{uuid}/export-zip");
            await web.SendWebRequestAsync();
            if (web.result != UnityWebRequest.Result.Success)
                throw new Exception($"Export failed ({web.responseCode}): {web.error}");
            return web.downloadHandler.data;
        }

        public async Task<ObjectResponse> DuplicateObjectAsync(string uuid)
        {
            using var web = CreateJsonRequest($"/objects/{uuid}/duplicate", "POST", "{}");
            return await SendAsync<ObjectResponse>(web);
        }

        public async Task<ObjectResponse> AssignObjectToProjectAsync(string uuid, string projectId)
        {
            var req = new AssignProjectRequest { project_id = projectId };
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/objects/{uuid}/assign-project", "POST", json);
            return await SendAsync<ObjectResponse>(web);
        }

        public async Task<ObjectResponse> SetObjectThumbnailAsync(string uuid, string assetId)
        {
            var req = new SetThumbnailRequest { asset_id = assetId };
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/objects/{uuid}/thumbnail", "POST", json);
            return await SendAsync<ObjectResponse>(web);
        }

        // ---- Object States ----

        public async Task<ObjectStateResponse> AddObjectStateAsync(string objUuid, ObjectStateRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/objects/{objUuid}/states", "POST", json);
            return await SendAsync<ObjectStateResponse>(web);
        }

        public async Task<ObjectStateResponse> UpdateObjectStateAsync(string objUuid, string stateUuid, ObjectStateRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/objects/{objUuid}/states/{stateUuid}", "PATCH", json);
            return await SendAsync<ObjectStateResponse>(web);
        }

        public async Task<AssetResponse> GetObjectStateSpritesheetAsync(string objUuid, string stateUuid)
        {
            using var web = CreateRequest($"/objects/{objUuid}/states/{stateUuid}/spritesheet");
            return await SendAsync<AssetResponse>(web);
        }

        // ---- Tilesets ----

        public async Task<PaginatedResponse<TilesetResponse>> ListTilesetsAsync(ListParams p = null)
        {
            p ??= new ListParams();
            var q = BuildQuery(p);
            using var web = CreateRequest($"/tilesets?{q}");
            return await SendAsync<PaginatedResponse<TilesetResponse>>(web);
        }

        public async Task<TilesetResponse> CreateTilesetAsync(TilesetRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/tilesets", "POST", json);
            return await SendAsync<TilesetResponse>(web);
        }

        public async Task<TilesetResponse> GetTilesetAsync(string uuid)
        {
            using var web = CreateRequest($"/tilesets/{uuid}");
            return await SendAsync<TilesetResponse>(web);
        }

        public async Task<TilesetResponse> UpdateTilesetAsync(string uuid, TilesetRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/tilesets/{uuid}", "PUT", json);
            return await SendAsync<TilesetResponse>(web);
        }

        public async Task DeleteTilesetAsync(string uuid)
        {
            using var web = CreateRequest($"/tilesets/{uuid}", "DELETE");
            await SendNoResponseAsync(web);
        }

        public async Task<byte[]> ExportTilesetZipAsync(string uuid)
        {
            using var web = CreateRequest($"/tilesets/{uuid}/export-zip");
            await web.SendWebRequestAsync();
            if (web.result != UnityWebRequest.Result.Success)
                throw new Exception($"Export failed ({web.responseCode}): {web.error}");
            return web.downloadHandler.data;
        }

        public async Task<TilesetResponse> DuplicateTilesetAsync(string uuid)
        {
            using var web = CreateJsonRequest($"/tilesets/{uuid}/duplicate", "POST", "{}");
            return await SendAsync<TilesetResponse>(web);
        }

        public async Task<TilesetResponse> AssignTilesetToProjectAsync(string uuid, string projectId)
        {
            var req = new AssignProjectRequest { project_id = projectId };
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/tilesets/{uuid}/assign-project", "POST", json);
            return await SendAsync<TilesetResponse>(web);
        }

        public async Task<TilesetResponse> SetTilesetThumbnailAsync(string uuid, string assetId)
        {
            var req = new SetThumbnailRequest { asset_id = assetId };
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/tilesets/{uuid}/thumbnail", "POST", json);
            return await SendAsync<TilesetResponse>(web);
        }

        public async Task<TilesetTileResponse> GetTilesetTilesAsync(string uuid)
        {
            using var web = CreateRequest($"/tilesets/{uuid}/tiles");
            return await SendAsync<TilesetTileResponse>(web);
        }

        // ---- Projects ----

        public async Task<PaginatedResponse<ProjectResponse>> ListProjectsAsync(ListParams p = null)
        {
            p ??= new ListParams();
            var q = new List<string>();
            if (!string.IsNullOrEmpty(p.search)) q.Add($"search={Uri.EscapeDataString(p.search)}");
            q.Add($"per_page={p.per_page}");
            q.Add($"page={p.page}");
            string qs = string.Join("&", q);
            using var web = CreateRequest($"/projects?{qs}");
            return await SendAsync<PaginatedResponse<ProjectResponse>>(web);
        }

        public async Task<ProjectResponse> CreateProjectAsync(ProjectRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/projects", "POST", json);
            return await SendAsync<ProjectResponse>(web);
        }

        public async Task<ProjectResponse> GetProjectAsync(string uuid)
        {
            using var web = CreateRequest($"/projects/{uuid}");
            return await SendAsync<ProjectResponse>(web);
        }

        public async Task<ProjectResponse> UpdateProjectAsync(string uuid, ProjectRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/projects/{uuid}", "PUT", json);
            return await SendAsync<ProjectResponse>(web);
        }

        public async Task DeleteProjectAsync(string uuid)
        {
            using var web = CreateRequest($"/projects/{uuid}", "DELETE");
            await SendNoResponseAsync(web);
        }

        public async Task<ProjectResponse> DuplicateProjectAsync(string uuid)
        {
            using var web = CreateJsonRequest($"/projects/{uuid}/duplicate", "POST", "{}");
            return await SendAsync<ProjectResponse>(web);
        }

        public async Task<ProjectResponse> ArchiveProjectAsync(string uuid)
        {
            using var web = CreateJsonRequest($"/projects/{uuid}/archive", "POST", "{}");
            return await SendAsync<ProjectResponse>(web);
        }

        public async Task<ProjectResponse> UnarchiveProjectAsync(string uuid)
        {
            using var web = CreateJsonRequest($"/projects/{uuid}/unarchive", "POST", "{}");
            return await SendAsync<ProjectResponse>(web);
        }

        // ---- Assets ----

        public async Task<PaginatedResponse<AssetResponse>> ListAssetsAsync(ListParams p = null)
        {
            p ??= new ListParams();
            var q = BuildQuery(p);
            using var web = CreateRequest($"/assets?{q}");
            return await SendAsync<PaginatedResponse<AssetResponse>>(web);
        }

        public async Task<AssetResponse> GetAssetAsync(string uuid)
        {
            using var web = CreateRequest($"/assets/{uuid}");
            return await SendAsync<AssetResponse>(web);
        }

        public async Task DeleteAssetAsync(string uuid)
        {
            using var web = CreateRequest($"/assets/{uuid}", "DELETE");
            await SendNoResponseAsync(web);
        }

        public async Task<StatusResponse> BulkDestroyAssetsAsync(string[] ids)
        {
            var req = new BulkDestroyRequest { ids = ids };
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/assets/bulk-destroy", "POST", json);
            return await SendAsync<StatusResponse>(web);
        }

        public async Task<AssetResponse> MoveAssetAsync(string uuid, string projectId)
        {
            var req = new MoveAssetRequest { project_id = projectId };
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/assets/{uuid}/move", "PATCH", json);
            return await SendAsync<AssetResponse>(web);
        }

        public async Task<DownloadUrlResponse> GetAssetDownloadUrlAsync(string uuid)
        {
            using var web = CreateRequest($"/assets/{uuid}/download");
            return await SendAsync<DownloadUrlResponse>(web);
        }

        public async Task<VersionListResponse> ListAssetVersionsAsync(string uuid)
        {
            using var web = CreateRequest($"/assets/{uuid}/versions");
            return await SendAsync<VersionListResponse>(web);
        }

        public async Task<AssetVersionResponse> CreateAssetVersionAsync(string uuid)
        {
            using var web = CreateJsonRequest($"/assets/{uuid}/versions", "POST", "{}");
            return await SendAsync<AssetVersionResponse>(web);
        }

        public async Task<AssetVersionResponse> RestoreAssetVersionAsync(string uuid, int version)
        {
            using var web = CreateJsonRequest($"/assets/{uuid}/versions/{version}/restore", "POST", "{}");
            return await SendAsync<AssetVersionResponse>(web);
        }

        // ---- Templates ----

        public async Task<PaginatedResponse<TemplateResponse>> ListTemplatesAsync(ListParams p = null)
        {
            p ??= new ListParams();
            var q = new List<string>();
            if (!string.IsNullOrEmpty(p.type)) q.Add($"type={Uri.EscapeDataString(p.type)}");
            if (!string.IsNullOrEmpty(p.search)) q.Add($"search={Uri.EscapeDataString(p.search)}");
            q.Add($"per_page={p.per_page}");
            q.Add($"page={p.page}");
            string qs = string.Join("&", q);
            using var web = CreateRequest($"/templates?{qs}");
            return await SendAsync<PaginatedResponse<TemplateResponse>>(web);
        }

        public async Task<TemplateResponse> CreateTemplateAsync(TemplateRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/templates", "POST", json);
            return await SendAsync<TemplateResponse>(web);
        }

        public async Task<TemplateResponse> GetTemplateAsync(string uuid)
        {
            using var web = CreateRequest($"/templates/{uuid}");
            return await SendAsync<TemplateResponse>(web);
        }

        public async Task<TemplateResponse> UpdateTemplateAsync(string uuid, TemplateRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest($"/templates/{uuid}", "PUT", json);
            return await SendAsync<TemplateResponse>(web);
        }

        public async Task DeleteTemplateAsync(string uuid)
        {
            using var web = CreateRequest($"/templates/{uuid}", "DELETE");
            await SendNoResponseAsync(web);
        }

        public async Task<TemplateResponse> ApplyTemplateAsync(string uuid)
        {
            using var web = CreateJsonRequest($"/templates/{uuid}/apply", "POST", "{}");
            return await SendAsync<TemplateResponse>(web);
        }

        public async Task<TemplateResponse> DuplicateTemplateAsync(string uuid)
        {
            using var web = CreateJsonRequest($"/templates/{uuid}/duplicate", "POST", "{}");
            return await SendAsync<TemplateResponse>(web);
        }

        // ---- Account ----

        public async Task<PaginatedResponse<ApiKeyResponse>> ListApiKeysAsync()
        {
            using var web = CreateRequest("/account/api-keys");
            return await SendAsync<PaginatedResponse<ApiKeyResponse>>(web);
        }

        public async Task<ApiKeyResponse> CreateApiKeyAsync(ApiKeyRequest req)
        {
            string json = JsonUtility.ToJson(req);
            using var web = CreateJsonRequest("/account/api-keys", "POST", json);
            return await SendAsync<ApiKeyResponse>(web);
        }

        public async Task DeleteApiKeyAsync(string uuid)
        {
            using var web = CreateRequest($"/account/api-keys/{uuid}", "DELETE");
            await SendNoResponseAsync(web);
        }

        public async Task<ApiKeyResponse> RevokeApiKeyAsync(string uuid)
        {
            using var web = CreateJsonRequest($"/account/api-keys/{uuid}/revoke", "POST", "{}");
            return await SendAsync<ApiKeyResponse>(web);
        }

        public async Task<ApiKeyResponse> RotateApiKeyAsync(string uuid)
        {
            using var web = CreateJsonRequest($"/account/api-keys/{uuid}/rotate", "POST", "{}");
            return await SendAsync<ApiKeyResponse>(web);
        }

        public async Task<AccountBalanceResponse> GetAccountBalanceAsync()
        {
            using var web = CreateRequest("/account/balance");
            return await SendAsync<AccountBalanceResponse>(web);
        }

        // ---- Utility ----

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

        // ---- Helpers ----

        private static string BuildQuery(ListParams p)
        {
            var q = new List<string>();
            if (!string.IsNullOrEmpty(p.project_id)) q.Add($"project_id={Uri.EscapeDataString(p.project_id)}");
            if (!string.IsNullOrEmpty(p.tag)) q.Add($"tag={Uri.EscapeDataString(p.tag)}");
            if (!string.IsNullOrEmpty(p.type)) q.Add($"type={Uri.EscapeDataString(p.type)}");
            if (!string.IsNullOrEmpty(p.search)) q.Add($"search={Uri.EscapeDataString(p.search)}");
            if (p.direction_count > 0) q.Add($"direction_count={p.direction_count}");
            q.Add($"per_page={p.per_page}");
            q.Add($"page={p.page}");
            return string.Join("&", q);
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
