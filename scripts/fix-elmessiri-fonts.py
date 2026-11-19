#!/usr/bin/env python3
"""Normalize page font stacks to inherit ElMessiri from global typography."""
from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(r"C:\GitHub\accountant_qlt_system\panel-admin-main\panel-admin-main\src")

# Replace local font-family declarations that break brand loading.
PATTERNS: list[tuple[re.Pattern[str], str]] = [
    (
        re.compile(
            r"font-family:\s*'El Messiri'(?:\s*,\s*'Segoe UI')?(?:\s*,\s*Tahoma)?(?:\s*,\s*sans-serif)?\s*;"
        ),
        "font-family: inherit;",
    ),
    (
        re.compile(
            r'font-family:\s*"El Messiri"(?:\s*,\s*"Segoe UI")?(?:\s*,\s*Tahoma)?(?:\s*,\s*sans-serif)?\s*;'
        ),
        "font-family: inherit;",
    ),
    (
        re.compile(
            r"font-family:\s*ElMessiri\s*,\s*'Segoe UI'(?:\s*,\s*Tahoma)?(?:\s*,\s*sans-serif)?\s*;"
        ),
        "font-family: inherit;",
    ),
    (
        re.compile(
            r"font-family:\s*'ElMessiri'\s*,\s*'Segoe UI'(?:\s*,\s*Tahoma)?(?:\s*,\s*sans-serif)?\s*;"
        ),
        "font-family: inherit;",
    ),
    (
        re.compile(
            r"font-family:\s*'El Messiri'\s*,\s*'Segoe UI'\s*,\s*Tahoma\s*,\s*sans-serif\s*;"
        ),
        "font-family: inherit;",
    ),
]


def main() -> None:
    changed: list[str] = []
    for path in ROOT.rglob("*"):
        if path.suffix.lower() not in {".vue", ".css"}:
            continue
        text = path.read_text(encoding="utf-8")
        new = text
        for pat, repl in PATTERNS:
            new = pat.sub(repl, new)
        # Also strip bare El Messiri with Tahoma common variants via looser pass
        new2 = re.sub(
            r"font-family:\s*[^;]*El Messiri[^;]*;",
            "font-family: inherit;",
            new,
        )
        new = new2
        if new != text:
            path.write_text(new, encoding="utf-8", newline="\n")
            changed.append(str(path.relative_to(ROOT)))
    print(f"updated {len(changed)} files")
    for c in changed:
        print(c)


if __name__ == "__main__":
    main()
