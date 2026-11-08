"""Verify the source/binary release manifest and required repository files."""

import sys
import json
import re
from release_tools import ROOT, DLL_NAME, binary_directory, verify_manifest


def main():
    try:
        manifest = verify_manifest()
        for name in ["README.md", "README.en.md", "LICENSE", "NOTICE.md", "CHANGELOG.md",
                     "docs/INSTALL_PL.md", "tests/SpawnBatchTests.csproj", "tests/Program.cs",
                     ".github/workflows/ci.yml", "tests/TuningChecks.cs",
                     "tests/GameApiChecks.cs", "tests/GameApiChecks.csproj", "tests/Directory.Build.props",
                     "tests/PointGrantChecks.cs", "tests/LocalizationCapacityChecks.cs",
                     "docs/INSTALL_EN.md"]:
            if not (ROOT / name).is_file():
                raise ValueError("Missing repository file: " + name)
        literal = r'"(?:[^"\\]|\\.)*"'
        catalog_source = (ROOT / "src/Localization.cs").read_text(encoding="utf-8")
        catalog = dict((json.loads(a), json.loads(b)) for a, b in re.findall(
            r"\[(" + literal + r")\]\s*=\s*(" + literal + r"),", catalog_source))
        if len(catalog) < 1:
            raise ValueError("No localization catalog entries found.")
        keys = set()
        for path in (ROOT / "src").glob("*.cs"):
            if path.name == "Localization.cs":
                continue
            text = path.read_text(encoding="utf-8")
            if re.search(r"\b(?:M|ReportError|Cancel)\(\s*T\(", text):
                raise ValueError("Status keys were translated before rendering: " + path.name)
            keys.update(json.loads(value) for value in re.findall(
                r"\b(?:T|F|M|ReportError|Cancel)\(\s*(" + literal + r")", text))
        missing = sorted(keys - catalog.keys())
        if missing:
            raise ValueError("Missing English translations: " + repr(missing))
        print("Verified " + str(len(catalog)) + " PL/EN translation pairs and all literal message keys.")
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
