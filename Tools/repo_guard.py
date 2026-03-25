#!/usr/bin/env python3

import json
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
RULES_PATH = ROOT / "Tools" / "repo_guard_rules.json"


def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def load_json(path: Path):
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def relative(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


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
    check_required_contains(rules.get("docs", {}), errors)
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
