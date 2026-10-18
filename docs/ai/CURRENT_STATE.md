# GameSaveCenter 当前事实入口

更新时间：2026-10-18。产品版本 `0.6.73`。旧记录已归档，本文仅保留当前身份、活动事项、硬性边界与证据索引。

## 当前身份与活动事项

- 远端 `origin/main` 当前在 `d85d2b53`，包含 S00-01 的实现与证据；本地 `main` 有 S00-02 实现/证据和 S07-01 文档记录，尚未推送。S00-02 的 UI 门禁仍未完成，不能把包含它的分支推到 `main`；跟踪文件干净。
- S00-01：已验收。稳定命令目录复用现有 ICommand；受控行为 `7/7`。见 [S00-01 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S00-01/README.md)。
- S00-02：代码完成待验收。Shell 内搜索 overlay，名称/别名/分类过滤，空输入最多 8 项，显示目标与可用状态；受控用例 `21/21`，200 项过滤 `0.554 ms`。见 [S00-02 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S00-02/README.md)。
- S00-02 依赖的真实 UI 门禁未通过：仓库要求精确 SDK `8.0.423`；当前 `dotnet` 仅有 `9.0.302`，常见隔离路径也未发现 SDK8。`GameSaveCenter-Run.cmd` 在构建/安装/版本核验前退出。不得把受控构建或合成呈现写成 Playnite 真实呈现。
- S07-01 已验收：本入口由 254 行压缩至 32 行，完整快照保存在 [归档](CURRENT_STATE_HISTORY_THROUGH_20261018.md)；实现提交 `4ea5e694`，压缩前正文逐字核对相同。验证及边界见 [S07-01 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S07-01/README.md)。后续准入见 [Round4 账本](../design/reviews/ui-finesse-round4-20261018/ROUND4_PROGRESS.md) 与 [自主积压](../AUTONOMOUS_BACKLOG.md)。
- S07-04 已建立 CI 结果记录规则：只在新失败/修复、发布或验收状态变化时新增结论，不为纯记录提交的通过结果再造一笔提交；保留 run/job/artifact 身份。详见 [仓库规则](../../AGENTS.md) 与 [S07-04 证据](../design/reviews/ui-finesse-round4-20261018/evidence/S07-04/README.md)。未调整 workflow/required checks。

## 已核验事实与外部边界

- 最近有完整双 SDK runner 记录的是 [Actions #1086](https://github.com/Nikilua/GameSaveCenter/actions/runs/39077533150)：SDK8.0.423/SDK10.0.401 的 Core、Worker、Playnite WPF、SDK8 Render QA 成功。它验证对应旧提交和 runner，不验证 S00-02 当前本机代码或用户 Playnite。
- 外机 R09 最近报告仍为一次真实 `1/2` 失败；所贴文本与旧输入相同，缺少同次 checkout、TRX、测试/插件 DLL 与 PDB 身份，根因未定。见 [R09 复核证据](../design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。
- 历史 CEF `0x5` 与 ETW 系统跟踪权限拒绝保持原边界；不绕过、不重复探测，只有前置条件可观察变化后再复查。
- Playnite 未由本地门禁运行；OS UIA/读屏、实际 IME、物理 DPI/跨屏和最终呈现仍未验。离屏 WPF、源码测试和 CI 均只证明各自覆盖范围。

## 产品结构与有效约束

- Playnite 插件目标 `net462`，Core/Contracts 为 `netstandard2.0`，Worker 为 `net8.0-windows10.0.19041.0`，Playnite 测试为 `net472`。插件与 Worker 通过 Named Pipe 协作，SQLite 由 Worker 持久化。
- 生产外壳为 `AcrylicProductionShellView`；共享主题入口为 `Themes/AcrylicProductionResources.xaml`、`DesignTokens.xaml`、`WpfUiProduction.xaml` 与 `Redesign.xaml`。六个工作区为 Overview、Saves、Trainers、Media、Tasks、Maintenance。
- Demo-first 仍是视觉优先级，但 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` 当前工作区不存在。缺少 Demo 源时沿用生产共享样式，并明确记录未完成 Demo 对照；不要伪造宿主、屏幕或主题验证。
- 保留真实命令与安全/取消/错误语义、键盘可达性、UI Automation、IME 输入、虚拟化、Playnite 兼容性和现有存档/媒体/云端保护流程。游戏平台筛选以 `DropDownClosed` 提交，避免关闭前的临时选择写入用户偏好。UI 变更须读 `.codex/skills/wpf-apple-desktop-ui/SKILL.md`、`docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`、`docs/design/UI_CHANGE_GATE.md`，并遵循当前 Demo-first AGENTS。

## 验证、提交与启动入口

- 按影响范围运行 `python scripts/validate-source.py .`、WPF 静态审查以及适用的 Release 构建/行为测试；行为矩阵入口为 `scripts/e01-behavior-matrix.ps1`。修改 XAML 后按 UI_CHANGE_GATE 使用 `GameSaveCenter-Run.cmd`；不绕过仓库 SDK 版本检查。`.github`、截图与人工运行项中标记的 `MANUAL QA REQUIRED` 仍需真实人工证据，自动检查通过不能替代。
- 每个独立阶段记录实际输入、结果、未验边界和证据，使用中文提交并推送当前远端分支。纯文档或证据更新不为记录同一次变化反复触发完整 CI。
- 新开发会话依次读取 `CURRENT_STATE.md`、`PROJECT_MEMORY.md`、`WORKLOG.md`、`DEVELOPMENT_HANDOFF.md`，再查最近 Git log/status 与 `AUTONOMOUS_BACKLOG.md`。历史细节按链接追溯，不将归档中的旧“当前状态”当作最新事实。
- 历史入口：[2026-10-18 压缩前快照](CURRENT_STATE_HISTORY_THROUGH_20261018.md)、[2026-09-30 之前历史](CURRENT_STATE_HISTORY_THROUGH_20260930.md)、[项目记忆](PROJECT_MEMORY.md)、[工作日志](WORKLOG.md)、[交接](../DEVELOPMENT_HANDOFF.md)。Round4 实现与逐项状态见 [计划](../design/UI_FINESSE_ROUND4_64_TASKS_2026-10-18.md) 和 [账本](../design/reviews/ui-finesse-round4-20261018/ROUND4_PROGRESS.md)。
