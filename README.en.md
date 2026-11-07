# Flotsam Plus

Spawn resources and disable hunger or thirst for your town. Open the Polish-language menu with **F8**. Item names follow the game's language.

**[⬇ Download Flotsam Plus 1.0.0](https://github.com/arasusx-cyber/flotsam-plus/raw/refs/heads/main/dist/Flotsam_Plus_1.0.0.zip)** · [Polski](README.md)

Windows x64 · BepInEx 5.4.x · Mod version 1.0.0

![Flotsam Plus menu](docs/images/menu.png)

## Features

| Option | Effect |
| --- | --- |
| Disable hunger | Clears current hunger and prevents it from increasing. |
| Disable thirst | Clears current thirst and prevents it from increasing. |
| Spawn resources | Search for an item and add 1–5000 units. |

Resources go to compatible, completed storages with available capacity. The menu reports the actual number added and supports cancellation. Hunger and thirst settings are saved.

## Installation

1. Install [BepInEx 5.4.21 x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21), run the game once, then close it. Skip this step if BepInEx 5.4.x is already installed.
2. Extract the **BepInEx** folder from the [mod ZIP](https://github.com/arasusx-cyber/flotsam-plus/raw/refs/heads/main/dist/Flotsam_Plus_1.0.0.zip) next to **Flotsam.exe**.
3. Load a save and press **F8**.

Plugin location:

```text
BepInEx/plugins/FlotsamPuchatekPlus/FlotsamPuchatekPlus.dll
```

Both need switches are off by default. Water requires liquid storage. A consumption job already in progress can still finish after its need is disabled. Back up your save before use: spawned items persist when the game is saved.

The original Hansjoy trainer can remain installed with its own **F9** menu.

## Documentation

[Detailed installation (Polish)](docs/INSTALL_PL.md) · [Build and development (Polish)](docs/DEVELOPMENT.md) · [Changelog](CHANGELOG.md)

Code: [MIT](LICENSE). Dependencies and credits: [NOTICE.md](NOTICE.md).
