#!/usr/bin/env python3
"""Validate the Round4 task index and render its summary/table into the ledger."""

from __future__ import annotations

import argparse
import json
import re
import sys
from collections import Counter
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[1]
STATUS_PATH = ROOT / "docs/design/reviews/ui-finesse-round4-20261018/ROUND4_STATUS.json"
PROGRESS_PATH = ROOT / "docs/design/reviews/ui-finesse-round4-20261018/ROUND4_PROGRESS.md"
PLAN_PATH = ROOT / "docs/design/UI_FINESSE_ROUND4_64_TASKS_2026-10-18.md"
START_MARKER = "<!-- ROUND4-STATUS-GENERATED-START -->"
END_MARKER = "<!-- ROUND4-STATUS-GENERATED-END -->"

TASK_ID = re.compile(r"S\d{2}-\d{2}")
TASK_STATES = {"待开始", "实施中", "代码完成待验收", "已验收", "已满足", "外部阻塞", "不适用"}
IMPLEMENTATION_STATES = {"not_started", "complete", "not_applicable"}
VERIFICATION_STATES = {"not_run", "pending", "passed", "not_applicable"}
STATE_ORDER = ["已验收", "已满足", "代码完成待验收", "实施中", "外部阻塞", "不适用", "待开始"]


def fail(message: str) -> None:
    raise ValueError(message)


def plan_tasks() -> dict[str, str]:
    rows: dict[str, str] = {}
    for line in PLAN_PATH.read_text(encoding="utf-8-sig").splitlines():
        if not line.startswith("| S"):
            continue
        cells = [cell.strip() for cell in line.strip().strip("|").split("|")]
        if len(cells) >= 2 and TASK_ID.fullmatch(cells[0]):
            if cells[0] in rows:
                fail(f"duplicate ID in plan: {cells[0]}")
            rows[cells[0]] = cells[1]
    return rows


def load_tasks() -> list[dict[str, Any]]:
    model = json.loads(STATUS_PATH.read_text(encoding="utf-8"))
    if not isinstance(model, dict):
        fail("ROUND4_STATUS.json must contain an object")
    if model.get("schemaVersion") != 1:
        fail("unsupported ROUND4_STATUS.json schemaVersion")
    if model.get("sourcePlan") != PLAN_PATH.relative_to(ROOT).as_posix():
        fail("sourcePlan must point to the current Round4 implementation plan")
    scope = model.get("scope", "")
    if "does not change legacy Q/R state" not in scope:
        fail("status index must explicitly preserve legacy Q/R state")

    tasks = model.get("tasks")
    if not isinstance(tasks, list):
        fail("tasks must be a JSON array")
    ids: set[str] = set()
    for task in tasks:
        if not isinstance(task, dict):
            fail("each task entry must be a JSON object")
        task_id = task.get("id", "")
        if not isinstance(task_id, str) or not TASK_ID.fullmatch(task_id):
            fail(f"invalid task ID: {task_id!r}")
        if task_id in ids:
            fail(f"duplicate task ID: {task_id}")
        ids.add(task_id)
        if task.get("state") not in TASK_STATES:
            fail(f"{task_id}: invalid state {task.get('state')!r}")
        if task.get("implementationState") not in IMPLEMENTATION_STATES:
            fail(f"{task_id}: invalid implementationState")
        if task.get("controlledVerificationState") not in VERIFICATION_STATES:
            fail(f"{task_id}: invalid controlledVerificationState")
        if task.get("hostVerificationState") not in VERIFICATION_STATES:
            fail(f"{task_id}: invalid hostVerificationState")
        if not isinstance(task.get("implementationCommit"), str) or not task["implementationCommit"].strip():
            fail(f"{task_id}: implementationCommit must be a non-empty string (use — when not implemented)")
        if task["implementationState"] == "complete" and task["implementationCommit"] == "—":
            fail(f"{task_id}: completed implementation needs a commit reference")

        evidence = task.get("evidence", [])
        if not isinstance(evidence, list):
            fail(f"{task_id}: evidence must be an array")
        indexed_paths: set[str] = set()
        indexed_urls: set[str] = set()
        for item in evidence:
            if not isinstance(item, dict):
                fail(f"{task_id}: evidence entries must be JSON objects")
            relative = item.get("path")
            if relative is None:
                url = item.get("url", "")
                if not url.startswith("https://"):
                    fail(f"{task_id}: evidence entry needs a repository path or HTTPS URL")
                indexed_urls.add(url)
                continue
            if not isinstance(relative, str):
                fail(f"{task_id}: evidence path must be a string")
            target = (ROOT / relative).resolve()
            try:
                target.relative_to(ROOT.resolve())
            except ValueError:
                fail(f"{task_id}: evidence path escapes the repository: {relative}")
            if not target.is_file():
                fail(f"{task_id}: evidence file is missing: {relative}")
            indexed_paths.add(target.relative_to(ROOT.resolve()).as_posix())

        if task["controlledVerificationState"] == "passed" and not evidence:
            fail(f"{task_id}: passed controlled verification needs evidence")
        if task["hostVerificationState"] == "passed" and not evidence:
            fail(f"{task_id}: passed host verification needs evidence")

        linked_paths: set[str] = set()
        linked_urls: set[str] = set()
        verification_text = task.get("verificationText", "")
        if not isinstance(verification_text, str):
            fail(f"{task_id}: verificationText must be a string")
        for _, target_text in re.findall(r"\[([^\]]+)\]\(([^)]+)\)", verification_text):
            if target_text.startswith("https://"):
                linked_urls.add(target_text)
                continue
            target = (PROGRESS_PATH.parent / target_text).resolve()
            try:
                linked_paths.add(target.relative_to(ROOT.resolve()).as_posix())
            except ValueError:
                fail(f"{task_id}: verification link escapes the repository: {target_text}")
        if linked_paths != indexed_paths or linked_urls != indexed_urls:
            fail(f"{task_id}: structured evidence index differs from verificationText links")

    expected = plan_tasks()
    if ids != set(expected):
        missing = sorted(set(expected) - ids)
        extra = sorted(ids - set(expected))
        fail(f"plan/status ID mismatch; missing={missing}, extra={extra}")
    for task in tasks:
        if task.get("title") != expected[task["id"]]:
            fail(f"{task['id']}: title differs from the implementation plan")
    return tasks


