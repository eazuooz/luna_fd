"""Hash Unity authoring inputs and separately report exact package-generated outputs."""
import hashlib
import json
from pathlib import Path
import subprocess
import sys

ALGORITHM_VERSION = "unity-authoring-sha256-v2"
# Addressables 2.x deletes these exact files on Editor initialization and recreates
# them for a Player build. The authored Assets/link.xml remains part of the digest.
# See AddressablesPlayerBuildProcessor.cs:35-49 and :118-124 in the installed package.
GENERATED_OUTPUTS = {
    "Assets/AddressableAssetsData/link.xml":
        "Addressables-generated Player preservation output; deleted at Editor startup and regenerated for a build.",
    "Assets/AddressableAssetsData/link.xml.meta":
        "Import metadata of the Addressables-generated Player preservation output; recreated after startup deletion.",
}

root = Path(__file__).resolve().parents[2]
paths = subprocess.check_output(
    ["git", "ls-files", "-z", "--cached", "--others", "--exclude-standard", "--", "Assets", "Packages", "ProjectSettings"],
    cwd=root,
).decode("utf-8").split("\0")
entries = []
digest = hashlib.sha256()
for name in sorted(set(paths)):
    path = root / name
    if not name or name in GENERATED_OUTPUTS or not path.is_file():
        continue
    content = path.read_bytes()
    sha = hashlib.sha256(content).hexdigest()
    entries.append({"path": name, "size": len(content), "sha256": sha})
    digest.update((name + "\0" + sha + "\n").encode("utf-8"))

excluded = []
for name, reason in GENERATED_OUTPUTS.items():
    path = root / name
    exists = path.is_file()
    content = path.read_bytes() if exists else None
    excluded.append({
        "path": name,
        "reason": reason,
        "exists": exists,
        "size": len(content) if exists else None,
        "sha256": hashlib.sha256(content).hexdigest() if exists else None,
    })

summary = {
    "algorithmVersion": ALGORITHM_VERSION,
    "sha256": digest.hexdigest(),
    "fileCount": len(entries),
    "excludedGeneratedOutputs": excluded,
}
result = dict(summary, files=entries)
if len(sys.argv) > 1:
    Path(sys.argv[1]).write_text(json.dumps(result, indent=2), encoding="utf-8")
print(json.dumps(summary))
