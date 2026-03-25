#!/usr/bin/env python3

from __future__ import annotations

import argparse
import json
import re
from collections import defaultdict
from datetime import datetime
from pathlib import Path
import xml.etree.ElementTree as ET


ROOT = Path(__file__).resolve().parent.parent
DEFAULT_RESULTS_ROOT = ROOT / "Logs" / "TestResults"
OUTPUT_PATH = ROOT / "Documentation" / "generated" / "test_results_index.md"
LOCAL_EVIDENCE_ROOT = ROOT / "Logs" / "UnityEvidence"
LOG_SUFFIXES = {".log", ".txt"}
MAX_CONSOLE_SNIPPETS = 2


def today() -> str:
    return datetime.now().strftime("%Y/%m/%d")


def rel(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def trim(text: str, limit: int = 160) -> str:
    clean = " ".join(text.split())
    if len(clean) <= limit:
        return clean
    return clean[: limit - 1] + "…"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--results-root",
        default=str(DEFAULT_RESULTS_ROOT),
        help="測試結果根目錄，可為本地 Logs 或下載回來的 CI artifact 根目錄。",
    )
    parser.add_argument(
        "--output-path",
        default=str(OUTPUT_PATH),
        help="輸出 markdown 路徑；預設為 Documentation/generated/test_results_index.md。",
    )
    return parser.parse_args()


def load_source_metadata(results_root: Path) -> dict | None:
    source_path = results_root / "_source.json"
    if not source_path.is_file():
        return None

    with source_path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def infer_suite(path: Path) -> str:
    lower_text = "/".join(part.lower() for part in path.parts)
    if "editmode" in lower_text:
        return "EditMode"
    if "playmode" in lower_text:
        return "PlayMode"
    if path.stem.startswith("OpsidanosInk_"):
        return path.stem.replace("OpsidanosInk_", "")
    return path.stem


def preferred_path(paths: list[Path], preferred_tokens: tuple[str, ...]) -> Path:
    def score(path: Path) -> tuple[int, str]:
        text = path.as_posix().lower()
        for index, token in enumerate(preferred_tokens):
            if token.lower() in text:
                return index, path.as_posix()
        return len(preferred_tokens), path.as_posix()

    return sorted(paths, key=score)[0]


def load_result(path: Path) -> dict[str, str]:
    root = ET.parse(path).getroot()
    return {
        "suite": infer_suite(path),
        "result": root.attrib.get("result", "Unknown"),
        "total": root.attrib.get("total", "0"),
        "passed": root.attrib.get("passed", "0"),
        "failed": root.attrib.get("failed", "0"),
        "skipped": root.attrib.get("skipped", "0"),
        "duration": root.attrib.get("duration", "0"),
        "start": root.attrib.get("start-time", "-"),
        "end": root.attrib.get("end-time", "-"),
        "source": rel(path),
    }


def missing_result(suite: str) -> dict[str, str]:
    return {
        "suite": suite,
        "result": "Missing",
        "total": "0",
        "passed": "0",
        "failed": "0",
        "skipped": "0",
        "duration": "0",
        "start": "-",
        "end": "-",
        "source": "-",
    }


def is_console_warning(line: str) -> bool:
    lower = line.lower()
    if re.search(r"\b0\s+warnings?\b", lower):
        return False
    return bool(re.search(r"\bwarning\b", lower)) or "warn:" in lower


def is_stack_trace_line(line: str) -> bool:
    stripped = line.strip()
    if not stripped:
        return False
    if stripped.startswith("(Filename:"):
        return True
    if stripped.startswith(("UnityEngine.", "UnityEditor.", "System.", "NUnit.", "MCPForUnity.")):
        return True
    if "(at " in stripped:
        return True
    return False


def is_console_error(line: str) -> bool:
    lower = line.lower()
    if re.search(r"\b0\s+errors?\b", lower):
        return False
    if is_stack_trace_line(line):
        return False
    if "assertion failed" in lower:
        return True
    if re.search(r"\b(?:[a-z_][a-z0-9_.]*exception)(?::|\s|$)", lower):
        return True
    return bool(re.search(r"\berror\b", lower))


def collect_paths_by_suite(paths: list[Path]) -> dict[str, list[Path]]:
    grouped: dict[str, list[Path]] = defaultdict(list)
    for path in paths:
        grouped[infer_suite(path)].append(path)
    return grouped


