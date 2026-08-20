# GameSaveCenter agent rules

## AI/Codex 长期记忆

开始任何开发前，先依次阅读：

1. `docs/ai/PROJECT_MEMORY.md`
2. `docs/ai/WORKLOG.md`
3. `docs/DEVELOPMENT_HANDOFF.md`
4. 最近的 Git log 与 `git status`

然后按 `docs/ai/PROJECT_MEMORY.md` 的启动协议继续；每完成一个独立阶段，编译、跑测试、更新 `docs/ai/WORKLOG.md` 与项目记忆，并单独 commit。

所有交付轮次由 Agent 自己完成 commit 并 push 到当前远端分支（默认 `main`），不要等用户每次提醒；commit 前保持工作树可复现、文档同步。

## 文件清理规则

1. 每完成一个独立阶段，及时清理本阶段不再需要的中间产物：`artifacts/`、`.tmp/` 下的旧构建目录、旧审计目录、旧 zip、离屏截图和一次性脚手架，避免无用文件堆积占用硬盘。
2. 只保留当前仍在使用的产物：最新安装包/打包目录、当前真机审计输出、当前文档引用的证据。旧的 phase/audit/dev-build/ui-qa/ui-audit-build 等目录完成后即可删除，Git 已跟踪的源码与文档不受影响。
3. 删除前先确认路径位于仓库的 `artifacts/` 或 `.tmp/` 内，使用 PowerShell `Remove-Item -LiteralPath -Recurse -Force`，不要跨 shell 拼接删除命令。
4. 不要在 Git 提交中包含 `artifacts/`、`.tmp/` 生成物；这些目录属于本地可再生的临时输出。

## WPF / XAML UI changes

### 当前 UI 重构授权（2026-08-17）

用户已明确要求 GameSaveCenter 进入页面级、整页 UI 重构阶段。页面信息架构、布局层级、导航方式（包括 Tab/Segmented）、控件类型、ControlTemplate、共享样式和滚动实现都可以按新设计重新选择；旧交接记录中“不要恢复”“不要替换”“明确不迁移”的表述，均只代表当时的 UiLab/AcrylicFork 迁移阶段，不再是当前的页面冻结规则。

当前仍然有效的是功能与质量底线，而不是旧页面结构：默认保留真实命令、Binding、数据契约、错误/取消/安全语义、可访问性、可扩展列表性能和 Playnite 兼容性。若新设计确实需要改变其中任何一项，应在任务范围中明确迁移方案并补齐对应测试；可以更换控件或滚动模型，但不能因为“保留旧实现”而阻止用户要求的整页改造。

### 最新视觉优先级：Demo-first（2026-08-20）

后续所有页面迁移以 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` 下的 `DesignShellView.xaml`、`Pages/*.xaml`、`DesignTokens.xaml`、`DesignColorsLight.xaml`、`DesignColorsDark.xaml` 和 `DesignControls.xaml` 为唯一主要视觉基准。Demo 与历史生产页面、UiLab 或通用 Apple-inspired 建议发生冲突时，以 Demo 的页面结构、信息架构、层级、间距、字体、颜色、控件和状态为准。

`wpf-apple-desktop-ui` 不再构成页面视觉或实现路线的优先制约，只作为质量检查依据：绑定、命令、真实业务状态、虚拟化、键盘/UI Automation、可访问性、主题/DPI 安全和 Playnite 兼容性仍须满足，但该技能的 Apple-inspired 偏好不能推翻 Demo。明确保留的例外是当前游戏选框、现有滚动条系统及目标文件中列出的真实功能与安全语义。

For any WPF, XAML, Playnite UI, theming, layout, controls, animation, dialog, toast,
DataGrid, ScrollBar, TabControl, responsive sizing, DPI, accessibility, or visual-regression work:

1. Use `wpf-apple-desktop-ui` only for quality review and inspect shared resources before editing. Its visual recommendations are subordinate to the Demo-first priority above. The skill is committed in the repository at `.codex/skills/wpf-apple-desktop-ui/SKILL.md`; read that file (and any task-relevant `references/`) first. On this machine it is also installed at `%USERPROFILE%\.codex\skills\wpf-apple-desktop-ui`.
2. Read `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md` and `docs/design/UI_CHANGE_GATE.md`.
3. Repair shared styles/templates rather than a single visual instance.
4. Preserve commands, bindings, business behavior, virtualization, keyboard access, UI Automation, and Playnite compatibility.
5. Validate all affected controls in every theme and never claim Playnite rendering was verified unless it was actually run.
