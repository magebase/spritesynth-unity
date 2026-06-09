# SpriteSynth Unity Editor Extension

[![UPM](https://img.shields.io/badge/Unity-2022.3%2B-blue)](https://unity.com)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

Generate pixel art game assets directly inside the Unity Editor using the SpriteSynth AI API.

## Features

- **AI Generation** — text-to-pixel-art with style reference, UI elements, and preview modes
- **Full API Coverage** — every SpriteSynth REST endpoint is exposed:
  - **Generations** — create, poll, cancel, retry, variation, list with filters
  - **Image Ops** — to-pixel, resize, remove background, inpaint, edit, rotate
  - **Characters** — CRUD, states, spritesheets, export ZIP, duplicate, assign project
  - **Objects** — CRUD, states, spritesheets, export ZIP, duplicate, assign project
  - **Tilesets** — CRUD, tile listing, export ZIP, duplicate, assign project
  - **Projects** — CRUD, duplicate, archive, unarchive
  - **Assets** — CRUD, bulk destroy, move, download, version history
  - **Templates** — CRUD, apply, duplicate
  - **Account** — API key management, balance, key rotation/revocation
- **Auto-Import** — generated PNGs are downloaded and imported as `Texture2D` assets
- **Generation History** — browse, preview, re-import, and delete past generations
- **Environment Variable Support** — `SPIRESYNTH_API_KEY` env var fallback
- **UPM Package** — standard Unity Package Manager format

## Quick Start

1. **Install** the package via UPM:
   ```
   https://github.com/magebase/spritesynth-unity.git
   ```
2. **Get an API key** from [spritesynth.com](https://spritesynth.com)
3. Open **Tools > SpriteSynth > Generator**
4. Paste your API key in the **Tools & Settings > Settings** tab and click **Save**
5. Write a prompt and click **Generate**

## Editor Window

| Tab | Section | Purpose |
|-----|---------|---------|
| **Generate** | Create Image / Style Reference / UI Elements / Preview | Enter prompt, configure size/seed/negative prompt, generate and preview results |
| **Manage** | Characters / Objects / Tilesets / Projects | Browse, create, edit, delete, duplicate, export assets. View character/object states |
| **Tools & Settings** | Image Ops | To Pixel Art, Resize, Remove BG, Inpaint, Edit, Rotate |
| **Tools & Settings** | History | Browse past generations, re-import, select in Project view, delete |
| **Tools & Settings** | Settings | API key, base URL, test connection, account balance, clear history |

## Import Pipeline

When a generation completes:
1. PNG is downloaded from the CDN
2. Saved to `Assets/Spritesynth/Generations/{timestamp}/{prompt}.png`
3. Asset database refreshes and imports the texture (Point filter, Sprite type)
4. The new asset is selected in the Project view
5. A success dialog is shown

## Environment Variable

If the `SPIRESYNTH_API_KEY` environment variable is set, the Settings tab shows a **"Using env var"** badge and the API key field is optional.

## API Coverage

The extension communicates with `https://api.spritesynth.com/api` and covers:

- **10 generation endpoints** — image, style, UI, preview, list, get, status, cancel, retry, variation
- **6 image operation endpoints** — to-pixel, resize, remove-bg, inpaint, edit, rotate
- **13 character endpoints** — CRUD + states, spritesheets, export, duplicate, thumbnail, assign
- **13 object endpoints** — CRUD + states, spritesheets, export, duplicate, thumbnail, assign
- **10 tileset endpoints** — CRUD + tiles, export, duplicate, thumbnail, assign
- **7 project endpoints** — CRUD + duplicate, archive, unarchive
- **8 asset endpoints** — CRUD + bulk destroy, move, download, versions, restore
- **7 template endpoints** — CRUD + apply, duplicate
- **5 account endpoints** — list, create, delete, revoke, rotate API keys + balance

## License

MIT
