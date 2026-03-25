#!/usr/bin/env python3

from __future__ import annotations

from datetime import datetime
from pathlib import Path
import xml.etree.ElementTree as ET


ROOT = Path(__file__).resolve().parent.parent
RESULTS_ROOT = ROOT / "Logs" / "TestResults"
OUTPUT_PATH = ROOT / "Documentation" / "generated" / "test_results_index.md"


def today() -> str:
    return datetime.now().strftime("%Y/%m/%d")


def rel(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def load_result(path: Path) -> dict[str, str]:
    root = ET.parse(path).getroot()
    return {
        "suite": path.stem.replace("OpsidanosInk_", ""),
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


def build_lines(results: list[dict[str, str]]) -> list[str]:
    lines = [
        "# test results index",
        "",
        f"> 最後更新：{today()}",
        "> 來源：`python3 Tools/generate_test_results_index.py`",
        "",
        "## 測試摘要",
        "",
        "| Suite | Result | Total | Passed | Failed | Skipped | Duration(s) | Start | End | Source |",
        "|-------|--------|-------|--------|--------|---------|-------------|-------|-----|--------|",
    ]

    for result in results:
        lines.append(
            "| `{suite}` | {result} | {total} | {passed} | {failed} | {skipped} | {duration} | {start} | {end} | `{source}` |".format(
                **result
            )
        )

    if not results:
        lines.append("| `none` | Missing | 0 | 0 | 0 | 0 | 0 | - | - | - |")

    return lines


def main() -> int:
    OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    result_files = sorted(RESULTS_ROOT.glob("*.xml"))
    results = [load_result(path) for path in result_files]
    OUTPUT_PATH.write_text("\n".join(build_lines(results)) + "\n", encoding="utf-8")
    print(f"[test_results_index] 已更新 {rel(OUTPUT_PATH)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
