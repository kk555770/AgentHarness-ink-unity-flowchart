#!/usr/bin/env python3

from __future__ import annotations

from datetime import datetime
from pathlib import Path

from doc_guard_utils import (
    DOC_ROOT,
    ROOT,
    all_documentation_markdown_files,
    freshness_status,
    indexed_markdown_targets,
    load_rules,
    parse_doc_owner,
    parse_last_updated_date,
    relative,
)

OUTPUT_PATH = DOC_ROOT / "generated" / "doc_garden_report.md"

TRACKED_GROUPS = [
    ("design-docs", DOC_ROOT / "design-docs" / "index.md"),
    ("exec-plans/active", DOC_ROOT / "exec-plans" / "active" / "index.md"),
    ("exec-plans/completed", DOC_ROOT / "exec-plans" / "completed" / "index.md"),
    ("product-specs", DOC_ROOT / "product-specs" / "index.md"),
    ("references", DOC_ROOT / "references" / "index.md"),
    ("generated", DOC_ROOT / "generated" / "index.md"),
]

def rel(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def today() -> str:
    return datetime.now().strftime("%Y/%m/%d")


def all_markdown_files() -> list[Path]:
    return sorted(DOC_ROOT.rglob("*.md"))


def missing_last_updated(files: list[Path]) -> list[Path]:
    return [path for path in files if parse_last_updated_date(path) is None]


def stale_docs(rules: dict) -> list[tuple[Path, str, str]]:
    results: list[tuple[Path, str, str]] = []
    freshness_checks = rules.get("docs", {}).get("freshness_checks", {})
    for rel_path, watched_rel_paths in freshness_checks.items():
        doc_path = ROOT / rel_path
        watched_paths = [ROOT / watched_rel_path for watched_rel_path in watched_rel_paths]
        last_updated, latest_change = freshness_status(doc_path, watched_paths)
        if last_updated is None or latest_change is None:
            continue
        if last_updated < latest_change:
            results.append(
                (
                    doc_path,
                    last_updated.strftime("%Y/%m/%d"),
                    latest_change.strftime("%Y/%m/%d"),
                )
            )
    return results


def owner_stats(files: list[Path]) -> tuple[list[Path], dict[str, int]]:
    missing: list[Path] = []
    counts: dict[str, int] = {}
    for path in files:
        owner = parse_doc_owner(path)
        if owner is None:
            missing.append(path)
            continue
        counts[owner] = counts.get(owner, 0) + 1
    return missing, counts


def build_lines() -> list[str]:
    files = all_documentation_markdown_files()
    missing_updated_headers = missing_last_updated(files)
    missing_owner_headers, owner_counts = owner_stats(files)
    rules = load_rules()
    outdated_docs = stale_docs(rules)
    lines = [
        "# doc garden report",
        "",
        "> 文件負責人：harness",
        f"> 最後更新：{today()}",
        "> 來源：`python3 Tools/doc_garden.py`",
        "",
        "## 摘要",
        "",
        f"- Documentation Markdown 總數：{len(files)}",
        f"- 缺少 `最後更新` 標頭：{len(missing_updated_headers)}",
        f"- 缺少 `文件負責人` 標頭：{len(missing_owner_headers)}",
        "",
        "## 分類內容盤點",
        "",
        "| 類別 | index 指到的正式文件數 |",
        "|------|------------------------|",
    ]

    for label, index_path in TRACKED_GROUPS:
        actual = len(indexed_markdown_targets(index_path))
        lines.append(f"| `{label}` | {actual} |")

    lines.extend(["", "## 缺少 `最後更新` 的文件", ""])

    if missing_updated_headers:
        for path in missing_updated_headers:
            lines.append(f"- `{rel(path)}`")
    else:
        lines.append("- 無")

    lines.extend(["", "## 文件負責人覆蓋", ""])

    if owner_counts:
        for owner, count in sorted(owner_counts.items()):
            lines.append(f"- `{owner}`：{count}")
    else:
        lines.append("- 無")

    lines.extend(["", "## 缺少 `文件負責人` 的文件", ""])

    if missing_owner_headers:
        for path in missing_owner_headers:
            lines.append(f"- `{rel(path)}`")
    else:
        lines.append("- 無")

    lines.extend(
        [
            "",
            "## 可能已過時的文件",
            "",
        ]
    )

    if outdated_docs:
        for path, last_updated, latest_change in outdated_docs:
            lines.append(
                f"- `{relative(path)}`：文件日期 `{last_updated}` 早於 watched paths 的最新日期 `{latest_change}`"
            )
    else:
        lines.append("- 無")

    lines.extend(
        [
            "",
            "## 備註",
            "",
            "- 這份報告現在以分類 index 指到的正式文件為準，不再只數資料夾內有幾個 markdown。",
            "- 若文件日期早於 watched paths 的最新 git 變更日期，表示它可能已過時，應由 docs-garden 回寫或開 PR。",
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
