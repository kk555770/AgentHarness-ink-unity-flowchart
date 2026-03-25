#!/usr/bin/env python3

from __future__ import annotations

import argparse
import json
from datetime import datetime, timezone
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--suite", required=True, help="測試 suite 名稱。")
    parser.add_argument("--output", required=True, help="manifest 輸出路徑。")
    parser.add_argument("--source-mode", required=True, help="證據來源模式，例如 local / ci。")
    parser.add_argument("--log-file", required=True, help="對應的 Unity log 路徑。")
    parser.add_argument(
        "--test-results-path",
        required=True,
        help="對應的 XML 或測試結果 artifact 路徑。",
    )
    parser.add_argument("--test-platform", default="", help="Unity testPlatform。")
    parser.add_argument("--assembly-name", default="", help="測試 assembly 名稱。")
    parser.add_argument("--repository", default="", help="GitHub repository。")
    parser.add_argument("--workflow-name", default="", help="Workflow 名稱。")
    parser.add_argument("--workflow-run-id", default="", help="Workflow run id。")
    parser.add_argument("--workflow-run-attempt", default="", help="Workflow run attempt。")
    parser.add_argument("--workflow-run-url", default="", help="Workflow run URL。")
    parser.add_argument("--job-name", default="", help="Job 名稱。")
    parser.add_argument("--head-sha", default="", help="對應 commit SHA。")
    parser.add_argument("--head-branch", default="", help="對應 branch。")
    parser.add_argument("--ref-name", default="", help="Git ref 名稱。")
    parser.add_argument("--actor", default="", help="觸發者。")
    parser.add_argument("--runner-os", default="", help="Runner 作業系統。")
    parser.add_argument("--runner-name", default="", help="Runner 名稱。")
    parser.add_argument("--unity-version", default="", help="Unity 版本。")
    parser.add_argument("--unity-auth-mode", default="", help="Unity 驗證模式。")
    return parser.parse_args()


def normalize_path(raw_path: str) -> str:
    path = Path(raw_path)
    if not path.is_absolute():
        path = (ROOT / path).resolve()

    try:
        return path.relative_to(ROOT).as_posix()
    except ValueError:
        return str(path)


def main() -> int:
    args = parse_args()
    output_path = Path(args.output).resolve()
    output_path.parent.mkdir(parents=True, exist_ok=True)

    manifest = {
        "suite": args.suite,
        "source_mode": args.source_mode,
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "test_platform": args.test_platform,
        "assembly_name": args.assembly_name,
        "log_file": normalize_path(args.log_file),
        "test_results_path": normalize_path(args.test_results_path),
        "repository": args.repository,
        "workflow_name": args.workflow_name,
        "workflow_run_id": args.workflow_run_id,
        "workflow_run_attempt": args.workflow_run_attempt,
        "workflow_run_url": args.workflow_run_url,
        "job_name": args.job_name,
        "head_sha": args.head_sha,
        "head_branch": args.head_branch,
        "ref_name": args.ref_name,
        "actor": args.actor,
        "runner_os": args.runner_os,
        "runner_name": args.runner_name,
        "unity_version": args.unity_version,
        "unity_auth_mode": args.unity_auth_mode,
    }
    filtered_manifest = {
        key: value for key, value in manifest.items() if value not in {"", None}
    }

    output_path.write_text(
        json.dumps(filtered_manifest, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    print(f"[write_unity_evidence_manifest] 已更新 {normalize_path(str(output_path))}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
