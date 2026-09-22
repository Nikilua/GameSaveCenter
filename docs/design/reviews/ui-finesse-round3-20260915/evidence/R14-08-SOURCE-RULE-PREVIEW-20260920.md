# R14-08 来源规则试运行

日期：2026-09-20
代码提交：`89141528`（`codex/ui-finesse-round2`）
状态：代码已提交，隔离构建与定向夹具通过；真实 Playnite 宿主与最终呈现待验。

## 实施事实

- 先复用现有 `MediaSourceRuleDto`、`MediaSyncService` 的媒体扩展名和文件模式匹配，不新建来源规则持久化模型。
- 新增 `MediaSourcePreviewRequestDto`、`MediaSourcePreviewDto` 和 `MediaSourcePreviewItemDto`，通过现有 IPC dispatcher 进入 Worker；每个样本显示命中/排除、文件名、大小、路径和确定原因。
- 试运行只枚举合成/隔离目录，不调用 `AddMediaSourceAsync`、媒体入库、移动或归类链；UI 只保留当前草稿，点击“添加来源”才走原有保存命令。
- Worker 由取消令牌与时间预算共同保护，并将样本数限制在 1–200、扫描数限制在 1–5000、时间限制在 100–5000ms；UI 请求默认 120 项、2000 个扫描项、1500ms。达到数量预算会返回 `Partial` 与 `ScanTruncated`，超时会返回 `Partial` 与 `TimedOut`。
- 来源设置页沿用当前滚动、命令绑定、选框与主题资源；新增有限高度 `MaxHeight=240`、Recycling 的样本列表。同步修复重复组卡片 `Border` 的单子级 XAML 结构，保留其原有虚拟化和滚动属性。

## 可复核证据

- `python scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1`：24/24 通过；`git diff --check`：通过。
- WPF 质量检查：0 errors / 28 warnings / 177 info。新增样本列表有 `FiniteViewport`、`MaxHeight=240` 和显式回收虚拟化；其余 warning 为仓库既有外层布局提示。
- Worker Release 隔离构建（`GscBuildOutputRoot` 指向独立可写目录）：0 warnings / 0 errors。
- Worker `MediaSourcePreviewIsBoundedReadOnlyAndExplainsMatchesAndExclusions`：1/1 通过，覆盖完整扫描、匹配/排除原因、数量预算截断、不写来源规则及不移动样本文件。
- Playnite Release `net462` 构建并运行 `R14SourceRulePreviewTests`：1/1 通过；构建输出有 `MediaCenterView.xaml.cs:664` 的既有 nullable warning 2 条，无错误。测试使用 `GscBuildCommit=89141528` 绑定当前 checkout。
- `render-qa.ps1` 本阶段尝试曾在 RenderHarness 的 linked `obj` 写入阶段被 `Access denied` 阻断且未产生报告；未将离屏代理或静态检查写成真实宿主呈现证据。

## 安全与未验边界

- 业务验证只使用合成文件、fake/隔离 SQLite 和隔离目录；没有修改真实存档、真实媒体、用户云端或发送诊断。Demo 原目录不可用，继续沿用已恢复的生产基线。
- 尚未验证真实 Playnite 加载来源页、真实权限拒绝目录、真实超大目录耗时、视频/图片宿主呈现、DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能；没有绕过被拒绝的系统跟踪权限。
- `main` 工作树仍有用户的 `DashboardView.xaml.cs`、两个恢复文件、R08 测试和 `src.zip`，本阶段未覆盖、未合并。

下一可执行任务：`R15-01 任务阶段可读`；先核对现有 TaskCoordinator/任务事件阶段 DTO，再以小批量推进。R14-08 的真实 Playnite/RenderHarness 复跑仍是待验边界。
