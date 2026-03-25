#!/usr/bin/env python3

from __future__ import annotations

import argparse
import io
import json
import os
import shutil
import subprocess
from datetime import datetime, timezone
from pathlib import Path
from urllib.error import HTTPError
from urllib.request import Request, urlopen
import zipfile


ROOT = Path(__file__).resolve().parent.parent
DEFAULT_OUTPUT_DIR = ROOT / "Artifacts" / "CI" / "TestResults"
ARTIFACT_PREFIXES = {
    "unity-test-results-": "test-results",
    "unity-evidence-": "evidence",
}


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--workflow-file",
        default="CI.yml",
        help="要抓測試 artifact 的 workflow 檔名。",
    )
    parser.add_argument(
        "--run-id",
        default="",
        help="指定要抓的 workflow run id；若未提供，才回退到 workflow_run env 或最新成功 CI。",
    )
    parser.add_argument(
        "--output-dir",
        default=str(DEFAULT_OUTPUT_DIR),
        help="下載後的輸出目錄。",
    )
    return parser.parse_args()


def classify_artifact(name: str) -> str | None:
    for prefix, kind in ARTIFACT_PREFIXES.items():
        if name.startswith(prefix):
            return kind
    return None


def github_headers(token: str) -> dict[str, str]:
    return {
        "Accept": "application/vnd.github+json",
        "Authorization": f"Bearer {token}",
        "X-GitHub-Api-Version": "2022-11-28",
    }


def normalize_api_target(url: str) -> str:
    prefix = "https://api.github.com/"
    if url.startswith(prefix):
        return url[len(prefix) :]
    return url


def gh_env(token: str) -> dict[str, str]:
    env = os.environ.copy()
    env["GH_TOKEN"] = token
    return env


def gh_api_json(url: str, token: str) -> dict:
    result = subprocess.run(
        ["gh", "api", normalize_api_target(url)],
        check=True,
        capture_output=True,
        text=True,
        env=gh_env(token),
    )
    return json.loads(result.stdout)


def gh_api_bytes(url: str, token: str) -> bytes:
    result = subprocess.run(
        ["gh", "api", normalize_api_target(url)],
        check=True,
        capture_output=True,
        env=gh_env(token),
    )
    return result.stdout


def api_json(url: str, token: str) -> dict:
    try:
        request = Request(url, headers=github_headers(token))
        with urlopen(request) as response:
            return json.load(response)
    except Exception:
        if shutil.which("gh"):
            return gh_api_json(url, token)
        raise


def download_bytes(url: str, token: str) -> bytes:
    try:
        request = Request(url, headers=github_headers(token))
        with urlopen(request) as response:
            return response.read()
    except Exception:
        if shutil.which("gh"):
            return gh_api_bytes(url, token)
        raise


def filter_relevant_artifacts(artifacts: list[dict]) -> list[dict]:
    selected: list[dict] = []
    for artifact in artifacts:
        name = artifact.get("name", "")
        kind = classify_artifact(name)
        if artifact.get("expired") is False and kind is not None:
            selected.append({**artifact, "kind": kind})
    return selected


def find_latest_successful_run(
    repo: str, workflow_file: str, token: str
) -> tuple[dict | None, list[dict]]:
    url = (
        f"https://api.github.com/repos/{repo}/actions/workflows/{workflow_file}/runs"
        "?status=completed&per_page=100"
    )
    payload = api_json(url, token)
    fallback_run = None
    for run in payload.get("workflow_runs", []):
        if run.get("conclusion") != "success":
            continue
        if fallback_run is None:
            fallback_run = run
        artifacts = filter_relevant_artifacts(
            list_artifacts(repo, str(run["id"]), token)
        )
        if artifacts:
            return run, artifacts
    return fallback_run, []


def get_run(repo: str, run_id: str, token: str) -> dict:
    url = f"https://api.github.com/repos/{repo}/actions/runs/{run_id}"
    return api_json(url, token)


def list_artifacts(repo: str, run_id: str, token: str) -> list[dict]:
    url = f"https://api.github.com/repos/{repo}/actions/runs/{run_id}/artifacts?per_page=100"
    payload = api_json(url, token)
    return payload.get("artifacts", [])


def list_jobs(repo: str, run_id: str, token: str) -> list[dict]:
    url = f"https://api.github.com/repos/{repo}/actions/runs/{run_id}/jobs?per_page=100"
    payload = api_json(url, token)
    return payload.get("jobs", [])