def markdown_cell(value: str) -> str:
    return str(value).replace("|", "\\|").replace("\r\n", "<br>").replace("\n", "<br>")


def render_block(tasks: list[dict[str, Any]]) -> str:
    state_counts = Counter(task["state"] for task in tasks)
    implementation_counts = Counter(task["implementationState"] for task in tasks)
    controlled_counts = Counter(task["controlledVerificationState"] for task in tasks)
    host_counts = Counter(task["hostVerificationState"] for task in tasks)

    status_text = "、".join(
        f"{state}{state_counts[state]}" for state in STATE_ORDER if state_counts[state]
    )
    implementation_text = "、".join(
        f"{label}{implementation_counts[state]}" for state, label in (
            ("complete", "完成"), ("not_started", "未开始"), ("not_applicable", "不适用")
        )
    )
    controlled_text = "、".join(
        f"{label}{controlled_counts[state]}" for state, label in (
            ("passed", "通过"), ("not_run", "未跑"), ("not_applicable", "不适用")
        )
    )
    host_text = "、".join(
        f"{label}{host_counts[state]}" for state, label in (
            ("passed", "通过"), ("pending", "待验"), ("not_run", "未跑"), ("not_applicable", "不适用")
        )
    )
    lines = [
        f"状态摘要（由 `ROUND4_STATUS.json` 生成）：任务 {len(tasks)} 项；状态 {status_text}。",
        f"实现状态：{implementation_text}；受控验证：{controlled_text}；宿主验证：{host_text}。",
        "",
        "| ID | 任务 | 状态 | 实现 commit | 验证与证据 | 边界/下一步 |",
        "| --- | --- | --- | --- | --- | --- |",
    ]
    for task in tasks:
        cells = [
            task["id"], task["title"], task["state"], task["implementationCommit"],
            task["verificationText"], task["boundary"],
        ]
        lines.append("| " + " | ".join(markdown_cell(cell) for cell in cells) + " |")
    return "\n".join(lines)


def replace_generated_block(document: str, block: str) -> str:
    if document.count(START_MARKER) != 1 or document.count(END_MARKER) != 1:
        fail("progress ledger must contain exactly one generated-block marker pair")
    start = document.index(START_MARKER) + len(START_MARKER)
    end = document.index(END_MARKER)
    if end < start:
        fail("progress ledger generated-block markers are reversed")
    return document[:start] + "\n\n" + block + "\n\n" + document[end:]


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument("--check", action="store_true", help="validate source index and generated ledger")
    mode.add_argument("--write", action="store_true", help="render the generated summary and task table")
    args = parser.parse_args()

    try:
        tasks = load_tasks()
        document = PROGRESS_PATH.read_text(encoding="utf-8-sig")
        rendered = replace_generated_block(document, render_block(tasks))
        if args.write:
            PROGRESS_PATH.write_text(rendered, encoding="utf-8", newline="\n")
            print(f"Rendered Round4 status for {len(tasks)} unique task IDs.")
            return 0
        if document.replace("\r\n", "\n") != rendered.replace("\r\n", "\n"):
            fail("ROUND4_PROGRESS.md is stale; run scripts/render-round4-progress.py --write")
        print(f"Round4 status valid: {len(tasks)} unique IDs, evidence paths present, summary/table current.")
        return 0
    except (OSError, ValueError, json.JSONDecodeError) as error:
        print(f"Round4 progress validation failed: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
