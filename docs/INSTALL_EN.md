# Flotsam Plus 1.3.0 installation

1. Close Flotsam.
2. Extract the ZIP's `BepInEx` folder beside `Flotsam.exe`, replacing the existing plugin DLL:

```text
BepInEx/plugins/FlotsamPuchatekPlus/FlotsamPuchatekPlus.dll
```

3. Load a save, press **F8** and select **EN** at the top of the menu.

First installation requires BepInEx 5.4.x x64. The package contains the plugin, not the loader or game libraries. Do not leave a duplicate DLL under another name. Existing configuration is preserved.

## Language and weight capacity

Switch **PL / EN** without restarting. Your selection is saved as `Jezyk = English` or `Jezyk = Polski` in the existing config's `[Menu]` section. Item/category names follow Flotsam's language; this option changes only the mod UI.

Open **Ship** and increase **Town weight capacity** from ×1 to ×100, or enable **Disable town weight limit** to keep building when overweight. The slider is inactive while unlimited mode is enabled. The game refreshes capacity on its next frame.

These settings change total town weight allowance, not storage slots. Actual town weight, item filters, mooring, geometric placement rules and unlock requirements stay native. Returning to normal capacity can make an expanded town overweight again. Ship reset and full reset restore normal capacity and keep your language choice.

## Other controls

- Ship speed, turning, acceleration and braking: ×1–×5, independently. Energy-free travel is optional.
- Residents: hunger/thirst/nighttime sleep switches, walking/swimming/island movement ×1–×5.
- Skill points: select a living resident, grant 1–100 unspent points, and spend them through the normal character panel. XP, levels and native caps remain unchanged.
- Construction: instant new buildings/upgrades without materials; existing jobs finish normally.
- Research: ×1–×100; points, a researcher and an active station are still required.
- Resources: select an item and add 1–5000 to compatible storage with space. Full storage stops the batch with the actual inserted count. Water needs liquid storage.
- Map: reveal all generated world tiles and optionally reveal future tiles after the game generates them. Locked narrative locations stay under game control.

Reset does not undo items, skill points, discoveries, completed buildings or research. Back up your save before editing it.

## Configuration and troubleshooting

```text
BepInEx/config/pl.puchatek.flotsam.surowce-potrzeby.cfg
BepInEx/LogOutput.log
```

F8 controls Flotsam Plus; the older Hansjoy trainer may remain on F9. For compatibility problems, check the log for the failed feature group. To uninstall, close the game and remove the plugin folder above.

1.3.0 passes 43 logic checks and 82 game API/IL checks; new gameplay still awaits user confirmation. Version 1.2.0 was confirmed working by the user on 8 November 2026. Older packages remain available.
