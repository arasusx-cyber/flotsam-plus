# Flotsam Plus

A Polish **F8** menu for ship handling, needs, construction, research and resources.

**[⬇ Download Flotsam Plus 1.1.0](https://github.com/arasusx-cyber/flotsam-plus/raw/refs/heads/main/dist/Flotsam_Plus_1.1.0.zip)** · [Polski](README.md)

Windows x64 · BepInEx 5.4.x · Version 1.1.0

| Tab | Features |
|---|---|
| Ship | Independent travel speed, turning, acceleration and braking multipliers ×1–×5; energy-free travel |
| Residents | Independent hunger, thirst and nighttime sleep switches |
| Construction and research | Instant new construction/upgrades without materials; research speed ×1–×100 |
| Resources | Search items and insert 1–5000 into compatible storage with available capacity |

Ship reset restores vanilla multipliers and travel energy use. A separate reset disables all conveniences. Existing hunger/thirst settings are preserved; new switches start disabled and multipliers at ×1.

Speed and turning cover manual movement and route travel. Acceleration changes force-based movement; direct movement modes have no acceleration phase. Braking reduces coasting after releasing movement controls.

Research still requires points and a station. Sleep already in progress finishes normally. Existing construction jobs are not forcibly completed.

## Update / install

Close the game and extract `BepInEx` beside `Flotsam.exe`, replacing:

```text
BepInEx/plugins/FlotsamPuchatekPlus/FlotsamPuchatekPlus.dll
```

Load a save and press **F8**. Do not keep duplicate copies of this plugin. First-time setup needs [BepInEx 5.4.21 x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21). The older Hansjoy trainer can remain on F9.

## Validation

Version 1.1.0 builds with zero errors/warnings and passes 21 resource/tuning/reset checks plus 43 game API/IL checks against the supplied game assembly. **The new version has not yet been tested in a running game.**

The previous [1.0.0 package](https://github.com/arasusx-cyber/flotsam-plus/raw/refs/heads/main/dist/Flotsam_Plus_1.0.0.zip) remains available.

[Installation details (PL)](docs/INSTALL_PL.md) · [Development (PL)](docs/DEVELOPMENT.md) · [Changelog](CHANGELOG.md)

[MIT license](LICENSE) · [Credits](NOTICE.md)
