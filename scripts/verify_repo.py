"""Verify the source/binary release manifest and required repository files."""

import sys
from release_tools import ROOT, DLL_NAME, binary_directory, verify_manifest


def main():
    try:
        manifest = verify_manifest()
        for name in ["README.md", "README.en.md", "LICENSE", "NOTICE.md", "CHANGELOG.md",
                     "docs/INSTALL_PL.md", "tests/SpawnBatchTests.csproj", "tests/Program.cs",
                     ".github/workflows/ci.yml"]:
            if not (ROOT / name).is_file():
                raise ValueError("Missing repository file: " + name)
        for path in ROOT.rglob("*.dll"):
            relative = path.relative_to(ROOT)
            if any(part in ("bin", "obj", ".git") for part in relative.parts):
                continue
            if not (len(relative.parts) == 3 and relative.parts[0] == "binaries"
                    and relative.name == DLL_NAME):
                raise ValueError("Unexpected dependency DLL in the repository: " + str(path))
        print("Verified Flotsam Plus " + manifest["version"])
        print("Plugin DLL and all source checksums match the release manifest.")
        return 0
    except (OSError, ValueError) as error:
        print("Verification failed: " + str(error), file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
