---
name: wpf-apple-desktop-ui
description: Design, audit, implement, and refactor polished WPF/XAML desktop interfaces with Apple-inspired hierarchy, restraint, materials, readability, adaptive layouts, complete control states, accessibility, animation, performance, and Windows-native behavior. Use for WPF, Playnite plugins, XAML themes, ControlTemplates, responsive layouts, DataGrid, ScrollBar, Button, ComboBox, SearchBox, tabs, dialogs, toast notifications, DPI, keyboard focus, and visual regression work.
license: MIT
compatibility: Codex or another Agent Skills-compatible coding agent; Windows WPF projects on .NET Framework or modern .NET. Playnite-specific guidance is included but optional.
metadata:
  author: OpenAI-generated for GameSaveCenter
  version: "1.0.0"
  language: zh-CN
---

# WPF Apple-inspired Desktop UI

Use this skill whenever the task concerns WPF/XAML interface design, visual polish, layout, theming, controls, readability, accessibility, animation, DPI, window resizing, or UI regression.

This is not a macOS skin. It translates the useful qualities of Apple interface design—clear hierarchy, calm surfaces, strong spacing, restrained materials, direct feedback, and adaptive layouts—into a Windows-native WPF implementation that preserves keyboard behavior, UI Automation, virtualization, Playnite hosting constraints, and standard desktop expectations.

## Mandatory operating rules

1. Inspect the real repository before proposing or implementing a design.
2. Read repository UI rules first. If present, always read:
   - `AGENTS.md`
   - `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`
   - `docs/design/UI_CHANGE_GATE.md`
   - existing theme/resource documentation
3. Treat screenshots as evidence of a shared-system problem, not permission to patch only one instance.
4. Fix shared styles/templates globally when the defect affects a control family.
5. Preserve business behavior, commands, bindings, automation names, keyboard use, virtualization, and Playnite compatibility.
6. Do not claim Windows rendering, DPI, or Playnite verification unless actually performed.
7. Prefer a coherent small diff over a speculative rewrite, but do not retain a structurally broken layout only to minimize changed lines.
8. Never replace WPF with HTML, WebView, Electron, Avalonia, WinUI, or another stack unless explicitly requested.
9. Do not use emoji as production icons. Use vector `Path`, `DrawingImage`, or theme-aware assets.
10. Recreate design principles, not copyrighted Apple artwork, fonts, icons, or proprietary visuals.

## Phase 1 — Repository reconnaissance

Before editing, locate and summarize:

- target framework and WPF version
- host environment: standalone `Window`, plugin `UserControl`, Playnite host, custom chrome
- root views and view models
- merged `ResourceDictionary` order
- theme switching and `DynamicResource` usage
- shared styles for every affected control
- layout containers and fixed sizing
- list/table virtualization
- dialogs, notifications, animation helpers, icons
- build, validation, packaging, and installation commands
- current Git state and branch

Search the entire repository for the affected control type and resource key. A defect in one screenshot often exists in every `DataGrid`, `ScrollBar`, `TabItem`, `Button`, `TextBox`, or `ComboBox`.

## Phase 2 — Diagnose before styling

Classify issues as:

- information architecture
- layout/measure/arrange
- control template leakage or missing states
- typography/readability
- color/material/contrast
- interaction affordance
- feedback/loading/empty/error states
- accessibility and keyboard
- performance and virtualization
- host integration and safe areas

State the root cause in WPF terms. Examples:

- `TabItem.HorizontalContentAlignment="Center"` propagated into the content presenter and centered the full page.
- A vertical `StackPanel` measured a `DataGrid` with infinite height, so it did not take the remaining `Grid` row.
- The host `ScrollBar` template remained active inside nested `ScrollViewer`s because the custom style was keyed but not applied.
- A DataGrid header background was outside the rounded clipping boundary and overpainted the shell.
- A progress percentage column had no reserved width, so `0%` was compressed away.

## Phase 3 — Read only the references needed

- Design direction: `references/DESIGN_PRINCIPLES.md`
- Layout/responsiveness: `references/LAYOUT_AND_RESPONSIVENESS.md`
- Typography/readability: `references/TYPOGRAPHY_AND_READABILITY.md`
- Color/material/theme: `references/COLOR_MATERIALS_AND_THEMES.md`
- Controls: `references/CONTROL_CATALOG.md`
- Tables and dense data: `references/DATA_DENSE_UI.md`
- Motion/feedback: `references/MOTION_AND_FEEDBACK.md`
- Accessibility: `references/ACCESSIBILITY.md`
- WPF engineering: `references/WPF_ENGINEERING.md`
- Playnite/GameSaveCenter: `references/PLAYNITE_AND_GAMESAVECENTER.md`
- Final checks: `references/UI_REVIEW_CHECKLIST.md`
- Research basis: `references/SOURCES.md`

## Phase 4 — Implementation order

1. design tokens and theme resources
2. layout tokens and adaptive modes
3. typography styles
4. shared primitive controls
5. complex controls and data templates
6. page layouts
7. feedback surfaces and motion
8. accessibility metadata and keyboard paths
9. validation and visual regression checks

Do not start with color tweaks while layout and hierarchy are wrong.

## Phase 5 — Verification

Run all available checks:

- parse/build XAML
- repository validation scripts
- solution build and tests
- packaging and plugin installation when available
- `python scripts/validate_wpf_ui.py <repo>`
- `git diff --check`
- inspect Binding errors and XAML load errors

Test at minimum:

- 1600×900, 1366×768, 1280×720, 1100×700, 980×640 or project minimum
- 100%, 125%, 150%, 200% DPI
- Light, Dark, Follow Host/System
- transparency disabled and high contrast
- keyboard-only navigation
- empty, normal, error, and very large data sets

When rendering tools are unavailable, report static checks as static checks and provide a precise Windows regression checklist.

## Output contract

For an implementation task, provide:

1. concise diagnosis
2. implementation plan
3. modified files
4. code changes
5. validation results
6. unresolved limitations

For a review-only task, prioritize:

- P0: clipping, overlap, unusable or inaccessible behavior
- P1: hierarchy, readability, inconsistent interaction
- P2: polish, material, motion

## Non-negotiable anti-patterns

Do not:

- wrap the whole app in a `Viewbox`
- use `Canvas` for primary layout
- solve responsiveness with a huge `MinWidth`
- animate `Width`, `Height`, `Margin`, or `GridLength` for routine transitions
- apply `BlurEffect` to rows, lists, or large moving surfaces
- place `DataGrid`, `ListBox`, `TabControl`, or large scroll content in a vertical `StackPanel`
- use negative margins to repair template geometry
- remove keyboard focus visuals without a replacement
- rely on color alone for status
- hardcode theme colors throughout views
- duplicate a shared control template in individual pages
- use `CornerRadius="999"` blindly on thin WPF elements
- build a ScrollBar thumb from overlapping translucent end caps
- assume an outer rounded Border clips inner content automatically
- expose internal C# type names or raw machine strings as primary labels
- truncate important content without Tooltip, details, copy, or horizontal access
- disable virtualization for styling convenience
- make every surface translucent

## Invocation examples

- `Use $wpf-apple-desktop-ui to audit and refactor this WPF page. Fix shared styles globally.`
- `Use the skill to repair all DataGrid, ScrollBar, Button, ComboBox, and TabControl states, then validate DPI and keyboard behavior.`
- `Apply the skill to GameSaveCenter. Read the repository design docs first and preserve Playnite compatibility.`
