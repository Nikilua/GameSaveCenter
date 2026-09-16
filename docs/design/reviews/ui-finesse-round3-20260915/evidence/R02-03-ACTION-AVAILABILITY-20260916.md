# R02-03 禁用原因可达证据

## 结论

R02-03 已满足。当前 `codex/ui-finesse-round2` 的代码提交为 `627f8640aa47bf0ae2e24af8e0e65beb5a84f2ad`，在同一工作区核对最新代码后完成；没有把 main 的旧实现覆盖到当前分支，也没有因为本项重建已有服务或 DTO。

- Restore、媒体归类/忽略、云传输校验/重试和远端恢复继续使用原有命令门禁；危险恢复命令没有被强行启用。
- 禁用原因由共享的纯状态计算集中生成，落到 Save、Media 和 Maintenance 的相邻说明；需要配置或 Worker 能力时提供可聚焦的“去维护中心检查”路径。
- 说明文本本身是键盘 Tab 停止点，并设置 AutomationProperties.Name/HelpText，不依赖禁用控件 Tooltip。

## 既有能力核对与实现

本项先核对当前命令和服务边界：

1. `RestoreCommand` 已要求 `!IsBusy`、游戏和备份均已选择、`Snapshot.LudusaviAvailable`；验证恢复准备命令仍要求游戏和备份选择。
2. Media Inbox 批量归类/忽略/恢复忽略仍要求待归类或已忽略模式、有效选中项和目标游戏；来源文件保留与原始副本策略未改。
3. 云传输的 Verify/Retry 继续由 `CanVerifySelectedCloudTransfer()` 和 `CanRetrySelectedCloudTransfer()` 门禁，Pending、Transferring、Verifying、Paused、AuthenticationRequired、RetryScheduled 等状态没有被放行。
4. 远端恢复仍要求先暂存并完成校验；没有对未校验快照放开恢复。

本次只补充可达说明层：

- `ActionAvailabilityHints` 复用现有 ViewModel 状态，按“最先需要解决的条件”生成 Restore、Media Inbox、Cloud Transfer 和 Remote Restore 文案，并区分 Worker 离线、配置缺失、未选择对象、暂存未校验和忙态。
- `DashboardViewModel.WorkspaceStates` 暴露说明及维护路径布尔值；选择游戏、备份、媒体、目标游戏、云传输、远端设备或暂存状态变化时同步通知，不改变命令 CanExecute。
- `WpfUiProduction.xaml` 提供共享 `GscActionAvailabilityHintText`；`Redesign.xaml` 复用现有诊断提示气泡几何。Save 的无选中状态、Media 的批量栏/选中检查器、Maintenance 的云传输/远端恢复检查器均显示说明。
- “去维护中心检查”绑定既有 `OpenMaintenanceCommand`，只在当前状态确实需要维护时出现；没有新造导航或外部写入路径。

## 行为与视觉验证

### R02 定向行为测试

在最终提交 `627f864` 的隔离 Release 输出上执行 `R02ActionAvailabilityHintTests`：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -Command '& { $env:GSC_BUILD_COMMIT = (& git rev-parse HEAD).Trim(); $env:GSC_SOURCE_ROOT = (Get-Location).Path; $buildRoot = (Join-Path (Get-Location) ".tmp\r02-03-build-final"); & dotnet test ".\tests\GameSaveCenter.Playnite.Tests\GameSaveCenter.Playnite.Tests.csproj" -c Release --no-build -m:1 "-p:GscBuildOutputRoot=$buildRoot" -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false --filter "FullyQualifiedName~R02ActionAvailabilityHintTests" --logger "console;verbosity=minimal" }'
```

结果：`4` 通过、`0` 失败、`0` 跳过。

- Restore 测试覆盖无游戏、无备份、Ludusavi 不可用、忙态和正向状态，并确认文案明确提示危险恢复仍禁用。
- Media/Cloud 测试覆盖未选择项、目标游戏缺失、Worker/配置/rclone 能力不足和云传输失败/重试状态。
- Remote Restore 测试覆盖缺少对比、缺远端 ID、未暂存、未校验和已校验正向状态。
- STA WPF 测试实例化生产 `TextBlock` 样式，确认可聚焦、可进入 Tab、存在 UI Automation peer，且 Name/HelpText 与可见说明一致；不是只做源码字符串断言。

### Release 构建与全量回归

执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build.ps1 -Configuration Release -OutputRoot .tmp\r02-03-build-final
```

结果：XAML `24/24`；解决方案构建 `0 warning / 0 error`；Core `83/83/0`、Worker `311/311/0`、Playnite `532` 通过、`57` 跳过、`0` 失败，总计 `589`。57 条跳过沿用 R01-08 的既有 LegacyProductionUiBaseline 分类，没有为本项改写。

`python scripts/validate-source.py` 通过。`wpf-apple-desktop-ui` 质量扫描全仓 770 个 XAML，结果为 `0 errors / 342 warnings / 1106 info`；提示主要来自既有 `.tmp` 宿主资源、历史兼容页面和共享资源启发式规则，不把它们写成本项新增缺陷。该扫描不替代 XAML 编译、行为测试或真实宿主验收。

### Light/Dark 受控 RenderHarness

执行 `scripts\render-qa.ps1 -Configuration Release -Output .tmp\r02-03-render-final`，报告头部为：

```text
EvidenceSource: OffscreenRenderHarness
Commit: 627f8640aa47bf0ae2e24af8e0e65beb5a84f2ad
WorkingTreeClean: True
```

Light/Dark 各覆盖 Overview、Save、Trainer、Media、Maintenance、Task、Settings 的 `1040x700`、`1100x720`、`1366x768`、`2560x1440` 组合，共 56 个视图/尺寸场景，均为 `OK`，最终为 `render-qa OK`。Save 选中版本图和 Media 批量归类图人工目检确认说明文字可见；Save/Media 的有限列表仍保持可读行数和原滚动条，未因增加说明挤压列表到不可用。

## 证据边界与安全范围

测试使用合成 DTO、fake Dashboard 数据、隔离 STA WPF Window 和 offscreen logical DIP；没有启动真实 Playnite，没有写真实存档、媒体、用户云端或外部诊断，也没有删除真实文件。RenderHarness 的报告和 PNG 位于 `.tmp` 临时目录，文档同步后清理，不作为长期二进制证据。

已验证的是状态分支、负例安全门禁、共享样式的可聚焦/Automation peer 行为、绑定状态更新、双主题受控布局和有限列表/滚动回归。未验证真实 Playnite 嵌入、宿主屏幕阅读器朗读、物理 DPI/跨屏、OS 键盘/IME、presented frame、ETW、真实 Worker 时序或大库物理性能；不能把离屏结果写成真实呈现或系统跟踪结论。游戏选框、滚动条系统、命令绑定、取消/错误语义、恢复保护和 net462 兼容未被本项破坏性改动。

## 下一步

下一可执行小批量为 R02-04“图文光学居中”：先核对共享按钮模板及已有四字/双词/图标数字按钮实例，补双主题的实际基线与几何负例；不以新增源码字符串断言代替视觉或行为证据。
