# Flotsam Plus

A Polish **F8** menu for ship handling, residents, skill points, map discovery, construction, research and resources.

**[⬇ Download Flotsam Plus 1.2.0](https://github.com/arasusx-cyber/flotsam-plus/raw/refs/heads/main/dist/Flotsam_Plus_1.2.0.zip)** · [Polski](README.md)

Windows x64 · BepInEx 5.4.x · Version 1.2.0

**Version 1.2.0 was confirmed working in-game by the user on 8 November 2026.**

| Tab | Features |
|---|---|
| Ship | Independent travel speed, turning, acceleration and braking multipliers ×1–×5; energy-free travel |
| Residents | Independent hunger, thirst and nighttime sleep switches; movement ×1–×5; grant unspent skill points to a selected resident |
| Construction and research | Instant new construction/upgrades without materials; research speed ×1–×100 |
| Resources | Search items and insert 1–5000 into compatible storage with available capacity |
| Map | Reveal the available map and optionally keep revealing newly generated areas |

Ship reset restores vanilla multipliers and travel energy use. A separate reset disables all conveniences. Existing hunger/thirst settings are preserved; new switches start disabled and multipliers at ×1.

Speed and turning cover manual movement and route travel. Acceleration changes force-based movement; direct movement modes have no acceleration phase. Braking reduces coasting after releasing movement controls.

Research still requires points and a station. Sleep already in progress finishes normally. Existing construction jobs are not forcibly completed.

## New in 1.2.0

Resident speed covers walking, swimming and island navigation. Select a living resident to grant 1–100 unspent skill points per click, then spend them through the normal character panel. XP, levels and native skill caps are preserved.

Map reveal covers all currently generated world tiles. Automatic reveal also handles areas generated later during travel. It does not generate future biomes or activate disabled narrative locations. Discovery and granted points persist in the save; resetting settings does not undo them.

## Update / install

Close the game and extract `BepInEx` beside `Flotsam.exe`, replacing:

```text
BepInEx/plugins/FlotsamPuchatekPlus/FlotsamPuchatekPlus.dll
```

Load a save and press **F8**. Do not keep duplicate copies of this plugin. First-time setup needs [BepInEx 5.4.21 x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21). The older Hansjoy trainer can remain on F9.

## In-game screenshot

![Flotsam Plus 1.2.0 in-game menu](docs/images/menu-1.2.0.png)

*Flotsam Plus 1.2.0 F8 menu, supplied by the user after confirming the mod working in-game.*

## Validation

Version 1.2.0 builds with zero errors/warnings and passes 27 resource/tuning/skill-point checks plus 68 game API/IL checks against the supplied game assembly. **The user confirmed version 1.2.0 working in a running game on 8 November 2026.**

The previous [1.1.0 package](https://github.com/arasusx-cyber/flotsam-plus/raw/refs/heads/main/dist/Flotsam_Plus_1.1.0.zip) was confirmed working in-game by the user on 8 November 2026. The [1.0.0 package](https://github.com/arasusx-cyber/flotsam-plus/raw/refs/heads/main/dist/Flotsam_Plus_1.0.0.zip) remains available.

[Installation details (PL)](docs/INSTALL_PL.md) · [Development (PL)](docs/DEVELOPMENT.md) · [Changelog](CHANGELOG.md)

[MIT license](LICENSE) · [Credits](NOTICE.md)
