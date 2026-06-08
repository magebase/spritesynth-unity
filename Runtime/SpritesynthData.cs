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

    [Serializable]
    public class HistoryEntry
    {
        public string job_id;
        public string prompt;
        public string image_size;
        public string negative_prompt;
        public int seed;
        public string model;
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
