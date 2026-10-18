from __future__ import annotations

import importlib.util
import json
import shutil
import unittest
from pathlib import Path


SCRIPT_PATH = Path(__file__).resolve().parents[1] / "render-round4-progress.py"
TEST_TEMP_ROOT = Path(__file__).resolve().parents[2] / ".tmp/gsc-round4-status-selftest"
SPEC = importlib.util.spec_from_file_location("round4_progress", SCRIPT_PATH)
assert SPEC is not None and SPEC.loader is not None
round4_progress = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(round4_progress)


class Round4ProgressTests(unittest.TestCase):
    def setUp(self) -> None:
        if TEST_TEMP_ROOT.exists():
            raise RuntimeError(f"Refusing to reuse an existing test directory: {TEST_TEMP_ROOT}")
        TEST_TEMP_ROOT.mkdir(parents=True)
        self.root = TEST_TEMP_ROOT
        self.status_path = self.root / "ROUND4_STATUS.json"
        self.progress_path = self.root / "ROUND4_PROGRESS.md"
        self.plan_path = self.root / "plan.md"
        self.progress_path.write_text(
            "start\n<!-- ROUND4-STATUS-GENERATED-START -->\n<!-- ROUND4-STATUS-GENERATED-END -->\n",
            encoding="utf-8",
        )
        evidence = self.root / "docs/evidence.md"
        evidence.parent.mkdir(parents=True, exist_ok=True)
        evidence.write_text("fixture evidence", encoding="utf-8")
        self.plan_path.write_text("| ID | Task |\n| --- | --- |\n| S00-01 | Command index |\n", encoding="utf-8")
        round4_progress.ROOT = self.root
        round4_progress.STATUS_PATH = self.status_path
        round4_progress.PROGRESS_PATH = self.progress_path
        round4_progress.PLAN_PATH = self.plan_path
        self.write_model([self.task()])

    def tearDown(self) -> None:
        if self.root.exists():
            shutil.rmtree(self.root)

    @staticmethod
    def task() -> dict[str, object]:
        return {
            "id": "S00-01",
            "title": "Command index",
            "state": "已验收",
            "implementationState": "complete",
            "implementationCommit": "abc12345",
            "controlledVerificationState": "passed",
            "hostVerificationState": "not_run",
            "verificationText": "[evidence](docs/evidence.md)",
            "evidence": [{"label": "evidence", "path": "docs/evidence.md"}],
            "boundary": "host not run",
        }

    def write_model(self, tasks: list[dict[str, object]]) -> None:
        model = {
            "schemaVersion": 1,
            "sourcePlan": round4_progress.PLAN_PATH.relative_to(round4_progress.ROOT).as_posix(),
            "scope": "Round4 only; does not change legacy Q/R state or external blockers.",
            "tasks": tasks,
        }
        self.status_path.write_text(json.dumps(model), encoding="utf-8")

    def test_accepts_unique_plan_aligned_status(self) -> None:
        tasks = round4_progress.load_tasks()
        self.assertEqual([task["id"] for task in tasks], ["S00-01"])
        block = round4_progress.render_block(tasks)
        self.assertIn("实现状态：完成1、未开始0、不适用0", block)
        self.assertIn("受控验证：通过1、未跑0、不适用0", block)
        self.assertIn("宿主验证：通过0、待验0、未跑1、不适用0", block)

    def test_rejects_duplicate_task_ids(self) -> None:
        self.write_model([self.task(), self.task()])
        with self.assertRaisesRegex(ValueError, "duplicate task ID"):
            round4_progress.load_tasks()

    def test_rejects_complete_implementation_without_commit(self) -> None:
        task = self.task()
        task["implementationCommit"] = "—"
        self.write_model([task])
        with self.assertRaisesRegex(ValueError, "completed implementation needs a commit reference"):
            round4_progress.load_tasks()

    def test_rejects_missing_evidence_files(self) -> None:
        task = self.task()
        task["evidence"] = [{"label": "evidence", "path": "docs/missing.md"}]
        self.write_model([task])
        with self.assertRaisesRegex(ValueError, "evidence file is missing"):
            round4_progress.load_tasks()

    def test_rejects_mismatch_between_evidence_index_and_row_link(self) -> None:
        evidence = self.root / "docs/evidence.md"
        evidence.parent.mkdir(parents=True, exist_ok=True)
        evidence.write_text("evidence", encoding="utf-8")
        other = self.root / "docs/other.md"
        other.write_text("other", encoding="utf-8")
        task = self.task()
        task["verificationText"] = "[evidence](docs/other.md)"
        task["evidence"] = [{"label": "evidence", "path": "docs/evidence.md"}]
        self.write_model([task])
        with self.assertRaisesRegex(ValueError, "structured evidence index differs"):
            round4_progress.load_tasks()


if __name__ == "__main__":
    unittest.main()
