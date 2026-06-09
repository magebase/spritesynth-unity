using System;
using System.Threading.Tasks;
using UnityEditor;

namespace Magebase.Spritesynth.Editor
{
    public static class SpritesynthSettings
    {
        private const string ApiKeyPref = "Spritesynth_API_Key";
        private const string BaseUrlPref = "Spritesynth_Base_URL";
        private const string DefaultBaseUrl = "https://api.spritesynth.com/api";
        private const string EnvVarName = "SPIRESYNTH_API_KEY";

        public static string ApiKey
        {
            get
            {
                string key = EditorPrefs.GetString(ApiKeyPref, "");
                if (string.IsNullOrEmpty(key))
                    key = Environment.GetEnvironmentVariable(EnvVarName);
                return key ?? "";
            }
            set
            {
                if (value == null)
                    EditorPrefs.DeleteKey(ApiKeyPref);
                else
                    EditorPrefs.SetString(ApiKeyPref, value);
            }
        }

        public static bool HasEnvVar =>
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EnvVarName));

        public static bool IsUsingEnvVar =>
            string.IsNullOrEmpty(EditorPrefs.GetString(ApiKeyPref, ""))
            && HasEnvVar;

        public static string BaseUrl
        {
            get => EditorPrefs.GetString(BaseUrlPref, DefaultBaseUrl);
            set => EditorPrefs.SetString(BaseUrlPref, value ?? DefaultBaseUrl);
        }

        public static async Task<int> FetchBalanceAsync()
        {
            string key = ApiKey;
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("API key is not set.");

            var client = new SpritesynthClient(key, BaseUrl);
            var balance = await client.GetAccountBalanceAsync();
            return balance.credits_balance;
        }

        public static void ClearApiKey()
        {
            EditorPrefs.DeleteKey(ApiKeyPref);
        }

        public static void ClearAll()
        {
            EditorPrefs.DeleteKey(ApiKeyPref);
            EditorPrefs.DeleteKey(BaseUrlPref);
        }
    }
}
