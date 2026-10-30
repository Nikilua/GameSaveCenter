#!/usr/bin/env python3
"""Rebuild the U00 fixed-cycle and focused page-action comparison."""

import csv
import hashlib
import json
import math
import re
from pathlib import Path


HERE = Path(__file__).resolve().parent
U00 = HERE / "u00"
CANDIDATE = HERE.parent / "fixed-gc-rerun-20261030"
ACTION_RUNS = [
    ("u00-run-4", U00 / "run-4-page-actions" / "actions.csv"),
    ("u00-run-5", U00 / "run-5-scroll-context" / "actions.csv"),
    ("candidate-run-1", HERE / "candidate-actions" / "run-1-page-actions" / "actions.csv"),
    ("candidate-run-2", HERE / "candidate-actions" / "run-2-scroll-context" / "actions.csv"),
]


def percentile(values, p):
    ordered = sorted(values)
    if not ordered:
        return None
    return ordered[max(0, math.ceil(p * len(ordered)) - 1)]


def sha256(path):
    h = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            h.update(block)
    return h.hexdigest()


def action_summary(path):
    with path.open(newline="", encoding="utf-8-sig") as stream:
        rows = list(csv.DictReader(stream))
    result = {}
    for action_type in ("scroll_offset", "synthetic_search_input"):
        selected = [
            row for row in rows
            if row.get("page") == "Task" and row.get("action", "").startswith(action_type)
        ]
        if not selected:
            continue
        durations = [float(row["duration_ms"]) for row in selected]
        allocations = [int(row["allocated_bytes"]) for row in selected]
        result[action_type] = {
            "n": len(selected),
            "actionNames": sorted({row["action"] for row in selected}),
            "p50MsNearestRank": percentile(durations, 0.50),
            "p95MsNearestRank": percentile(durations, 0.95),
            "maxMs": max(durations),
            "p95AllocatedBytesNearestRank": percentile(allocations, 0.95),
            "maxAllocatedBytes": max(allocations),
        }
    return result


def fixed_cycle_rows(directory, source_name):
    rows = []
    for run_dir in sorted(directory.glob("run-*")):
        if run_dir.name not in ("run-1", "run-2", "run-3"):
            continue
        csv_path = run_dir / "parts.csv"
        if not csv_path.is_file():
            continue
        with csv_path.open(newline="", encoding="utf-8-sig") as stream:
            for row in csv.DictReader(stream):
                if row["part"] not in ("theme", "layout"):
                    continue
                rows.append({
                    "source": source_name,
                    "run": run_dir.name,
                    "part": row["part"],
                    "durationMs": float(row["duration_ms"]),
                    "allocatedBytes": int(row["allocated_bytes"]),
                    "gc0Delta": int(row["gc0_delta"]),
                    "gc1Delta": int(row["gc1_delta"]),
                    "gc2Delta": int(row["gc2_delta"]),
                })
    return rows


def segment_summary(rows):
    result = {}
    for part in ("theme", "layout"):
        values = [row for row in rows if row["part"] == part]
        durations = [row["durationMs"] for row in values]
        result[part] = {
            "n": len(values),
            "p50MsNearestRank": percentile(durations, 0.50),
            "p95MsNearestRank": percentile(durations, 0.95),
            "maxMs": max(durations) if durations else None,
            "over100Ms": sum(value > 100 for value in durations),
            "over300Ms": sum(value > 300 for value in durations),
            "operationsWithAnyGc": sum(
                row["gc0Delta"] + row["gc1Delta"] + row["gc2Delta"] > 0
                for row in values
            ),
            "operationsWithGen0": sum(row["gc0Delta"] > 0 for row in values),
        }
    return result


