#!/usr/bin/env python3

from __future__ import annotations

from datetime import datetime
from pathlib import Path

from doc_guard_utils import (
    DOC_ROOT,
    ROOT,
    all_documentation_markdown_files,
    format_workflow_permissions,
    freshness_status,
    indexed_markdown_targets,
    load_rules,
    parse_doc_owner,
    parse_last_updated_date,
    parse_workflow_permissions,
    relative,
)

OUTPUT_PATH = DOC_ROOT / "generated" / "doc_garden_report.md"

TRACKED_GROUPS = [
    ("DocsIndex", DOC_ROOT / "DocsIndex.md"),
    ("design-docs", DOC_ROOT / "design-docs" / "index.md"),
    ("exec-plans", DOC_ROOT / "exec-plans" / "index.md"),
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


def workflow_permissions_status(
    rules: dict,
) -> list[tuple[Path, str, str, str]]:
    results: list[tuple[Path, str, str, str]] = []
    workflow_permissions = rules.get("workflow", {}).get("permissions", {})
    for rel_path, expected_permissions in workflow_permissions.items():
        workflow_path = ROOT / rel_path
        expected_text = format_workflow_permissions(expected_permissions)
        if not workflow_path.is_file():
            results.append((workflow_path, "缺少文件", "缺少文件", expected_text))
            continue

        actual_permissions, actual_scalar = parse_workflow_permissions(workflow_path)
        if actual_scalar is not None:
            actual_text = actual_scalar
            status = "不符"
        elif actual_permissions is None:
            actual_text = "缺少 permissions 區塊"
            status = "不符"
        else:
            actual_text = format_workflow_permissions(actual_permissions)
            status = "OK" if actual_permissions == expected_permissions else "不符"

        results.append((workflow_path, status, actual_text, expected_text))
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
    freshness_rules = rules.get("docs", {}).get("freshness_checks", {})
    workflow_permissions = workflow_permissions_status(rules)
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
        f"- freshness checks 覆蓋文件數：{len(freshness_rules)}",
        f"- workflow permissions 覆蓋 workflow 數：{len(workflow_permissions)}",
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
            "## Workflow / Security 檢查",
            "",
            "| workflow | 狀態 | 實際 permissions | 預期 permissions |",
            "|----------|------|------------------|------------------|",
        ]
    )

    if workflow_permissions:
        for path, status, actual_text, expected_text in workflow_permissions:
            lines.append(
                f"| `{rel(path)}` | {status} | `{actual_text}` | `{expected_text}` |"
            )
    else:
        lines.append("| 無 | - | - | - |")

    lines.extend(
        [
            "",
            "## 備註",
            "",
            "- 這份報告現在以分類 index 指到的正式文件為準，不再只數資料夾內有幾個 markdown。",
            "- 若文件日期早於 watched paths 的最新 git 變更日期，表示它可能已過時，應由 docs-garden 回寫或開 PR。",
            "- Workflow 權限檢查會與 `repo_guard` 同源，避免 docs 與 guard 對安全邊界的判斷不一致。",
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
