#!/usr/bin/env python3

import sys
from pathlib import Path

from doc_guard_utils import (
    ROOT,
    all_documentation_markdown_files,
    count_indexed_markdown_targets,
    freshness_status,
    load_json,
    parse_doc_owner,
    parse_workflow_permissions,
    read_text,
    relative,
)

RULES_PATH = ROOT / "Tools" / "repo_guard_rules.json"


def check_required_files(rules: dict, errors: list[str]) -> None:
    for rel_path in rules.get("required_files", []):
        path = ROOT / rel_path
        if not path.is_file():
            errors.append(f"缺少必要文件：{rel_path}")


def check_required_contains(rules: dict, errors: list[str]) -> None:
    required_contains = rules.get("required_contains", {})
    for rel_path, patterns in required_contains.items():
        path = ROOT / rel_path
        if not path.is_file():
            errors.append(f"缺少必要文件：{rel_path}")
            continue

        text = read_text(path)
        for pattern in patterns:
            if pattern not in text:
                errors.append(f"{rel_path} 缺少必要內容：{pattern}")


def check_last_updated_headers(rules: dict, errors: list[str]) -> None:
    required_headers = rules.get("last_updated_headers", [])
    for rel_path in required_headers:
        path = ROOT / rel_path
        if not path.is_file():
            errors.append(f"缺少必要文件：{rel_path}")
            continue

        text = read_text(path)
        if "最後更新：" not in text:
            errors.append(f"{rel_path} 缺少最後更新標頭")


def count_non_index_markdown_files(path: Path) -> int:
    count = 0
    for candidate in path.glob("*.md"):
        if candidate.name != "index.md" and candidate.is_file():
            count += 1
    return count


def check_min_non_index_docs(rules: dict, errors: list[str]) -> None:
    min_docs = rules.get("min_non_index_docs", {})
    for rel_path, minimum in min_docs.items():
        path = ROOT / rel_path
        if not path.is_dir():
            errors.append(f"缺少必要資料夾：{rel_path}")
            continue

        actual = count_non_index_markdown_files(path)
        if actual < int(minimum):
            errors.append(
                f"{rel_path} 正式文件數不足：current={actual} minimum={minimum}"
            )


def check_indexed_doc_counts(rules: dict, errors: list[str]) -> None:
    indexed_doc_counts = rules.get("indexed_doc_counts", {})
    for rel_path, minimum in indexed_doc_counts.items():
        path = ROOT / rel_path
        if not path.is_file():
            errors.append(f"缺少必要索引：{rel_path}")
            continue

        actual = count_indexed_markdown_targets(path)
        if actual < int(minimum):
            errors.append(
                f"{rel_path} 索引到的正式文件數不足：current={actual} minimum={minimum}"
            )


def check_freshness(rules: dict, errors: list[str]) -> None:
    freshness_checks = rules.get("freshness_checks", {})
    for rel_path, watched_rel_paths in freshness_checks.items():
        doc_path = ROOT / rel_path
        if not doc_path.is_file():
            errors.append(f"缺少必要文件：{rel_path}")
            continue

        watched_paths = [
            ROOT / watched_rel_path for watched_rel_path in watched_rel_paths
        ]
        last_updated, latest_change = freshness_status(doc_path, watched_paths)

        if last_updated is None:
            errors.append(f"{rel_path} 缺少可解析的最後更新日期")
            continue

        if latest_change is None:
            errors.append(f"{rel_path} 找不到 watched paths 的 git 變更紀錄")
            continue

        if last_updated < latest_change:
            errors.append(
                f"{rel_path} 已過時：last_updated={last_updated.strftime('%Y/%m/%d')} "
                f"latest_change={latest_change.strftime('%Y/%m/%d')}"
            )