def active_allocations(directory):
    values = []
    for run_dir in sorted(directory.glob("run-*")):
        if run_dir.name not in ("run-1", "run-2", "run-3"):
            continue
        path = run_dir / "performance-summary.txt"
        if not path.is_file():
            continue
        match = re.search(r"^ActiveAllocatedBytes=(\d+)", path.read_text(encoding="utf-8-sig"), re.MULTILINE)
        if match:
            values.append({"run": run_dir.name, "bytes": int(match.group(1))})
    return values


def main():
    u00_analysis = json.loads((U00 / "analysis.json").read_text(encoding="utf-8"))
    candidate_analysis = json.loads((HERE / "candidate-analysis.json").read_text(encoding="utf-8"))
    u00_rows = fixed_cycle_rows(U00, "U00-reconstructed")
    candidate_rows = fixed_cycle_rows(CANDIDATE, "candidate")
    u00_alloc = active_allocations(U00)
    candidate_alloc = active_allocations(CANDIDATE)
    u00_mean = sum(item["bytes"] for item in u00_alloc) / len(u00_alloc)
    candidate_mean = sum(item["bytes"] for item in candidate_alloc) / len(candidate_alloc)
    action_runs = {
        name: {
            "path": path.relative_to(HERE).as_posix(),
            "sha256": sha256(path),
            "taskPage": action_summary(path),
        }
        for name, path in ACTION_RUNS
    }
    output = {
        "schemaVersion": 1,
        "baselineCommit": "90bf040567f013f3a8d9ed8667028a4edb2ec524",
        "candidateCommit": "5c0c57b06aed27dc46c41ef3d655ec20dafa5ec7",
        "measurement": {
            "sdk": "8.0.423",
            "sampleTimeUtc": "2026-10-28T09:00:00Z",
            "fixedCycles": 70,
            "completedActionsPerRun": 297,
            "postActionSettleSeconds": 30,
            "forcedGc": False,
            "windowDip": "1040x700",
            "themes": ["light", "dark"],
        },
        "fixedCycle": {
            "u00": {
                "rawRows": len(u00_rows),
                "segments": segment_summary(u00_rows),
                "runs": u00_analysis["runs"],
                "activeAllocatedBytes": u00_alloc,
                "meanActiveAllocatedBytes": u00_mean,
            },
            "candidate": {
                "rawRows": len(candidate_rows),
                "segments": segment_summary(candidate_rows),
                "runs": candidate_analysis["runs"],
                "activeAllocatedBytes": candidate_alloc,
                "meanActiveAllocatedBytes": candidate_mean,
            },
            "activeAllocationChangeBytes": candidate_mean - u00_mean,
            "activeAllocationChangePercentOfU00": (candidate_mean - u00_mean) / u00_mean * 100,
            "renderingAggregate": {
                "u00": u00_analysis.get("combined", {}).get("activeRenderingIntervals"),
                "candidate": candidate_analysis.get("combined", {}).get("activeRenderingIntervals"),
            },
        },
        "taskPageActionRuns": action_runs,
        "limitations": [
            "The U00 source predates FakeSampleClock; 45 FakeDashboardData DateTime.UtcNow uses were redirected to the pinned fake clock in the isolated probe copy.",
            "These are controlled offscreen WPF harness measurements, not Playnite host rendering or physical presented/VSync frames.",
            "Programmatic search edits are logical input proxies, not physical keyboard or IME response measurements.",
            "The scroll action calls ScrollToVerticalOffset and UpdateLayout; its measured duration includes forced layout work.",
            "A single focused action run per source does not identify a causal template or resource regression.",
            "GC counts are operation-window deltas and correlation is descriptive, not causal.",
        ],
    }
    output_path = HERE / "comparison.json"
    output_path.write_text(json.dumps(output, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({
        "output": output_path.as_posix(),
        "u00Rows": len(u00_rows),
        "candidateRows": len(candidate_rows),
        "activeAllocationChangePercentOfU00": output["fixedCycle"]["activeAllocationChangePercentOfU00"],
        "actionRuns": list(action_runs),
    }, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
