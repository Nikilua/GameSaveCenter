#!/usr/bin/env python3
"""Heuristic WPF UI audit. It does not replace XAML compilation or Windows rendering."""
from __future__ import annotations
import argparse, re, sys
import xml.etree.ElementTree as ET
from dataclasses import dataclass
from pathlib import Path

@dataclass
class Finding:
    severity: str
    path: Path
    line: int
    message: str

IGNORE = "wpf-ui-lint: ignore"

def files(root: Path):
    for p in root.rglob("*.xaml"):
        low = {x.lower() for x in p.parts}
        if not ({"bin", "obj", ".git"} & low):
            yield p

def lineno(text: str, pos: int) -> int:
    return text.count("\n", 0, pos) + 1

def matches(out, path, text, pattern, severity, message):
    for m in re.finditer(pattern, text, re.I | re.M | re.S):
        a = text.rfind("\n", 0, m.start()) + 1
        b = text.find("\n", m.start())
        b = len(text) if b < 0 else b
        if IGNORE not in text[a:b]:
            out.append(Finding(severity, path, lineno(text, m.start()), message))

def audit(path: Path):
    out = []
    try:
        text = path.read_text(encoding="utf-8-sig")
    except UnicodeDecodeError:
        return [Finding("error", path, 1, "File is not UTF-8.")]
    try:
        ET.fromstring(text)
    except ET.ParseError as e:
        return [Finding("error", path, getattr(e, "position", (1, 0))[0], f"XAML parse error: {e}")]

    rules = [
        (r'Margin\s*=\s*"\s*-\d', "warning", "Negative Margin can hide a layout/template defect."),
        (r'FocusVisualStyle\s*=\s*"\{x:Null\}"', "warning", "Focus visual removed; verify a keyboard focus ring exists."),
        (r'<(?:\w+:)?Canvas\b', "warning", "Canvas found; do not use it for primary application layout."),
        (r'<(?:\w+:)?TabItem\b[^>]*HorizontalContentAlignment\s*=\s*"Center"', "error", "TabItem content alignment is Center; center only its header."),
        (r'<(?:\w+:)?TabItem\b[^>]*VerticalContentAlignment\s*=\s*"Center"', "error", "TabItem content alignment is Center; keep page content Stretch."),
        (r'CornerRadius\s*=\s*"(?:[1-9]\d{2,}|999)"', "warning", "Very large CornerRadius can render asymmetrically on thin WPF elements."),
        (r'<(?:\w+:)?Ellipse\b.{0,600}<(?:\w+:)?Thumb\b|<(?:\w+:)?Thumb\b.{0,600}<(?:\w+:)?Ellipse\b', "warning", "Ellipse near Thumb template; avoid overlapping translucent end caps."),
        (r'<(?:\w+:)?DataGrid\b[^>]*(?:EnableRowVirtualization\s*=\s*"False"|EnableColumnVirtualization\s*=\s*"False")', "warning", "DataGrid virtualization is disabled."),
        (r'<(?:\w+:)?ListBox\b[^>]*VirtualizingPanel\.IsVirtualizing\s*=\s*"False"', "warning", "ListBox virtualization is disabled."),
        (r'<(?:\w+:)?StackPanel\b.{0,2500}<(?:\w+:)?(?:DataGrid|ListView|ListBox|TabControl)\b', "warning", "Large scrollable control may be inside StackPanel; verify finite remaining size."),
        (r'#[0-9A-Fa-f]{6,8}', "info", "Hardcoded color found; views should usually use shared theme resources."),
    ]
    for pattern, severity, message in rules:
        matches(out, path, text, pattern, severity, message)
    return out

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("path", nargs="?", default=".")
    ap.add_argument("--strict", action="store_true")
    args = ap.parse_args()
    root = Path(args.path).resolve()
    if not root.exists():
        print(f"ERROR: path does not exist: {root}", file=sys.stderr); return 2
    xs = list(files(root)); findings = []
    for p in xs: findings.extend(audit(p))
    rank = {"error": 0, "warning": 1, "info": 2}
    findings.sort(key=lambda f: (rank.get(f.severity, 9), str(f.path), f.line))
    for f in findings:
        try: rel = f.path.relative_to(root)
        except ValueError: rel = f.path
        print(f"{f.severity.upper():7} {rel}:{f.line}  {f.message}")
    errors = sum(f.severity == "error" for f in findings)
    warnings = sum(f.severity == "warning" for f in findings)
    infos = sum(f.severity == "info" for f in findings)
    print(f"\nScanned {len(xs)} XAML files: {errors} errors, {warnings} warnings, {infos} info.")
    return 1 if errors or (args.strict and warnings) else 0

if __name__ == "__main__":
    raise SystemExit(main())
