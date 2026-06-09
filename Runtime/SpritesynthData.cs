using System;
using System.Collections.Generic;

namespace Magebase.Spritesynth
{
    [Serializable]
    public class CreateImageRequest
    {
        public string description;
        public string image_size = "128x128";
        public int seed;
        public string negative_prompt;
        public string model;
        public string project_id;
    }

    [Serializable]
    public class CreateImageResponse
    {
        public string job_id;
        public string status;
    }

    [Serializable]
    public class StyleGenRequest
    {
        public string description;
        public string style_image;
        public string image_size = "128x128";
        public int seed;
        public string project_id;
    }

    [Serializable]
    public class UiGenRequest
    {
        public string description;
        public string image_size = "128x128";
        public string style;
        public int seed;
        public string project_id;
    }

    [Serializable]
    public class PreviewRequest
    {
        public string description;
        public string image_size = "128x128";
        public string model;
    }

    [Serializable]
    public class GenerationResponse
    {
        public string id;
        public string uuid;
        public string user_id;
        public string project_id;
        public string asset_id;
        public string type;
        public string model;
        public string prompt;
        public string negative_prompt;
        public string status;
        public string runpod_job_id;
        public string runpod_endpoint;
        public int credits_cost;
        public long duration_ms;
        public string error_message;
        public int retry_count;
        public int max_retries;
        public string created_at;
        public string updated_at;
        public AssetInfo asset;
    }

    [Serializable]
    public class GenerationStatusResponse
    {
        public string status;
        public int credits_cost;
        public long duration_ms;
    }

    [Serializable]
    public class AssetInfo
    {
        public string id;
        public string uuid;
        public string url;
        public string cdn_url;
        public int width;
        public int height;
        public string mime_type;
        public int file_size;
        public string filename;
        public string type;
    }

    [Serializable]
    public class CharacterRequest
    {
        public string name;
        public string description;
        public string project_id;
        public int direction_count = 1;
        public string metadata;
    }

    [Serializable]
    public class CharacterResponse
    {
        public string id;
        public string uuid;
        public string user_id;
        public string project_id;
        public string name;
        public string description;
        public string asset_id;
        public string thumbnail_asset_id;
        public int direction_count;
        public int width;
        public int height;
        public string tags;
        public string metadata;
        public string generation_id;
        public string created_at;
        public string updated_at;
        public AssetInfo asset;
        public AssetInfo thumbnail;
        public List<CharacterStateResponse> states;
    }

    [Serializable]
    public class CharacterStateRequest
    {
        public string name;
        public string asset_id;
        public string direction;
        public int frame_count = 1;
        public int frame_duration_ms = 200;
    }

    [Serializable]
    public class CharacterStateResponse
    {
        public string id;
        public string uuid;
        public string character_id;
        public string name;
        public string direction;
        public string asset_id;
        public int frame_count;
        public int frame_width;
        public int frame_height;
        public int frame_duration_ms;
        public string metadata;
        public string created_at;
        public string updated_at;
        public AssetInfo asset;
    }

    [Serializable]
    public class ObjectRequest
    {
        public string name;
        public string description;
        public string project_id;
        public int direction_count = 1;
        public string tags;
        public string metadata;
    }

    [Serializable]
    public class ObjectResponse
    {
        public string id;
        public string uuid;
        public string user_id;
        public string project_id;
        public string name;
        public string description;
        public string asset_id;
        public string thumbnail_asset_id;
        public int direction_count;
        public string tags;
        public string metadata;
        public string generation_id;
        public string created_at;
        public string updated_at;
        public AssetInfo asset;
        public AssetInfo thumbnail;
        public List<ObjectStateResponse> states;
    }

    [Serializable]
    public class ObjectStateRequest
    {
        public string name;
        public string asset_id;
        public string direction;
        public int frame_count = 1;
        public int frame_duration_ms = 200;
    }

    [Serializable]
    public class ObjectStateResponse
    {
        public string id;
        public string uuid;
        public string object_id;
        public string name;
        public string direction;
        public string asset_id;
        public int frame_count;
        public int frame_width;
        public int frame_height;
        public int frame_duration_ms;
        public string metadata;
        public string created_at;
        public string updated_at;
        public AssetInfo asset;
    }

    [Serializable]
    public class TilesetRequest
    {
        public string name;
        public string description;
        public string type = "top_down";
        public int tile_size = 16;
        public string project_id;
        public string metadata;
    }

    [Serializable]
    public class TilesetResponse
    {
        public string id;
        public string uuid;
        public string user_id;
        public string project_id;
        public string name;
        public string description;
        public string type;
        public int tile_size;
        public int tile_count;
        public string asset_ids;
        public string thumbnail_asset_id;
        public string metadata;
        public string generation_id;
        public string created_at;
        public string updated_at;
        public AssetInfo thumbnail;
        public AssetInfo asset;
    }

