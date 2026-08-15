# GameSaveCenter agent rules

## AI/Codex 长期记忆

开始任何开发前，先依次阅读：

1. `docs/ai/PROJECT_MEMORY.md`
2. `docs/ai/WORKLOG.md`
3. `docs/DEVELOPMENT_HANDOFF.md`
4. 最近的 Git log 与 `git status`

然后按 `docs/ai/PROJECT_MEMORY.md` 的启动协议继续；每完成一个独立阶段，编译、跑测试、更新 `docs/ai/WORKLOG.md` 与项目记忆，并单独 commit。

所有交付轮次由 Agent 自己完成 commit 并 push 到当前远端分支（默认 `main`），不要等用户每次提醒；commit 前保持工作树可复现、文档同步。

## WPF / XAML UI changes

For any WPF, XAML, Playnite UI, theming, layout, controls, animation, dialog, toast,
DataGrid, ScrollBar, TabControl, responsive sizing, DPI, accessibility, or visual-regression work:

1. Use the `wpf-apple-desktop-ui` skill and inspect the shared resources before editing. The skill is committed in the repository at `.codex/skills/wpf-apple-desktop-ui/SKILL.md`; read that file (and any task-relevant `references/`) first. On this machine it is also installed at `%USERPROFILE%\.codex\skills\wpf-apple-desktop-ui`.
2. Read `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md` and `docs/design/UI_CHANGE_GATE.md`.
3. Repair shared styles/templates rather than a single visual instance.
4. Preserve commands, bindings, business behavior, virtualization, keyboard access, UI Automation, and Playnite compatibility.
5. Validate all affected controls in every theme and never claim Playnite rendering was verified unless it was actually run.
