# R03-06 双语长度压力证据（2026-09-17）

## 结论

R03-06 的受控验收条件已满足。当前代码提交为 `52527b6fa23a7af6d9cfa1bf48662f402783417d`（`完善双语窄宽度文案布局`），已推送到 `codex/ui-finesse-round2`。

- 共享生产文字按钮模板从 `CharacterEllipsis/NoWrap` 改为 `TextWrapping=Wrap`、`TextTrimming=None`；窄动作槽保留完整动作文案，不会只留下歧义前缀，Tooltip 仍保留完整值。
- `BilingualLengthStress` 合成 profile 同时放入英文长句和中文长游戏名：中文名称进入当前游戏、任务、活动和关注项；英文长句进入任务详情、活动摘要和关注项建议动作。
- `R03BilingualLengthTests` 定向测试 `2/2`：实际 STA WPF 生产资源在 `154 DIP` 窄槽内测量英文/中文动作，两个按钮均保持 `TextWrapping=Wrap`、`TextTrimming=None`、完整 Tooltip，实际高度超过单行最小高度。
- clean commit 上的 `overviewedges` 双主题报告覆盖 `820x700`、`1040x700`、`1600x900` 窗口 DIP：双语样本均为 `surfaces=6/6`、标题 `CharacterEllipsis`、`titleTooltip=True`、英文长句可见、当前游戏动作 `2` 个且实测高度 `30,30`、`pageOverflowH=False`，最终 `overviewedges OK`。
- clean commit 上的统一 `render-qa` 报告绑定同一 commit，`WorkingTreeClean=True`，Light/Dark、多尺寸页面、滚动/虚拟化、Shell 与 resize 探针均 `render-qa OK`，共 `297` 张 PNG。
- 全量 Release：XAML `24/24`；构建 `0` 警告、`0` 错误；Core `83/83`；Playnite `563/620` 通过、`57` 跳过、`0` 失败。Worker 本阶段未改源代码，沿用同分支最近完整基线 `311/311`。

## 变更范围

- 共享 `WpfUiProduction.xaml` 的文字动作模板统一允许换行；标题、路径、表格详情继续由各自 `TextTrimming=CharacterEllipsis`、Tooltip 或详情入口承载，未把长标题强行展开到破坏页面宽度。
- `FakeDashboardData` 与 `RenderHarness` 只增加合成长度 profile 和首页窄宽度证据，不启动 Worker/Playnite 服务，不改变真实 DTO、命令绑定、取消/错误、恢复保护、游戏选框、滚动条或有限列表策略。
- `R03BilingualLengthTests` 同时保留源契约检查，但签收依据是实际控件模板测量和 `overviewedges` 真实视图树数据，不是单纯字符串存在性断言。

## 可复现入口

```powershell
$env:GSC_BUILD_COMMIT=(git rev-parse HEAD).Trim()
$env:GSC_SOURCE_ROOT=(Get-Location).Path
dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj --no-restore --filter FullyQualifiedName~R03BilingualLengthTests --verbosity minimal
scripts/build.ps1 -Configuration Release -OutputRoot .tmp/r03-06-build
scripts/render-qa.ps1 -Configuration Release -Output .tmp/r03-06-render-final
dotnet run --project tests/GameSaveCenter.RenderHarness/GameSaveCenter.RenderHarness.csproj -c Release --no-build -- overviewedges .tmp/r03-06-overviewedges-final
```

最终报告来自合成数据、隔离输出、受控 STA WPF 与 offscreen logical DIP。它们不等价于真实 Playnite presented frame、宿主字体替换、OS 剪贴板/输入/IME、屏幕阅读器、物理 DPI/跨屏、ETW 或宿主性能；未写真实存档、媒体、云端，也未把离屏截图描述为物理跨屏或系统跟踪证据。

下一可执行任务：R03-07 文案标点统一；先核对现有术语/错误码/单位和复制路径语义，再做最小变更与证据校正。