def normalize_job(job: dict) -> dict:
    return {
        "id": job.get("id"),
        "name": job.get("name"),
        "status": job.get("status"),
        "conclusion": job.get("conclusion"),
        "started_at": job.get("started_at"),
        "completed_at": job.get("completed_at"),
        "steps": [
            {
                "name": step.get("name"),
                "status": step.get("status"),
                "conclusion": step.get("conclusion"),
            }
            for step in job.get("steps", [])
        ],
    }


def write_metadata(
    output_dir: Path,
    mode: str,
    run: dict,
    jobs: list[dict],
    artifacts: list[dict],
    artifact_roots: list[Path],
) -> None:
    normalized_jobs = [normalize_job(job) for job in jobs]
    normalized_artifacts = []
    result_roots: list[str] = []
    evidence_roots: list[str] = []

    for artifact, path in zip(artifacts, artifact_roots):
        rel_path = path.relative_to(ROOT).as_posix()
        normalized_artifacts.append(
            {
                "name": artifact.get("name"),
                "kind": artifact.get("kind"),
                "expired": artifact.get("expired", False),
                "root": rel_path,
            }
        )
        if artifact.get("kind") == "test-results":
            result_roots.append(rel_path)
        if artifact.get("kind") == "evidence":
            evidence_roots.append(rel_path)

    metadata = {
        "mode": mode,
        "run_id": run.get("id"),
        "run_url": run.get("html_url"),
        "run_name": run.get("name"),
        "run_status": run.get("status"),
        "run_conclusion": run.get("conclusion"),
        "head_branch": run.get("head_branch"),
        "head_sha": run.get("head_sha"),
        "artifact_names": [artifact.get("name") for artifact in artifacts],
        "artifact_types": sorted(
            {
                artifact.get("kind")
                for artifact in artifacts
                if artifact.get("kind") is not None
            }
        ),
        "artifacts": normalized_artifacts,
        "jobs": normalized_jobs,
        "artifact_roots": [path.relative_to(ROOT).as_posix() for path in artifact_roots],
        "result_roots": result_roots,
        "evidence_roots": evidence_roots,
        "downloaded_at": datetime.now(timezone.utc).isoformat(),
    }
    (output_dir / "_source.json").write_text(
        json.dumps(metadata, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )


def extract_artifact_zip(zip_bytes: bytes, destination: Path) -> None:
    destination.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(io.BytesIO(zip_bytes)) as archive:
        archive.extractall(destination)


def main() -> int:
    args = parse_args()
    repo = os.environ.get("GITHUB_REPOSITORY")
    token = os.environ.get("GITHUB_TOKEN")
    requested_run_id = args.run_id or os.environ.get("DOCS_GARDEN_SOURCE_RUN_ID", "")

    if not repo or not token:
        raise SystemExit("缺少 GITHUB_REPOSITORY 或 GITHUB_TOKEN")

    output_dir = Path(args.output_dir).resolve()
    output_dir.mkdir(parents=True, exist_ok=True)

    if requested_run_id:
        run = get_run(repo, requested_run_id, token)
        mode = "workflow_run"
        artifacts = filter_relevant_artifacts(list_artifacts(repo, str(run["id"]), token))
    else:
        run, artifacts = find_latest_successful_run(repo, args.workflow_file, token)
        mode = "latest_successful_ci"

    if not run:
        write_metadata(output_dir, "missing_ci_run", {}, [], [], [])
        print("[fetch_ci_test_results] 找不到成功的 CI run")
        return 0

    jobs = list_jobs(repo, str(run["id"]), token)
    artifact_roots: list[Path] = []
    for artifact in artifacts:
        destination = output_dir / f"run_{run['id']}" / artifact["name"]
        try:
            zip_bytes = download_bytes(artifact["archive_download_url"], token)
        except HTTPError as error:
            raise SystemExit(
                f"下載 artifact 失敗：name={artifact['name']} status={error.code}"
            ) from error
        extract_artifact_zip(zip_bytes, destination)
        artifact_roots.append(destination)

    write_metadata(output_dir, mode, run, jobs, artifacts, artifact_roots)
    print(
        "[fetch_ci_test_results] 已同步 run {run_id} 的 {count} 個 artifact".format(
            run_id=run["id"],
            count=len(artifacts),
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
