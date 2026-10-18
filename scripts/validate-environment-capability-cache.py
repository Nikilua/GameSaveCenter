#!/usr/bin/env python3
"""Validate the saved environment prerequisites and their retry conditions."""

from __future__ import annotations

import argparse
import json
import re
import sys
from datetime import date
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[1]
CACHE_PATH = ROOT / "docs/ai/ENVIRONMENT_CAPABILITY_CACHE.json"
ID_PATTERN = re.compile(r"[a-z0-9]+(?:-[a-z0-9]+)*\Z")
STATUSES = {
    "available",
    "missing",
    "not_checked",
    "blocked_by_permission",
    "waiting_for_external_evidence",
}


def validate_cache(model: Any, root: Path) -> None:
    if not isinstance(model, dict) or model.get("schemaVersion") != 1:
        raise ValueError("schemaVersion must be 1")
    try:
        date.fromisoformat(model.get("updatedOn"))
    except (TypeError, ValueError) as exc:
        raise ValueError("updatedOn must be an ISO date") from exc
    policy = model.get("retryPolicy")
    if not isinstance(policy, dict) or not isinstance(policy.get("retryOnlyWhen"), list) or not policy["retryOnlyWhen"]:
        raise ValueError("retryPolicy.retryOnlyWhen must be a non-empty list")
    if not isinstance(policy.get("doNotRepeatWithoutChange"), list) or not policy["doNotRepeatWithoutChange"]:
        raise ValueError("retryPolicy.doNotRepeatWithoutChange must be a non-empty list")
    for field in ("retryOnlyWhen", "doNotRepeatWithoutChange"):
        if any(not isinstance(item, str) or not item.strip() for item in policy[field]):
            raise ValueError(f"retryPolicy.{field} must contain non-empty strings")

    capabilities = model.get("capabilities")
    if not isinstance(capabilities, list) or not capabilities:
        raise ValueError("capabilities must be a non-empty list")

    seen: set[str] = set()
    root = root.resolve()
    for index, capability in enumerate(capabilities):
        label = f"capabilities[{index}]"
        if not isinstance(capability, dict):
            raise ValueError(f"{label} must be an object")
        identifier = capability.get("id")
        if not isinstance(identifier, str) or not ID_PATTERN.fullmatch(identifier):
            raise ValueError(f"{label}.id is invalid")
        if identifier in seen:
            raise ValueError(f"duplicate capability ID: {identifier}")
        seen.add(identifier)
        if not isinstance(capability.get("scope"), str) or not capability["scope"].strip():
            raise ValueError(f"{label}.scope must be a non-empty string")
        if not isinstance(capability.get("status"), str) or capability["status"] not in STATUSES:
            raise ValueError(f"{label}.status is invalid")
        retry_when = capability.get("retryWhen")
        if not isinstance(retry_when, list) or not retry_when or any(not isinstance(item, str) or not item.strip() for item in retry_when):
            raise ValueError(f"{label}.retryWhen must be a non-empty list of strings")

        if "lastCheck" not in capability:
            raise ValueError(f"{label}.lastCheck must be present, using null when no check was made")
        last_check = capability["lastCheck"]
        if capability["status"] == "not_checked" and last_check is not None:
            raise ValueError(f"{label}: not_checked capability must have a null lastCheck")
        if last_check is not None:
            if not isinstance(last_check, dict):
                raise ValueError(f"{label}.lastCheck must be an object or null")
            checked_on = last_check.get("checkedOn")
            if checked_on is not None:
                try:
                    date.fromisoformat(checked_on)
                except (TypeError, ValueError) as exc:
                    raise ValueError(f"{label}.lastCheck.checkedOn must be an ISO date or null") from exc
            for field in ("method", "result"):
                if not isinstance(last_check.get(field), str) or not last_check[field].strip():
                    raise ValueError(f"{label}.lastCheck.{field} must be a non-empty string")

        evidence = capability.get("evidence")
        if not isinstance(evidence, list) or not evidence:
            raise ValueError(f"{label}.evidence must be a non-empty list")
        for evidence_path in evidence:
            if not isinstance(evidence_path, str) or not evidence_path.strip():
                raise ValueError(f"{label}.evidence entries must be repository-relative paths")
            relative = Path(evidence_path)
            if relative.is_absolute() or ".." in relative.parts:
                raise ValueError(f"{label}.evidence path escapes the repository")
            resolved = (root / relative).resolve()
            if root not in resolved.parents or not resolved.is_file():
                raise ValueError(f"{label}.evidence file is missing or outside the repository: {evidence_path}")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--path", type=Path, default=CACHE_PATH, help="cache JSON path")
    args = parser.parse_args()
    try:
        model = json.loads(args.path.read_text(encoding="utf-8-sig"))
        validate_cache(model, ROOT)
    except (OSError, json.JSONDecodeError, ValueError) as exc:
        print(f"Environment capability cache validation failed: {exc}", file=sys.stderr)
        return 1
    print(f"Environment capability cache valid: {len(model['capabilities'])} entries, retry gates and evidence present.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
