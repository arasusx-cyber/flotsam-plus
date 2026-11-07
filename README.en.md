# Flotsam Plus

**Spawn resources, disable hunger and thirst independently, and control everything from an in-game menu.**

A Windows plugin for Flotsam using **BepInEx 5.4.x**. Press **F8** to open
the Polish-language menu. Item names follow the game's language.

[Polski](README.md) · [Download v1.0.0](dist/Flotsam_Plus_1.0.0.zip)

## Features

- Independent hunger and thirst switches for living members of the player's town.
- Immediately clears the selected need and prevents new consumption projects.
- Searchable item list, including internal names and IDs.
- Spawn batches of 1–5000 items with presets, cancellation and exact completion counts.
- Respects completed buildings, storage filters and available capacity.
- Excludes quest items and cancels pending batches when the save or town changes.
- Saves need settings and supports a configurable menu key.

Both need switches are off by default. An eating or drinking job already
in progress can still finish after the corresponding switch is enabled.

## Installation

1. Install [BepInEx 5.4.21 x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21) and launch the game once.
2. Close the game and extract the `BepInEx` folder from the release ZIP next to `Flotsam.exe`.
3. Load a save and press **F8**.

Plugin location:

```text
BepInEx/plugins/FlotsamPuchatekPlus/FlotsamPuchatekPlus.dll
```

The original Hansjoy trainer can stay installed under **F9**. Back up a
save before testing: added resources persist when the game is saved.

## Build and tests

Requires **.NET SDK 8**, **Python 3.9+**, and a local game installation with
BepInEx. Game and framework reference DLLs are read from that installation.

```shell
python scripts/build.py --game-dir "/path/to/Flotsam"
dotnet run --project tests/SpawnBatchTests.csproj -c Release
```

The build script tests the queue, compiles the plugin, updates the binary
manifest, and creates the release ZIP. `--dotnet` selects an SDK executable.

GitHub Actions runs the queue tests and verifies and packages the checked-in
binary. It does not compile the full plugin without game reference assemblies.
A matching version tag such as `v1.0.0` publishes a GitHub Release.

## Validation

Version 1.0.0 compiled without errors or warnings against the supplied game
assembly. Eight queue checks passed. The user confirmed that this exact
binary worked in their game on 2026-11-07. Other game builds remain unverified.

Code: [MIT](LICENSE). Dependency and original trainer credits: [NOTICE.md](NOTICE.md).
