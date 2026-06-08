# SpriteSynth Unity SDK

[![openupm](https://img.shields.io/npm/v/com.magebase.spritesynth?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.magebase.spritesynth/)

Generate pixel art game assets directly from the Unity Editor using the SpriteSynth AI API.

## Quick Start

1. **Install the package** via UPM (Unity Package Manager) using the Git URL:
   ```
   https://github.com/magebase/spritesynth-unity.git
   ```
2. **Get an API key** from [spritesynth.com](https://spritesynth.com)
3. **Open the generator window**: `Tools > SpriteSynth > Generator`
4. Paste your API key, write a prompt, and click **Generate**

### Code Example

```csharp
using Magebase.Spritesynth;
using UnityEngine;

public class SpriteGenExample : MonoBehaviour
{
    async void Start()
    {
        var client = new SpritesynthClient("your-api-key");

        var request = new CreateImageRequest
        {
            description = "a 16x16 pixel art knight with blue armor",
            image_size = "128x128",
            seed = 42
        };

        var response = await client.CreateImageAsync(request);
        var result = await client.PollGenerationAsync(response.job_id);

        if (result.status == "completed")
        {
            byte[] png = await client.DownloadAssetAsync(result.asset.url);
            // Create a texture, save to disk, etc.
        }
    }
}
```

## API Reference

Full API documentation: [https://spritesynth.com/api-reference/v1](https://spritesynth.com/api-reference/v1)

## License

MIT
