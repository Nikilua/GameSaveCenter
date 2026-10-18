from __future__ import annotations

import importlib.util
import shutil
import unittest
from pathlib import Path


SCRIPT_PATH = Path(__file__).resolve().parents[1] / "validate-environment-capability-cache.py"
REPO_ROOT = Path(__file__).resolve().parents[2]
TEST_TEMP_ROOT = REPO_ROOT / ".tmp/gsc-capability-cache-selftest"
SPEC = importlib.util.spec_from_file_location("environment_capability_cache", SCRIPT_PATH)
assert SPEC is not None and SPEC.loader is not None
cache_validator = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(cache_validator)


class EnvironmentCapabilityCacheTests(unittest.TestCase):
    def setUp(self) -> None:
        if TEST_TEMP_ROOT.exists():
            raise RuntimeError(f"Refusing to reuse an existing test directory: {TEST_TEMP_ROOT}")
        TEST_TEMP_ROOT.mkdir(parents=True)
        self.root = TEST_TEMP_ROOT
        evidence = self.root / "evidence.md"
        evidence.write_text("evidence", encoding="utf-8")
        self.model = {
            "schemaVersion": 1,
            "updatedOn": "2026-10-18",
            "retryPolicy": {
                "retryOnlyWhen": ["a prerequisite changes"],
                "doNotRepeatWithoutChange": ["do not rerun a blocked probe"],
            },
            "capabilities": [
                {
                    "id": "sdk-8-0-423",
                    "scope": "S00-02 UI gate",
                    "status": "missing",
                    "lastCheck": {
                        "checkedOn": "2026-10-18",
                        "method": "read-only SDK list",
                        "result": "SDK is absent",
                    },
                    "retryWhen": ["SDK becomes available"],
                    "evidence": ["evidence.md"],
                }
            ],
        }

    def tearDown(self) -> None:
        if self.root.exists():
            tmp_root = (REPO_ROOT / ".tmp").resolve()
            resolved = self.root.resolve()
            if tmp_root not in resolved.parents:
                raise RuntimeError(f"Refusing to clean a test directory outside .tmp: {resolved}")
            shutil.rmtree(resolved)

    def test_accepts_saved_capability_and_retry_gate(self) -> None:
        cache_validator.validate_cache(self.model, self.root)

    def test_rejects_duplicate_ids(self) -> None:
        duplicate = dict(self.model["capabilities"][0])
        self.model["capabilities"].append(duplicate)
        with self.assertRaisesRegex(ValueError, "duplicate capability ID"):
            cache_validator.validate_cache(self.model, self.root)

    def test_rejects_missing_evidence(self) -> None:
        self.model["capabilities"][0]["evidence"] = ["missing.md"]
        with self.assertRaisesRegex(ValueError, "evidence file is missing"):
            cache_validator.validate_cache(self.model, self.root)

    def test_rejects_unconditional_retry_state(self) -> None:
        capability = self.model["capabilities"][0]
        capability["status"] = "not_checked"
        with self.assertRaisesRegex(ValueError, "must have a null lastCheck"):
            cache_validator.validate_cache(self.model, self.root)

    def test_rejects_unknown_status(self) -> None:
        self.model["capabilities"][0]["status"] = "assumed_available"
        with self.assertRaisesRegex(ValueError, "status is invalid"):
            cache_validator.validate_cache(self.model, self.root)

    def test_rejects_omitted_check_state(self) -> None:
        del self.model["capabilities"][0]["lastCheck"]
        with self.assertRaisesRegex(ValueError, "lastCheck must be present"):
            cache_validator.validate_cache(self.model, self.root)

    def test_rejects_path_escape(self) -> None:
        self.model["capabilities"][0]["evidence"] = ["../outside.md"]
        with self.assertRaisesRegex(ValueError, "escapes the repository"):
            cache_validator.validate_cache(self.model, self.root)


if __name__ == "__main__":
    unittest.main()
