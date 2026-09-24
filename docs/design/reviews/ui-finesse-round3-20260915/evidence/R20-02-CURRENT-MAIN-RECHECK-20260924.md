# R20-02 当前 main 证据复核（2026-09-24）

## 结论与实现

R20-02 在当前 main 已满足，真实 Playnite 环境验证仍待宿主可用。当前实现复用 `DashboardSnapshotDto.GeneratedUtc`、既有全库计数和 `SelectedGame`；没有新造 DTO、Worker 服务或统计入口。

- Overview 全库数字有范围和生成时间；当前游戏摘要明确标成当前游戏并解释与快照同步。
- 首次快照未加载时显示 `—` 并给出未知状态，不伪装为确定的零；成功加载的合法零仍显示 `0`，缺失时间显示“更新时间未知”。未加载时依赖快照的比例条/状态胶囊隐藏。
- 现有导航/命令入口和选择语义保持；没有引入用户目录、真实存档/媒体或云端写入。

## 当前身份行为验证

在 Release 代码/测试身份 `cdd28fd2` 编译后，串行运行以下隔离测试：

| 测试类 | 结果 |
| --- | ---: |
| `OverviewSnapshotDisplayTests` | 3/3 |
| `OverviewPriorityResolverTests` | 15/15 |
| `GamePickerViewModelTests` | 22/22 |
| `OverviewInteractionTests` | 4/4 |
| `GamePickerShellSourceTests` | 4/4 |
| **核心合计** | **48/48** |
| `ReportedWorkspaceLayoutBehaviorTests`（Light/Dark 各四页） | 8/8 |
| **本轮总计** | **56/56，0 失败、0 跳过** |

`OverviewSnapshotDisplayTests` 覆盖未加载占位、合法零与未知时间；其余核心用例复核已有来源/范围与 Overview 真实命令路由。布局类用隔离 STA WPF Window 记录逻辑 DIP，不能代替 Playnite package-host 或物理屏幕。

Release solution build 成功，XAML `24/24`、0 errors，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warnings。六份 TRX 位于 [当前身份原始结果](R20-02-current-main-20260924/)；所有六份均为通过、无跳过，TRX 未含 `InvalidComObjectException`。

## 外部验证边界

未运行真实 Playnite/Worker/外部工具，未读写真实存档、媒体或云端；未声称 presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW 或宿主性能通过。正常隔离 Playnite host 仍受 CEF `platform_channel 0x5` 拒绝访问阻挡，不绕过。

完成 R20-02 证据复核后，先处理用户新报告的 DataGrid 排序崩溃、选中行内容位移和 Media Inbox 冗余目标选择器，再继续 `R20-03` 或其他依赖已满足的 Q/R 小批量。

原始结果：

- [OverviewSnapshotDisplayTests](R20-02-current-main-20260924/OverviewSnapshotDisplayTests.trx)
- [OverviewPriorityResolverTests](R20-02-current-main-20260924/OverviewPriorityResolverTests.trx)
- [GamePickerViewModelTests](R20-02-current-main-20260924/GamePickerViewModelTests.trx)
- [OverviewInteractionTests](R20-02-current-main-20260924/OverviewInteractionTests.trx)
- [GamePickerShellSourceTests](R20-02-current-main-20260924/GamePickerShellSourceTests.trx)
- [ReportedWorkspaceLayoutBehaviorTests](R20-02-current-main-20260924/ReportedWorkspaceLayoutBehaviorTests.trx)