def check_workflow_permissions(rules: dict, errors: list[str]) -> None:
    workflow_permissions = rules.get("permissions", {})
    for rel_path, expected_permissions in workflow_permissions.items():
        path = ROOT / rel_path
        if not path.is_file():
            errors.append(f"缺少必要 workflow：{rel_path}")
            continue

        actual_permissions, actual_scalar = parse_workflow_permissions(path)
        if actual_scalar is not None:
            if actual_scalar in {"read-all", "write-all"}:
                errors.append(f"{rel_path} 使用過寬的 permissions：{actual_scalar}")
            else:
                errors.append(
                    f"{rel_path} permissions 必須是明確映射，不能是單一值：{actual_scalar}"
                )
            continue

        if actual_permissions is None:
            errors.append(f"{rel_path} 缺少 permissions 區塊")
            continue

        if actual_permissions != expected_permissions:
            errors.append(
                f"{rel_path} permissions 不符合預期："
                f" expected={expected_permissions} actual={actual_permissions}"
            )


def check_doc_owners(rules: dict, errors: list[str]) -> None:
    allowed_owners = set(rules.get("allowed_doc_owners", []))
    for path in all_documentation_markdown_files():
        rel_path = relative(path)
        owner = parse_doc_owner(path)
        if owner is None:
            errors.append(f"{rel_path} 缺少文件負責人標頭")
            continue

        if allowed_owners and owner not in allowed_owners:
            errors.append(
                f"{rel_path} 的文件負責人不在允許清單內：owner={owner}"
            )


def check_exact_asmdef_references(rules: dict, errors: list[str]) -> None:
    exact_references = rules.get("exact_references", {})
    for rel_path, expected_references in exact_references.items():
        path = ROOT / rel_path
        if not path.is_file():
            errors.append(f"缺少 asmdef：{rel_path}")
            continue

        asmdef = load_json(path)
        actual_references = asmdef.get("references", [])
        if actual_references != expected_references:
            errors.append(
                f"{rel_path} references 不符合預期："
                f" expected={expected_references} actual={actual_references}"
            )


def line_count(path: Path) -> int:
    return len(read_text(path).splitlines())


def iter_cs_files(roots: list[str]):
    for rel_root in roots:
        root = ROOT / rel_root
        if not root.exists():
            continue

        for path in sorted(root.rglob("*.cs")):
            if path.is_file():
                yield path


def check_file_sizes(rules: dict, errors: list[str]) -> None:
    default_cs_max_lines = int(rules.get("default_cs_max_lines", 0))
    allowlist = rules.get("allowlist", {})
    roots = rules.get("roots", [])

    for path in iter_cs_files(roots):
        rel_path = relative(path)
        limit = int(allowlist.get(rel_path, default_cs_max_lines))
        current_lines = line_count(path)
        if limit > 0 and current_lines > limit:
            errors.append(
                f"{rel_path} 行數超出限制：current={current_lines} limit={limit}"
            )


def main() -> int:
    if not RULES_PATH.is_file():
        print(f"[repo_guard] 找不到規則檔：{relative(RULES_PATH)}")
        return 1

    rules = load_json(RULES_PATH)
    errors: list[str] = []

    check_required_files(rules.get("docs", {}), errors)
    check_required_files(rules.get("workflow", {}), errors)
    check_required_contains(rules.get("docs", {}), errors)
    check_last_updated_headers(rules.get("docs", {}), errors)
    check_min_non_index_docs(rules.get("docs", {}), errors)
    check_indexed_doc_counts(rules.get("docs", {}), errors)
    check_freshness(rules.get("docs", {}), errors)
    check_workflow_permissions(rules.get("workflow", {}), errors)
    check_doc_owners(rules.get("docs", {}), errors)
    check_exact_asmdef_references(rules.get("asmdef", {}), errors)
    check_file_sizes(rules.get("filesize", {}), errors)

    if errors:
        print("[repo_guard] 失敗")
        for index, error in enumerate(errors, start=1):
            print(f"{index}. {error}")
        return 1

    print("[repo_guard] 通過")
    return 0


if __name__ == "__main__":
    sys.exit(main())
