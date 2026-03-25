from __future__ import annotations

import json
import re
import subprocess
from datetime import date, datetime
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
DOC_ROOT = ROOT / "Documentation"
RULES_PATH = ROOT / "Tools" / "repo_guard_rules.json"

BACKTICK_MARKDOWN_PATH_RE = re.compile(r"`([^`]+\.md)`")
MARKDOWN_LINK_RE = re.compile(r"\[[^\]]+\]\(([^)]+\.md)\)")
LAST_UPDATED_RE = re.compile(r"最後更新：\s*(\d{4}/\d{2}/\d{2})")
DOC_OWNER_RE = re.compile(r"文件負責人：\s*([a-z-]+)")


def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def load_json(path: Path):
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def load_rules() -> dict:
    return load_json(RULES_PATH)


def relative(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def parse_last_updated_date(path: Path) -> date | None:
    match = LAST_UPDATED_RE.search(read_text(path))
    if not match:
        return None
    return datetime.strptime(match.group(1), "%Y/%m/%d").date()


def parse_doc_owner(path: Path) -> str | None:
    match = DOC_OWNER_RE.search(read_text(path))
    if not match:
        return None
    return match.group(1)


def resolve_repo_path(raw_path: str, source_path: Path) -> Path | None:
    raw_path = raw_path.strip()
    if not raw_path or raw_path.startswith(("http://", "https://")):
        return None

    candidate = Path(raw_path)
    if candidate.is_absolute():
        resolved = candidate
    elif raw_path.startswith((".", "..")):
        resolved = (source_path.parent / candidate).resolve()
    else:
        resolved = (ROOT / candidate).resolve()

    try:
        resolved.relative_to(ROOT.resolve())
    except ValueError:
        return None

    return resolved


def extract_markdown_targets(text: str, source_path: Path) -> list[Path]:
    candidates: list[Path] = []
    for pattern in (BACKTICK_MARKDOWN_PATH_RE, MARKDOWN_LINK_RE):
        for match in pattern.finditer(text):
            resolved = resolve_repo_path(match.group(1), source_path)
            if resolved is None or not resolved.is_file():
                continue
            if resolved.suffix != ".md":
                continue
            if resolved not in candidates:
                candidates.append(resolved)
    return candidates


def indexed_markdown_targets(index_path: Path) -> list[Path]:
    targets = extract_markdown_targets(read_text(index_path), index_path)
    return [path for path in targets if path != index_path]


def count_indexed_markdown_targets(index_path: Path) -> int:
    return len(indexed_markdown_targets(index_path))


def all_documentation_markdown_files() -> list[Path]:
    return sorted(DOC_ROOT.rglob("*.md"))


def git_last_change_date(paths: list[Path]) -> date | None:
    rel_paths = [relative(path) for path in paths if path.exists()]
    if not rel_paths:
        return None

    result = subprocess.run(
        ["git", "log", "-1", "--format=%cs", "--", *rel_paths],
        cwd=ROOT,
        check=False,
        capture_output=True,
        text=True,
    )
    output = result.stdout.strip()
    if result.returncode != 0 or not output:
        return None

    return datetime.strptime(output, "%Y-%m-%d").date()


def freshness_status(doc_path: Path, watched_paths: list[Path]) -> tuple[date | None, date | None]:
    return parse_last_updated_date(doc_path), git_last_change_date(watched_paths)
