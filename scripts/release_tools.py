"""Local build and deterministic packaging helpers; standard library only."""

import hashlib
import json
import re
import xml.etree.ElementTree as ET
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DLL_NAME = "FlotsamPuchatekPlus.dll"
SOURCE_FILES = ("src/FlotsamPlus.csproj", "src/Plugin.cs", "src/SpawnBatch.cs")


def sha256(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def project_version():
    version = ET.parse(ROOT / "src/FlotsamPlus.csproj").findtext(".//Version")
    if not version or not re.fullmatch(r"\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?", version):
        raise ValueError("The project must declare a valid release version.")
    return version


def binary_directory():
    return ROOT / "binaries" / project_version()


def source_hashes():
    return {name: sha256(ROOT / name) for name in SOURCE_FILES}


def write_manifest(build_info):
    directory = binary_directory()
    manifest = {
        "version": project_version(),
        "binary": DLL_NAME,
        "binary_sha256": sha256(directory / DLL_NAME),
        "source_sha256": source_hashes(),
        "build_info": build_info,
    }
    (directory / "manifest.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    return manifest


def verify_manifest():
    directory = binary_directory()
    manifest = json.loads((directory / "manifest.json").read_text(encoding="utf-8"))
    if manifest.get("version") != project_version() or manifest.get("binary") != DLL_NAME:
        raise ValueError("Binary manifest version or filename does not match the project.")
    if manifest.get("binary_sha256") != sha256(directory / DLL_NAME):
        raise ValueError("Plugin DLL checksum mismatch. Rebuild before packaging.")
    if manifest.get("source_sha256") != source_hashes():
        raise ValueError("Plugin sources changed. Run scripts/build.py before packaging.")
    source = (ROOT / "src/Plugin.cs").read_text(encoding="utf-8")
    matches = re.findall(r'\[BepInPlugin\([^\n]*,\s*"([0-9A-Za-z.-]+)"\)\]', source)
    if matches != [project_version()]:
        raise ValueError("BepInPlugin version and project version do not match.")
    return manifest


def package_release(tag=None):
    manifest = verify_manifest()
    version = project_version()
    if tag is not None and tag != "v" + version:
        raise ValueError("Release tag must be v" + version)
    directory = ROOT / "dist"
    directory.mkdir(exist_ok=True)
    target = directory / ("Flotsam_Plus_" + version + ".zip")
    payload = {
        "BepInEx/plugins/FlotsamPuchatekPlus/" + DLL_NAME: (binary_directory() / DLL_NAME).read_bytes(),
        "INSTRUKCJA_PL.txt": (ROOT / "docs/INSTALL_PL.md").read_bytes(),
        "LICENSE": (ROOT / "LICENSE").read_bytes(),
        "NOTICE.md": (ROOT / "NOTICE.md").read_bytes(),
        "CHANGELOG.md": (ROOT / "CHANGELOG.md").read_bytes(),
    }
    # Fixed timestamps and ordering give identical ZIPs for identical contents.
    with zipfile.ZipFile(target, "w", compression=zipfile.ZIP_DEFLATED) as archive:
        for name, data in sorted(payload.items()):
            info = zipfile.ZipInfo(name, date_time=(1980, 1, 1, 0, 0, 0))
            info.compress_type = zipfile.ZIP_DEFLATED
            info.external_attr = 0o100644 << 16
            archive.writestr(info, data)
    with zipfile.ZipFile(target) as archive:
        if archive.testzip() is not None:
            raise ValueError("Release archive is corrupt.")
        dll = archive.read("BepInEx/plugins/FlotsamPuchatekPlus/" + DLL_NAME)
        if hashlib.sha256(dll).hexdigest() != manifest["binary_sha256"]:
            raise ValueError("Packaged DLL does not match the verified binary.")
    checksum_file = directory / "SHA256SUMS.txt"
    checksum_file.write_text(
        sha256(target) + "  " + target.name + "\n", encoding="ascii"
    )
    return target, checksum_file