def xml_output_lines(path: Path) -> list[tuple[str, str]]:
    root = ET.parse(path).getroot()
    lines: list[tuple[str, str]] = []
    for output in root.findall(".//output"):
        text = (output.text or "").strip()
        if not text:
            continue
        for raw_line in text.splitlines():
            line = raw_line.strip()
            if line:
                lines.append((path.name, line))
    return lines


def collect_console_evidence(xml_paths: list[Path], log_paths: list[Path]) -> dict[str, str]:
    warnings = 0
    errors = 0
    snippets: list[str] = []
    has_console_source = False

    def digest_line(source_name: str, line: str) -> None:
        nonlocal warnings, errors
        if is_console_error(line):
            errors += 1
            if len(snippets) < MAX_CONSOLE_SNIPPETS:
                snippets.append(f"{source_name}: {trim(line)}")
            return

        if is_console_warning(line):
            warnings += 1
            if len(snippets) < MAX_CONSOLE_SNIPPETS:
                snippets.append(f"{source_name}: {trim(line)}")

    for path in xml_paths:
        for source_name, line in xml_output_lines(path):
            has_console_source = True
            digest_line(source_name, line)

    for path in log_paths:
        if path.suffix.lower() not in LOG_SUFFIXES:
            continue
        for raw_line in path.read_text(encoding="utf-8", errors="replace").splitlines():
            line = raw_line.strip()
            if not line:
                continue
            has_console_source = True
            digest_line(path.name, line)

    if not has_console_source:
        return {
            "console_errors": "-",
            "console_warnings": "-",
            "console_summary": "Missing console source",
            "log_source": "-",
        }

    summary_bits = [f"errors={errors}", f"warnings={warnings}"]
    if snippets:
        summary_bits.append("; ".join(snippets))
    elif errors == 0 and warnings == 0:
        summary_bits.append("clean")
    elif not log_paths:
        summary_bits.append("xml-output-only")

    return {
        "console_errors": str(errors),
        "console_warnings": str(warnings),
        "console_summary": "; ".join(summary_bits),
        "log_source": ", ".join(rel(path) for path in log_paths) if log_paths else "-",
    }


def metadata_roots(metadata: dict | None, keys: tuple[str, ...]) -> list[Path]:
    if not metadata:
        return []

    roots: list[Path] = []
    for key in keys:
        for rel_root in metadata.get(key, []):
            path = ROOT / rel_root
            if path.exists() and path not in roots:
                roots.append(path)
    return roots


def files_for_suffixes(roots: list[Path], suffixes: set[str]) -> list[Path]:
    results: list[Path] = []
    seen: set[Path] = set()
    for root in roots:
        if not root.exists():
            continue
        for path in sorted(root.rglob("*")):
            if not path.is_file():
                continue
            if path.suffix.lower() not in suffixes:
                continue
            if path in seen:
                continue
            seen.add(path)
            results.append(path)
    return results


def result_files_for_root(results_root: Path, metadata: dict | None) -> list[Path]:
    roots = metadata_roots(metadata, ("result_roots", "artifact_roots"))
    if not roots:
        roots = [results_root]
    return files_for_suffixes(roots, {".xml"})


def log_files_for_root(results_root: Path, metadata: dict | None) -> list[Path]:
    roots = metadata_roots(metadata, ("artifact_roots", "evidence_roots", "result_roots"))
    if not roots:
        roots = [results_root]
        if (
            results_root.resolve() == DEFAULT_RESULTS_ROOT.resolve()
            and LOCAL_EVIDENCE_ROOT.exists()
            and LOCAL_EVIDENCE_ROOT not in roots
        ):
            roots.append(LOCAL_EVIDENCE_ROOT)
    return files_for_suffixes(roots, LOG_SUFFIXES)


def build_suite_rows(results_root: Path, metadata: dict | None) -> list[dict[str, str]]:
    xml_files = result_files_for_root(results_root, metadata)
    log_files = log_files_for_root(results_root, metadata)
    xml_by_suite = collect_paths_by_suite(xml_files)
    log_by_suite = collect_paths_by_suite(log_files)
    suite_names = sorted(set(xml_by_suite) | set(log_by_suite))

    rows: list[dict[str, str]] = []
    for suite in suite_names:
        xml_candidates = xml_by_suite.get(suite, [])
        log_candidates = log_by_suite.get(suite, [])

        if xml_candidates:
            xml_path = preferred_path(
                xml_candidates,
                ("unity-test-results", "result", "evidence"),
            )
            row = load_result(xml_path)
        else:
            row = missing_result(suite)

        if log_candidates:
            log_sources = [
                preferred_path(
                    log_candidates,
                    ("unity-evidence", "evidence", "unity-test-results", "result"),
                )
            ]
        else:
            log_sources = []

        console = collect_console_evidence(xml_candidates, log_sources)

        row.update(console)
        rows.append(row)

    return rows


