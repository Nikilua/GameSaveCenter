#!/usr/bin/env python3
"""Write SHA-256 identities for every file in this evidence folder."""

import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path


ROOT = Path(__file__).resolve().parent
MANIFEST = ROOT / "sha256.json"

files = []
for path in sorted(p for p in ROOT.rglob("*") if p.is_file() and p != MANIFEST):
    digest = hashlib.sha256(path.read_bytes()).hexdigest().upper()
    files.append({
        "path": path.relative_to(ROOT).as_posix(),
        "sha256": digest,
        "bytes": path.stat().st_size,
    })

MANIFEST.write_text(json.dumps({
    "generatedAtUtc": datetime.now(timezone.utc).isoformat(),
    "fileCount": len(files),
    "files": files,
}, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps({"manifest": MANIFEST.as_posix(), "fileCount": len(files)}, indent=2))
