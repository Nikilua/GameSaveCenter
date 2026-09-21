# R22-01 Overview 快照更新时间

日期：2026-09-21  
代码提交：`60db7534`（`统一概览快照更新时间`）

## 本批范围

先核对 `OverviewSnapshotDisplay`、`DashboardViewModel` 和 `OverviewView` 的生产绑定，确认全库/当前游戏快照时间确实进入首页正文与提示。本批只处理快照生成时间，不改变快照加载状态、默认值不等于真实零值、指标计算、首页命令、选框、滚动或 Playnite/net462 兼容。

复用 Contracts 中已有 `TimeDisplayFormatter`：

- `OverviewSnapshotScopeDisplay` 和当前游戏范围摘要正文使用相对时间。
- `OverviewSnapshotUpdatedDisplay` 使用完整本地时区时间和 round-trip UTC 原值；首页快照摘要 TextBlock 的 Tooltip 与 Automation HelpText 绑定同一属性。
- `OverviewSnapshotUpdatedRawUtcDisplay` 保留原始 UTC 投影；旧 `OverviewSnapshotDisplay.Updated` 入口继续返回完整提示语义。
- 未加载或默认 `GeneratedUtc` 仍返回“尚未加载/更新时间未知”，不会把默认快照当作已生成事实。

## 行为证据

`OverviewInteractionTests.OverviewLatestBackupRendersRelativeTextAndFullAutomationEvidence` 使用实际 STA WPF `OverviewView`，读取生产摘要 TextBlock：

- 快照范围正文为合成的相对时间文本“更新于 刚刚”。
- 同一 TextBlock 的 Tooltip 和 `AutomationProperties.HelpText` 均为完整本地时间与 UTC 原值。

`OverviewSnapshotDisplayTests` 覆盖已加载、未加载、默认时间、相对/完整/原始 UTC 三种投影；原有快照计数和未加载不报零断言保持。

## 验证结果

- `OverviewSnapshotDisplayTests`：`3/3`
- `OverviewInteractionTests`：`2/2`
- 本批定向合计：`5/5`
- Release 隔离构建：XAML `24/24`，Playnite/net462、Tests/net472、Worker `0 errors`；仍为既有 `MediaCenterView.xaml.cs:671 CS8602` 2 条 warning。
- `python scripts/validate-source.py`：通过。
- WPF 静态审查：`0 errors / 27 warnings / 177 info`。
- 另运行资源大类后保留 3 条未修改的旧断言漂移：Settings `AutomationIntervalFields`、Media 空态右侧结构、Inbox `GscComboBoxLongText`；该大类为 `139 passed / 39 skipped / 3 failed`，未将其写成本批通过。
- `git diff --check`：通过。

## 未验边界

证据只来自合成快照、fake DataContext、隔离 STA WPF、隔离 Release 构建和隔离测试目录；没有真实存档、媒体、云端或诊断写入。未宣称真实 Playnite/package-host、Windows UIA/读屏、系统时钟跳变、DPI/物理跨屏、最终呈现帧、ETW 或宿主性能已验；Demo 原目录不可用，参考已恢复生产基线。远端备份隔离状态/有效期及其他残余用户可见旧 `ToLocalTime` 入口仍需逐项核对。