    [Serializable]
    public class TilesetTileResponse
    {
        public List<TileInfo> tiles;
    }

    [Serializable]
    public class TileInfo
    {
        public int index;
        public int x;
        public int y;
        public int width;
        public int height;
        public string asset_url;
    }

    [Serializable]
    public class ProjectRequest
    {
        public string name;
        public string description;
        public string settings;
        public string thumbnail_asset_id;
    }

    [Serializable]
    public class ProjectResponse
    {
        public string id;
        public string uuid;
        public string user_id;
        public string name;
        public string description;
        public string settings;
        public string thumbnail_asset_id;
        public string created_at;
        public string updated_at;
        public AssetInfo thumbnail;
    }

    [Serializable]
    public class AssetRequest
    {
        public string name;
        public string project_id;
    }

    [Serializable]
    public class AssetResponse
    {
        public string id;
        public string uuid;
        public string user_id;
        public string project_id;
        public string type;
        public string path;
        public string filename;
        public string original_filename;
        public string mime_type;
        public int width;
        public int height;
        public int file_size;
        public string s3_key;
        public string s3_bucket;
        public string cdn_url;
        public string metadata;
        public string created_at;
        public string updated_at;
    }

    [Serializable]
    public class AssetVersionResponse
    {
        public string id;
        public string asset_id;
        public int version_number;
        public string s3_key;
        public int file_size;
        public string metadata;
        public string created_by;
        public string created_at;
    }

    [Serializable]
    public class TemplateRequest
    {
        public string name;
        public string description;
        public string type = "generation";
        public string config;
        public bool is_public;
    }

    [Serializable]
    public class TemplateResponse
    {
        public string id;
        public string uuid;
        public string user_id;
        public string name;
        public string description;
        public string type;
        public string config;
        public bool is_public;
        public int downloads_count;
        public float rating_avg;
        public string created_at;
        public string updated_at;
    }

    [Serializable]
    public class ApiKeyRequest
    {
        public string name;
    }

    [Serializable]
    public class ApiKeyResponse
    {
        public string id;
        public string uuid;
        public string name;
        public string key_prefix;
        public string plain_text_key;
        public string last_used_at;
        public string expires_at;
        public int rate_limit;
        public string scopes;
        public bool is_active;
        public string created_at;
    }

    [Serializable]
    public class AccountBalanceResponse
    {
        public int credits_balance;
    }

    [Serializable]
    public class BulkDestroyRequest
    {
        public string[] ids;
    }

    [Serializable]
    public class MoveAssetRequest
    {
        public string project_id;
    }

    [Serializable]
    public class AssignProjectRequest
    {
        public string project_id;
    }

    [Serializable]
    public class SetThumbnailRequest
    {
        public string asset_id;
    }

    [Serializable]
    public class ImageOpRequest
    {
        public string image;
        public string image_size;
        public int pixel_size = 8;
        public string mask;
        public string prompt;
        public float strength = 0.8f;
        public int width = 128;
        public int height = 128;
        public int degrees = 90;
        public bool expand;
    }

    [Serializable]
    public class ImageUploadRequest
    {
        public string filename;
        public string data_base64;
        public string mime_type = "image/png";
    }

    [Serializable]
    public class GenerationListParams
    {
        public string type;
        public string model;
        public string status;
        public string date_from;
        public string date_to;
        public int per_page = 20;
        public int page = 1;
    }

    [Serializable]
    public class ListParams
    {
        public string project_id;
        public string tag;
        public int direction_count;
        public string type;
        public string search;
        public int per_page = 20;
        public int page = 1;
    }

    [Serializable]
    public class PaginatedResponse<T>
    {
        public List<T> data;
        public PaginationMeta meta;
    }

    [Serializable]
    public class PaginationMeta
    {
        public int current_page;
        public int last_page;
        public int total;
        public int per_page;
    }

    [Serializable]
    public class StatusResponse
    {
        public string status;
    }

    [Serializable]
    public class DownloadUrlResponse
    {
        public string url;
    }

    [Serializable]
    public class VersionListResponse
    {
        public List<AssetVersionResponse> data;
    }

    [Serializable]
    public class HistoryEntry
    {
        public string job_id;
        public string prompt;
        public string image_size;
        public string negative_prompt;
        public int seed;
        public string model;
        public string generation_type;
        public string status;
        public string asset_url;
        public int width;
        public int height;
        public int credits_cost;
        public long duration_ms;
        public string date;
        public string local_path;
    }

    [Serializable]
    public class GenerationHistory
    {
        public List<HistoryEntry> entries = new List<HistoryEntry>();
    }
}
