# SpriteSynth Unity Editor Extension

[![UPM](https://img.shields.io/badge/Unity-2022.3%2B-blue)](https://unity.com)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

Generate pixel art game assets directly inside the Unity Editor using the SpriteSynth AI API.

![Generator Window](docs/screenshots/generator-window.png)

## Features

- **AI Generation** — text-to-pixel-art with configurable size, seed, and negative prompt
- **Auto-Import** — generated PNGs are downloaded and imported as `Texture2D` assets into your project
- **Generation History** — browse, preview, re-import, and delete past generations
- **Environment Variable Support** — `SPIRESYNTH_API_KEY` env var fallback for CI/CD workflows
- **UPM Package** — standard Unity Package Manager format

## Quick Start

1. **Install** the package via UPM:
   ```
   https://github.com/magebase/spritesynth-unity.git
   ```
2. **Get an API key** from [spritesynth.com](https://spritesynth.com)
3. Open **Tools > SpriteSynth > Generator**
4. Paste your API key in the **Settings** tab and click **Save**
5. Write a prompt and click **Generate**

## Usage

### Generator Window

| Tab | Purpose |
|-----|---------|
| **Generate** | Enter prompt, image size, seed, negative prompt — click Generate. Progress bar shows status. |
| **History** | Browse past generations. Click **Import** to bring a PNG into your project, **Select** to locate it in Project view, **Delete** to remove the history entry. |
| **Settings** | Manage API key, base URL, test connection, clear history. |

### Import Pipeline

When a generation completes:

1. PNG is downloaded from the CDN
2. Saved to `Assets/Spritesynth/Generations/{timestamp}/{prompt}.png`
3. Asset database refreshes and imports the texture
4. The new asset is selected in the Project view
5. A success dialog is shown

### Environment Variable

If the `SPIRESYNTH_API_KEY` environment variable is set, the Settings tab shows a **"Using env var"** badge and the API key field is optional.

## API

The extension communicates with `https://api.spritesynth.com/api`:

- `POST /generations/image` — create a generation job
- `GET /generations/{job_id}` — poll for completion
- `GET {asset.url}` — download the resulting PNG

## License

MIT
