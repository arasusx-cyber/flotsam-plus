"""Build Flotsam Plus using reference DLLs from a local game installation."""

import argparse
import shutil
import subprocess
import sys
from pathlib import Path
from release_tools import ROOT, DLL_NAME, binary_directory, package_release, write_manifest, sha256


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--game-dir", required=True, type=Path, help="Directory containing Flotsam.exe.")
    parser.add_argument("--dotnet", default="dotnet", help="Path or command for the .NET 8 SDK.")
    args = parser.parse_args()
    game = args.game_dir.expanduser().resolve()
    managed = game / "Flotsam_Data" / "Managed"
    core = game / "BepInEx" / "core"
    required = [managed / "Assembly-CSharp.dll", core / "BepInEx.dll", core / "0Harmony.dll"]
    required += [managed / name for name in [
        "UnityEngine.dll", "UnityEngine.CoreModule.dll", "UnityEngine.IMGUIModule.dll",
        "UnityEngine.InputLegacyModule.dll", "UnityEngine.TextRenderingModule.dll"
    ]]
    missing = [str(path) for path in required if not path.is_file()]
    if missing:
        print("Missing game/BepInEx reference files:\n" + "\n".join(missing), file=sys.stderr)
        return 1
    try:
        subprocess.run([args.dotnet, "run", "--project", str(ROOT / "tests/SpawnBatchTests.csproj"),
                        "-c", "Release"], cwd=ROOT, check=True)
        subprocess.run([args.dotnet, "build", str(ROOT / "src/FlotsamPlus.csproj"), "-c", "Release",
                        "-p:GameDir=" + str(game)], cwd=ROOT, check=True)
        built = ROOT / "src/bin/Release/netstandard2.1" / DLL_NAME
        directory = binary_directory()
        directory.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(built, directory / DLL_NAME)
        write_manifest({
            "method": "Local build using scripts/build.py",
            "game_assembly_sha256": sha256(managed / "Assembly-CSharp.dll"),
            "target_framework": "netstandard2.1",
            "gameplay_test": "Not performed by this script; test the new DLL in the game.",
        })
        for path in package_release():
            print(path)
    except (OSError, ValueError, subprocess.CalledProcessError) as error:
        print("Build failed: " + str(error), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
