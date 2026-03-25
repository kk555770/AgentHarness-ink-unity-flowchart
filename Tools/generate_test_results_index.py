#!/usr/bin/env python3

from __future__ import annotations

import argparse
import json
from datetime import datetime
from pathlib import Path
import xml.etree.ElementTree as ET


ROOT = Path(__file__).resolve().parent.parent
DEFAULT_RESULTS_ROOT = ROOT / "Logs" / "TestResults"
OUTPUT_PATH = ROOT / "Documentation" / "generated" / "test_results_index.md"


def today() -> str:
    return datetime.now().strftime("%Y/%m/%d")


def rel(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--results-root",
        default=str(DEFAULT_RESULTS_ROOT),
        help="測試結果根目錄，可為本地 Logs 或下載回來的 CI artifact 根目錄。",
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


def result_files_for_root(results_root: Path, metadata: dict | None) -> list[Path]:
    if metadata is not None:
        result_files: list[Path] = []
        for rel_root in metadata.get("result_roots", []):
            result_files.extend(sorted((ROOT / rel_root).rglob("*.xml")))
        return result_files

    return sorted(results_root.rglob("*.xml"))


def source_lines(results_root: Path, metadata: dict | None) -> list[str]:
    if metadata:
        lines = [
            "> 測試來源：`{mode}`，run `{run_id}`，artifact `{artifacts}`".format(
                mode=metadata.get("mode", "unknown"),
                run_id=metadata.get("run_id", "unknown"),
                artifacts=", ".join(metadata.get("artifact_names", [])) or "none",
            )
        ]
        if metadata.get("run_url"):
            lines.append(f"> Run URL：{metadata['run_url']}")
        return lines

    return [f"> 測試來源：本地結果 `{rel(results_root)}`"]


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
    lines.extend(
        [
            "",
            "## 測試摘要",
            "",
            "| Suite | Result | Total | Passed | Failed | Skipped | Duration(s) | Start | End | Source |",
            "|-------|--------|-------|--------|--------|---------|-------------|-------|-----|--------|",
        ]
    )

    for result in sorted(results, key=lambda item: item["suite"]):
        lines.append(
            "| `{suite}` | {result} | {total} | {passed} | {failed} | {skipped} | {duration} | {start} | {end} | `{source}` |".format(
                **result
            )
        )

    if not results:
        lines.append("| `none` | Missing | 0 | 0 | 0 | 0 | 0 | - | - | - |")

    return lines


def main() -> int:
    args = parse_args()
    results_root = Path(args.results_root).resolve()
    OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    metadata = load_source_metadata(results_root)
    result_files = result_files_for_root(results_root, metadata)
    results = [load_result(path) for path in result_files]
    OUTPUT_PATH.write_text(
        "\n".join(build_lines(results, results_root, metadata)) + "\n",
        encoding="utf-8",
    )
    print(f"[test_results_index] 已更新 {rel(OUTPUT_PATH)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
