"""Import the supplied store packages without overwriting project settings or GUIDs."""
import argparse
import hashlib
import json
from pathlib import Path, PurePosixPath
import tarfile


def import_package(package, project):
    imported = []
    with tarfile.open(package, "r:gz") as archive:
        members = {item.name: item for item in archive.getmembers()}
        for name, member in members.items():
            if not name.endswith("/pathname"):
                continue
            relative = PurePosixPath(archive.extractfile(member).read().decode().rstrip("\0\r\n"))
            if relative.is_absolute() or ".." in relative.parts:
                raise ValueError(f"Unsafe asset path: {relative}")
            prefix = name.rsplit("/", 1)[0]
            asset = members.get(prefix + "/asset")
            if str(relative) == "Packages/manifest.json":
                incoming = json.loads(archive.extractfile(asset).read())
                target = project / str(relative)
                manifest = json.loads(target.read_text(encoding="utf-8-sig"))
                for key, value in incoming["dependencies"].items():
                    manifest["dependencies"].setdefault(key, value)
                target.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
                continue
            if relative.parts[0] != "Assets":
                raise ValueError(f"Unexpected package path: {relative}")
            target = project / str(relative)
            target.parent.mkdir(parents=True, exist_ok=True)
            if asset is None:
                target.mkdir(exist_ok=True)
            else:
                content = archive.extractfile(asset).read()
                if target.exists() and target.read_bytes() != content:
                    raise FileExistsError(f"Refusing to overwrite existing asset: {target}")
                target.write_bytes(content)
            meta = members.get(prefix + "/asset.meta")
            if meta:
                destination = Path(str(target) + ".meta")
                content = archive.extractfile(meta).read()
                if not destination.exists():
                    destination.write_bytes(content)
                elif asset is not None and destination.read_bytes() != content:
                    raise FileExistsError(f"Conflicting asset GUID: {destination}")
            imported.append(str(relative))
    return {"package": package.name, "sha256": hashlib.sha256(package.read_bytes()).hexdigest(), "assets": imported}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("packages", nargs="+", type=Path)
    parser.add_argument("--project", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    results = [import_package(package, args.project.resolve()) for package in args.packages]
    destination = args.project / "Logs/Revival/package-import.json"
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text(json.dumps(results, indent=2), encoding="utf-8")
    print("Imported:", ", ".join(f"{item['package']} ({len(item['assets'])} assets)" for item in results))
