# Save history summary/action separation — 2026-09-26

## 问题与修复

扩展 `ReportedWorkspaceLayoutBehaviorTests` 时发现窗口化 Save history 摘要中，摘要内容与命令区之间只有 `1.6 DIP`，低于既有 `8–14 DIP` 可读间隔断言；单独运行 Light/Dark 都可复现，排除了测试顺序影响。根因是紧凑行布局给 `SaveHistorySummaryActions` 的顶部 margin 仅 `2 DIP`，WPF 布局取整后实际间隔更小。

将紧凑布局顶部 margin 调整为 `10 DIP`，保留原来的宽布局 `14 DIP` 横向间隔、Grid 行切换和动作命令，不改测试断言。新增注释记录布局取整的原因。

## 验证结果

- Release Playnite.Tests 与依赖的隔离构建 `0 warnings / 0 errors`（复用仓库现有 restore assets，`--no-restore`，输出置于 `.tmp`）。
- `WindowedSaveHistoryDoesNotExpandTheSummaryCardAroundItsActions` Light/Dark `2/2`；两主题实测 gap 均为 `9.6 DIP`，落在既有 `8–14 DIP` 断言范围。摘要卡 `1011.2×429.6 DIP`，内容 `989.6×374.4 DIP`，动作区 `989.6×36 DIP`。
- Release RenderHarness 隔离构建 `0 warnings / 0 errors`；完整双主题/多尺寸运行退出码 0，报告结尾 `render-qa OK`，`PROBLEM` 为 0。Save 页 1040×700 DIP 样本表格仍可读 `4/4` 行。完整报告和中间截图只在本地 `.tmp`，已核对报告后清理。
- Source validation、XAML `24/24`、`git diff --check` 通过。

普通 NuGet restore 受 `%AppData%\NuGet\NuGet.Config` ACL 阻止；未改权限或启动 Playnite。以上是离屏 WPF/逻辑 DIP 与当前测试夹具结果，不代替宿主、物理 DPI、UI Automation 或最终呈现验证。R ledger 仍为 192 个唯一 ID，状态 `106/83/1/1/1`。

## 提交与后续

Settings 阶段独立提交为 `f0999af3`，SaveHistory 阶段独立提交为 `15a5fe22`。首次推送尝试被审批策略拦截；用户批准这两条精确提交后，于 2026-09-26 普通快进推送至 `origin/main`（`b85e53ed..15a5fe22`）。推送后本地 HEAD 与远端分支一致，工作树干净。
