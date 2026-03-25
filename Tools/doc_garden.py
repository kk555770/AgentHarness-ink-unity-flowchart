#!/usr/bin/env python3

from __future__ import annotations

from datetime import datetime
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
DOC_ROOT = ROOT / "Documentation"
OUTPUT_PATH = DOC_ROOT / "generated" / "doc_garden_report.md"

TRACKED_GROUPS = [
    ("design-docs", DOC_ROOT / "design-docs"),
    ("exec-plans/active", DOC_ROOT / "exec-plans" / "active"),
    ("exec-plans/completed", DOC_ROOT / "exec-plans" / "completed"),
    ("product-specs", DOC_ROOT / "product-specs"),
    ("references", DOC_ROOT / "references"),
]

GENERATED_OUTPUTS = [
    DOC_ROOT / "generated" / "doc_garden_report.md",
    DOC_ROOT / "generated" / "test_results_index.md",
]


def rel(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def today() -> str:
    return datetime.now().strftime("%Y/%m/%d")


def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def all_markdown_files() -> list[Path]:
    return sorted(DOC_ROOT.rglob("*.md"))


def missing_last_updated(files: list[Path]) -> list[Path]:
    return [path for path in files if "最後更新：" not in read_text(path)]


def count_non_index_markdown(directory: Path) -> int:
    return sum(
        1
        for path in directory.glob("*.md")
        if path.is_file() and path.name != "index.md"
    )


def build_lines() -> list[str]:
    files = all_markdown_files()
    missing_headers = missing_last_updated(files)
    lines = [
        "# doc garden report",
        "",
        f"> 最後更新：{today()}",
        "> 來源：`python3 Tools/doc_garden.py`",
        "",
        "## 摘要",
        "",
        f"- Documentation Markdown 總數：{len(files)}",
        f"- 缺少 `最後更新` 標頭：{len(missing_headers)}",
        "",
        "## 分類內容盤點",
        "",
        "| 類別 | 非 index 正式文件數 |",
        "|------|--------------------|",
    ]

    for label, directory in TRACKED_GROUPS:
        lines.append(f"| `{label}` | {count_non_index_markdown(directory)} |")
    lines.append(
        f"| `generated` | {sum(1 for path in GENERATED_OUTPUTS if path.is_file())} |"
    )

    lines.extend(["", "## 缺少 `最後更新` 的文件", ""])

    if missing_headers:
        for path in missing_headers:
            lines.append(f"- `{rel(path)}`")
    else:
        lines.append("- 無")

    lines.extend(
        [
            "",
            "## 備註",
            "",
            "- 這份報告是 doc-gardening 的最小版掃描，不直接修改其他文件。",
            "- 若某個分類長期只有 `index.md`，表示它仍偏向入口殼，尚未長出正式內容。",
        ]
    )
    return lines


def main() -> int:
    OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT_PATH.write_text("\n".join(build_lines()) + "\n", encoding="utf-8")
    print(f"[doc_garden] 已更新 {rel(OUTPUT_PATH)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