def source_lines(results_root: Path, metadata: dict | None) -> list[str]:
    if metadata:
        artifact_types = ", ".join(metadata.get("artifact_types", [])) or "none"
        lines = [
            "> 測試來源：`{mode}`，run `{run_id}`，artifact 類型 `{artifacts}`".format(
                mode=metadata.get("mode", "unknown"),
                run_id=metadata.get("run_id", "unknown"),
                artifacts=artifact_types,
            )
        ]
        if metadata.get("run_status") or metadata.get("run_conclusion"):
            lines.append(
                "> Run 狀態：`{status}` / `{conclusion}`".format(
                    status=metadata.get("run_status", "-"),
                    conclusion=metadata.get("run_conclusion", "-"),
                )
            )
        if metadata.get("head_branch") or metadata.get("head_sha"):
            lines.append(
                "> Head：branch `{branch}` / sha `{sha}`".format(
                    branch=metadata.get("head_branch", "-"),
                    sha=metadata.get("head_sha", "-"),
                )
            )
        if metadata.get("run_url"):
            lines.append(f"> Run URL：{metadata['run_url']}")
        return lines

    return [f"> 測試來源：本地結果 `{rel(results_root)}`"]


def job_lines(metadata: dict | None) -> list[str]:
    if not metadata:
        return []

    jobs = metadata.get("jobs", [])
    if not jobs:
        return []

    lines = [
        "",
        "## CI 作業狀態",
        "",
        "| Job | Status | Conclusion | Steps | Started | Completed |",
        "|-----|--------|------------|-------|---------|-----------|",
    ]
    for job in jobs:
        step_count = len(job.get("steps", []))
        lines.append(
            "| `{name}` | `{status}` | `{conclusion}` | {steps} | `{started}` | `{completed}` |".format(
                name=job.get("name", "-"),
                status=job.get("status", "-"),
                conclusion=job.get("conclusion", "-"),
                steps=step_count,
                started=job.get("started_at", "-"),
                completed=job.get("completed_at", "-"),
            )
        )
    return lines


def build_lines(
    results: list[dict[str, str]], results_root: Path, metadata: dict | None
) -> list[str]:
    lines = [
        "# test results index",
        "",
        "> 文件負責人：harness",
        f"> 最後更新：{today()}",
        f"> 來源：`python3 Tools/generate_test_results_index.py --results-root {rel(results_root)}`",
    ]
    lines.extend(source_lines(results_root, metadata))
    lines.extend(job_lines(metadata))
    lines.extend(
        [
            "",
            "## 測試摘要",
            "",
            "| Suite | Result | Total | Passed | Failed | Skipped | Duration(s) | Start | End | Console errors | Console warnings | Console summary | XML source | Log source |",
            "|-------|--------|-------|--------|--------|---------|-------------|-------|-----|----------------|-----------------|-----------------|------------|-----------|",
        ]
    )

    for result in sorted(results, key=lambda item: item["suite"]):
        lines.append(
            "| `{suite}` | {result} | {total} | {passed} | {failed} | {skipped} | {duration} | {start} | {end} | {console_errors} | {console_warnings} | {console_summary} | `{source}` | `{log_source}` |".format(
                **result
            )
        )

    if not results:
        lines.append(
            "| `none` | Missing | 0 | 0 | 0 | 0 | 0 | - | - | - | - | Missing console source | - | - |"
        )

    return lines


def main() -> int:
    args = parse_args()
    results_root = Path(args.results_root).resolve()
    output_path = Path(args.output_path).resolve()
    output_path.parent.mkdir(parents=True, exist_ok=True)
    metadata = load_source_metadata(results_root)
    results = build_suite_rows(results_root, metadata)
    output_path.write_text(
        "\n".join(build_lines(results, results_root, metadata)) + "\n",
        encoding="utf-8",
    )
    print(f"[test_results_index] 已更新 {rel(output_path)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
